using FluentAssertions;
using HttpClient.Caching.Tests.TestData;
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
            var memoryCache = new MemoryCache(options);
            var entryOptions = new MemoryCacheEntryOptions { SlidingExpiration = expirationTimeSpan };

            // Act
            for (var i = 1; i <= 10; i++)
            {
                memoryCache.Set($"{i}", new TestPayload(i), entryOptions);
            }

            // Assert
            memoryCache.TryGetValue("1", out var result1);
            result1.Should().NotBeNull();
            result1.Should().BeOfType<TestPayload>().Which.Id.Should().Be(1);
            memoryCache.Count.Should().Be(10);
        }

        [Fact]
        public void ShouldClearCache()
        {
            // Arrange
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            };
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            for (var i = 1; i <= 10; i++)
            {
                memoryCache.Set($"{i}", new TestPayload(i), options);
            }

            // Act
            ((IMemoryCache)memoryCache).Clear();

            // Assert
            memoryCache.Count.Should().Be(0);
            memoryCache.TryGetValue("1", out var result1).Should().BeFalse();
            result1.Should().BeNull();
        }
    }
}
