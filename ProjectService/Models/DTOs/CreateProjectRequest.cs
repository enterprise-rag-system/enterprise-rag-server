namespace ProjectService.Models.DTOs;

public class CreateProjectRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}