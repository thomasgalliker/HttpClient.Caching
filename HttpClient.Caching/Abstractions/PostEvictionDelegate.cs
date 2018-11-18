// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Caching.Memory.PostEvictionDelegate
// Assembly: Microsoft.Extensions.Caching.Abstractions, Version=1.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E327E23F-23AA-413B-8382-1A0C0F261081
// Assembly location: Microsoft.Extensions.Caching.Abstractions.1.1.2\lib\netstandard1.0\Microsoft.Extensions.Caching.Abstractions.dll

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Signature of the callback which gets called when a cache entry expires.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="reason">The <see cref="T:Microsoft.Extensions.Caching.Memory.EvictionReason" />.</param>
    /// <param name="state">The information that was passed when registering the callback.</param>
    public delegate void PostEvictionDelegate(object key, object value, EvictionReason reason, object state);
}
