using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.InMemory
{
    internal class CacheEntry : ICacheEntry
    {
        private static readonly Action<object> ExpirationCallback = ExpirationTokensExpired;
        internal readonly object Lock = new object();
        private bool added;
        private readonly Action<CacheEntry> notifyCacheOfExpiration;
        private readonly Action<CacheEntry> notifyCacheEntryDisposed;
        private IList<IDisposable> expirationTokenRegistrations;
        private IList<PostEvictionCallbackRegistration> postEvictionCallbacks;
        private bool isExpired;
        internal IList<IChangeToken> expirationTokens;
        internal DateTimeOffset? absoluteExpiration;
        internal TimeSpan? absoluteExpirationRelativeToNow;
        private TimeSpan? slidingExpiration;

        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        public DateTimeOffset? AbsoluteExpiration
        {
            get { return this.absoluteExpiration; }
            set { this.absoluteExpiration = value; }
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get { return this.absoluteExpirationRelativeToNow; }
            set
            {
                var nullable = value;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.AbsoluteExpirationRelativeToNow), value, "The relative expiration value must be positive.");
                }

                this.absoluteExpirationRelativeToNow = value;
            }
        }

        public TimeSpan? SlidingExpiration
        {
            get { return this.slidingExpiration; }
            set
            {
                var nullable = value;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= TimeSpan.Zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.SlidingExpiration), value, "The sliding expiration value must be positive.");
                }

                this.slidingExpiration = value;
            }
        }

        public IList<IChangeToken> ExpirationTokens
        {
            get
            {
                if (this.expirationTokens == null)
                {
                    this.expirationTokens = new List<IChangeToken>();
                }

                return this.expirationTokens;
            }
        }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks
        {
            get
            {
                if (this.postEvictionCallbacks == null)
                {
                    this.postEvictionCallbacks = new List<PostEvictionCallbackRegistration>();
                }

                return this.postEvictionCallbacks;
            }
        }

        public object Key { get; private set; }

        public object Value { get; set; }

        internal DateTimeOffset LastAccessed { get; set; }

        internal EvictionReason EvictionReason { get; private set; }

        internal CacheEntry(object key, Action<CacheEntry> notifyCacheEntryDisposed, Action<CacheEntry> notifyCacheOfExpiration)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (notifyCacheEntryDisposed == null)
            {
                throw new ArgumentNullException(nameof(notifyCacheEntryDisposed));
            }

            if (notifyCacheOfExpiration == null)
            {
                throw new ArgumentNullException(nameof(notifyCacheOfExpiration));
            }

            this.Key = key;
            this.notifyCacheEntryDisposed = notifyCacheEntryDisposed;
            this.notifyCacheOfExpiration = notifyCacheOfExpiration;
        }

        public void Dispose()
        {
            if (this.added)
            {
                return;
            }

            this.added = true;
            this.notifyCacheEntryDisposed(this);
            //this.PropagateOptions(_added);
        }

        internal bool CheckExpired(DateTimeOffset now)
        {
            if (!this.isExpired && !this.CheckForExpiredTime(now))
            {
                return this.CheckForExpiredTokens();
            }

            return true;
        }

        internal void SetExpired(EvictionReason reason)
        {
            if (this.EvictionReason == null)
            {
                this.EvictionReason = reason;
            }

            this.isExpired = true;
            this.DetachTokens();
        }

        private bool CheckForExpiredTime(DateTimeOffset now)
        {
            if (this.absoluteExpiration.HasValue && this.absoluteExpiration.Value <= now)
            {
                this.SetExpired(EvictionReason.Expired);
                return true;
            }

            if (this.slidingExpiration.HasValue)
            {
                var timeSpan = now - this.LastAccessed;
                var slidingExpiration = this.slidingExpiration;
                if ((slidingExpiration.HasValue ? (timeSpan >= slidingExpiration.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                {
                    this.SetExpired(EvictionReason.Expired);
                    return true;
                }
            }

            return false;
        }

        internal bool CheckForExpiredTokens()
        {
            if (this.expirationTokens != null)
            {
                for (var index = 0; index < this.expirationTokens.Count; ++index)
                {
                    if (this.expirationTokens[index].HasChanged)
                    {
                        this.SetExpired(EvictionReason.TokenExpired);
                        return true;
                    }
                }
            }

            return false;
        }

        internal void AttachTokens()
        {
            if (this.expirationTokens == null)
            {
                return;
            }

            lock (this.Lock)
            {
                for (var i = 0; i < this.expirationTokens.Count; ++i)
                {
                    var expirationToken = this.expirationTokens[i];
                    if (expirationToken.ActiveChangeCallbacks)
                    {
                        if (this.expirationTokenRegistrations == null)
                        {
                            this.expirationTokenRegistrations = new List<IDisposable>(1);
                        }

                        this.expirationTokenRegistrations.Add(expirationToken.RegisterChangeCallback(ExpirationCallback, this));
                    }
                }
            }
        }

        private static void ExpirationTokensExpired(object obj)
        {
            Task.Factory.StartNew(state =>
            {
                var cacheEntry = (CacheEntry)state;
                cacheEntry.SetExpired(EvictionReason.TokenExpired);
                cacheEntry.notifyCacheOfExpiration(cacheEntry);
            }, obj, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        private void DetachTokens()
        {
            lock (this.Lock)
            {
                var tokenRegistrations = this.expirationTokenRegistrations;
                if (tokenRegistrations == null)
                {
                    return;
                }

                this.expirationTokenRegistrations = null;
                foreach (var disposable in tokenRegistrations)
                {
                    disposable.Dispose();
                }
            }
        }

        internal void InvokeEvictionCallbacks()
        {
            if (this.postEvictionCallbacks == null)
            {
                return;
            }

            var factory = Task.Factory;
            var none = CancellationToken.None;
            var scheduler = TaskScheduler.Default;
            factory.StartNew(state => InvokeCallbacks((CacheEntry)state), this, none, TaskCreationOptions.DenyChildAttach, scheduler);
        }

        private static void InvokeCallbacks(CacheEntry entry)
        {
            var callbackRegistrationList = Interlocked.Exchange(ref entry.postEvictionCallbacks, null);
            if (callbackRegistrationList == null)
            {
                return;
            }

            foreach (var callbackRegistration in callbackRegistrationList)
            {
                try
                {
                    var evictionCallback = callbackRegistration.EvictionCallback;
                    if (evictionCallback != null)
                    {
                        var key = entry.Key;
                        var obj = entry.Value;
                        var evictionReason = entry.EvictionReason;
                        var state = callbackRegistration.State;
                        evictionCallback.Invoke(key, obj, evictionReason, state);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }
            }
        }

        internal void PropagateOptions(CacheEntry parent)
        {
            if (parent == null)
            {
                return;
            }

            if (this.expirationTokens != null)
            {
                lock (this.Lock)
                {
                    lock (parent.Lock)
                    {
                        using (var changeTokenEnumerator = this.expirationTokens.GetEnumerator())
                        {
                            while (changeTokenEnumerator.MoveNext())
                            {
                                var changeToken = changeTokenEnumerator.Current;
                                parent.AddExpirationToken(changeToken);
                            }
                        }
                    }
                }
            }

            if (!this.absoluteExpiration.HasValue)
            {
                return;
            }

            if (parent.absoluteExpiration.HasValue)
            {
                var absoluteExpiration = this.absoluteExpiration;
                var parentAbsoluteExpiration = parent.absoluteExpiration;
                if ((absoluteExpiration.HasValue & parentAbsoluteExpiration.HasValue ? (absoluteExpiration.GetValueOrDefault() < parentAbsoluteExpiration.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                {
                    return;
                }
            }

            parent.absoluteExpiration = this.absoluteExpiration;
        }
    }
}