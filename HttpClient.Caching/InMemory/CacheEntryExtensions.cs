// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.CacheEntryExtensions
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class CacheEntryExtensions
    {
        /// <summary>
        ///     Sets the priority for keeping the cache entry in the cache during a memory pressure tokened cleanup.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="priority"></param>
        public static ICacheEntry SetPriority(this ICacheEntry entry, CacheItemPriority priority)
        {
            entry.Priority = priority;
            return entry;
        }

        /// <summary>
        ///     Expire the cache entry if the given <see cref="T:Microsoft.Extensions.Primitives.IChangeToken" /> expires.
        /// </summary>
        /// <param name="entry">The <see cref="T:Microsoft.Extensions.Caching.Memory.ICacheEntry" />.</param>
        /// <param name="expirationToken">
        ///     The <see cref="T:Microsoft.Extensions.Primitives.IChangeToken" /> that causes the cache
        ///     entry to expire.
        /// </param>
        public static ICacheEntry AddExpirationToken(this ICacheEntry entry, IChangeToken expirationToken)
        {
            if (expirationToken == null)
            {
                throw new ArgumentNullException("expirationToken");
            }
            entry.ExpirationTokens.Add(expirationToken);
            return entry;
        }

        /// <summary>Sets an absolute expiration time, relative to now.</summary>
        /// <param name="entry"></param>
        /// <param name="relative"></param>
        public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, TimeSpan relative)
        {
            entry.AbsoluteExpirationRelativeToNow = new TimeSpan?(relative);
            return entry;
        }

        /// <summary>Sets an absolute expiration date for the cache entry.</summary>
        /// <param name="entry"></param>
        /// <param name="absolute"></param>
        public static ICacheEntry SetAbsoluteExpiration(this ICacheEntry entry, DateTimeOffset absolute)
        {
            entry.AbsoluteExpiration = new DateTimeOffset?(absolute);
            return entry;
        }

        /// <summary>
        ///     Sets how long the cache entry can be inactive (e.g. not accessed) before it will be removed.
        ///     This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="offset"></param>
        public static ICacheEntry SetSlidingExpiration(this ICacheEntry entry, TimeSpan offset)
        {
            entry.SlidingExpiration = new TimeSpan?(offset);
            return entry;
        }

        /// <summary>
        ///     The given callback will be fired after the cache entry is evicted from the cache.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="callback"></param>
        public static ICacheEntry RegisterPostEvictionCallback(this ICacheEntry entry, PostEvictionDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return entry.RegisterPostEvictionCallback(callback, (object)null);
        }

        /// <summary>
        ///     The given callback will be fired after the cache entry is evicted from the cache.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public static ICacheEntry RegisterPostEvictionCallback(this ICacheEntry entry, PostEvictionDelegate callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration() { EvictionCallback = callback, State = state });
            return entry;
        }

        /// <summary>Sets the value of the cache entry.</summary>
        /// <param name="entry"></param>
        /// <param name="value"></param>
        public static ICacheEntry SetValue(this ICacheEntry entry, object value)
        {
            entry.Value = value;
            return entry;
        }

        /// <summary>
        ///     Applies the values of an existing <see cref="T:Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions" /> to
        ///     the entry.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="options"></param>
        public static ICacheEntry SetOptions(this ICacheEntry entry, MemoryCacheEntryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            entry.AbsoluteExpiration = options.AbsoluteExpiration;
            entry.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
            entry.SlidingExpiration = options.SlidingExpiration;
            entry.Priority = options.Priority;
            foreach (IChangeToken expirationToken in (IEnumerable<IChangeToken>)options.ExpirationTokens)
            {
                entry.AddExpirationToken(expirationToken);
            }
            foreach (PostEvictionCallbackRegistration evictionCallback in (IEnumerable<PostEvictionCallbackRegistration>)options.PostEvictionCallbacks)
            {
                entry.RegisterPostEvictionCallback(evictionCallback.EvictionCallback, evictionCallback.State);
            }
            return entry;
        }
    }
}