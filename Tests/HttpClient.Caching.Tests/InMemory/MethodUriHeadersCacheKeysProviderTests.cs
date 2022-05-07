using System;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Caching.InMemory;
using Xunit;

namespace HttpClient.Caching.Tests.InMemory
{
    public class MethodUriHeadersCacheKeysProviderTests
    {
        private readonly string url = "http://unittest/";

        [Fact]
        public void ShouldGetKey_EmptyHeaderNames()
        {
            // Arrange
            var headersNames = new string[] { };
            var cacheKeysProvider = new MethodUriHeadersCacheKeysProvider(headersNames);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.url),
                Method = HttpMethod.Get
            };
            request.Headers.Add("X-HEADER-1", "Value1");

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;URI_http://unittest/;");
        }

        [Fact]
        public void ShouldGetKey_WithMatchingHeader()
        {
            // Arrange
            var headersNames = new[] { "X-HEADER-2", "X-HEADER-1" };
            var cacheKeysProvider = new MethodUriHeadersCacheKeysProvider(headersNames);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.url),
                Method = HttpMethod.Get
            };
            request.Headers.Add("X-HEADER-1", "Value1");

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;HEA_X-HEADER-1_Value1;URI_http://unittest/;");
        }

        [Fact]
        public void ShouldGetKey_WithoutMatchingHeader()
        {
            // Arrange
            var headersNames = new[] { "X-HEADER-2", "X-HEADER-1" };
            var cacheKeysProvider = new MethodUriHeadersCacheKeysProvider(headersNames);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.url),
                Method = HttpMethod.Get
            };
            request.Headers.Add("X-HEADER-3", "Value3");

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;URI_http://unittest/;");
        }
    }
}