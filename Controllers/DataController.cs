using EBISX_POS.API.Models;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EBISX_POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController(IData _dataService, ILogger<DataController> _logger) : ControllerBase
    {

        /// <summary>
        /// Adds a new user to the system
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <param name="managerEmail">The email of the manager approving the addition (optional for first user)</param>
        /// <returns>Success status, message, and updated user list</returns>
        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] User user, [FromQuery] string? managerEmail = null)
        {
            try
            {
                var (isSuccess, message, users) = await _dataService.AddUser(user, managerEmail);
                
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message, users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddUser endpoint");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Retrieves all active users
        /// </summary>
        /// <returns>List of active users</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _dataService.GetUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsers endpoint");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Updates an existing user's information
        /// </summary>
        /// <param name="user">The user with updated information</param>
        /// <param name="managerEmail">The email of the manager performing the update</param>
        /// <returns>Success status and message</returns>
        [HttpPut("users")]
        public async Task<IActionResult> UpdateUser([FromBody] User user, [FromQuery] string managerEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(managerEmail))
                {
                    return BadRequest(new { message = "Manager email is required" });
                }

                var (isSuccess, message) = await _dataService.UpdateUser(user, managerEmail);
                
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUser endpoint");
                return StatusCode(500, new { message = "An error occurred while updating the user" });
            }
        }

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        /// <param name="userEmail">The email of the user to deactivate</param>
        /// <param name="managerEmail">The email of the manager performing the deactivation</param>
        /// <returns>Success status and message</returns>
        [HttpDelete("users/{userEmail}")]
        public async Task<IActionResult> DeactivateUser(string userEmail, [FromQuery] string managerEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(managerEmail))
                {
                    return BadRequest(new { message = "Manager email is required" });
                }

                var (isSuccess, message) = await _dataService.DeactivateUser(userEmail, managerEmail);
                
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeactivateUser endpoint");
                return StatusCode(500, new { message = "An error occurred while deactivating the user" });
            }
        }
    }
}
