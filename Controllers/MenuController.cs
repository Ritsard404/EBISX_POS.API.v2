using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using EBISX_POS.API.Services.Interfaces;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Requests;

namespace EBISX_POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController(IMenu _menu, ILogger<MenuController> _logger) : ControllerBase
    {
        #region Menu Operations
        /// <summary>
        /// Gets all menus
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMenus()
        {
            try
            {
                var menus = await _menu.GetAllMenus();
                return Ok(menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menus");
                return StatusCode(500, new { message = "An error occurred while retrieving menus" });
            }
        }

        /// <summary>
        /// Gets a menu by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            try
            {
                var menu = await _menu.GetMenuById(id);
                if (menu == null)
                {
                    return NotFound(new { message = "Menu not found" });
                }
                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu");
                return StatusCode(500, new { message = "An error occurred while retrieving the menu" });
            }
        }

        /// <summary>
        /// Adds a new menu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddMenu([FromBody] Menu menu, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.AddMenu(menu, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding menu");
                return StatusCode(500, new { message = "An error occurred while adding the menu" });
            }
        }

        /// <summary>
        /// Updates an existing menu
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMenu([FromBody] Menu menu, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.UpdateMenu(menu, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu");
                return StatusCode(500, new { message = "An error occurred while updating the menu" });
            }
        }

        /// <summary>
        /// Deletes a menu
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.DeleteMenu(id, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu");
                return StatusCode(500, new { message = "An error occurred while deleting the menu" });
            }
        }

        /// <summary>
        /// Updates a menu's image
        /// </summary>
        [HttpPut("{id}/image")]
        public async Task<IActionResult> UpdateMenuImage(int id, [FromBody] string imagePath, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.UpdateMenuImage(id, imagePath, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu image");
                return StatusCode(500, new { message = "An error occurred while updating the menu image" });
            }
        }
        #endregion

        #region Menu Operations
        /// <summary>
        /// Gets all categories
        /// </summary>
        [HttpGet("categories/all")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _menu.Categories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "An error occurred while retrieving categories" });
            }
        }

        /// <summary>
        /// Gets all menus for a specific category
        /// </summary>
        [HttpGet("menus/{categoryId}")]
        public async Task<IActionResult> GetMenus(int categoryId)
        {
            try
            {
                var menus = await _menu.Menus(categoryId);
                return Ok(menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menus");
                return StatusCode(500, new { message = "An error occurred while retrieving menus" });
            }
        }

        /// <summary>
        /// Gets all drinks and available sizes for a specific menu
        /// </summary>
        [HttpGet("drinks/{menuId}")]
        public async Task<IActionResult> GetDrinks(int menuId)
        {
            try
            {
                var (drinks, sizes) = await _menu.Drinks(menuId);
                return Ok(new { drinks, sizes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drinks");
                return StatusCode(500, new { message = "An error occurred while retrieving drinks" });
            }
        }

        /// <summary>
        /// Gets all add-ons for a specific menu
        /// </summary>
        [HttpGet("addons/{menuId}")]
        public async Task<IActionResult> GetAddOns(int menuId)
        {
            try
            {
                var addOns = await _menu.AddOns(menuId);
                return Ok(addOns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving add-ons");
                return StatusCode(500, new { message = "An error occurred while retrieving add-ons" });
            }
        }
        #endregion

        #region AddOnType Endpoints
        /// <summary>
        /// Adds a new add-on type
        /// </summary>
        [HttpPost("addon-types")]
        public async Task<IActionResult> AddAddOnType([FromBody] AddOnType addOnType, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message, addOnTypes) = await _menu.AddAddOnType(addOnType, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message, addOnTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding add-on type");
                return StatusCode(500, new { message = "An error occurred while adding the add-on type" });
            }
        }

        /// <summary>
        /// Gets all add-on types
        /// </summary>
        [HttpGet("addon-types")]
        public async Task<IActionResult> GetAddOnTypes()
        {
            try
            {
                var addOnTypes = await _menu.GetAddOnTypes();
                return Ok(addOnTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving add-on types");
                return StatusCode(500, new { message = "An error occurred while retrieving add-on types" });
            }
        }

        /// <summary>
        /// Updates an existing add-on type
        /// </summary>
        [HttpPut("addon-types")]
        public async Task<IActionResult> UpdateAddOnType([FromBody] AddOnType addOnType, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.UpdateAddOnType(addOnType, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating add-on type");
                return StatusCode(500, new { message = "An error occurred while updating the add-on type" });
            }
        }

        /// <summary>
        /// Deletes an add-on type
        /// </summary>
        [HttpDelete("addon-types/{id}")]
        public async Task<IActionResult> DeleteAddOnType(int id, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.DeleteAddOnType(id, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting add-on type");
                return StatusCode(500, new { message = "An error occurred while deleting the add-on type" });
            }
        }
        #endregion

        #region Category Endpoints
        /// <summary>
        /// Adds a new category
        /// </summary>
        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] Category category, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message, categories) = await _menu.AddCategory(category, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message, categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                return StatusCode(500, new { message = "An error occurred while adding the category" });
            }
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _menu.GetCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "An error occurred while retrieving categories" });
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategory([FromBody] Category category, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.UpdateCategory(category, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "An error occurred while updating the category" });
            }
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.DeleteCategory(id, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new { message = "An error occurred while deleting the category" });
            }
        }
        #endregion

        #region DrinkType Endpoints
        /// <summary>
        /// Adds a new drink type
        /// </summary>
        [HttpPost("drink-types")]
        public async Task<IActionResult> AddDrinkType([FromBody] DrinkType drinkType, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message, drinkTypes) = await _menu.AddDrinkType(drinkType, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message, drinkTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding drink type");
                return StatusCode(500, new { message = "An error occurred while adding the drink type" });
            }
        }

        /// <summary>
        /// Gets all drink types
        /// </summary>
        [HttpGet("drink-types")]
        public async Task<IActionResult> GetDrinkTypes()
        {
            try
            {
                var drinkTypes = await _menu.GetDrinkTypes();
                return Ok(drinkTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drink types");
                return StatusCode(500, new { message = "An error occurred while retrieving drink types" });
            }
        }

        /// <summary>
        /// Updates an existing drink type
        /// </summary>
        [HttpPut("drink-types")]
        public async Task<IActionResult> UpdateDrinkType([FromBody] DrinkType drinkType, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.UpdateDrinkType(drinkType, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drink type");
                return StatusCode(500, new { message = "An error occurred while updating the drink type" });
            }
        }

        /// <summary>
        /// Deletes a drink type
        /// </summary>
        [HttpDelete("drink-types/{id}")]
        public async Task<IActionResult> DeleteDrinkType(int id, [FromQuery] string managerEmail)
        {
            try
            {
                var (isSuccess, message) = await _menu.DeleteDrinkType(id, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting drink type");
                return StatusCode(500, new { message = "An error occurred while deleting the drink type" });
            }
        }
        #endregion

        #region Coupon and Promo Operations
        /// <summary>
        /// Get all coupon promos
        /// </summary>
        [HttpGet("coupon-promos")]
        public async Task<IActionResult> GetAllCouponPromos()
        {
            try
            {
                var couponPromos = await _menu.GetAllCouponPromos();
                return Ok(couponPromos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promos");
                return StatusCode(500, "An error occurred while retrieving coupon promos");
            }
        }

        /// <summary>
        /// Get coupon promo by ID
        /// </summary>
        [HttpGet("coupon-promos/{id}")]
        public async Task<IActionResult> GetCouponPromoById(int id)
        {
            try
            {
                var couponPromo = await _menu.GetCouponPromoById(id);
                if (couponPromo == null)
                {
                    return NotFound("Coupon/Promo not found");
                }
                return Ok(couponPromo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promo");
                return StatusCode(500, "An error occurred while retrieving the coupon promo");
            }
        }

        /// <summary>
        /// Get coupon promo by code
        /// </summary>
        [HttpGet("coupon-promos/code/{code}")]
        public async Task<IActionResult> GetCouponPromoByCode(string code)
        {
            try
            {
                var couponPromo = await _menu.GetCouponPromoByCode(code);
                if (couponPromo == null)
                {
                    return NotFound("Coupon/Promo not found");
                }
                return Ok(couponPromo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promo");
                return StatusCode(500, "An error occurred while retrieving the coupon promo");
            }
        }

        /// <summary>
        /// Add new coupon promo
        /// </summary>
        [HttpPost("coupon-promos")]
        public async Task<IActionResult> AddCouponPromo([FromBody] CouponPromo couponPromo, [FromHeader] string managerEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return BadRequest("Manager email is required");
                }

                var (isSuccess, message, couponPromos) = await _menu.AddCouponPromo(couponPromo, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(new { message, couponPromos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coupon promo");
                return StatusCode(500, "An error occurred while adding the coupon promo");
            }
        }

        /// <summary>
        /// Update coupon promo
        /// </summary>
        [HttpPut("coupon-promos")]
        public async Task<IActionResult> UpdateCouponPromo([FromBody] CouponPromo couponPromo, [FromHeader] string managerEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return BadRequest("Manager email is required");
                }

                var (isSuccess, message) = await _menu.UpdateCouponPromo(couponPromo, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon promo");
                return StatusCode(500, "An error occurred while updating the coupon promo");
            }
        }

        /// <summary>
        /// Delete coupon promo
        /// </summary>
        [HttpDelete("coupon-promos/{id}")]
        public async Task<IActionResult> DeleteCouponPromo(int id, [FromHeader] string managerEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(managerEmail))
                {
                    return BadRequest("Manager email is required");
                }

                var (isSuccess, message) = await _menu.DeleteCouponPromo(id, managerEmail);
                if (!isSuccess)
                {
                    return BadRequest(message);
                }
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon promo");
                return StatusCode(500, "An error occurred while deleting the coupon promo");
            }
        }

        /// <summary>
        /// Validate coupon promo code
        /// </summary>
        [HttpPost("coupon-promos/validate")]
        public async Task<IActionResult> ValidateCouponPromo([FromBody] ValidateCouponPromoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    return BadRequest("Code is required");
                }

                if (request.SelectedMenus == null || !request.SelectedMenus.Any())
                {
                    return BadRequest("Selected menus are required");
                }

                var (isSuccess, message) = await _menu.ValidateCouponPromo(request.Code, request.SelectedMenus);
                return Ok(new { isSuccess, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon promo");
                return StatusCode(500, "An error occurred while validating the coupon promo");
            }
        }
        #endregion
    }
}
