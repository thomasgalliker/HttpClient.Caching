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

        #region Tests with cache key provider MethodUriHeadersCacheKeysProvider

        /// <summary>
        /// By using <see cref="MethodUriHeadersCacheKeysProvider"/> without any header then <see cref="InMemoryCacheHandler"/> should
        /// behave like using <see cref="DefaultCacheKeysProvider"/>
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProvider()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            // no headers is provided so ICacheKeyProvider MethodUriHeadersCacheKeysProvider should behave like DefaultCacheKeysProvider
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(null);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));

            // execute twice
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // validate
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with custom headers, but requests don't specify any header
        /// used by cache key provider
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_NoHeaders()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify different value for the header
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_DifferentValues()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value2");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// one request specify a value for the header, the other none doesn't specify any hader value
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_HeaderValueInOneRequest()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify same value for the header
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_SameValues()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify same value for the headers
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_SameValues_MultipleHeaders()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER", "ANOTHER-HEADER", "HEADER3" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            request1.Headers.Add("ANOTHER-HEADER", "Value2");
            request1.Headers.Add("HEADER3", "Value3");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");
            request2.Headers.Add("ANOTHER-HEADER", "Value2");
            request2.Headers.Add("HEADER3", "Value3");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify same value for the headers that are common but not all specific request contains the same
        /// headers
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_SameValues_MultipleHeaders2()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER", "ANOTHER-HEADER", "HEADER3" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            request1.Headers.Add("ANOTHER-HEADER", "Value2");
            request1.Headers.Add("HEADER3", "Value3");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");
            request2.Headers.Add("HEADER3", "Value3");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify same value for the headers but in different order
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_SameValues_MultipleHeaders_DifferentOrder()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER", "ANOTHER-HEADER", "HEADER3" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            request1.Headers.Add("HEADER3", "Value3");
            request1.Headers.Add("ANOTHER-HEADER", "Value2");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");
            request2.Headers.Add("ANOTHER-HEADER", "Value2");
            request2.Headers.Add("HEADER3", "Value3");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with specific headers, 
        /// both requests specify same value for specific header.
        /// A request include some headers which aren't considered for cache key composition
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_SameValues2()
        {
            // arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            request1.Headers.Add("ANOTHER-CUSTOM-HEADER", "ValueX"); // this header isn't considered for cache key composition
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");

            // act
            // execute twice
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        #endregion Tests with cache key provider MethodUriHeadersCacheKeysProvider

        [Fact]
        public async Task GetsTheDataAgainAfterEntryIsGoneFromCache()
        {
            // setup
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var inMemoryCacheHandler = new InMemoryCacheHandler(testMessageHandler, null, null, cache, null);
            var client = new System.Net.Http.HttpClient(inMemoryCacheHandler);

            // execute twice
            await client.GetAsync("http://unittest");
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://unittest"));
            cache.Remove(inMemoryCacheHandler.CacheKeysProvider.GetKey(request));
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
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

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
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

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
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

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
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

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
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

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