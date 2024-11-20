using System.Reflection;

namespace Grapeseed;

public static class RestRouteAttributeExtensions
{
    public static object[] GenerateRouteConstructorArguments(this RestRouteAttribute attribute, MethodInfo methodInfo, string? basePath = null)
    {
        object[] args = new object[6];

        if (!methodInfo.IsStatic)
        {
            args[0] = methodInfo;
        }
        else
        {
            Func<IHttpContext, Task> action = async (context) =>
            {
                var task = methodInfo.Invoke(null, [context]);
                if (task is Task t)
                {
                    await t;
                }
            };

            args[0] = action;
        }

        args[1] = attribute.HttpMethod;

        var basepath = basePath?.SanitizePath();
        if (!string.IsNullOrWhiteSpace(attribute.RouteTemplate))
        {
            var appendStart = "";
            if (attribute.RouteTemplate.StartsWith('^'))
            {
                appendStart = "^";
                attribute.RouteTemplate = attribute.RouteTemplate.TrimStart('^');
            }
            basepath = $"{appendStart}{basepath}{attribute.RouteTemplate.SanitizePath()}";
        }

        args[2] = basepath ?? string.Empty;

        args[3] = attribute.Enabled;

        if (!string.IsNullOrWhiteSpace(attribute.Name))
        {
            args[4] = attribute.Name;
        }
        else
        {
            var type = methodInfo.DeclaringType;
            args[4] = type is not null
                ? $"{type.Name}.{methodInfo.Name}"
                : $"{methodInfo.Name}";
        }

        args[5] = attribute.Description;

        return args;
    }
}
