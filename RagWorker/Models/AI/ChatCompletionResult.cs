namespace RagWorker.Models.AI;

public class ChatCompletionResult
{
    public string Answer { get; set; } = default!;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }
}