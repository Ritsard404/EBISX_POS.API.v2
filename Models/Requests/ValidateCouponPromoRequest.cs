using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EBISX_POS.API.Models;

namespace EBISX_POS.API.Models.Requests
{
    public class ValidateCouponPromoRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public List<Menu> SelectedMenus { get; set; } = new List<Menu>();
    }
} 