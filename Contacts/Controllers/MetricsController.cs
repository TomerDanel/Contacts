using BL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Controllers;

[ApiController]
[Route("api/metrics")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public IActionResult GetMetrics()
    {
        var metrics = _metricsService.GetCurrentMetrics();
        return Ok(metrics);
    }
}