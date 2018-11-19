using System;
using System.Diagnostics;

namespace HttpClient.Caching.Tests.Testdata
{
    [DebuggerDisplay("{this.Id}")]
    public class TestPayload
    {
        private const int BufferSize = 10 * 1024; // 1MB buffer size

        private static readonly Random Random = new Random();
        private readonly byte[] buffer = new byte[BufferSize];

        public TestPayload(int id)
        {
            this.Id = id;
            this.buffer[Random.Next(BufferSize)] = 1;
        }

        public int Id { get; set; }
    }
}