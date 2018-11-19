namespace Microsoft.Extensions.Caching.Abstractions
{
    public enum EvictionReason
    {
        None,
        Removed,
        Replaced,
        Expired,
        TokenExpired,
        Capacity,
    }
}