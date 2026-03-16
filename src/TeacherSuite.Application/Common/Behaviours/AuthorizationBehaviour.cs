using System.Reflection;
using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using UnauthorizedAccessException = TeacherSuite.Application.Common.Exceptions.UnauthorizedAccessException;

namespace TeacherSuite.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse>(ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();

        if (authorizeAttributes.Count == 0)
        {
            return await next(cancellationToken);
        }

        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        var attributesWithRoles = authorizeAttributes
            .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
            .ToList();

        if (attributesWithRoles.Count != 0)
        {
            var authorized = false;

            foreach (var roles in attributesWithRoles.Select(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
            {
                foreach (var role in roles)
                {
                    if (currentUserService.IsInRole(role.Trim()))
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
