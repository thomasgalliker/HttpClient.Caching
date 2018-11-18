// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.CacheExtensions
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class CacheExtensions
    {
        public static object Get(this IMemoryCache cache, object key)
        {
            object obj = null;
            cache.TryGetValue(key, out obj);
            return obj;
        }

        public static TItem Get<TItem>(this IMemoryCache cache, object key)
        {
            TItem obj;
            cache.TryGetValue(key, out obj);
            return obj;
        }

        public static bool TryGetValue<TItem>(this IMemoryCache cache, object key, out TItem value)
        {
            object obj;
            if (cache.TryGetValue(key, out obj))
            {
                value = (TItem)obj;
                return true;
            }
            value = default(TItem);
            return false;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value)
        {
            ICacheEntry entry = cache.CreateEntry(key);
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            ICacheEntry entry = cache.CreateEntry(key);
            DateTimeOffset? nullable = absoluteExpiration;
            entry.AbsoluteExpiration = nullable;
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            ICacheEntry entry = cache.CreateEntry(key);
            TimeSpan? nullable = absoluteExpirationRelativeToNow;
            entry.AbsoluteExpirationRelativeToNow = nullable;
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, IChangeToken expirationToken)
        {
            ICacheEntry entry = cache.CreateEntry(key);
            IChangeToken expirationToken1 = expirationToken;
            entry.AddExpirationToken(expirationToken1);
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions options)
        {
            using (ICacheEntry entry = cache.CreateEntry(key))
            {
                if (options != null)
                    entry.SetOptions(options);
                entry.Value = value;
            }
            return value;
        }

        public static TItem GetOrCreate<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory)
        {
            object obj;
            if (!cache.TryGetValue(key, out obj))
            {
                ICacheEntry entry = cache.CreateEntry(key);
                obj = factory(entry);
                entry.SetValue(obj);
                entry.Dispose();
            }
            return (TItem)obj;
        }

        public static async Task<TItem> GetOrCreateAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        {
            object obj;
            if (!cache.TryGetValue(key, out obj))
            {
                ICacheEntry entry = cache.CreateEntry(key);
                obj = await factory(entry);
                entry.SetValue(obj);
                entry.Dispose();
                entry = null;
            }
            return (TItem)obj;
        }
    }
}
