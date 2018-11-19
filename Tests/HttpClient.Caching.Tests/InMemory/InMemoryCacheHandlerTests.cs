using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpClient.Caching.Tests.Testdata;
using Microsoft.Extensions.Caching.InMemory;
using Xunit;

namespace HttpClient.Caching.Tests.InMemory
{
    public class InMemoryCacheHandlerTests
    {
        [Fact]
        public async Task CachesTheResult()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler));

            // execute twice
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        [Fact]
        public async Task GetsTheDataAgainAfterEntryIsGoneFromCache()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute twice
            await client.GetAsync("http://unittest");
            cache.Remove(HttpMethod.Get + new Uri("http://unittest").ToString());
            await client.GetAsync("http://unittest");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task IsCaseSensitive()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute for different URLs, only different by casing
            await client.GetAsync("http://unittest/foo.html");
            await client.GetAsync("http://unittest/FOO.html");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task CachesPerUrl()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute for different URLs
            await client.GetAsync("http://unittest1");
            await client.GetAsync("http://unittest2");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task OnlyCachesGetAndHeadResults()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute twice for different methods
            await client.PostAsync("http://unittest", new StringContent(string.Empty));
            await client.PostAsync("http://unittest", new StringContent(string.Empty));
            await client.PutAsync("http://unittest", new StringContent(string.Empty));
            await client.PutAsync("http://unittest", new StringContent(string.Empty));
            await client.DeleteAsync("http://unittest");
            await client.DeleteAsync("http://unittest");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(6);
        }

        [Fact]
        public async Task CachesHeadAndGetRequestWithoutConflict()
        {
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute twice for different methods
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "http://unittest"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://unittest"));

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task DataFromCallMatchesDataFromCache()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache));

            // execute twice for different methods
            var originalResult = await client.GetAsync("http://unittest");
            var cachedResult = await client.GetAsync("http://unittest");
            var originalResultString = await originalResult.Content.ReadAsStringAsync();
            var cachedResultString = await cachedResult.Content.ReadAsStringAsync();

            // validate
            originalResultString.Should().BeEquivalentTo(cachedResultString);
            originalResultString.Should().BeEquivalentTo(TestMessageHandler.DefaultContent);
        }

        [Fact]
        public async Task ReturnsResponseHeader()
        {
            // setup
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.OK, "test content", "text/plain", Encoding.UTF8);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler));

            // execute 
            HttpResponseMessage response = await client.GetAsync("http://unittest");

            // validate
            response.Content.Headers.ContentType.MediaType.Should().Be("text/plain");
            response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
        }

        [Fact]
        public async Task DisableCachePerStatusCode()
        {
            // setup
            var cacheExpirationPerStatusCode = new Dictionary<HttpStatusCode, TimeSpan>();

            cacheExpirationPerStatusCode.Add((HttpStatusCode)200, TimeSpan.FromSeconds(0));

            var testMessageHandler = new TestMessageHandler();
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheExpirationPerStatusCode));

            // execute twice
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task InvalidatesCacheCorrectly()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var handler = new InMemoryCacheHandler(testMessageHandler);
            var client = new System.Net.Http.HttpClient(handler);

            // execute twice, with cache invalidation in between
            var uri = new Uri("http://unittest");
            await client.GetAsync(uri);
            handler.InvalidateCache(uri);
            await client.GetAsync(uri);

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task InvalidatesCachePerMethod()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var handler = new InMemoryCacheHandler(testMessageHandler);
            var client = new System.Net.Http.HttpClient(handler);

            // execute with two methods, and clean up one cache
            var uri = new Uri("http://unittest");
            await client.GetAsync(uri);
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            testMessageHandler.NumberOfCalls.Should().Be(2);

            // clean cache
            handler.InvalidateCache(uri, HttpMethod.Head);

            // execute both actions, and only one should be retrieved from cache
            await client.GetAsync(uri);
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            testMessageHandler.NumberOfCalls.Should().Be(3);
        }
    }
}