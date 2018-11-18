// Decompiled with JetBrains decompiler
// Type: Microsoft.Extensions.Primitives.IChangeToken
// Assembly: Microsoft.Extensions.Primitives, Version=1.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: E2E2AE84-9284-4D35-8DD3-6210BF615CED
// Assembly location: C:\src\FishApp\FishApp\packages\Microsoft.Extensions.Primitives.1.1.1\lib\netstandard1.0\Microsoft.Extensions.Primitives.dll

using System;

namespace Microsoft.Extensions.Primitives
{
    /// <summary>Propagates notifications that a change has occured.</summary>
    public interface IChangeToken
    {
        /// <summary>Gets a value that indicates if a change has occured.</summary>
        bool HasChanged { get; }

        /// <summary>
        /// Indicates if this token will pro-actively raise callbacks. Callbacks are still guaranteed to fire, eventually.
        /// </summary>
        bool ActiveChangeCallbacks { get; }

        /// <summary>
        /// Registers for a callback that will be invoked when the entry has changed.
        /// <see cref="P:Microsoft.Extensions.Primitives.IChangeToken.HasChanged" /> MUST be set before the callback is invoked.
        /// </summary>
        /// <param name="callback">The <see cref="T:System.Action`1" /> to invoke.</param>
        /// <param name="state">State to be passed into the callback.</param>
        /// <returns>An <see cref="T:System.IDisposable" /> that is used to unregister the callback.</returns>
        IDisposable RegisterChangeCallback(Action<object> callback, object state);
    }
}
