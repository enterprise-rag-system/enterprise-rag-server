namespace DocumentService.Model.DTOs;

public class DocumentListDto
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
}