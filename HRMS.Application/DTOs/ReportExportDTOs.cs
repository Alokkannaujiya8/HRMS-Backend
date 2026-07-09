namespace HRMS.Application.DTOs
{
    public class ReportFileResponse
    {
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
