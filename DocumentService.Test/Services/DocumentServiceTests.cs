using DocumentService.Services.Interfaces;
using DocumentService.Infrastructure;
using DocumentService.Model.Entities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DocumentService.Test.Services;

public class DocumentServiceTests
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<DocumentService.Services.DocumentService> _logger;
    private readonly DocumentService.Services.DocumentService _service;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _fileStorage = A.Fake<IFileStorage>();
        _publisher = A.Fake<IRabbitMqPublisher>();
        _logger = A.Fake<ILogger<DocumentService.Services.DocumentService>>();

        _service = new DocumentService.Services.DocumentService(
            _logger,
            _db,
            _fileStorage,
            _publisher
        );
    }
    
    [Fact]
    public async Task UploadAsync_Should_save_document_and_publish_event()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns("test.pdf");

        A.CallTo(() => _fileStorage.SaveAsync(
                file,
                A<CancellationToken>._))
            .Returns("stored/path/test.pdf");

        // Act
        var result = await _service.UploadAsync(
            projectId,
            file,
            userId,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(Status.Uploaded.ToString());

        var doc = await _db.Documents.FirstAsync();
        doc.ProjectId.Should().Be(projectId);
        doc.UploadedBy.Should().Be(userId);
    }
    
    [Fact]
    public async Task UploadAsync_Should_throw_when_file_null()
    {
        await FluentActions
            .Invoking(() => _service.UploadAsync(
                Guid.NewGuid(),
                null!,
                Guid.NewGuid(),
                CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByProjectAsync_Should_return_documents()
    {
        var projectId = Guid.NewGuid();

        IFormFile file = A.Fake<IFormFile>();
        A.CallTo(() => file.FileName).Returns("doc1.pdf");
        A.CallTo(()=> file.ContentType).Returns("application/pdf");
        _db.Documents.Add(new Document
        {
            ProjectId = projectId,
            FileName = file.FileName,
            Status = Status.Uploaded,
            ContentType=file.ContentType,
        });

        await _db.SaveChangesAsync();

        var result = await _service.GetByProjectAsync(
            projectId,
            CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("doc1.pdf");
    }


}