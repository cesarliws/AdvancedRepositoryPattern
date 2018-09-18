using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AdvancedRepositoryPattern.Domain.Models;

namespace AdvancedRepositoryPattern.EntityFrameworkCore.Repositories
{
    public enum LoadStrategy
    {
        AsNoTracking,
        AutoDetectChanges
    }

    public abstract class RepositoryBase<TEntity, TContext>
        where TEntity : Entity
        where TContext : DbContext
    {
        protected DbSet<TEntity> DbSet;
        protected TContext DbContext;
        protected RepositoryBase(TContext dbContext)
        {
            DbContext = dbContext;
            DbSet = DbContext.Set<TEntity>();
        }

        public virtual async Task<Tuple<IEnumerable<TEntity>, int>> GetAll
        (
            int skip,
            int take,
            LoadStrategy loadStrategy = LoadStrategy.AsNoTracking
        )
        {
            var databaseCount = await DbSet
                .CountAsync()
                .ConfigureAwait(false);
                
            if (loadStrategy == LoadStrategy.AsNoTracking)
                return new Tuple<IEnumerable<TEntity>, int>
                (
                    await DbSet.AsNoTracking()
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        .ConfigureAwait(false),
                    databaseCount
                );

            return new Tuple<IEnumerable<TEntity>, int>
            (
                await DbSet
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
                    .ConfigureAwait(false),
                databaseCount
            );
        }

        public virtual async Task<Tuple<IEnumerable<TEntity>, int>> GetAll
        (
            int skip,
            int take,
            Expression<Func<TEntity, bool>> where,
            LoadStrategy loadStrategy = LoadStrategy.AsNoTracking
        )
        {
            var databaseCount = await DbSet.CountAsync().ConfigureAwait(false);
            if (loadStrategy == LoadStrategy.AsNoTracking)
                return new Tuple<IEnumerable<TEntity>, int>
                (
                    await DbSet
                        .AsNoTracking()
                        .Where(where)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        .ConfigureAwait(false),
                    databaseCount
                );

            return new Tuple<IEnumerable<TEntity>, int>
            (
                await DbSet
                    .Where(where)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
                    .ConfigureAwait(false),
                databaseCount
            );
        }

        public virtual async Task<Tuple<IEnumerable<TEntity>, int>> GetAll
        (
            int skip,
            int take,
            Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, object>> orderBy,
            LoadStrategy loadStrategy = LoadStrategy.AsNoTracking
        )
        {
            var databaseCount = await DbSet
                .CountAsync()
                .ConfigureAwait(false);

            if (loadStrategy == LoadStrategy.AsNoTracking)
                return new Tuple<IEnumerable<TEntity>, int>
                (
                    await DbSet
                        .AsNoTracking()
                        .OrderBy(orderBy)
                        .Where(where)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        .ConfigureAwait(false),
                    databaseCount
                );

            return new Tuple<IEnumerable<TEntity>, int>
            (
                await DbSet
                    .OrderBy(orderBy)
                    .Where(where)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync()
                    .ConfigureAwait(false),
                databaseCount
            );
        }

        public virtual async Task<TEntity> GetByIdAsync(int entityId, LoadStrategy loadStrategy = LoadStrategy.AsNoTracking)
        {
            return loadStrategy == LoadStrategy.AsNoTracking
                ? await DbSet
                    .AsNoTracking()
                    .SingleOrDefaultAsync(entity => entity.Id == entityId)
                    .ConfigureAwait(false)
                : await DbSet.FindAsync(entityId).ConfigureAwait(false);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet
                .AddAsync(entity)
                .ConfigureAwait(false);
        }

        public virtual async Task AddCollectionAsync(IEnumerable<TEntity> entities)
        {
            await DbSet
                .AddRangeAsync(entities)
                .ConfigureAwait(false);
        }

        public virtual IEnumerable<TEntity> AddCollectionWithProxy(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                DbSet.Add(entity);
                yield return entity;
            }
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateCollectionAsync(IEnumerable<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual IEnumerable<TEntity> UpdateCollectionWithProxy(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                DbSet.Update(entity);
                yield return entity;
            }
        }

        public virtual Task RemoveByAsync(Func<TEntity, bool> where)
        {
            DbSet.RemoveRange(DbSet.ToList().Where(where));
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(TEntity entity)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task SaveChangesAsync()
        {
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
