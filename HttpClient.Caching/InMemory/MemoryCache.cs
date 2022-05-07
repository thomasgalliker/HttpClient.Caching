using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory.Internal;

namespace Microsoft.Extensions.Caching.InMemory
{
    public class MemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<object, CacheEntry> entries;
        private bool disposed;
        private readonly Action<CacheEntry> setEntry;
        private readonly Action<CacheEntry> entryExpirationNotification;
        private readonly ISystemClock clock;
        private readonly TimeSpan expirationScanFrequency;
        private DateTimeOffset lastExpirationScan;

        public int Count => this.entries.Count;

        private ICollection<KeyValuePair<object, CacheEntry>> EntriesCollection => this.entries;

        public MemoryCache() : this(new MemoryCacheOptions())
        {
        }

        public MemoryCache(MemoryCacheOptions memoryCacheOptions)
        {
            if (memoryCacheOptions == null)
            {
                throw new ArgumentNullException(nameof(memoryCacheOptions));
            }

            this.entries = new ConcurrentDictionary<object, CacheEntry>();
            this.setEntry = this.SetEntry;
            this.entryExpirationNotification = this.EntryExpired;
            this.clock = memoryCacheOptions.Clock ?? new SystemClock();
            this.expirationScanFrequency = memoryCacheOptions.ExpirationScanFrequency;
            this.lastExpirationScan = this.clock.UtcNow;
        }

        ~MemoryCache()
        {
            this.Dispose(false);
        }

        public ICacheEntry CreateEntry(object key)
        {
            this.CheckDisposed();
            return new CacheEntry(key, this.setEntry, this.entryExpirationNotification);
        }

        private void SetEntry(CacheEntry entry)
        {
            if (this.disposed)
            {
                return;
            }

            var utcNow = this.clock.UtcNow;
            var nullable = new DateTimeOffset?();
            if (entry.absoluteExpirationRelativeToNow.HasValue)
            {
                var dateTimeOffset = utcNow;
                var expirationRelativeToNow = entry.absoluteExpirationRelativeToNow;
                nullable = expirationRelativeToNow.HasValue ? dateTimeOffset + expirationRelativeToNow.GetValueOrDefault() : new DateTimeOffset?();
            }
            else if (entry.absoluteExpiration.HasValue)
            {
                nullable = entry.absoluteExpiration;
            }

            if (nullable.HasValue && (!entry.absoluteExpiration.HasValue || nullable.Value < entry.absoluteExpiration.Value))
            {
                entry.absoluteExpiration = nullable;
            }

            entry.LastAccessed = utcNow;
            if (this.entries.TryGetValue(entry.Key, out var cacheEntry))
            {
                cacheEntry.SetExpired(EvictionReason.Replaced);
            }

            if (!entry.CheckExpired(utcNow))
            {
                bool flag;
                if (cacheEntry == null)
                {
                    flag = this.entries.TryAdd(entry.Key, entry);
                }
                else
                {
                    flag = this.entries.TryUpdate(entry.Key, entry, cacheEntry);
                    if (!flag)
                    {
                        flag = this.entries.TryAdd(entry.Key, entry);
                    }
                }

                if (flag)
                {
                    entry.AttachTokens();
                }
                else
                {
                    entry.SetExpired((EvictionReason)2);
                    entry.InvokeEvictionCallbacks();
                }

                if (cacheEntry != null)
                {
                    cacheEntry.InvokeEvictionCallbacks();
                }
            }
            else
            {
                entry.InvokeEvictionCallbacks();
                if (cacheEntry != null)
                {
                    this.RemoveEntry(cacheEntry);
                }
            }

            this.StartScanForExpiredItems();
        }

        public bool TryGetValue(object key, out object result)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.CheckDisposed();
            result = null;
            var utcNow = this.clock.UtcNow;
            var flag = false;
            if (this.entries.TryGetValue(key, out var entry))
            {
                if (entry.CheckExpired(utcNow) && entry.EvictionReason != EvictionReason.Replaced)
                {
                    this.RemoveEntry(entry);
                }
                else
                {
                    flag = true;
                    entry.LastAccessed = utcNow;
                    result = entry.Value;
                    //entry.PropagateOptions(CacheEntryHelper.Current);
                }
            }

