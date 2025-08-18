using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookIt.API.Middleware.Logging;

public class CallerEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var stackTrace = new StackTrace(true);
        var frames = stackTrace.GetFrames();

        if (frames != null)
        {
            var userFrame = frames.Skip(3).FirstOrDefault(f =>
            {
                var method = f.GetMethod();
                if (method == null) return false;

                var declaringType = method.DeclaringType;
                if (declaringType == null) return false;

                var typeName = declaringType.FullName ?? "";

                return !typeName.StartsWith("Serilog") &&
                       !typeName.StartsWith("Microsoft.Extensions.Logging") &&
                       !declaringType.IsDefined(typeof(CompilerGeneratedAttribute), false);
            });

            if (userFrame != null)
            {
                var method = userFrame.GetMethod();
                var className = method?.DeclaringType?.Name ?? "Unknown";
                var methodName = GetActualMethodName(method);
                var lineNumber = userFrame.GetFileLineNumber();

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Method", methodName));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Class", className));
                if (lineNumber > 0)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Line", lineNumber));
                }
            }
        }
    }

    private static string GetActualMethodName(System.Reflection.MethodBase? method)
    {
        if (method == null) return "Unknown";

        var methodName = method.Name;

        if (methodName == "MoveNext" && method.DeclaringType != null)
        {
            var typeName = method.DeclaringType.Name;

            if (typeName.StartsWith("<") && typeName.Contains(">d__"))
            {
                var startIndex = 1;
                var endIndex = typeName.IndexOf('>');
                if (endIndex > startIndex)
                {
                    return typeName.Substring(startIndex, endIndex - startIndex);
                }
            }
        }

        return methodName;
    }
}