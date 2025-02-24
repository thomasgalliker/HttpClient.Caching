using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler));

            // Act
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

#if NET5_0_OR_GREATER
        [Fact]
        public void CachesTheResult_Send()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler));

            // Act
            var response1 = client.Send(new HttpRequestMessage(HttpMethod.Get, "http://unittest"));
            var response2 = client.Send(new HttpRequestMessage(HttpMethod.Get, "http://unittest"));

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
            response1.Content.Should().BeEquivalentTo(response2.Content);
        }
#endif

        #region Tests with cache key provider MethodUriHeadersCacheKeysProvider

        /// <summary>
        /// By using <see cref="MethodUriHeadersCacheKeysProvider"/> without any header then <see cref="InMemoryCacheHandler"/> should
        /// behave like using <see cref="DefaultCacheKeysProvider"/>
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProvider()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            // no headers are provided so ICacheKeyProvider MethodUriHeadersCacheKeysProvider should behave like DefaultCacheKeysProvider
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(null);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));

            // Act
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        /// <summary>
        /// <see cref="InMemoryCacheHandler"/> use <see cref="MethodUriHeadersCacheKeysProvider"/> with custom headers, but requests don't specify any header
        /// used by cache key provider
        /// </summary>
        [Fact]
        public async Task CachesTheResult_MethodUriHeadersCacheKeysProviderWithCustomHeaders_NoHeaders()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value2");

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
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

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
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

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
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

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
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
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new [] { "CUSTOM-HEADER" }); //this is the header that will be included in cache key generator
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheKeysProvider: cacheKeyProvider));
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request1.Headers.Add("CUSTOM-HEADER", "Value1");
            request1.Headers.Add("ANOTHER-CUSTOM-HEADER", "ValueX"); // this header isn't considered for cache key composition
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            request2.Headers.Add("CUSTOM-HEADER", "Value1");

            // Act
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(1);
        }

        #endregion Tests with cache key provider MethodUriHeadersCacheKeysProvider

        [Fact]
        public async Task GetsTheDataAgainAfterEntryIsGoneFromCache()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var inMemoryCacheHandler = new InMemoryCacheHandler(testMessageHandler, null, null, cache, null);
            var client = new System.Net.Http.HttpClient(inMemoryCacheHandler);

            // Act
            await client.GetAsync("http://unittest");
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://unittest"));
            cache.Remove(inMemoryCacheHandler.CacheKeysProvider.GetKey(request));
            await client.GetAsync("http://unittest");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task IsCaseSensitive()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            // Act for different URLs, only different by casing
            await client.GetAsync("http://unittest/foo.html");
            await client.GetAsync("http://unittest/FOO.html");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task CachesPerUrl()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            // Act for different URLs
            await client.GetAsync("http://unittest1");
            await client.GetAsync("http://unittest2");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task CachesWithCacheControlHeaders_NoCacheTrue()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue{ NoCache = true, NoStore = true };
            var testMessageHandler = new TestMessageHandler(cacheControl: cacheControl);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://unittest");

            // Act for different URLs
            await client.SendAsync(request1);
            await client.SendAsync(request2);

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task OnlyCachesGetAndHeadResults()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            // Act for different methods
            await client.PostAsync("http://unittest", new StringContent(string.Empty));
            await client.PostAsync("http://unittest", new StringContent(string.Empty));
            await client.PutAsync("http://unittest", new StringContent(string.Empty));
            await client.PutAsync("http://unittest", new StringContent(string.Empty));
            await client.DeleteAsync("http://unittest");
            await client.DeleteAsync("http://unittest");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(6);
        }

        [Fact]
        public async Task CachesHeadAndGetRequestWithoutConflict()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            // Act for different methods
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "http://unittest"));
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://unittest"));

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task DataFromCallMatchesDataFromCache()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, null, null, cache, null));

            // Act for different methods
            var originalResult = await client.GetAsync("http://unittest");
            var cachedResult = await client.GetAsync("http://unittest");
            var originalResultString = await originalResult.Content.ReadAsStringAsync();
            var cachedResultString = await cachedResult.Content.ReadAsStringAsync();

            // Assert
            originalResultString.Should().BeEquivalentTo(cachedResultString);
            originalResultString.Should().BeEquivalentTo(TestMessageHandler.DefaultContent);
        }

        [Fact]
        public async Task ReturnsResponseHeader()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler(HttpStatusCode.OK, "test content", "text/plain", Encoding.UTF8);
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler));

            // Act
            var response = await client.GetAsync("http://unittest");

            // Assert
            response.Content.Headers.ContentType.MediaType.Should().Be("text/plain");
            response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
        }

        [Fact]
        public async Task DisableCachePerStatusCode()
        {
            // Arrange
            var cacheExpirationPerStatusCode = new Dictionary<HttpStatusCode, TimeSpan>
            {
                { (HttpStatusCode)200, TimeSpan.FromSeconds(0) }
            };

            var testMessageHandler = new TestMessageHandler();
            var client = new System.Net.Http.HttpClient(new InMemoryCacheHandler(testMessageHandler, cacheExpirationPerStatusCode));

            // Act
            await client.GetAsync("http://unittest");
            await client.GetAsync("http://unittest");

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task InvalidatesCacheCorrectly()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var handler = new InMemoryCacheHandler(testMessageHandler);
            var client = new System.Net.Http.HttpClient(handler);

            // Act, with cache invalidation in between
            var uri = new Uri("http://unittest");
            await client.GetAsync(uri);
            handler.InvalidateCache(uri);
            await client.GetAsync(uri);

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(2);
        }

        [Fact]
        public async Task InvalidatesCachePerMethod()
        {
            // Arrange
            var testMessageHandler = new TestMessageHandler();
            var handler = new InMemoryCacheHandler(testMessageHandler);
            var client = new System.Net.Http.HttpClient(handler);

            // Act with two methods, and clean up one cache
            var uri = new Uri("http://unittest");
            await client.GetAsync(uri);
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            testMessageHandler.NumberOfCalls.Should().Be(2);

            // clean cache
            handler.InvalidateCache(uri, HttpMethod.Head);

            // Act both actions, and only one should be retrieved from cache
            await client.GetAsync(uri);
            await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

            // Assert
            testMessageHandler.NumberOfCalls.Should().Be(3);
        }
    }
}