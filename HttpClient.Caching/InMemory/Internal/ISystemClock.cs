// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Internal.ISystemClock
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

using System;

namespace Microsoft.Extensions.Internal
{
    /// <summary>Abstracts the system clock to facilitate testing.</summary>
    public interface ISystemClock
    {
        /// <summary>Retrieves the current system time in UTC.</summary>
        DateTimeOffset UtcNow { get; }
    }
}
