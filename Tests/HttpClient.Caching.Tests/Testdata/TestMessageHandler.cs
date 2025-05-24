using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClient.Caching.Tests.Testdata
{
    internal class TestMessageHandler : HttpMessageHandler
    {
        internal const HttpStatusCode DefaultResponseStatusCode = HttpStatusCode.OK;
        internal const string DefaultContent = "unit test";
        internal const string DefaultContentType = "text/plain";

        private readonly HttpStatusCode responseStatusCode;
        private readonly string content;
        private readonly string contentType;
        private readonly TimeSpan delay;
        private readonly CacheControlHeaderValue cacheControl;
        private readonly Encoding encoding;

        public int NumberOfCalls { get; set; }

        public TestMessageHandler(
            HttpStatusCode responseStatusCode = DefaultResponseStatusCode,
            string content = DefaultContent,
            string contentType = DefaultContentType,
            Encoding encoding = null,
            TimeSpan delay = default,
            CacheControlHeaderValue cacheControl = null)
        {
            this.responseStatusCode = responseStatusCode;
            this.content = content;
            this.contentType = contentType;
            this.delay = delay;
            this.cacheControl = cacheControl;
            this.encoding = encoding ?? Encoding.UTF8;
        }

        private HttpResponseMessage CreateHttpResponseMessage()
        {
            var httpResponseMessage = new HttpResponseMessage
            {
                Content = new StringContent(this.content, this.encoding, this.contentType),
                StatusCode = this.responseStatusCode,
            };

            if (this.cacheControl != null)
            {
                httpResponseMessage.Headers.CacheControl = this.cacheControl;
            }

            return httpResponseMessage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.NumberOfCalls++;

            if (this.delay != default)
            {
                await Task.Delay(this.delay, cancellationToken);
            }

            return this.CreateHttpResponseMessage();
        }

#if NET5_0_OR_GREATER
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.NumberOfCalls++;

            if (this.delay != default)
            {
                Thread.Sleep(this.delay);
            }

            return this.CreateHttpResponseMessage();
        }
#endif
    }
}