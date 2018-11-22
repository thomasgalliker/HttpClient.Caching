using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.InMemory
{
    public class MemoryCacheEntryOptions
    {
        private TimeSpan? absoluteExpirationRelativeToNow;
        private TimeSpan? slidingExpiration;

        /// <summary>
        ///     Gets the <see cref="T:Microsoft.Extensions.Caching.Abstractions.IChangeToken" /> instances which cause the cache
        ///     entry to
        ///     expire.
        /// </summary>
        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

        /// <summary>
        ///     Gets or sets the callbacks will be fired after the cache entry is evicted from the cache.
        /// </summary>
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();

        /// <summary>
        ///     Gets or sets the priority for keeping the cache entry in the cache during a
        ///     memory pressure triggered cleanup. The default is
        ///     <see cref="F:Microsoft.Extensions.Caching.Abstractions.CacheItemPriority.Normal" />.
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
            get { return this.absoluteExpirationRelativeToNow; }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.AbsoluteExpirationRelativeToNow), value, "The relative expiration value must be positive.");
                }

                this.absoluteExpirationRelativeToNow = value;
            }
        }

        /// <summary>
        ///     Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        ///     This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration
        {
            get { return this.slidingExpiration; }
            set
            {
                TimeSpan? nullable = value;
                TimeSpan zero = TimeSpan.Zero;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= zero ? 1 : 0) : 0) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.SlidingExpiration), value, "The sliding expiration value must be positive.");
                }

                this.slidingExpiration = value;
            }
        }
    }
}