﻿using Hotel.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hotel.DataAccess.Repositories.IRepositories
{
    public interface IRoomDetailRepository
    {
        Task<IEnumerable<RoomDetail>> BrowserAsync();
        Task<RoomDetail?> FindAsync(Expression<Func<RoomDetail, bool>> predicate);
        Task<RoomDetail?> FindAsync(int id);
        Task CreateAsync(RoomDetail entity);
        Task SaveChangeAsync();
        Task  UpdateAsync(RoomDetail entity);
        Task DeleteAsync( RoomDetail entity);
        Task<IEnumerable<int>> GetAllId();
    }
}
