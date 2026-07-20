namespace IELTSLms.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface for requests that require authorization. Handlers annotated with this
/// are checked by <see cref="Behaviors.AuthorizationPipelineBehavior{TRequest,TResponse}"/>
/// before the handler runs.
/// </summary>
public interface IAuthorizeRequest
{
    string[] RequiredRoles => [];
    string[] RequiredPermissions => [];
}
