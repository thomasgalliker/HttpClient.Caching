using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.InMemory;

namespace Microsoft.Extensions.Caching.Abstractions
{
    public static class CacheExtensions
    {
        public static object Get(this IMemoryCache cache, object key)
        {
            cache.TryGetValue(key, out var obj);
            return obj;
        }

        public static TItem Get<TItem>(this IMemoryCache cache, object key)
        {
            cache.TryGetValue(key, out TItem obj);
            return obj;
        }

        public static bool TryGetValue<TItem>(this IMemoryCache cache, object key, out TItem value)
        {
            if (cache.TryGetValue(key, out var obj))
            {
                value = (TItem)obj;
                return true;
            }

            value = default(TItem);
            return false;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value)
        {
            var entry = cache.CreateEntry(key);
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, DateTimeOffset absoluteExpiration)
        {
            var entry = cache.CreateEntry(key);
            DateTimeOffset? nullable = absoluteExpiration;
            entry.AbsoluteExpiration = nullable;
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var entry = cache.CreateEntry(key);
            TimeSpan? nullable = absoluteExpirationRelativeToNow;
            entry.AbsoluteExpirationRelativeToNow = nullable;
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, IChangeToken expirationToken)
        {
            var entry = cache.CreateEntry(key);
            var expirationToken1 = expirationToken;
            entry.AddExpirationToken(expirationToken1);
            entry.Value = value;
            entry.Dispose();
            return value;
        }

        public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions options)
        {
            using (var entry = cache.CreateEntry(key))
            {
                if (options != null)
                {
                    entry.SetOptions(options);
                }

                entry.Value = value;
            }

            return value;
        }

        public static TItem GetOrCreate<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory)
        {
            if (!cache.TryGetValue(key, out var obj))
            {
                var entry = cache.CreateEntry(key);
                obj = factory(entry);
                entry.SetValue(obj);
                entry.Dispose();
            }

            return (TItem)obj;
        }

        public static async Task<TItem> GetOrCreateAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        {
            if (!cache.TryGetValue(key, out var obj))
            {
                var entry = cache.CreateEntry(key);
                obj = await factory(entry);
                entry.SetValue(obj);
                entry.Dispose();
                entry = null;
            }

            return (TItem)obj;
        }
    }
}