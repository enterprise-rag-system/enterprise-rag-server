namespace ProjectService.Models.DTOs;

public class ProjectResponse
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}