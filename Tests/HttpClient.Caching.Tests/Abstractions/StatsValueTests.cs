using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class StatsValueTests
    {
        [Fact]
        public void CalculatesTotalCorrectly()
        {
            // setup
            var stats = new StatsValue { CacheHit = 100, CacheMiss = 50 };

            // validate
            stats.TotalRequests.Should().Be(150);
        }

        [Theory]
        [InlineData(100, 0, 1)]
        [InlineData(0, 154, 0)]
        [InlineData(10, 10, 0.5)]
        [InlineData(30, 10, 0.75)]
        public void CalculatesHitsPercentCorrectly(long hits, long misses, double percent)
        {
            // setup
            var stats = new StatsValue { CacheHit = hits, CacheMiss = misses };

            // validate
            stats.CacheHitsPercent.Should().Be(percent);
        }

        [Theory]
        [InlineData(100, 0, 0)]
        [InlineData(0, 154, 1)]
        [InlineData(10, 10, 0.5)]
        [InlineData(30, 10, 0.25)]
        public void CalculatesMissPercentCorrectly(long hits, long misses, double percent)
        {
            // setup
            var stats = new StatsValue { CacheHit = hits, CacheMiss = misses };

            // validate
            stats.CacheMissPercent.Should().Be(percent);
        }
    }
}