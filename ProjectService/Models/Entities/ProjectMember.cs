namespace ProjectService.Models.Entities;

public class ProjectMember
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = "OWNER";

    public DateTime CreatedAt { get; set; }
}