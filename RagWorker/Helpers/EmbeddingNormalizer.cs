using System;
using System.Collections.Generic;
using System.Linq;
using Pgvector;

namespace RagWorker.Helpers;

public static class EmbeddingNormalizer
{
    public static float[] Normalize(IReadOnlyList<float> embedding)
    {
        if (embedding == null || embedding.Count == 0)
            throw new ArgumentException("Embedding is empty");

        var magnitude = Math.Sqrt(
            embedding.Sum(x => x * x));

        if (magnitude == 0)
            throw new InvalidOperationException(
                "Cannot normalize zero vector");

        return embedding
            .Select(x => (float)(x / magnitude))
            .ToArray();
    }
    
    public static Vector ToVector(IReadOnlyList<float> embedding)
    {
        if (embedding == null || embedding.Count == 0)
            throw new ArgumentException("Embedding is empty");

        return new Vector(embedding.ToArray());
    }
}