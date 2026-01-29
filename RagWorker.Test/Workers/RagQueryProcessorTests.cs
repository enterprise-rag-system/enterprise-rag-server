using FakeItEasy;
using FluentAssertions;
using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Factory;
using RagWorker.Interfaces.Rag;
using RagWorker.Interfaces.Vector;
using RagWorker.Models.AI;
using RagWorker.Models.Entities;
using RagWorker.Models.Rag;
using RagWorker.Workers;
using Xunit;

namespace RagWorker.Test.Workers;

public class RagQueryProcessorTests
{
    private readonly IVectorStore _vectorStore;
    private readonly IChatCompletionProviderFactory _chatFactory;
    private readonly IEmbeddingProviderFactory _embeddingFactory;
    private readonly IChatCompletionProvider _chatProvider;
    private readonly IEmbeddingProvider _embeddingProvider;

    private readonly RagQueryProcessor _sut;

    public RagQueryProcessorTests()
    {
        _vectorStore = A.Fake<IVectorStore>();
        _chatFactory = A.Fake<IChatCompletionProviderFactory>();
        _embeddingFactory = A.Fake<IEmbeddingProviderFactory>();
        _chatProvider = A.Fake<IChatCompletionProvider>();
        _embeddingProvider = A.Fake<IEmbeddingProvider>();

        A.CallTo(() => _chatFactory.Create())
            .Returns(_chatProvider);

        A.CallTo(() => _embeddingFactory.Create())
            .Returns(_embeddingProvider);

        _sut = new RagQueryProcessor(
            _vectorStore,
            _chatFactory,
            _embeddingFactory);
    }


    [Fact]
    public async Task ProcessAsync_Should_throw_when_query_is_null()
    {
        await FluentActions
            .Invoking(() => _sut.ProcessAsync(null!, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessAsync_Should_throw_when_projectId_is_empty()
    {
        var query = new RagQuery
        {
            ProjectId = Guid.Empty,
            Question = "What is RAG?"
        };

        await FluentActions
            .Invoking(() => _sut.ProcessAsync(query, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*ProjectId*");
    }

    [Fact]
    public async Task ProcessAsync_Should_throw_when_question_is_empty()
    {
        var query = new RagQuery
        {
            ProjectId = Guid.NewGuid(),
            Question = " "
        };

        await FluentActions
            .Invoking(() => _sut.ProcessAsync(query, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Question*");
    }

    [Fact]
    public async Task ProcessAsync_Should_return_rag_result_and_call_dependencies()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var question = "Explain RAG";

        var query = new RagQuery
        {
            ProjectId = projectId,
            Question = question
        };

        IReadOnlyList<float> embedding = new List<float> { 0.1f, 0.2f, 0.3f };

        var chunks = new List<DocumentChunk>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ChunkText = "RAG combines retrieval and generation"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ChunkText = "Vector databases enable similarity search"
            }
        };

        var completionResult = new ChatCompletionResult
        {
            Answer = "RAG uses retrieved context to answer questions.",
            PromptTokens = 40,
            CompletionTokens = 30
        };

        A.CallTo(() => _embeddingProvider.GenerateEmbeddingAsync(
                question,
                A<CancellationToken>._))
            .Returns(embedding);

        A.CallTo(() => _vectorStore.SearchAsync(
                projectId,
                A<IReadOnlyList<float>>._,
                5,
                A<CancellationToken>._))
            .Returns(chunks);

        A.CallTo(() => _chatProvider.CompleteAsync(
                A<ChatCompletionRequest>._,
                A<CancellationToken>._))
            .Returns(completionResult);

        // Act
        var result = await _sut.ProcessAsync(query, CancellationToken.None);

        // Assert (result)
        result.Should().NotBeNull();
        result.Answer.Should().Be(completionResult.Answer);
        result.SourceChunkIds.Should().BeEquivalentTo(chunks.Select(c => c.Id));
        result.TotalTokens.Should().Be(70);

        // Assert (interactions)
        A.CallTo(() => _embeddingProvider.GenerateEmbeddingAsync(
                question,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _vectorStore.SearchAsync(
                projectId,
                A<IReadOnlyList<float>>._,
                5,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _chatProvider.CompleteAsync(
                A<ChatCompletionRequest>.That.Matches(req =>
                    req.Prompt.Contains(question)),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

}