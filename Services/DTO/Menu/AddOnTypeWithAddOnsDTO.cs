using System.Collections.Generic;

namespace EBISX_POS.API.Services.DTO.Menu
{
    public class AddOnTypeWithAddOnsDTO
    {
        public int AddOnTypeId { get; set; }
        public string AddOnTypeName { get; set; }
        public List<AddOnTypeDTO> AddOns { get; set; } = new List<AddOnTypeDTO>();
    }
}
