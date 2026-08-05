namespace HRMS.Application.Common.Interfaces
{
    /// <summary>
    /// Distributed caching contract supporting typed retrieval, storage, and key invalidation.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieves a cached item by key asynchronously.
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a cached item with absolute and/or sliding expiration asynchronously.
        /// </summary>
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates/removes a single cache key asynchronously.
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
