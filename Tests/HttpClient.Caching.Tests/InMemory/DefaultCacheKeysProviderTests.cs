using System;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Caching.InMemory;
using Xunit;

namespace HttpClient.Caching.Tests.InMemory
{
    public class DefaultCacheKeysProviderTests
    {
        private readonly string url = "http://unittest/";

        [Fact]
        public void ShouldGetKey()
        {
            // Arrange
            var cacheKeysProvider = new DefaultCacheKeysProvider();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.url),
                Method = HttpMethod.Get,
            };

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;URI_http://unittest/;");
        }
    }
}