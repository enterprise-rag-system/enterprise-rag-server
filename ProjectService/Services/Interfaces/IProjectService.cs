using ProjectService.Models.DTOs;

namespace ProjectService.Services.Interfaces;

public interface IProjectService
{
    Task<Guid> CreateProjectAsync(Guid userId, CreateProjectRequest request);

    Task<List<ProjectListItemResponse>> GetProjectsAsync(Guid userId);

    Task<ProjectResponse?> GetProjectByIdAsync(Guid projectId, Guid userId);

    Task<bool> UpdateProjectAsync(Guid projectId, Guid userId, UpdateProjectRequest request);

    Task<bool> DeleteProjectAsync(Guid projectId, Guid userId);


}