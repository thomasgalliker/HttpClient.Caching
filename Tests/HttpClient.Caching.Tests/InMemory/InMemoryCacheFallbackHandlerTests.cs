using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClient.Caching.Tests.Testdata;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Moq;
using Xunit;

namespace HttpClient.Caching.Tests.InMemory
{
    public class InMemoryCacheFallbackHandlerTests
    {
        private readonly string url = "http://unittest/";

        [Fact]
        public async Task AlwaysCallsTheHttpHandler()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));

            // execute twice
            await client.GetAsync(this.url);
            cache.Get(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url).Should().NotBeNull(); // ensure it's cached before the 2nd call
            await client.GetAsync(this.url);

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task AlwaysUpdatesTheCacheOnSuccess()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url));
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // execute twice, validate cache is called each time
            await client.GetAsync(this.url);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url), Times.Once);
            await client.GetAsync(this.url);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdatesTheCacheForHeadAndGetIndependently()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url));
            cache.Setup(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Head + this.url));
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // execute twice, validate cache is called each time
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, this.url));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, this.url));
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Head + this.url), Times.Once);
            cache.Verify(c => c.CreateEntry(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url), Times.Once);
        }

        [Fact]
        public async Task NeverUpdatesTheCacheOnFailure()
        {
            // setup
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            object expectedValue;
            cache.Setup(c => c.CreateEntry(It.IsAny<string>()));
            cache.Setup(c => c.TryGetValue(this.url, out expectedValue)).Returns(false);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // execute
            await client.GetAsync(this.url);

            // validate
            cache.Verify(c => c.CreateEntry(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TriesToAccessCacheOnFailureButReturnsErrorIfNotInCache()
        {
            // setup
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            var cacheTime = TimeSpan.FromSeconds(123);
            object expectedValue;
            cache.Setup(c => c.TryGetValue(this.url, out expectedValue)).Returns(false);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache.Object));

            // execute
            var result = await client.GetAsync(this.url);

            // validate
            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetsItFromTheHttpCallAfterBeingInCache()
        {
            // setup
            var testMessageHandler1 = new TestMessageHandler(content: "message-1", delay: TimeSpan.FromMilliseconds(100));
            var testMessageHandler2 = new TestMessageHandler(content: "message-2");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client1 = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler1, TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(1), null, cache));
            var client2 = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler2, TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(1), null, cache));

            // execute twice
            var result1 = await client1.GetAsync(this.url);
            cache.Get(InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url).Should().NotBeNull();
            var result2 = await client2.GetAsync(this.url);

            // validate
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
            // setup
            var testMessageHandler1 = new TestMessageHandler(HttpStatusCode.OK, "message-1");
            var testMessageHandler2 = new TestMessageHandler(HttpStatusCode.InternalServerError, "message-2");
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client1 = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler1, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));
            var client2 = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler2, TimeSpan.FromDays(1), TimeSpan.FromDays(1), null, cache));

            // execute twice
            var result1 = await client1.GetAsync(this.url);
            var result2 = await client2.GetAsync(this.url);

            // validate
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