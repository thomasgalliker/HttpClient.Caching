// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.ICacheEntry
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Represents an entry in the <see cref="T:Microsoft.Extensions.Caching.Memory.IMemoryCache" /> implementation.
    /// </summary>
    public interface ICacheEntry : IDisposable
    {
        /// <summary>Gets the key of the cache entry.</summary>
        object Key { get; }

        /// <summary>Gets or set the value of the cache entry.</summary>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Extensions.Primitives.IChangeToken" /> instances which cause the cache entry to expire.
        /// </summary>
        IList<IChangeToken> ExpirationTokens { get; }

        /// <summary>
        /// Gets or sets the callbacks will be fired after the cache entry is evicted from the cache.
        /// </summary>
        IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }

        /// <summary>
        /// Gets or sets the priority for keeping the cache entry in the cache during a
        /// memory pressure triggered cleanup. The default is <see cref="F:Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal" />.
        /// </summary>
        CacheItemPriority Priority { get; set; }
    }
}
