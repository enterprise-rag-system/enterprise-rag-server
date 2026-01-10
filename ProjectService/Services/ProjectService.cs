using Microsoft.EntityFrameworkCore;
using ProjectService.Infrastructure;
using ProjectService.Models.DTOs;
using ProjectService.Models.Entities;
using ProjectService.Services.Interfaces;

namespace ProjectService.Services;

public class ProjectService:IProjectService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(AppDbContext dbContext, ILogger<ProjectService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Guid> CreateProjectAsync(Guid userId, CreateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new project");
            var projectExist = _dbContext.Projects.Any(p=>p.Name == request.Name &&  p.OwnerUserId == userId);
            if (projectExist)
            {
                _logger.LogInformation("Project already exists");
                throw new InvalidOperationException("Project already exists");
            }
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Projects.Add(project);
            
            _dbContext.ProjectMembers.Add(new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = userId,
                Role = "OWNER",
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();

            return project.Id;

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Project creation failed");
            throw;
        }
    }

    public async Task<List<ProjectListItemResponse>> GetProjectsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting all projects");
            var projects = await _dbContext.Projects.Where(p => p.OwnerUserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(p=>new ProjectListItemResponse
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    CreatedAt = p.CreatedAt,
                }).ToListAsync();
            _logger.LogInformation("Fetched Project List for user {0}, Project Count : {1}", userId, projects.Count);
            _logger.LogInformation("Returning all projects");
            return projects;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Project fetching failed");
            throw;
        }
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(Guid projectId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting project by id");
            var project = await _dbContext.Projects.Where(p => p.Id == projectId && p.OwnerUserId == userId)
                .Select(p=>new ProjectResponse
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogInformation("Project not found");
                throw new KeyNotFoundException("Project not found");
            }
            _logger.LogInformation("Returning project by id");
            return project;
            
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Project fetching failed");
            throw;
        }
    }

    public async Task<bool> UpdateProjectAsync(Guid projectId, Guid userId, UpdateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Updating project by id");
            var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerUserId == userId);
            if (project == null)
            {
                _logger.LogInformation("Project not found");
                throw new KeyNotFoundException("Project not found");
            }
            
            project.Name = request.Name;
            project.Description = request.Description;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Project updated");
            return true;

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Project Update failed");
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Deleting project by id");
            var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerUserId == userId);
            if (project == null)
            {
                _logger.LogInformation("Project not found");
                throw new KeyNotFoundException("Project not found");
            }
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Project deleted");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Project Delete failed");
            throw;
        }
    }
}