using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class StatsResultTests
    {
        [Fact]
        public void CalculatesTotalCorrectly()
        {
            var stats = new StatsResult("unit-test");

            long hits = 0;
            long misses = 0;

            for (var i = 0; i < 10; i++)
            {
                stats.PerStatusCode.Add((HttpStatusCode)400 + i, new StatsValue { CacheHit = 2 * i, CacheMiss = i });
                hits += 2 * i;
                misses += i;
            }

            stats.Total.CacheHit.Should().Be(hits);
            stats.Total.CacheMiss.Should().Be(misses);
        }
    }
}