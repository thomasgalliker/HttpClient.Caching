using System;
using Microsoft.Extensions.Caching.InMemory.Internal;

namespace Microsoft.Extensions.Caching.InMemory
{
    public class MemoryCacheOptions
    {
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1.0);

        public ISystemClock Clock { get; set; }
    }
}