namespace Utility.Configuration;

public class TracingSettings
{
    public bool Enabled { get; set; }
    public string OtlpEndpoint { get; set; }
    public string OtlpAuthorizationHeader { get; set; }
    public string ServiceName { get; set; }
}