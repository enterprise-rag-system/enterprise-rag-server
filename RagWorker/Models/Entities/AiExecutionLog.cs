using System;
using RagWorker.Models.Common;

namespace RagWorker.Models.Entities;

public class AiExecutionLog : BaseEntity
{
    public Guid ProjectId { get; set; }

    public Guid CorrelationId { get; set; }

    public string Question { get; set; } = default!;

    public string Answer { get; set; } = default!;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public string SourceChunkIdsJson { get; set; } = default!;
}