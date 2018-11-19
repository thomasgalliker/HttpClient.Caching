namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Signature of the callback which gets called when a cache entry expires.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="reason">The <see cref="T:Microsoft.Extensions.Caching.Abstractions.EvictionReason" />.</param>
    /// <param name="state">The information that was passed when registering the callback.</param>
    public delegate void PostEvictionDelegate(object key, object value, EvictionReason reason, object state);
}