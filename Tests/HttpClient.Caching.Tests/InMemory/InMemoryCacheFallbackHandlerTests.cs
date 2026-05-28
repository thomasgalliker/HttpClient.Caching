namespace HttpClient.Caching.Tests.InMemory
{
    using HttpClient = System.Net.Http.HttpClient;

    public class InMemoryCacheFallbackHandlerTests
    {
        private const string TestUrl = "http://unittest/";

        [Fact]
        public async Task AlwaysCallsTheHttpHandler()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));

            // Act twice
            await client.GetAsync(TestUrl);
            cache.Get(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl).Should().NotBeNull(); // ensure it's cached before the 2nd call
            await client.GetAsync(TestUrl);

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task AlwaysUpdatesTheCacheOnSuccess()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl));
            var client = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // Act twice, validate cache is called each time
            await client.GetAsync(TestUrl);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl), Times.Once);
            await client.GetAsync(TestUrl);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdatesTheCacheForHeadAndGetIndependently()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl));
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Head + TestUrl));
            var client = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // Act twice, validate cache is called each time
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, TestUrl));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, TestUrl));
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Head + TestUrl), Times.Once);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl), Times.Once);
        }

        [Fact]
        public async Task NeverUpdatesTheCacheOnFailure()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            object? expectedValue;
            cache.Setup(c => c.CreateEntry(It.IsAny<string>()));
            cache.Setup(c => c.TryGetValue(TestUrl, out expectedValue)).Returns(false);
            var client = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // Act
            await client.GetAsync(TestUrl);

            // Assert
            cache.Verify(c => c.CreateEntry(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TriesToAccessCacheOnFailureButReturnsErrorIfNotInCache()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            object? expectedValue;
            cache.Setup(c => c.TryGetValue(TestUrl, out expectedValue)).Returns(false);
            var client = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // Act
            var result = await client.GetAsync(TestUrl);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetsItFromTheHttpCallAfterBeingInCache()
        {
            // Arrange
            var testMessageHandler1 = new TestMessageHandler(content: "message-1", delay: TimeSpan.FromMilliseconds(100));
            var testMessageHandler2 = new TestMessageHandler(content: "message-2");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client1 = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler1, TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(1), null, cache));
            var client2 = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler2, TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(1), null, cache));

            // Act twice
            var result1 = await client1.GetAsync(TestUrl);
            cache.Get(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + TestUrl).Should().NotBeNull();
            var result2 = await client2.GetAsync(TestUrl);

            // Assert
            // - that each message handler got called
            testMessageHandler1.NumberOfCalls.Should().Be(1);
            testMessageHandler2.NumberOfCalls.Should().Be(1);

            // - that the 2nd result got served from the http call
            var data1 = await result1.Content.ReadAsStringAsync();
            var data2 = await result2.Content.ReadAsStringAsync();
            data1.Should().BeEquivalentTo("message-1");
            data2.Should().BeEquivalentTo("message-2");
        }

        [Fact]
        public async Task GetsItFromTheCacheWhenUnsuccessful()
        {
            // Arrange
            var testMessageHandler1 = new TestMessageHandler(HttpStatusCode.OK, "message-1");
            var testMessageHandler2 = new TestMessageHandler(HttpStatusCode.InternalServerError, "message-2");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client1 = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler1, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));
            var client2 = new HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler2, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));

            // Act twice
            var result1 = await client1.GetAsync(TestUrl);
            var result2 = await client2.GetAsync(TestUrl);

            // Assert
            // - that each message handler got called
            testMessageHandler1.NumberOfCalls.Should().Be(1);
            testMessageHandler2.NumberOfCalls.Should().Be(1);

            // - that the 2nd result got served from cache
            var data1 = await result1.Content.ReadAsStringAsync();
            var data2 = await result2.Content.ReadAsStringAsync();
            data1.Should().BeEquivalentTo("message-1");
            data2.Should().BeEquivalentTo(data1);
        }
    }
}
