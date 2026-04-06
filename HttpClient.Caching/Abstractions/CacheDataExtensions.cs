using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Internals;

namespace Microsoft.Extensions.Caching.Abstractions
{
    public static class CacheDataExtensions
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new CacheDataJsonConverter() }
        };

        public static byte[] Serialize(this CacheData cacheData)
        {
            return JsonSerializer.SerializeToUtf8Bytes(cacheData, SerializerOptions);
        }

        public static CacheData Deserialize(this byte[] cacheData)
        {
            try
            {
                return JsonSerializer.Deserialize<CacheData>(cacheData, SerializerOptions)!;
            }
            catch
            {
                return null!;
            }
        }
    }
}
