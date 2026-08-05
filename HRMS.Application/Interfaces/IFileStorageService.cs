using Microsoft.AspNetCore.Http;

namespace HRMS.Application.Interfaces
{
    /// <summary>
    /// Enterprise contract for file storage abstraction (Local Disk, Azure Blob, S3, GCS).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads and saves a file asynchronously to a target storage sub-folder and returns the relative file URL.
        /// </summary>
        /// <param name="file">The uploaded file stream wrapper.</param>
        /// <param name="subFolder">Target sub-folder (e.g. photos, resumes, documents).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Relative URL path to access the file.</returns>
        Task<string> SaveFileAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an existing file asynchronously by relative URL path.
        /// </summary>
        /// <param name="relativeUrl">Relative path/URL of the target file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deletion succeeded; otherwise false.</returns>
        Task<bool> DeleteFileAsync(string relativeUrl, CancellationToken cancellationToken = default);
    }
}
