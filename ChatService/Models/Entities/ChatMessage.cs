namespace ChatService.Models.Entities;

public class ChatMessage
{
    
        public Guid Id { get; set; }

        // Scope
        public Guid ProjectId { get; set; }

        // Who sent it
        public MessageSender Sender { get; set; }

        // Message content
        public string Content { get; set; } = null!;

        // Correlation (ties USER message ↔ AI response)
        public string CorrelationId { get; set; } = null!;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete for compliance
        public bool IsDeleted { get; set; }

        // ---- AI-specific metadata (nullable) ----
        public int? TokenUsage { get; set; }

        // Store chunk IDs used for answer (traceability)
        public string? SourceChunkIdsJson { get; set; }
    

}
