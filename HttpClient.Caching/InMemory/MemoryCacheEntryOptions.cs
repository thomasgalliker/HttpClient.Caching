// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Caching.Memory
{
    public class MemoryCacheEntryOptions
    {
        private TimeSpan? _absoluteExpirationRelativeToNow;
        private TimeSpan? _slidingExpiration;

        /// <summary>
        ///     Gets the <see cref="T:Microsoft.Extensions.Primitives.IChangeToken" /> instances which cause the cache entry to
        ///     expire.
        /// </summary>
        public IList<IChangeToken> ExpirationTokens { get; } = (IList<IChangeToken>)new List<IChangeToken>();

        /// <summary>
        ///     Gets or sets the callbacks will be fired after the cache entry is evicted from the cache.
        /// </summary>
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = (IList<PostEvictionCallbackRegistration>)new List<PostEvictionCallbackRegistration>();

        /// <summary>
        ///     Gets or sets the priority for keeping the cache entry in the cache during a
        ///     memory pressure triggered cleanup. The default is
        ///     <see cref="F:Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal" />.
        /// </summary>
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        /// <summary>
        ///     Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        ///     Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get
            {
                return this._absoluteExpirationRelativeToNow;
            }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException("AbsoluteExpirationRelativeToNow", (object)value, "The relative expiration value must be positive.");
                }
                this._absoluteExpirationRelativeToNow = value;
            }
        }

        /// <summary>
        ///     Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        ///     This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration
        {
            get
            {
                return this._slidingExpiration;
            }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException("SlidingExpiration", (object)value, "The sliding expiration value must be positive.");
                }
                this._slidingExpiration = value;
            }
        }
    }
}