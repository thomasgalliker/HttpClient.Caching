namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Specifies how items are prioritized for preservation during a memory pressure triggered cleanup.
    /// </summary>
    public enum CacheItemPriority
    {
        Low,
        Normal,
        High,
        NeverRemove,
    }
}