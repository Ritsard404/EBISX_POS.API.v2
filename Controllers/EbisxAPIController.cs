using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Journal;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBISX_POS.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EbisxAPIController : ControllerBase
    {
        private readonly IEbisxAPI _ebisx;

        public EbisxAPIController(IEbisxAPI ebisx)
        {
            _ebisx = ebisx;
        }

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

        [HttpGet]
        public async Task<IActionResult> ValidateTerminal()
        {
            var (isValid, message) = await _ebisx.ValidateTerminalExpiration();
            if (!isValid)
            {
                return BadRequest(new { isValid, message });
            }
            return Ok(new { isValid, message });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTerminalExpiration()
        {
            var isExpired = await _ebisx.IsTerminalExpired();
            var isExpiringSoon = await _ebisx.IsTerminalExpiringSoon();
            
            return Ok(new 
            { 
                isExpired,
                isExpiringSoon,
                message = isExpired ? "Terminal has expired" : 
                         isExpiringSoon ? "Terminal will expire soon" : 
                         "Terminal is valid"
            });
        }
    }
}
