using System;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ICoverDownloadService
    {
        Task<Tuple<byte[], byte[]>> DownloadCover(string albumName, string artist);
    }
}