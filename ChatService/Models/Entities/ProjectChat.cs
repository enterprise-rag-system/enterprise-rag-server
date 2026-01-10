namespace ChatService.Models.Entities;

public class ProjectChat
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; }
}