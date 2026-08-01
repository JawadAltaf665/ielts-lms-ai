using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment;

internal static class MockExamErrors
{
    public static readonly Error ActiveExamExists = Error.Conflict(
        "BUSINESS_ACTIVE_EXAM_EXISTS", "An active mock exam session already exists for this user.");

    public static readonly Error NotFound = Error.NotFound(
        "NOT_FOUND_MOCK_EXAM", "Mock exam not found.");

    public static readonly Error ResultNotFound = Error.NotFound(
        "NOT_FOUND_MOCK_EXAM_RESULT", "Mock exam result not found.");

    public static readonly Error ContentIdRequired = Error.Validation(
        "VALIDATION_MOCK_EXAM_CONTENT_ID_REQUIRED", "A contentId is required to start this section.");

    public static readonly Error SectionsIncomplete = Error.Conflict(
        "BUSINESS_MOCK_EXAM_SECTIONS_INCOMPLETE", "Not all sections have been evaluated yet.");
}
