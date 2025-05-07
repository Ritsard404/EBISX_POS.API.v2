using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Services.DTO.Menu;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBISX_POS.API.Services.Repositories
{
    public class MenuRepository(DataContext _dataContext) : IMenu
    {
        public async Task<List<AddOnTypeWithAddOnsDTO>> AddOns(int menuId)
        {
            var menuExist = await _dataContext.Menu
                .FirstOrDefaultAsync(m => m.Id == menuId && m.MenuIsAvailable && m.HasAddOn);

            // If no add-ons found, return empty lists
            if (menuExist == null)
            {
                return new List<AddOnTypeWithAddOnsDTO>();
            }

            // Get add-ons for the given menuId and group by AddOnType
            var addOns = await _dataContext.Menu
                .Where(m => m.MenuIsAvailable && m.IsAddOn && m.AddOnType != null)
                .Select(m => new
                {
                    m.AddOnType!.Id,
                    m.AddOnType.AddOnTypeName,
                    AddOn = new AddOnTypeDTO
                    {
                        MenuId = m.Id,
                        MenuName = m.MenuName,
                        MenuImagePath = m.MenuImagePath,
                        Price = m.MenuPrice,
                        Size = m.Size
                    }
                })
                .ToListAsync();

            var groupedAddOns = addOns
                .GroupBy(a => new { a.Id, a.AddOnTypeName })
                .Select(g => new AddOnTypeWithAddOnsDTO
                {
                    AddOnTypeId = g.Key.Id,
                    AddOnTypeName = g.Key.AddOnTypeName,
                    AddOns = g.Select(a => a.AddOn).ToList()
                })
                .ToList();

            // For each add-on type group, further group by MenuName
            // and subtract the price of the regular size ("R") from the other sizes.
            foreach (var addOnTypeGroup in groupedAddOns)
            {
                // Group by the add-on's name
                var addOnsByName = addOnTypeGroup.AddOns.GroupBy(a => a.MenuName);
                foreach (var nameGroup in addOnsByName)
                {
                    // Find the regular add-on (Size == "R") if it exists
                    var regularAddOn = nameGroup.FirstOrDefault(a =>
                        string.Equals(a.Size, "R", StringComparison.OrdinalIgnoreCase));

                    if (regularAddOn != null)
                    {
                        decimal regularPrice = regularAddOn.Price ?? 0m;
                        regularAddOn.Price = 0m; // Set the regular price to 0

                        foreach (var addOn in nameGroup)
                        {
                            // Only adjust non-regular sizes
                            if (!string.Equals(addOn.Size, "R", StringComparison.OrdinalIgnoreCase))
                            {
                                addOn.Price = addOn.Price - regularPrice;
                            }
                        }
                    }
                }
            }

            return groupedAddOns;
        }


        public async Task<List<Category>> Categories()
        {
            return await _dataContext.Category
                .ToListAsync();
        }
        public async Task<(List<DrinkTypeWithDrinksDTO>, List<string>)> Drinks(int menuId)
        {
            var menuExists = await _dataContext.Menu
                .FirstOrDefaultAsync(m => m.Id == menuId && m.MenuIsAvailable && m.HasDrink);

            if (menuExists == null)
            {
                return (new List<DrinkTypeWithDrinksDTO>(), new List<string>());
            }

            // Get menus for available drinks, including Size and Price.
            // Also include DrinkName for grouping.
            var queryResults = await _dataContext.Menu
                .Where(m => m.MenuIsAvailable && m.DrinkType != null)
                .Select(m => new
                {
                    DrinkTypeId = m.DrinkType.Id,
                    DrinkTypeName = m.DrinkType.DrinkTypeName,
                    DrinkName = m.MenuName, // for grouping by drink name
                    Drink = new DrinksDTO
                    {
                        MenuId = m.Id,
                        MenuName = m.MenuName,
                        MenuImagePath = m.MenuImagePath,
                        MenuPrice = m.MenuPrice
                    },
                    Size = m.Size,
                    Price = m.MenuPrice
                })
                .ToListAsync();

            // Adjust prices: for each drink name group, use the regular (Size "R") drink's price
            // as the base. Set the regular drink's price to 0 and subtract that value from the others.
            var adjustedResults = queryResults
                .GroupBy(x => new { x.DrinkTypeId, x.DrinkTypeName })
                .SelectMany(g =>
                    g.GroupBy(x => x.DrinkName)
                     .SelectMany(drinkGroup =>
                     {
                         // Find the regular drink (Size == "R") if it exists.
                         var regular = drinkGroup.FirstOrDefault(x =>
                             string.Equals(x.Size, "R", StringComparison.OrdinalIgnoreCase));
                         decimal regularPrice = regular?.Price ?? 0m;

                         return drinkGroup.Select(x => new
                         {
                             x.DrinkTypeId,
                             x.DrinkTypeName,
                             // Adjust the price: if the drink is regular, price becomes 0;
                             // otherwise, subtract the regular price.
                             Drink = new DrinksDTO
                             {
                                 MenuId = x.Drink.MenuId,
                                 MenuName = x.Drink.MenuName,
                                 MenuImagePath = x.Drink.MenuImagePath,
                                 MenuPrice = string.Equals(x.Size, "R", StringComparison.OrdinalIgnoreCase)
                                                 ? 0m
                                                 : x.Price - regularPrice,
                                 Size = x.Size
                             },
                             x.Size,
                             Price = string.Equals(x.Size, "R", StringComparison.OrdinalIgnoreCase)
                                         ? 0m
                                         : x.Price - regularPrice
                         });
                     }))
                .ToList();

            // Group the adjusted results by DrinkType and then by Size
            var groupedDrinks = adjustedResults
                .GroupBy(x => new { x.DrinkTypeId, x.DrinkTypeName })
                .Select(g => new DrinkTypeWithDrinksDTO
                {
                    DrinkTypeId = g.Key.DrinkTypeId,
                    DrinkTypeName = g.Key.DrinkTypeName,
                    SizesWithPrices = g
                        .Where(x => !string.IsNullOrEmpty(x.Size))
                        .GroupBy(x => x.Size)
                        .Select(sizeGroup => new SizesWithPricesDTO
                        {
                            Size = sizeGroup.Key,
                            // If needed, you can set a representative price here.
                            // Price = sizeGroup.First().Price,
                            Drinks = sizeGroup.Select(x => x.Drink)
                                              .Distinct() // Ensure distinct drinks (make sure DrinksDTO implements equality)
                                              .ToList()
                        })
                        .ToList()
                })
                .ToList();

            // Get distinct drink sizes.
            var sizes = await _dataContext.Menu
                .Where(d => d.DrinkType != null && d.MenuIsAvailable && !string.IsNullOrEmpty(d.Size))
                .Select(d => d.Size!)
                .Distinct()
                .ToListAsync();

            return (groupedDrinks, sizes);
        }


        public async Task<List<Menu>> Menus(int ctgryId)
        {
            return await _dataContext.Menu
                .Where(c => c.Category.Id == ctgryId && c.MenuIsAvailable)
                .Include(c => c.Category)
                .Include(d => d.DrinkType)
                .ToListAsync();
        }
    }
}
