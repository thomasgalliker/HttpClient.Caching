using System;
using System.Threading.Tasks;

namespace WpfSample.Services
{
    public interface ILocationService : IDisposable
    {
        Task<string> GetCity();
    }
}