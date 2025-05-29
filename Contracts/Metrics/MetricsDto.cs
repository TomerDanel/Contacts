namespace Contracts.Metrics;

public class MetricsDto
{
    public int TotalRequests { get; set; }
    public int TotalErrors { get; set; }
    public int UptimeSeconds { get; set; }
}