namespace DocumentService.Model.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }

    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }

    public Status Status { get; set; }

    public DateTime UploadedAt { get; set; }
    public Guid UploadedBy { get; set; }

    // AI-ready fields (future)
    public bool IsAiReady => Status == Status.AiReady;

}