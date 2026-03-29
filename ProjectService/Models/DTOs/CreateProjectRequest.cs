using System.ComponentModel.DataAnnotations;

namespace ProjectService.Models.DTOs;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters")] 
    public string Name { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "Description too long")]
    public string? Description { get; set; }
}