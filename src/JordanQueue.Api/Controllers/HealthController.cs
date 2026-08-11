using JordanQueue.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "Healthy",
            service = "Jordan Queue API",
            timestamp = DateTime.UtcNow
        }));
    }
}
