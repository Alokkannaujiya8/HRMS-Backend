using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services
{
    /// <summary>
    /// Local physical disk implementation of <see cref="IFileStorageService"/>.
    /// Manages file uploads within the application's wwwroot directory.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Uploaded file is null or empty.", nameof(file));
            }

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var targetFolder = Path.Combine(webRootPath, "uploads", subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var safeFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await file.CopyToAsync(fileStream, cancellationToken);
            }

            _logger.LogInformation("Successfully saved file to {FilePath}", filePath);
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        /// <inheritdoc />
        public Task<bool> DeleteFileAsync(string relativeUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return Task.FromResult(false);
            }

            try
            {
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var normalizedRelative = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRootPath, normalizedRelative);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted file at {FilePath}", fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file at relative path {RelativeUrl}", relativeUrl);
                return Task.FromResult(false);
            }
        }
    }
}
