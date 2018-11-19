using System;
using FluentAssertions;
using HttpClient.Caching.Tests.Testdata;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace HttpClient.Caching.Tests
{
    public class MemoryCacheTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public MemoryCacheTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldSetCache()
        {
            // Arrange
            var expirationTimeSpan = TimeSpan.FromHours(1);
            var options = new MemoryCacheOptions();
            var cache = new MemoryCache(options);
            var cacheEntryOptions = new MemoryCacheEntryOptions { SlidingExpiration = expirationTimeSpan };

            // Act
            for (var i = 1; i <= 10; i++)
            {
                cache.Set($"{i}", new TestPayload(i), cacheEntryOptions);
            }

            // Assert
            cache.TryGetValue("1", out var result1);
            result1.Should().NotBeNull();
            result1.Should().BeOfType<TestPayload>().Which.Id.Should().Be(1);
            cache.Count.Should().Be(10);
        }
    }
}