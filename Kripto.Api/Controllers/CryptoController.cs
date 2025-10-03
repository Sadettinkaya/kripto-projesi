using Microsoft.AspNetCore.Mvc;
using Kripto.Api.Services;

namespace Kripto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly OkxWebSocketService _okxService;

        public CryptoController(OkxWebSocketService okxService)
        {
            _okxService = okxService;
        }

        [HttpGet("prices")]
        public IActionResult GetPrices()
        {
            var prices = _okxService.GetPrices();
            return Ok(prices);
        }
    }
}