using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using RaceTimings.ProtoActorServer.Cache;
using RaceTimings.ProtoActorServer.Entities;

namespace RaceTimings.ProtoActorServer.Repositories;

public interface IEntityRepository
{
    Task<Maybe<TEntity>> GetAsync<TEntity, TKey>(TKey id, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task<IEnumerable<TEntity>> GetAllAsync<TEntity, TKey>(string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task<bool> CheckExistsAsync<TEntity, TKey>(TKey id, Func<TKey, string> cacheKeyFactory)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task AddAsync<TEntity, TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task UpdateAsync<TEntity, TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task AddOrUpdateAsync<TEntity, TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task DeleteAsync<TEntity, TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;

    Task<IEnumerable<TEntity>> SearchAsync<TEntity, TKey>(Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryCustomization = null)
        where TEntity : class, IEntityWithId<TKey> where TKey : notnull;
}

public class EntityRepository(IHybridCache cache, ApplicationDbContext dbContext): IEntityRepository 
    
{
    //Get
    public virtual async Task<Maybe<TEntity>> GetAsync<TEntity,TKey>(TKey id, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        return await cache.GetOrCreateAsync(cacheKeyFactory(id), GetFromDb, cacheKeyCollection);
        
        async Task<Maybe<TEntity>> GetFromDb() => (await dbContext.Set<TEntity>().FindAsync(id)).AsMaybe();
    }
    
    //GetAll
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity,TKey>(string cacheKeyCollection)where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
         var keys = await cache.GetAllKeysAsync(cacheKeyCollection);
         var results = new List<TEntity>();
         foreach (var key in keys)
         {
             var item = await cache.GetAsync<TEntity>(key);
             if(item.HasValue)
                 results.Add(item.Value);
         }
         return results.AsEnumerable();
    }
    
    //Check Id Exists
    public virtual async Task<bool> CheckExistsAsync<TEntity,TKey>(TKey id, Func<TKey, string> cacheKeyFactory) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        return await cache.KeyExistsAsync(cacheKeyFactory(id));
    }
    
    //Add
    public virtual async Task AddAsync<TEntity,TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        await dbContext.Set<TEntity>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
        await cache.SetAsync(cacheKeyFactory(entity.Id), entity, cacheKeyCollection);
    }
    
    //Update
    public virtual async Task UpdateAsync<TEntity,TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        dbContext.Set<TEntity>().Add(entity);
        await dbContext.SaveChangesAsync();
        await cache.SetAsync(cacheKeyFactory(entity.Id), entity, cacheKeyCollection);
    }
    
    //AddOrUpdate
    public virtual async Task AddOrUpdateAsync<TEntity,TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        var ct = CancellationToken.None;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // Check if the entity exists in the database
            // ReSharper disable once MethodSupportsCancellation
            var existingEntity = await dbContext.Set<TEntity>().FindAsync(entity.Id,ct);

            if (existingEntity == null)
            {
                // Add the new entity
                await dbContext.Set<TEntity>().AddAsync(entity, ct);
            }
            else
            {
                // Update the existing entity
                dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            }

            // Save changes in DB
            await dbContext.SaveChangesAsync(ct);

            // Commit the transaction
            await transaction.CommitAsync(ct);

            // Perform cache upsert
            await cache.SetAsync(cacheKeyFactory(entity.Id), entity, cacheKeyCollection);
        }
        catch (Exception)
        {
            // Rollback the transaction in case of failure
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    //Delete
    public async Task DeleteAsync<TEntity,TKey>(TEntity entity, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        dbContext.Set<TEntity>().Remove(entity);
        await dbContext.SaveChangesAsync();
        await cache.RemoveAsync(cacheKeyFactory(entity.Id), cacheKeyCollection);
    }
    
    //Delete by Id
    public async Task DeleteAsync<TEntity,TKey>(TKey id, Func<TKey, string> cacheKeyFactory, string cacheKeyCollection) where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        var entity = await dbContext.Set<TEntity>().FindAsync(id);
        if (entity is not null)
        {
            dbContext.Set<TEntity>().Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        await cache.RemoveAsync(cacheKeyFactory(id),cacheKeyCollection);
    }
    
    //Search
    public async Task<IEnumerable<TEntity>> SearchAsync<TEntity,TKey>(Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryCustomization = null)  where TEntity: class, IEntityWithId<TKey> where TKey : notnull
    {
        IQueryable<TEntity> query = dbContext.Set<TEntity>();
        query = query.Where(filter);

        if (queryCustomization != null)
        {
            query = queryCustomization(query);
        }

        return await query.ToListAsync();
    }
}
