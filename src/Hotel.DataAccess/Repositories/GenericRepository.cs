﻿using Hotel.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hotel.DataAccess.Repositories;

internal class GenericRepository<TEntity>: IGenericRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _collection;
    // pass dbcontext here
    // generate dataset
    public GenericRepository(
        AppDbContext context)
    {
        _context= context;
        _collection = _context.Set<TEntity>();
    }

    public Task<IEnumerable<TEntity>> BrowserAsync()
    {
        throw new NotImplementedException();
    }

    public async Task CreateAsync(TEntity entity)
    {
        await _collection.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(TEntity entity)
    {
        _collection.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entity = _collection.SingleOrDefaultAsync(predicate);
        return entity;
    }

    public async Task<TEntity?> FindAsync(Guid id)
    {
        var entity = await _collection.FindAsync(id);
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _collection.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}