using System;
using System.Collections.Generic;

namespace RagWorker.Models.Rag;

public class RagResult
{
    public string Answer { get; set; } = default!;

    public IReadOnlyList<Guid> SourceChunkIds { get; set; }
        = new List<Guid>();

    public int TotalTokens { get; set; }
}