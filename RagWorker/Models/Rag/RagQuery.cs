using System;

namespace RagWorker.Models.Rag;

public class RagQuery
{
    public Guid ProjectId { get; set; }

    public string Question { get; set; } = default!;
}