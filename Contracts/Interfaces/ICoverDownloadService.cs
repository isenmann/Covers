using System;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ICoverDownloadService
    {
        Task<Tuple<byte[], byte[]>> DownloadCoverAsync(string albumName, string artist);
    }
}