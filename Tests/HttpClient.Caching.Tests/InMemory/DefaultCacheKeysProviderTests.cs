namespace HttpClient.Caching.Tests.InMemory
{
    public class DefaultCacheKeysProviderTests
    {
        private const string TestUrl = "http://unittest/";

        [Fact]
        public void ShouldGetKey()
        {
            // Arrange
            var cacheKeysProvider = new DefaultCacheKeysProvider();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(TestUrl),
                Method = HttpMethod.Get,
            };

            // Act
            var cacheKey = cacheKeysProvider.GetKey(request);

            // Assert
            cacheKey.Should().Be("MET_GET;URI_http://unittest/;");
        }
    }
}