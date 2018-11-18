// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.MemoryCacheOptions
// Assembly: Microsoft.Extensions.Caching.Memory, Version=2.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: 78529ED0-C4AD-4926-BA4D-60032404EE9B
// Assembly location: C:\Users\thomas\AppData\Local\Temp\Rar$DI01.488\Microsoft.Extensions.Caching.Memory.dll

using System;

using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.Caching.InMemory
{
    public class MemoryCacheOptions 
    {
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1.0);

        public ISystemClock Clock { get; set; }

    }
}
