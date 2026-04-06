using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class CacheDataExtensionsTests
    {
        [Fact]
        public void SerializeAndDeserialize_RoundTripsCacheData()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage
            {
                ReasonPhrase = "Accepted",
                StatusCode = HttpStatusCode.Accepted,
                Version = new Version(2, 0)
            };

            var cacheData = new CacheData(
                new byte[] { 1, 2, 3 },
                httpResponseMessage,
                new Dictionary<string, IEnumerable<string>>
                {
                    ["X-Test"] = new[] { "one", "two" }
                },
                new Dictionary<string, IEnumerable<string>>
                {
                    ["Content-Type"] = new[] { "application/json" }
                });

            // Act
            var serialized = cacheData.Serialize();
            var deserialized = serialized.Deserialize();

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.Data.Should().Equal(1, 2, 3);
            deserialized.CachableResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            deserialized.CachableResponse.ReasonPhrase.Should().Be("Accepted");
            deserialized.CachableResponse.Version.Should().Be(new Version(2, 0));
            deserialized.Headers.Should().ContainKey("X-Test");
            deserialized.Headers["X-Test"].Should().Equal("one", "two");
            deserialized.ContentHeaders.Should().ContainKey("Content-Type");
            deserialized.ContentHeaders["Content-Type"].Should().Equal("application/json");
        }

        [Fact]
        public void Deserialize_InvalidPayload_ReturnsNull()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3 };

            // Act
            var deserialized = bytes.Deserialize();

            // Assert
            deserialized.Should().BeNull();
        }

        [Fact]
        public void Deserialize_LegacyNewtonsoftPayload_ReturnsNull()
        {
            // Arrange
            const string legacyJson = @"{
  ""CachableResponse"": {
    ""Version"": ""2.0"",
    ""Content"": {
      ""Headers"": []
    },
    ""StatusCode"": 202,
    ""ReasonPhrase"": ""Accepted"",
    ""Headers"": [
      {
        ""Key"": ""X-From-Response"",
        ""Value"": [
          ""header-value""
        ]
      }
    ],
    ""TrailingHeaders"": [],
    ""RequestMessage"": null,
    ""IsSuccessStatusCode"": true
  },
  ""Data"": ""AQID"",
  ""Headers"": {
    ""X-Test"": [
      ""one"",
      ""two""
    ]
  },
  ""ContentHeaders"": {
    ""Content-Type"": [
      ""application/json""
    ]
  }
}";

            var chars = legacyJson.ToCharArray();
            var legacyBytes = new byte[chars.Length * sizeof(char)];
            Buffer.BlockCopy(chars, 0, legacyBytes, 0, legacyBytes.Length);

            // Act
            var deserialized = legacyBytes.Deserialize();

            // Assert
            deserialized.Should().BeNull();
        }
    }
}
