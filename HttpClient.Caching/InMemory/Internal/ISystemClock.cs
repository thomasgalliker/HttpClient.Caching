using System;

namespace Microsoft.Extensions.Caching.InMemory.Internal
{
    /// <summary>Abstracts the system clock to facilitate testing.</summary>
    public interface ISystemClock
    {
        /// <summary>Retrieves the current system time in UTC.</summary>
        DateTimeOffset UtcNow { get; }
    }
}