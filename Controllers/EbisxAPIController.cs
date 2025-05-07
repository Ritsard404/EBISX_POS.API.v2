using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Journal;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EbisxAPIController(IEbisxAPI _ebisx) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SetPosTerminalInfo(PosTerminalInfo posTerminalInfo)
        {
            var (success, message) = await _ebisx.SetPosTerminalInfo(posTerminalInfo);
            if (success)
            {
                return Ok(message);
            }
            return BadRequest(message);
        }

        [HttpGet()]
        public async Task<IActionResult> PosTerminalInfo()
        {
            var info = await _ebisx.PosTerminalInfo();
            if (info == null)
                return NotFound("POS terminal not configured.");

            return Ok(info);
        }
    }
}
