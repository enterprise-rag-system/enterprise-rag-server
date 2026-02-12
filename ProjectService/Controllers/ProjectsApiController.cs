using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Helpers;
using ProjectService.Models.DTOs;
using ProjectService.Services.Interfaces;

namespace ProjectService.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/projects")]
[ApiVersion("1.0")]

public class ProjectsApiController:ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsApiController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest request)
    {
        var userId = User.GetUserId();

        var projectId = await _projectService
            .CreateProjectAsync(userId, request);

        return CreatedAtAction(
            nameof(GetProjectById),
            new { projectId },
            new { projectId });
    }
    
    // ----------------------------
    // Get All Projects (Dashboard)
    // ----------------------------
    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var userId = User.GetUserId();

        var projects = await _projectService
            .GetProjectsAsync(userId);

        return Ok(projects);
    }
    
    // ----------------------------
    // Get Project By Id
    // ----------------------------
    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetProjectById(Guid projectId)
    {
        var userId = User.GetUserId();

        var project = await _projectService
            .GetProjectByIdAsync(projectId, userId);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    // ----------------------------
    // Update Project
    // ----------------------------
    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> UpdateProject(
        Guid projectId,
        [FromBody] UpdateProjectRequest request)
    {
        var userId = User.GetUserId();

        var updated = await _projectService
            .UpdateProjectAsync(projectId, userId, request);

        if (!updated)
            return NotFound();

        return Ok("Project updated");
    }

    // ----------------------------
    // Delete Project
    // ----------------------------
    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        var userId = User.GetUserId();

        var deleted = await _projectService
            .DeleteProjectAsync(projectId, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}