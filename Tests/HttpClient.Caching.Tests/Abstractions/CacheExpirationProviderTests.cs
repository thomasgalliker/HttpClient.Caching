using System;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class CacheExpirationProviderTests
    {
        [Fact]
        public void PutsTheCorrectStatusCodesIntoTheMapping()
        {
            // execute
            var mappings = CacheExpirationProvider.CreateSimple(TimeSpan.FromTicks(1), TimeSpan.FromTicks(2), TimeSpan.FromTicks(3));

            // validate
            mappings.Count.Should().Be(3);
            mappings[HttpStatusCode.OK].Should().Be(TimeSpan.FromTicks(1));
            mappings[HttpStatusCode.BadRequest].Should().Be(TimeSpan.FromTicks(2));
            mappings[HttpStatusCode.InternalServerError].Should().Be(TimeSpan.FromTicks(3));
        }
    }
}