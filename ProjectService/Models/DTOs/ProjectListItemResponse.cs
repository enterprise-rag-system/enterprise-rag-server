namespace ProjectService.Models.DTOs;

public class ProjectListItemResponse
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

}