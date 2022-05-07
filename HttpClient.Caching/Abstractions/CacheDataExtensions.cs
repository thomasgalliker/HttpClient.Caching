using System;
using Newtonsoft.Json;

namespace Microsoft.Extensions.Caching.Abstractions
{
    public static class CacheDataExtensions
    {
        public static byte[] Serialize(this CacheData cacheData)
        {
            var json = JsonConvert.SerializeObject(cacheData);
            var bytes = new byte[json.Length * sizeof(char)];
            Buffer.BlockCopy(json.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static CacheData Deserialize(this byte[] cacheData)
        {
            try
            {
                var chars = new char[cacheData.Length / sizeof(char)];
                Buffer.BlockCopy(cacheData, 0, chars, 0, cacheData.Length);
                var json = new string(chars);
                var data = JsonConvert.DeserializeObject<CacheData>(json);
                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}