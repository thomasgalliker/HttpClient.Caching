// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Internal.SystemClock
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.Internal
{
    /// <summary>Provides access to the normal system clock.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SystemClock : ISystemClock
    {
        /// <summary>Retrieves the current system time in UTC.</summary>
        public DateTimeOffset UtcNow
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}
