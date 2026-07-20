using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tenancy;

public static class TenantErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "TENANT_NOT_FOUND", "The requested tenant does not exist.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "TENANT_ALREADY_ACTIVE", "The tenant is already in the active state.");

    public static readonly Error AlreadyDeactivated = Error.Conflict(
        "TENANT_ALREADY_DEACTIVATED", "The tenant is already deactivated.");

    public static readonly Error Deleted = Error.Conflict(
        "TENANT_DELETED", "The tenant has been soft-deleted and must be restored first.");

    public static readonly Error SlugTaken = Error.Conflict(
        "TENANT_SLUG_TAKEN", "A tenant with this slug already exists.");

    public static readonly Error CodeTaken = Error.Conflict(
        "TENANT_CODE_TAKEN", "A tenant with this code already exists.");

    public static readonly Error InvalidSlug = Error.Validation(
        "TENANT_INVALID_SLUG", "The slug is invalid. Use lowercase letters, digits, and hyphens (3–63 chars).");

    public static readonly Error InvalidCode = Error.Validation(
        "TENANT_INVALID_CODE", "The code is invalid. Use uppercase letters and digits (2–16 chars).");

    public static readonly Error NotDeleted = Error.Conflict(
        "TENANT_NOT_DELETED", "Only soft-deleted tenants can be restored.");
}
