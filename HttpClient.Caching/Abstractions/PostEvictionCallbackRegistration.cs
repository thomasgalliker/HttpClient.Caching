// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.PostEvictionCallbackRegistration
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

namespace Microsoft.Extensions.Caching.Memory
{
    public class PostEvictionCallbackRegistration
    {
        public PostEvictionDelegate EvictionCallback { get; set; }

        public object State { get; set; }
    }
}
