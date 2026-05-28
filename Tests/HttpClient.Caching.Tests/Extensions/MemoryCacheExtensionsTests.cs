namespace HttpClient.Caching.Tests.Extensions
{
    public class MemoryCacheExtensionsTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public MemoryCacheExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Clear_RemovesAllEntries()
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

        [Fact]
        public void TryGetValue_ReturnsTrueAndTypedValue()
        {
            // Arrange
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("1", new TestPayload(1));

            // Act
            var result = MemoryCacheExtensions.TryGetValue<TestPayload>(memoryCache, "1", out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().NotBeNull();
            value!.Id.Should().Be(1);
        }

        [Fact]
        public void TryGetValue_ReturnFalseAndNull()
        {
            // Arrange
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set<object?>("1", null);

            // Act
            var result = MemoryCacheExtensions.TryGetValue<TestPayload>(memoryCache, "1", out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }
    }
}
