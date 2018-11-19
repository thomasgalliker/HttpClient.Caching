using System;
using System.Net;
using System.Net.Http;
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
        private readonly Encoding encoding;

        public int NumberOfCalls { get; set; }

        public TestMessageHandler(HttpStatusCode responseStatusCode = DefaultResponseStatusCode, string content = DefaultContent, string contentType = DefaultContentType, Encoding encoding = null, TimeSpan delay = default(TimeSpan))
        {
            this.responseStatusCode = responseStatusCode;
            this.content = content;
            this.contentType = contentType;
            this.delay = delay;
            this.encoding = encoding ?? Encoding.UTF8;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.NumberOfCalls++;

            // optional delay
            if (this.delay != default(TimeSpan))
            {
                await Task.Delay(this.delay, cancellationToken);
            }

            // simulate actual result
            return new HttpResponseMessage { Content = new StringContent(this.content, this.encoding, this.contentType), StatusCode = this.responseStatusCode };
        }
    }
}