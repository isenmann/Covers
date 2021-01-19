﻿using Covers.Persistency.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface IAlbumService
    {
        Task<Album> GetAsync(long id);
        Task<List<Album>> GetAsync();
        Task AddAsync(Album album);
        Task AddAsync(List<Album> album);
        Task UpdateAsync(Album album);
        Task DeleteAsync(Album album);
    }
}
