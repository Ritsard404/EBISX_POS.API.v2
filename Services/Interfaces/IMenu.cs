using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Menu;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IMenu
    {
        Task<List<Category>> Categories();
        Task<List<Menu>> Menus(int ctgryId);
        Task<(List<DrinkTypeWithDrinksDTO>, List<string>)> Drinks(int menuId);
        Task<List<AddOnTypeWithAddOnsDTO>> AddOns(int menuId);
    }
}
