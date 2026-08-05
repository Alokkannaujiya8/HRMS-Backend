using System.Linq.Expressions;

namespace HRMS.Application.Common.Interfaces
{
    /// <summary>
    /// Contract for generic repository operations supporting async CRUD and LINQ specifications.
    /// </summary>
    /// <typeparam name="TEntity">Entity type managed by the repository.</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Retrieves an entity by its unique primary key.
        /// </summary>
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities from the data store.
        /// </summary>
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities matching a specific predicate.
        /// </summary>
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a single entity matching a specific predicate.
        /// </summary>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity satisfies the given predicate.
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a range of new entities to the repository.
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity state in the repository.
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Removes an entity from the repository.
        /// </summary>
        void Remove(TEntity entity);

        /// <summary>
        /// Removes a range of entities from the repository.
        /// </summary>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Counts total entities matching a given optional predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}