            this.StartScanForExpiredItems();
            return flag;
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.CheckDisposed();
            if (this.entries.TryRemove(key, out var cacheEntry))
            {
                cacheEntry.SetExpired(EvictionReason.Removed);
                cacheEntry.InvokeEvictionCallbacks();
            }

            this.StartScanForExpiredItems();
        }

        public void Clear()
        {
            this.CheckDisposed();
            var keys = this.entries.Keys.ToList();
            foreach (var key in keys)
            {
                if (this.entries.TryRemove(key, out var cacheEntry))
                {
                    cacheEntry.SetExpired(EvictionReason.Removed);
                    cacheEntry.InvokeEvictionCallbacks();
                }
            }

            this.StartScanForExpiredItems();
        }

        private void RemoveEntry(CacheEntry entry)
        {
            if (!this.EntriesCollection.Remove(new KeyValuePair<object, CacheEntry>(entry.Key, entry)))
            {
                return;
            }

            entry.InvokeEvictionCallbacks();
        }

        private void EntryExpired(CacheEntry entry)
        {
            this.RemoveEntry(entry);
            this.StartScanForExpiredItems();
        }

        private void StartScanForExpiredItems()
        {
            var utcNow = this.clock.UtcNow;
            if (!(this.expirationScanFrequency < utcNow - this.lastExpirationScan))
            {
                return;
            }

            this.lastExpirationScan = utcNow;
            var factory = Task.Factory;
            var none = CancellationToken.None;
            var scheduler = TaskScheduler.Default;
            factory.StartNew(state => ScanForExpiredItems((MemoryCache)state), this, none, TaskCreationOptions.DenyChildAttach, scheduler);
        }

        private static void ScanForExpiredItems(MemoryCache cache)
        {
            var utcNow = cache.clock.UtcNow;
            foreach (var entry in cache.entries.Values)
            {
                if (entry.CheckExpired(utcNow))
                {
                    cache.RemoveEntry(entry);
                }
            }
        }

        public void Compact(double percentage)
        {
            var entriesToRemove = new List<CacheEntry>();
            var priorityEntries1 = new List<CacheEntry>();
            var priorityEntries2 = new List<CacheEntry>();
            var priorityEntries3 = new List<CacheEntry>();
            var utcNow = this.clock.UtcNow;

            foreach (var cacheEntry in this.entries.Values)
            {
                if (cacheEntry.CheckExpired(utcNow))
                {
                    entriesToRemove.Add(cacheEntry);
                }
                else
                {
                    switch ((int)cacheEntry.Priority)
                    {
                        case 0:
                            priorityEntries1.Add(cacheEntry);
                            continue;
                        case 1:
                            priorityEntries2.Add(cacheEntry);
                            continue;
                        case 2:
                            priorityEntries3.Add(cacheEntry);
                            continue;
                        case 3:
                            continue;
                        default:
                            throw new NotSupportedException("Not implemented: " + cacheEntry.Priority);
                    }
                }
            }

            var removalCountTarget = (int)(this.entries.Count * percentage);
            this.ExpirePriorityBucket(removalCountTarget, entriesToRemove, priorityEntries1);
            this.ExpirePriorityBucket(removalCountTarget, entriesToRemove, priorityEntries2);
            this.ExpirePriorityBucket(removalCountTarget, entriesToRemove, priorityEntries3);
            foreach (var entry in entriesToRemove)
            {
                this.RemoveEntry(entry);
            }
        }

        private void ExpirePriorityBucket(int removalCountTarget, List<CacheEntry> entriesToRemove, List<CacheEntry> priorityEntries)
        {
            if (removalCountTarget <= entriesToRemove.Count)
            {
                return;
            }

            if (entriesToRemove.Count + priorityEntries.Count <= removalCountTarget)
            {
                foreach (var priorityEntry in priorityEntries)
                {
                    priorityEntry.SetExpired(EvictionReason.Capacity);
                }

                entriesToRemove.AddRange(priorityEntries);
            }
            else
            {
                foreach (var cacheEntry in priorityEntries.OrderBy(entry => entry.LastAccessed))
                {
                    cacheEntry.SetExpired(EvictionReason.Capacity);
                    entriesToRemove.Add(cacheEntry);
                    if (removalCountTarget <= entriesToRemove.Count)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            this.disposed = true;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(typeof(MemoryCache).FullName);
            }
        }
    }
}