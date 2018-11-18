// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.IMemoryCache
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Represents a local in-memory cache whose values are not serialized.
    /// </summary>
    public interface IMemoryCache : IDisposable
    {
        /// <summary>Gets the item associated with this key if present.</summary>
        /// <param name="key">An object identifying the requested entry.</param>
        /// <param name="value">The located value or null.</param>
        /// <returns>True if the key was found.</returns>
        bool TryGetValue(object key, out object value);

        /// <summary>Create or overwrite an entry in the cache.</summary>
        /// <param name="key">An object identifying the entry.</param>
        /// <returns>The newly created <see cref="T:Microsoft.Extensions.Caching.Memory.ICacheEntry" /> instance.</returns>
        ICacheEntry CreateEntry(object key);

        /// <summary>Removes the object associated with the given key.</summary>
        /// <param name="key">An object identifying the entry.</param>
        void Remove(object key);

        void Clear();
    }
}
