using ChatService.Infrastructure;
using ChatService.Interfaces;
using ChatService.Models.Entities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Shared;

namespace ChatService.Test.Services;

public class ChatServiceTests
{
    private readonly IChatRepository _repo;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<ChatService.Service.ChatService> _logger;
    private readonly ChatService.Service.ChatService _service;

    public ChatServiceTests()
    {
        _repo = A.Fake<IChatRepository>();
        _publisher = A.Fake<IEventPublisher>();
        _logger = A.Fake<ILogger<ChatService.Service.ChatService>>();

        _service = new ChatService.Service.ChatService(
            _repo,
            _publisher,
            _logger
        );
    }
    
    [Fact]
    public async Task AddUserMessageAsync_Should_save_message_and_publish_event()
    {
        var projectId = Guid.NewGuid();
        var content = "What is RAG?";

        A.CallTo(() => _repo.AddAsync(A<ChatMessage>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var result = await _service.AddUserMessageAsync(
            projectId,
            content,
            CancellationToken.None);

        result.Should().NotBeNull();

        A.CallTo(() => _repo.AddAsync(
                A<ChatMessage>.That.Matches(m =>
                    m.ProjectId == projectId &&
                    m.Content == content &&
                    m.Sender == MessageSender.USER),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _publisher.PublishAsync(
                A<ChatMessageCreatedEvent>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

    }
    
    [Fact]
    public async Task AddUserMessageAsync_Should_throw_when_message_empty()
    {
        await FluentActions
            .Invoking(() => _service.AddUserMessageAsync(
                Guid.NewGuid(),
                "",
                CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetChatHistoryAsync_Should_return_messages()
    {
        var projectId = Guid.NewGuid();

        A.CallTo(() => _repo.GetByProjectAsync(
                projectId,
                10,
                A<CancellationToken>._))
            .Returns(new List<ChatMessage>
            {
                new ChatMessage
                {
                    ProjectId = projectId,
                    Content = "Hello",
                    Sender = MessageSender.USER
                }
            });

        var result = await _service.GetChatHistoryAsync(
            projectId,
            10,
            CancellationToken.None);

        result.Messages.Should().HaveCount(1);
        result.Messages.First().Message.Should().Be("Hello");
    }


}