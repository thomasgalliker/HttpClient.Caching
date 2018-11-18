// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.CacheEntry
// Assembly: Microsoft.Extensions.Caching.Memory, Version=2.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: 78529ED0-C4AD-4926-BA4D-60032404EE9B
// Assembly location: C:\Users\thomas\AppData\Local\Temp\Rar$DI01.488\Microsoft.Extensions.Caching.Memory.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Caching.InMemory
{
    internal class CacheEntry : ICacheEntry
    {
        private static readonly Action<object> ExpirationCallback = ExpirationTokensExpired;
        internal readonly object Lock = new object();
        private bool added;
        private readonly Action<CacheEntry> _notifyCacheOfExpiration;
        private readonly Action<CacheEntry> _notifyCacheEntryDisposed;
        private IList<IDisposable> expirationTokenRegistrations;
        private IList<PostEvictionCallbackRegistration> _postEvictionCallbacks;
        private bool isExpired;
        internal IList<IChangeToken> expirationTokens;
        internal DateTimeOffset? absoluteExpiration;
        internal TimeSpan? absoluteExpirationRelativeToNow;
        private TimeSpan? slidingExpiration;

        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        public DateTimeOffset? AbsoluteExpiration
        {
            get
            {
                return this.absoluteExpiration;
            }
            set
            {
                this.absoluteExpiration = value;
            }
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get
            {
                return this.absoluteExpirationRelativeToNow;
            }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException("AbsoluteExpirationRelativeToNow", (object)value, "The relative expiration value must be positive.");
                }
                this.absoluteExpirationRelativeToNow = value;
            }
        }

        public TimeSpan? SlidingExpiration
        {
            get
            {
                return this.slidingExpiration;
            }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException("SlidingExpiration", (object)value, "The sliding expiration value must be positive.");
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
                    this.expirationTokens = (IList<IChangeToken>)new List<IChangeToken>();
                }
                return this.expirationTokens;
            }
        }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks
        {
            get
            {
                if (this._postEvictionCallbacks == null)
                {
                    this._postEvictionCallbacks = (IList<PostEvictionCallbackRegistration>)new List<PostEvictionCallbackRegistration>();
                }
                return this._postEvictionCallbacks;
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
                throw new ArgumentNullException("notifyCacheEntryDisposed");
            }
            if (notifyCacheOfExpiration == null)
            {
                throw new ArgumentNullException("notifyCacheOfExpiration");
            }
            this.Key = key;
            this._notifyCacheEntryDisposed = notifyCacheEntryDisposed;
            this._notifyCacheOfExpiration = notifyCacheOfExpiration;
        }

        public void Dispose()
        {
            if (this.added)
            {
                return;
            }
            this.added = true;
            this._notifyCacheEntryDisposed(this);
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
                this.SetExpired((EvictionReason)3);
                return true;
            }
            if (this.slidingExpiration.HasValue)
            {
                TimeSpan timeSpan = now - this.LastAccessed;
                TimeSpan? slidingExpiration = this.slidingExpiration;
                if ((slidingExpiration.HasValue ? (timeSpan >= slidingExpiration.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                {
                    this.SetExpired((EvictionReason)3);
                    return true;
                }
            }
            return false;
        }

        internal bool CheckForExpiredTokens()
        {
            if (this.expirationTokens != null)
            {
                for (int index = 0; index < ((ICollection<IChangeToken>)this.expirationTokens).Count; ++index)
                {
                    if (this.expirationTokens[index].HasChanged)
                    {
                        this.SetExpired((EvictionReason)4);
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
                for (int i = 0; i < this.expirationTokens.Count; ++i)
                {
                    IChangeToken expirationToken = this.expirationTokens[i];
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
            Task.Factory.StartNew((Action<object>)(state =>
                                                          {
                                                              CacheEntry cacheEntry = (CacheEntry)state;
                                                              cacheEntry.SetExpired((EvictionReason)4);
                                                              cacheEntry._notifyCacheOfExpiration(cacheEntry);
                                                          }), obj, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
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
                for (int i = 0; i < tokenRegistrations.Count; ++i)
                {
                    tokenRegistrations[i].Dispose();
                }
            }
        }

        internal void InvokeEvictionCallbacks()
        {
            if (this._postEvictionCallbacks == null)
            {
                return;
            }
            TaskFactory factory = Task.Factory;
            CancellationToken none = CancellationToken.None;
            int num = 8;
            TaskScheduler scheduler = TaskScheduler.Default;
            factory.StartNew((Action<object>)(state => InvokeCallbacks((CacheEntry)state)), (object)this, none, (TaskCreationOptions)num, scheduler);
        }

        private static void InvokeCallbacks(CacheEntry entry)
        {
            IList<PostEvictionCallbackRegistration> callbackRegistrationList = Interlocked.Exchange<IList<PostEvictionCallbackRegistration>>(ref entry._postEvictionCallbacks, (IList<PostEvictionCallbackRegistration>)null);
            if (callbackRegistrationList == null)
            {
                return;
            }
            for (int index = 0; index < ((ICollection<PostEvictionCallbackRegistration>)callbackRegistrationList).Count; ++index)
            {
                PostEvictionCallbackRegistration callbackRegistration = callbackRegistrationList[index];
                try
                {
                    PostEvictionDelegate evictionCallback = callbackRegistration.EvictionCallback;
                    if (evictionCallback != null)
                    {
                        object key = entry.Key;
                        object obj = entry.Value;
                        EvictionReason evictionReason = entry.EvictionReason;
                        object state = callbackRegistration.State;
                        evictionCallback.Invoke(key, obj, evictionReason, state);
                    }
                }
                catch (Exception ex)
                {
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
                        using (IEnumerator<IChangeToken> resource_0 = ((IEnumerable<IChangeToken>)this.expirationTokens).GetEnumerator())
                        {
                            while (((IEnumerator)resource_0).MoveNext())
                            {
                                IChangeToken local_5 = resource_0.Current;
                                CacheEntryExtensions.AddExpirationToken((ICacheEntry)parent, local_5);
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
                DateTimeOffset? absoluteExpiration1 = this.absoluteExpiration;
                DateTimeOffset? absoluteExpiration2 = parent.absoluteExpiration;
                if ((absoluteExpiration1.HasValue & absoluteExpiration2.HasValue ? (absoluteExpiration1.GetValueOrDefault() < absoluteExpiration2.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                {
                    return;
                }
            }
            parent.absoluteExpiration = this.absoluteExpiration;
        }
    }
}