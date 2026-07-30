using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;

namespace IELTSLms.Application.Vocabulary.GetVocabularyWord;

public sealed record GetVocabularyWordQuery(Guid TenantId, Guid WordId) : IQuery<VocabularyWordResponse>;
