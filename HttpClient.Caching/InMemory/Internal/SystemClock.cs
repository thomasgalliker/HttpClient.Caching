using System;
using System.ComponentModel;

namespace Microsoft.Extensions.Caching.InMemory.Internal
{
    /// <summary>Provides access to the normal system clock.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SystemClock : ISystemClock
    {
        /// <summary>Retrieves the current system time in UTC.</summary>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}