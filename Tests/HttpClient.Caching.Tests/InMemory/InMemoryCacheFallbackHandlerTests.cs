using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClient.Caching.Tests.Testdata;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
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
            var cache = new MemoryCache(new MemoryCacheOptions());
            var cacheTime = TimeSpan.FromSeconds(123);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache));
            var key = InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url;

            // execute twice and validate cache is updated each time
            await client.GetAsync(this.url);
            cache.Get(key).Should().NotBeNull();
            await client.GetAsync(this.url);
            cache.Get(key).Should().NotBeNull();
        }

        [Fact]
        public async Task UpdatesTheCacheForHeadAndGetIndependently()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var cacheTime = TimeSpan.FromSeconds(123);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache));
            var getKey = InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url;
            var headKey = InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Head + this.url;

            // execute and validate cache entries for both methods are created
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, this.url));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, this.url));
            cache.Get(headKey).Should().NotBeNull();
            cache.Get(getKey).Should().NotBeNull();
        }

        [Fact]
        public async Task NeverUpdatesTheCacheOnFailure()
        {
            // setup
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var cacheTime = TimeSpan.FromSeconds(123);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache));
            var key = InMemoryCacheFallbackHandler.CacheFallbackKeyPrefix + HttpMethod.Get + this.url;

            // execute
            await client.GetAsync(this.url);

            // validate
            cache.Get(key).Should().BeNull();
        }

        [Fact]
        public async Task TriesToAccessCacheOnFailureButReturnsErrorIfNotInCache()
        {
            // setup
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.InternalServerError);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var cacheTime = TimeSpan.FromSeconds(123);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheFallbackHandler(testMessageHandler, TimeSpan.FromDays(1), cacheTime, null, cache));

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
