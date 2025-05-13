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

        Task<(bool isSuccess, string message, List<AddOnType> addOnTypes)> AddAddOnType(AddOnType addOnType, string managerEmail);
        Task<List<AddOnType>> GetAddOnTypes();
        Task<(bool isSuccess, string message)> UpdateAddOnType(AddOnType addOnType, string managerEmail);
        Task<(bool isSuccess, string message)> DeleteAddOnType(int id, string managerEmail);

        Task<(bool isSuccess, string message, List<Category> categories)> AddCategory(Category category, string managerEmail);
        Task<List<Category>> GetCategories();
        Task<(bool isSuccess, string message)> UpdateCategory(Category category, string managerEmail);
        Task<(bool isSuccess, string message)> DeleteCategory(int id, string managerEmail);

        Task<(bool isSuccess, string message, List<DrinkType> drinkTypes)> AddDrinkType(DrinkType drinkType, string managerEmail);
        Task<List<DrinkType>> GetDrinkTypes();
        Task<(bool isSuccess, string message)> UpdateDrinkType(DrinkType drinkType, string managerEmail);
        Task<(bool isSuccess, string message)> DeleteDrinkType(int id, string managerEmail);


        // Menu CRUD Operations
        Task<(bool isSuccess, string message)> AddMenu(Menu menu, string managerEmail);
        Task<List<Menu>> GetAllMenus();
        Task<Menu?> GetMenuById(int id);
        Task<(bool isSuccess, string message)> UpdateMenu(Menu menu, string managerEmail);
        Task<(bool isSuccess, string message)> DeleteMenu(int id, string managerEmail);
        Task<(bool isSuccess, string message)> UpdateMenuImage(int id, string imagePath, string managerEmail);

        // Coupon and Promo Operations
        Task<(bool isSuccess, string message, List<CouponPromo> couponPromos)> AddCouponPromo(CouponPromo couponPromo, string managerEmail);
        Task<List<CouponPromo>> GetAllCouponPromos();
        Task<CouponPromo?> GetCouponPromoById(int id);
        Task<CouponPromo?> GetCouponPromoByCode(string code);
        Task<(bool isSuccess, string message)> UpdateCouponPromo(CouponPromo couponPromo, string managerEmail);
        Task<(bool isSuccess, string message)> DeleteCouponPromo(int id, string managerEmail);
        Task<(bool isSuccess, string message)> ValidateCouponPromo(string code, List<Menu> selectedMenus);
    }
}
