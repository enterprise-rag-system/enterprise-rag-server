namespace ProjectService.Models.DTOs;

public class UpdateProjectRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}