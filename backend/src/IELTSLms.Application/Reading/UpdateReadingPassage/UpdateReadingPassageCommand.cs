using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Reading.UpdateReadingPassage;

public sealed record UpdateReadingPassageCommand(
    Guid TenantId, Guid PassageId, string Title, string PassageText, ReadingTrack Track, ReadingDifficulty Difficulty,
    Guid? CategoryId, int DurationSeconds) : ICommand<ReadingPassageResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.manage"];
}
