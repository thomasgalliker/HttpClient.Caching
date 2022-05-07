using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Abstractions;
using Xunit;

namespace HttpClient.Caching.Tests.Abstractions
{
    public class StatusCodeExtensionsTests
    {
        [Fact]
        public void TakesValueFromHttpCode()
        {
            // setup
            var ticks = new Random().Next(0, 100000);
            var code = HttpStatusCode.NotFound;
            var mappings = new Dictionary<HttpStatusCode, TimeSpan> { { code, TimeSpan.FromTicks(ticks) } };

            // execute
            var result = code.GetAbsoluteExpirationRelativeToNow(mappings);

            // validate
            result.Should().Be(mappings[code]);
        }

        [Fact]
        public void TakesValueFromHttpCodeCategory()
        {
            // setup
            var random = new Random();
            var mappings = CacheExpirationProvider.CreateSimple(TimeSpan.FromTicks(random.Next(0, 100000)), TimeSpan.FromTicks(random.Next(0, 100000)), TimeSpan.FromTicks(random.Next(0, 100000)));

            // execute
            var successResult = HttpStatusCode.Created.GetAbsoluteExpirationRelativeToNow(mappings);
            var clientErrorResult = HttpStatusCode.NotFound.GetAbsoluteExpirationRelativeToNow(mappings);
            var serverErrorResult = HttpStatusCode.GatewayTimeout.GetAbsoluteExpirationRelativeToNow(mappings);

            // validate
            successResult.Should().Be(mappings[HttpStatusCode.OK]);
            clientErrorResult.Should().Be(mappings[HttpStatusCode.BadRequest]);
            serverErrorResult.Should().Be(mappings[HttpStatusCode.InternalServerError]);
        }

        [Fact]
        public void TakesDefaultValue()
        {
            // setup
            var mappings = new Dictionary<HttpStatusCode, TimeSpan>();

            // execute
            var result = HttpStatusCode.OK.GetAbsoluteExpirationRelativeToNow(mappings);

            // validate
            result.Should().Be(TimeSpan.FromDays(1));
        }
    }
}