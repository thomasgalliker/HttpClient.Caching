using System;

namespace WpfSample.Services
{
    public interface IApiServiceConfiguration
    {
        bool CachingEnabled { get; }

        TimeSpan Timeout { get; set; }

        string BaseUrl { get; set; }
    }
}