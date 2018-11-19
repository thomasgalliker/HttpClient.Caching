using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class StatsProviderTests
    {
        [Fact]
        public void RecordsHitsCorrectly()
        {
            var provider = new StatsProvider("unit-test");

            for (var i = 0; i < 10; i++)
            {
                provider.ReportCacheHit(HttpStatusCode.OK);
                provider.GetStatistics().PerStatusCode[HttpStatusCode.OK].CacheHit.Should().Be(i + 1);
                provider.ReportCacheHit(HttpStatusCode.BadRequest);
                provider.GetStatistics().PerStatusCode[HttpStatusCode.BadRequest].CacheHit.Should().Be(i + 1);
                provider.GetStatistics().Total.CacheHit.Should().Be(2 * (i + 1));
                provider.GetStatistics().Total.CacheMiss.Should().Be(0);
            }
        }

        [Fact]
        public void RecordsMissesCorrectly()
        {
            var provider = new StatsProvider("unit-test");

            for (var i = 0; i < 10; i++)
            {
                provider.ReportCacheMiss(HttpStatusCode.OK);
                provider.GetStatistics().PerStatusCode[HttpStatusCode.OK].CacheMiss.Should().Be(i + 1);
                provider.ReportCacheMiss(HttpStatusCode.InternalServerError);
                provider.GetStatistics().PerStatusCode[HttpStatusCode.InternalServerError].CacheMiss.Should().Be(i + 1);
                provider.GetStatistics().Total.CacheMiss.Should().Be(2 * (i + 1));
                provider.GetStatistics().Total.CacheHit.Should().Be(0);
            }
        }
    }
}