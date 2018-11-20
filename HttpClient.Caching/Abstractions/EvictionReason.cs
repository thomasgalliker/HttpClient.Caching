namespace Microsoft.Extensions.Caching.Abstractions
{
    public enum EvictionReason
    {
        None = 0,
        Removed = 1,
        Replaced = 2,
        Expired = 3,
        TokenExpired = 4,
        Capacity = 5,
    }
}