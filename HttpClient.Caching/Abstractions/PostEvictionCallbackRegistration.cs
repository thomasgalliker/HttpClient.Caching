namespace Microsoft.Extensions.Caching.Abstractions
{
    public class PostEvictionCallbackRegistration
    {
        public PostEvictionDelegate EvictionCallback { get; set; }

        public object State { get; set; }
    }
}