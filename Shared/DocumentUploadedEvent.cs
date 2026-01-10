namespace Shared;

public record DocumentUploadedEvent(
    Guid ProjectId,
    Guid DocumentId,
    string FilePath
);