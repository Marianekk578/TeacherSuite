using System.Reflection;
using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehaviour(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();

        if (authorizeAttributes.Count == 0)
        {
            return await next(cancellationToken);
        }

        if (!_currentUserService.IsAuthenticated)
        {
            throw new Exceptions.UnauthorizedAccessException();
        }

        var attributesWithRoles = authorizeAttributes
            .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
            .ToList();

        if (attributesWithRoles.Count != 0)
        {
            var authorized = false;

            foreach (var roles in attributesWithRoles.Select(a => a.Roles!.Split(',')))
            {
                foreach (var role in roles)
                {
                    if (_currentUserService.IsInRole(role.Trim()))
                    {
                        authorized = true;
                        break;
                    }
                }

                if (authorized)
                {
                    break;
                }
            }

            if (!authorized)
            {
                throw new ForbiddenAccessException();
            }
        }

        return await next(cancellationToken);
    }
}
