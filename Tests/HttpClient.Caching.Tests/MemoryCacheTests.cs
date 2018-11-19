using System;
using FluentAssertions;
using Microsoft.Extensions.Caching.InMemory;
using Microsoft.Extensions.Caching.Memory;
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
            for (var i = 1; i <= 1000; i++)
            {
                if (i % 100 == 0)
                {
                    this.testOutputHelper.WriteLine($"Added {i} items");
                }

                cache.Set($"{i}", new TestPayload(i), cacheEntryOptions);
            }

            // Assert
            cache.Count.Should().Be(1000);
        }
    }
}