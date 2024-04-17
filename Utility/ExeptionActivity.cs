using System.Diagnostics;
using OpenTelemetry.Trace;

namespace Utility;

public class ExeptionActivity
{
    public static void SetExceptionActivity(Exception e, CustomParam? customParam = null)
    {
        if (customParam != null)
        {
            Activity.Current?.SetTag(customParam.Name, customParam.Value);
        }

        Activity.Current?.RecordException(e);
        Activity.Current?.SetStatus(ActivityStatusCode.Error, "ERROR");
    }

    public class CustomParam
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}