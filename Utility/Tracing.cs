using System.Diagnostics;

namespace Utility;

public static class Tracing
{
    public const string Source = "out-box";
    public const string Version = "1.0.0";
    public static readonly ActivitySource ActivitySource = new(Source, Version);
}