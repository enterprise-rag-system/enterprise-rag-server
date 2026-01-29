using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectService.Infrastructure;
using ProjectService.Models.DTOs;
using ProjectService.Models.Entities;
using ProjectService.Services;
using Xunit;

namespace ProjectService.Test.Services;

public class ProjectServiceTests
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectService.Services.ProjectService> _logger;
    private readonly ProjectService.Services.ProjectService _service;
    

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _logger = A.Fake<ILogger<ProjectService.Services.ProjectService>>();
        _service = new ProjectService.Services.ProjectService(_db,  _logger);
        
    }
    
    [Fact]
    public async Task CreateProjectAsync_Should_create_project_and_member()
    {
        var userId = Guid.NewGuid();

        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "Desc"
        };

        var projectId = await _service.CreateProjectAsync(userId, request);

        var project = await _db.Projects.FirstAsync();
        var member = await _db.ProjectMembers.FirstAsync();

        project.Id.Should().Be(projectId);
        project.OwnerUserId.Should().Be(userId);
        project.Name.Should().Be("Test Project");

        member.ProjectId.Should().Be(projectId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be("OWNER");
    }

    [Fact]
    public async Task CreateProjectAsync_Should_throw_when_project_exists()
    {
        var userId = Guid.NewGuid();

        _db.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate",
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var request = new CreateProjectRequest
        {
            Name = "Duplicate"
        };

        await FluentActions
            .Invoking(() => _service.CreateProjectAsync(userId, request))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Project already exists");
    }

    [Fact]
    public async Task GetProjectsAsync_Should_return_only_user_projects()
    {
        var userId = Guid.NewGuid();

        _db.Projects.AddRange(
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Mine",
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Not Mine",
                OwnerUserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });

        await _db.SaveChangesAsync();

        var result = await _service.GetProjectsAsync(userId);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Mine");
    }

    [Fact]
    public async Task GetProjectByIdAsync_Should_return_project()
    {
        var userId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "My Project",
            Description = "Desc",
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var result = await _service.GetProjectByIdAsync(project.Id, userId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("My Project");
    }

    [Fact]
    public async Task GetProjectByIdAsync_Should_throw_when_not_found()
    {
        await FluentActions
            .Invoking(() => _service.GetProjectByIdAsync(
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Project not found");
    }
    
    [Fact]
    public async Task UpdateProjectAsync_Should_update_project()
    {
        var userId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Old",
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var request = new UpdateProjectRequest
        {
            Name = "New",
            Description = "Updated"
        };

        var result = await _service.UpdateProjectAsync(project.Id, userId, request);

        result.Should().BeTrue();
        project.Name.Should().Be("New");
    }


    [Fact]
    public async Task DeleteProjectAsync_Should_delete_project()
    {
        var userId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Delete Me",
            OwnerUserId = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var result = await _service.DeleteProjectAsync(project.Id, userId);

        result.Should().BeTrue();
        (await _db.Projects.AnyAsync()).Should().BeFalse();
    }


}
