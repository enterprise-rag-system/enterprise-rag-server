using Shared;

namespace DocumentService.Services.Interfaces;

public interface IRabbitMqPublisher
{
    Task PublishMessage(DocumentUploadedEvent evt);
}