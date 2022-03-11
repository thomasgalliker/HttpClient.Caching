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
    public class MethodUriHeadersCacheKeysProviderTests
    {
        private readonly string url = "http://unittest/";

        [Fact]
        public void ShouldGetKey()
        {
            // Arrange
            var headersNames = new[] { "X-API-VERSION" };
            var cacheKeysProvider = new MethodUriHeadersCacheKeysProvider(headersNames);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.url),
                Method = HttpMethod.Get 
            };
            request.Headers.Add("X-API-VERSION", "v1");

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;HEA_X-API-VERSION_v1;URI_http://unittest/;");
        }
    }
}