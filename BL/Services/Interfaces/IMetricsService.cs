using Contracts.Metrics;

namespace BL.Services.Interfaces;

public interface IMetricsService
{
    void IncrementRequests();
    void IncrementErrors();
    MetricsDto GetCurrentMetrics();
}