using System.Diagnostics;
using OpenTelemetry;

namespace TimeTracker.Telemetry;

/// <summary>
///     Drops successful (HTTP &lt; 400) telemetry for noise-only endpoints
///     so they don't pollute Application Insights.
///     Filtered paths: /, /health, /robots933456.txt
/// </summary>
public class HealthRequestFilter : BaseProcessor<Activity>
{
    private static readonly HashSet<string> FilteredPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/", "/health", "/robots933456.txt"
    };

    public override void OnEnd(Activity activity)
    {
        if (activity.Kind != ActivityKind.Server) return;
        if (activity.GetTagItem("url.path") is not string path || !FilteredPaths.Contains(path)) return;
        int statusCode = ParseStatusCode(activity.GetTagItem("http.response.status_code"));
        if (statusCode is > 0 and < 400) activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
    }

    private static int ParseStatusCode(object? value)
    {
        return value switch
        {
            int code => code,
            string s when int.TryParse(s, out int code) => code,
            _ => 0
        };
    }
}
