using System.Text;

namespace RagWorker.Helpers;

public static class PromptBuilder
{
    public static string Build(
        string question,
        IReadOnlyList<string> contextChunks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("""
                      You are an AI assistant.
                      Answer the question using ONLY the provided context.
                      If the answer is not present in the context, say:
                      "I don't know based on the provided knowledge."
                      """);

        sb.AppendLine();
        sb.AppendLine("Context:");
        sb.AppendLine("---------");

        foreach (var chunk in contextChunks)
        {
            sb.AppendLine(chunk);
            sb.AppendLine();
        }

        sb.AppendLine("---------");
        sb.AppendLine($"Question: {question}");
        sb.AppendLine("Answer:");

        return sb.ToString();
    }
}