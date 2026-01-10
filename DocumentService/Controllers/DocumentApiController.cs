using DocumentService.Helpers;
using DocumentService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentService.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/document")]
public class DocumentApiController: ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentApiController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromQuery] Guid projectId,
        [FromForm(Name = "file")] IFormFile file,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        Console.WriteLine(projectId);
        
        
        var result = await _documentService.UploadAsync(
            projectId, file, userId, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var documents = await _documentService
            .GetByProjectAsync(projectId, cancellationToken);

        return Ok(documents);
    }

}