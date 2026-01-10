namespace ChatService.Models.DTOs;

public class ChatMessageResponse
{
    public string Id { get; set; } = null!;
    public string Sender { get; set; }
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }

}