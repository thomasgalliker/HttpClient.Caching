using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.Internals
{
    internal sealed class CacheDataJsonConverter : JsonConverter<CacheData>
    {
        public override CacheData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var data = root.TryGetProperty("data", out var dataElement)
                ? dataElement.Deserialize<byte[]>(options) ?? Array.Empty<byte>()
                : Array.Empty<byte>();

            var reasonPhrase = root.TryGetProperty("reasonPhrase", out var reasonPhraseElement)
                ? reasonPhraseElement.GetString()
                : null;

            var statusCode = root.TryGetProperty("statusCode", out var statusCodeElement)
                ? statusCodeElement.GetInt32()
                : (int)HttpStatusCode.OK;

            var version = root.TryGetProperty("version", out var versionElement)
                ? versionElement.GetString()
                : null;

            var headers = root.TryGetProperty("headers", out var headersElement)
                ? headersElement.Deserialize<Dictionary<string, string[]>>(options)
                : null;

            var contentHeaders = root.TryGetProperty("contentHeaders", out var contentHeadersElement)
                ? contentHeadersElement.Deserialize<Dictionary<string, string[]>>(options)
                : null;

            var httpResponseMessage = new HttpResponseMessage { ReasonPhrase = reasonPhrase, StatusCode = (HttpStatusCode)statusCode, };

            if (ParseVersion(version) is Version v)
            {
                httpResponseMessage.Version = v;
            }

            return new CacheData(
                data,
                httpResponseMessage,
                ConvertHeaders(headers),
                ConvertHeaders(contentHeaders));
        }

        public override void Write(Utf8JsonWriter writer, CacheData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBase64String("data", value.Data);
            writer.WriteString("reasonPhrase", value.CachableResponse.ReasonPhrase);
            writer.WriteNumber("statusCode", (int)value.CachableResponse.StatusCode);
            writer.WriteString("version", value.CachableResponse.Version?.ToString());

            writer.WritePropertyName("headers");
            JsonSerializer.Serialize(writer, ConvertHeaders(value.Headers), options);

            writer.WritePropertyName("contentHeaders");
            JsonSerializer.Serialize(writer, ConvertHeaders(value.ContentHeaders), options);

            writer.WriteEndObject();
        }


        private static Dictionary<string, string[]> ConvertHeaders(Dictionary<string, IEnumerable<string>>? headers)
        {
            return headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()) ?? new Dictionary<string, string[]>();
        }

        private static Dictionary<string, IEnumerable<string>> ConvertHeaders(Dictionary<string, string[]>? headers)
        {
            return headers?.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value) ?? new Dictionary<string, IEnumerable<string>>();
        }

        private static Version? ParseVersion(string? version)
        {
            return string.IsNullOrWhiteSpace(version)
                ? null
                : Version.Parse(version);
        }
    }
}