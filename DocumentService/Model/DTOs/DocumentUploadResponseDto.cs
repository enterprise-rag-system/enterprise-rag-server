namespace DocumentService.Model.DTOs;

public class DocumentUploadResponseDto
{
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = default!;

}