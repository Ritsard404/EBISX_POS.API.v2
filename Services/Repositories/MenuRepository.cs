using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Utils;
using EBISX_POS.API.Services.DTO.Menu;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Services.Repositories
{
    public class MenuRepository(DataContext _dataContext, ILogger<MenuRepository> _logger) : IMenu
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

        #region AddOnType Operations
        public async Task<(bool isSuccess, string message, List<AddOnType> addOnTypes)> AddAddOnType(AddOnType addOnType, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials", new List<AddOnType>());
                }

                // Validate add-on type
                if (string.IsNullOrWhiteSpace(addOnType.AddOnTypeName))
                {
                    return (false, "Add-on type name is required", new List<AddOnType>());
                }

                // Check for duplicate
                if (await _dataContext.AddOnType.AnyAsync(a => a.AddOnTypeName == addOnType.AddOnTypeName))
                {
                    return (false, "Add-on type with this name already exists", new List<AddOnType>());
                }

                // Add add-on type
                await _dataContext.AddOnType.AddAsync(addOnType);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Added new add-on type: {addOnType.AddOnTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();

                // Return updated list
                var addOnTypes = await _dataContext.AddOnType.OrderBy(a => a.AddOnTypeName).ToListAsync();
                return (true, "Add-on type added successfully", addOnTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding add-on type");
                return (false, "An error occurred while adding the add-on type", new List<AddOnType>());
            }
        }

        public async Task<List<AddOnType>> GetAddOnTypes()
        {
            try
            {
                return await _dataContext.AddOnType.OrderBy(a => a.AddOnTypeName).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving add-on types");
                return new List<AddOnType>();
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateAddOnType(AddOnType addOnType, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Validate add-on type
                if (string.IsNullOrWhiteSpace(addOnType.AddOnTypeName))
                {
                    return (false, "Add-on type name is required");
                }

                // Get existing add-on type
                var existingAddOnType = await _dataContext.AddOnType.FindAsync(addOnType.Id);
                if (existingAddOnType == null)
                {
                    return (false, "Add-on type not found");
                }

                // Check for duplicate
                if (await _dataContext.AddOnType.AnyAsync(a => a.AddOnTypeName == addOnType.AddOnTypeName && a.Id != addOnType.Id))
                {
                    return (false, "Add-on type with this name already exists");
                }

                // Update add-on type
                existingAddOnType.AddOnTypeName = addOnType.AddOnTypeName;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated add-on type: {addOnType.AddOnTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Add-on type updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating add-on type");
                return (false, "An error occurred while updating the add-on type");
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteAddOnType(int id, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get add-on type
                var addOnType = await _dataContext.AddOnType.FindAsync(id);
                if (addOnType == null)
                {
                    return (false, "Add-on type not found");
                }

                // Check if add-on type is in use
                if (await _dataContext.Menu.AnyAsync(m => m.AddOnType != null && m.AddOnType.Id == id))
                {
                    return (false, "Cannot delete add-on type that is in use");
                }

                // Delete add-on type
                _dataContext.AddOnType.Remove(addOnType);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deleted add-on type: {addOnType.AddOnTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Add-on type deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting add-on type");
                return (false, "An error occurred while deleting the add-on type");
            }
        }
        #endregion

        #region Category Operations
        public async Task<(bool isSuccess, string message, List<Category> categories)> AddCategory(Category category, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials", new List<Category>());
                }

                // Validate category
                if (string.IsNullOrWhiteSpace(category.CtgryName))
                {
                    return (false, "Category name is required", new List<Category>());
                }

                // Check for duplicate
                if (await _dataContext.Category.AnyAsync(c => c.CtgryName == category.CtgryName))
                {
                    return (false, "Category with this name already exists", new List<Category>());
                }

                // Add category
                await _dataContext.Category.AddAsync(category);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Added new category: {category.CtgryName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();

                // Return updated list
                var categories = await _dataContext.Category.OrderBy(c => c.CtgryName).ToListAsync();
                return (true, "Category added successfully", categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                return (false, "An error occurred while adding the category", new List<Category>());
            }
        }

        public async Task<List<Category>> GetCategories()
        {
            try
            {
                return await _dataContext.Category.OrderBy(c => c.CtgryName).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return new List<Category>();
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateCategory(Category category, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Validate category
                if (string.IsNullOrWhiteSpace(category.CtgryName))
                {
                    return (false, "Category name is required");
                }

                // Get existing category
                var existingCategory = await _dataContext.Category.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    return (false, "Category not found");
                }

                // Check for duplicate
                if (await _dataContext.Category.AnyAsync(c => c.CtgryName == category.CtgryName && c.Id != category.Id))
                {
                    return (false, "Category with this name already exists");
                }

                // Update category
                existingCategory.CtgryName = category.CtgryName;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated category: {category.CtgryName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return (false, "An error occurred while updating the category");
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteCategory(int id, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get category
                var category = await _dataContext.Category.FindAsync(id);
                if (category == null)
                {
                    return (false, "Category not found");
                }

                // Check if category is in use
                if (await _dataContext.Menu.AnyAsync(m => m.Category.Id == id))
                {
                    return (false, "Cannot delete category that is in use");
                }

                // Delete category
                _dataContext.Category.Remove(category);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deleted category: {category.CtgryName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return (false, "An error occurred while deleting the category");
            }
        }
        #endregion

        #region DrinkType Operations
        public async Task<(bool isSuccess, string message, List<DrinkType> drinkTypes)> AddDrinkType(DrinkType drinkType, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials", new List<DrinkType>());
                }

                // Validate drink type
                if (string.IsNullOrWhiteSpace(drinkType.DrinkTypeName))
                {
                    return (false, "Drink type name is required", new List<DrinkType>());
                }

                // Check for duplicate
                if (await _dataContext.DrinkType.AnyAsync(d => d.DrinkTypeName == drinkType.DrinkTypeName))
                {
                    return (false, "Drink type with this name already exists", new List<DrinkType>());
                }

                // Add drink type
                await _dataContext.DrinkType.AddAsync(drinkType);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Added new drink type: {drinkType.DrinkTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();

                // Return updated list
                var drinkTypes = await _dataContext.DrinkType.OrderBy(d => d.DrinkTypeName).ToListAsync();
                return (true, "Drink type added successfully", drinkTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding drink type");
                return (false, "An error occurred while adding the drink type", new List<DrinkType>());
            }
        }

        public async Task<List<DrinkType>> GetDrinkTypes()
        {
            try
            {
                return await _dataContext.DrinkType.OrderBy(d => d.DrinkTypeName).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drink types");
                return new List<DrinkType>();
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateDrinkType(DrinkType drinkType, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Validate drink type
                if (string.IsNullOrWhiteSpace(drinkType.DrinkTypeName))
                {
                    return (false, "Drink type name is required");
                }

                // Get existing drink type
                var existingDrinkType = await _dataContext.DrinkType.FindAsync(drinkType.Id);
                if (existingDrinkType == null)
                {
                    return (false, "Drink type not found");
                }

                // Check for duplicate
                if (await _dataContext.DrinkType.AnyAsync(d => d.DrinkTypeName == drinkType.DrinkTypeName && d.Id != drinkType.Id))
                {
                    return (false, "Drink type with this name already exists");
                }

                // Update drink type
                existingDrinkType.DrinkTypeName = drinkType.DrinkTypeName;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated drink type: {drinkType.DrinkTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Drink type updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drink type");
                return (false, "An error occurred while updating the drink type");
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteDrinkType(int id, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get drink type
                var drinkType = await _dataContext.DrinkType.FindAsync(id);
                if (drinkType == null)
                {
                    return (false, "Drink type not found");
                }

                // Check if drink type is in use
                if (await _dataContext.Menu.AnyAsync(m => m.DrinkType != null && m.DrinkType.Id == id))
                {
                    return (false, "Cannot delete drink type that is in use");
                }

                // Delete drink type
                _dataContext.DrinkType.Remove(drinkType);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deleted drink type: {drinkType.DrinkTypeName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Drink type deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting drink type");
                return (false, "An error occurred while deleting the drink type");
            }
        }
        #endregion

        #region Menu Operations
        public async Task<(bool isSuccess, string message, List<Menu> menus)> AddMenu(Menu menu, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials", new List<Menu>());
                }

                // Validate menu
                if (string.IsNullOrWhiteSpace(menu.MenuName))
                {
                    return (false, "Menu name is required", new List<Menu>());
                }

                if (menu.MenuPrice <= 0)
                {
                    return (false, "Menu price must be greater than 0", new List<Menu>());
                }

                if (menu.Category == null)
                {
                    return (false, "Category is required", new List<Menu>());
                }

                // Check if category exists
                var category = await _dataContext.Category.FindAsync(menu.Category.Id);
                if (category == null)
                {
                    return (false, "Category not found", new List<Menu>());
                }

                // Check if drink type exists if provided
                if (menu.DrinkType != null)
                {
                    var drinkType = await _dataContext.DrinkType.FindAsync(menu.DrinkType.Id);
                    if (drinkType == null)
                    {
                        return (false, "Drink type not found", new List<Menu>());
                    }
                }

                // Check if add-on type exists if provided
                if (menu.AddOnType != null)
                {
                    var addOnType = await _dataContext.AddOnType.FindAsync(menu.AddOnType.Id);
                    if (addOnType == null)
                    {
                        return (false, "Add-on type not found", new List<Menu>());
                    }
                }

                // Add menu
                await _dataContext.Menu.AddAsync(menu);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Added new menu: {menu.MenuName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();

                // Return updated list
                var menus = await _dataContext.Menu
                    .Include(m => m.Category)
                    .Include(m => m.DrinkType)
                    .Include(m => m.AddOnType)
                    .OrderBy(m => m.MenuName)
                    .ToListAsync();

                return (true, "Menu added successfully", menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding menu");
                return (false, "An error occurred while adding the menu", new List<Menu>());
            }
        }

        public async Task<List<Menu>> GetAllMenus()
        {
            try
            {
                return await _dataContext.Menu
                    .Include(m => m.Category)
                    .Include(m => m.DrinkType)
                    .Include(m => m.AddOnType)
                    .OrderBy(m => m.MenuName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menus");
                return new List<Menu>();
            }
        }

        public async Task<Menu?> GetMenuById(int id)
        {
            try
            {
                return await _dataContext.Menu
                    .Include(m => m.Category)
                    .Include(m => m.DrinkType)
                    .Include(m => m.AddOnType)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu");
                return null;
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateMenu(Menu menu, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Validate menu
                if (string.IsNullOrWhiteSpace(menu.MenuName))
                {
                    return (false, "Menu name is required");
                }

                if (menu.MenuPrice <= 0)
                {
                    return (false, "Menu price must be greater than 0");
                }

                // Get existing menu
                var existingMenu = await _dataContext.Menu.FindAsync(menu.Id);
                if (existingMenu == null)
                {
                    return (false, "Menu not found");
                }

                // Check if category exists
                if (menu.Category != null)
                {
                    var category = await _dataContext.Category.FindAsync(menu.Category.Id);
                    if (category == null)
                    {
                        return (false, "Category not found");
                    }
                }

                // Check if drink type exists if provided
                if (menu.DrinkType != null)
                {
                    var drinkType = await _dataContext.DrinkType.FindAsync(menu.DrinkType.Id);
                    if (drinkType == null)
                    {
                        return (false, "Drink type not found");
                    }
                }

                // Check if add-on type exists if provided
                if (menu.AddOnType != null)
                {
                    var addOnType = await _dataContext.AddOnType.FindAsync(menu.AddOnType.Id);
                    if (addOnType == null)
                    {
                        return (false, "Add-on type not found");
                    }
                }

                // Update menu
                existingMenu.MenuName = menu.MenuName;
                existingMenu.MenuPrice = menu.MenuPrice;
                existingMenu.Category = menu.Category;
                existingMenu.DrinkType = menu.DrinkType;
                existingMenu.AddOnType = menu.AddOnType;
                existingMenu.Size = menu.Size;
                existingMenu.HasDrink = menu.HasDrink;
                existingMenu.HasAddOn = menu.HasAddOn;
                existingMenu.IsAddOn = menu.IsAddOn;
                //existingMenu.MenuIsAvailable = menu.MenuIsAvailable;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated menu: {menu.MenuName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Menu updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu");
                return (false, "An error occurred while updating the menu");
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteMenu(int id, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get menu
                var menu = await _dataContext.Menu.FindAsync(id);
                if (menu == null)
                {
                    return (false, "Menu not found");
                }

                // Delete menu
                _dataContext.Menu.Remove(menu);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deleted menu: {menu.MenuName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Menu deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu");
                return (false, "An error occurred while deleting the menu");
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateMenuImage(int id, string imagePath, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get menu
                var menu = await _dataContext.Menu.FindAsync(id);
                if (menu == null)
                {
                    return (false, "Menu not found");
                }

                // Update image path
                menu.MenuImagePath = imagePath;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated menu image: {menu.MenuName}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Menu image updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu image");
                return (false, "An error occurred while updating the menu image");
            }
        }
        #endregion

        #region Coupon and Promo Operations
        public async Task<(bool isSuccess, string message, List<CouponPromo> couponPromos)> AddCouponPromo(CouponPromo couponPromo, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials", new List<CouponPromo>());
                }

                // Validate coupon/promo
                if (string.IsNullOrWhiteSpace(couponPromo.Description))
                {
                    return (false, "Description is required", new List<CouponPromo>());
                }

                if (couponPromo.PromoAmount <= 0)
                {
                    return (false, "Promo amount must be greater than 0", new List<CouponPromo>());
                }

                if (couponPromo.ExpirationTime <= DateTimeOffset.UtcNow)
                {
                    return (false, "Expiration time must be in the future", new List<CouponPromo>());
                }

                // Check for duplicate codes
                if (!string.IsNullOrEmpty(couponPromo.PromoCode))
                {
                    if (await _dataContext.CouponPromo.AnyAsync(cp => cp.PromoCode == couponPromo.PromoCode))
                    {
                        return (false, "Promo code already exists", new List<CouponPromo>());
                    }
                }

                if (!string.IsNullOrEmpty(couponPromo.CouponCode))
                {
                    if (await _dataContext.CouponPromo.AnyAsync(cp => cp.CouponCode == couponPromo.CouponCode))
                    {
                        return (false, "Coupon code already exists", new List<CouponPromo>());
                    }
                }

                // Validate menu items if provided
                if (couponPromo.CouponMenus != null && couponPromo.CouponMenus.Any())
                {
                    foreach (var menu in couponPromo.CouponMenus)
                    {
                        var existingMenu = await _dataContext.Menu.FindAsync(menu.Id);
                        if (existingMenu == null)
                        {
                            return (false, $"Menu with ID {menu.Id} not found", new List<CouponPromo>());
                        }
                    }
                }

                // Add coupon/promo
                await _dataContext.CouponPromo.AddAsync(couponPromo);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Added new {(string.IsNullOrEmpty(couponPromo.PromoCode) ? "coupon" : "promo")}: {couponPromo.Description}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();

                // Return updated list
                var couponPromos = await _dataContext.CouponPromo
                    .Include(cp => cp.CouponMenus)
                    .OrderBy(cp => cp.Description)
                    .ToListAsync();

                return (true, "Coupon/Promo added successfully", couponPromos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coupon/promo");
                return (false, "An error occurred while adding the coupon/promo", new List<CouponPromo>());
            }
        }

        public async Task<List<CouponPromo>> GetAllCouponPromos()
        {
            try
            {
                return await _dataContext.CouponPromo
                    .Include(cp => cp.CouponMenus)
                    .OrderBy(cp => cp.Description)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promos");
                return new List<CouponPromo>();
            }
        }

        public async Task<CouponPromo?> GetCouponPromoById(int id)
        {
            try
            {
                return await _dataContext.CouponPromo
                    .Include(cp => cp.CouponMenus)
                    .FirstOrDefaultAsync(cp => cp.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promo");
                return null;
            }
        }

        public async Task<CouponPromo?> GetCouponPromoByCode(string code)
        {
            try
            {
                return await _dataContext.CouponPromo
                    .Include(cp => cp.CouponMenus)
                    .FirstOrDefaultAsync(cp => cp.PromoCode == code || cp.CouponCode == code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon promo by code");
                return null;
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateCouponPromo(CouponPromo couponPromo, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Validate coupon/promo
                if (string.IsNullOrWhiteSpace(couponPromo.Description))
                {
                    return (false, "Description is required");
                }

                if (couponPromo.PromoAmount <= 0)
                {
                    return (false, "Promo amount must be greater than 0");
                }

                if (couponPromo.ExpirationTime <= DateTimeOffset.UtcNow)
                {
                    return (false, "Expiration time must be in the future");
                }

                // Get existing coupon/promo
                var existingCouponPromo = await _dataContext.CouponPromo.FindAsync(couponPromo.Id);
                if (existingCouponPromo == null)
                {
                    return (false, "Coupon/Promo not found");
                }

                // Check for duplicate codes
                if (!string.IsNullOrEmpty(couponPromo.PromoCode) && couponPromo.PromoCode != existingCouponPromo.PromoCode)
                {
                    if (await _dataContext.CouponPromo.AnyAsync(cp => cp.PromoCode == couponPromo.PromoCode))
                    {
                        return (false, "Promo code already exists");
                    }
                }

                if (!string.IsNullOrEmpty(couponPromo.CouponCode) && couponPromo.CouponCode != existingCouponPromo.CouponCode)
                {
                    if (await _dataContext.CouponPromo.AnyAsync(cp => cp.CouponCode == couponPromo.CouponCode))
                    {
                        return (false, "Coupon code already exists");
                    }
                }

                // Validate menu items if provided
                if (couponPromo.CouponMenus != null && couponPromo.CouponMenus.Any())
                {
                    foreach (var menu in couponPromo.CouponMenus)
                    {
                        var existingMenu = await _dataContext.Menu.FindAsync(menu.Id);
                        if (existingMenu == null)
                        {
                            return (false, $"Menu with ID {menu.Id} not found");
                        }
                    }
                }

                // Update coupon/promo
                existingCouponPromo.Description = couponPromo.Description;
                existingCouponPromo.PromoCode = couponPromo.PromoCode;
                existingCouponPromo.CouponCode = couponPromo.CouponCode;
                existingCouponPromo.PromoAmount = couponPromo.PromoAmount;
                existingCouponPromo.CouponItemQuantity = couponPromo.CouponItemQuantity;
                existingCouponPromo.CouponMenus = couponPromo.CouponMenus;
                existingCouponPromo.ExpirationTime = couponPromo.ExpirationTime;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated {(string.IsNullOrEmpty(couponPromo.PromoCode) ? "coupon" : "promo")}: {couponPromo.Description}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Coupon/Promo updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon/promo");
                return (false, "An error occurred while updating the coupon/promo");
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteCouponPromo(int id, string managerEmail)
        {
            try
            {
                // Validate manager
                var manager = await ValidateManager(managerEmail);
                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get coupon/promo
                var couponPromo = await _dataContext.CouponPromo.FindAsync(id);
                if (couponPromo == null)
                {
                    return (false, "Coupon/Promo not found");
                }

                // Delete coupon/promo
                _dataContext.CouponPromo.Remove(couponPromo);

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deleted {(string.IsNullOrEmpty(couponPromo.PromoCode) ? "coupon" : "promo")}: {couponPromo.Description}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                return (true, "Coupon/Promo deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon/promo");
                return (false, "An error occurred while deleting the coupon/promo");
            }
        }

        public async Task<(bool isSuccess, string message)> ValidateCouponPromo(string code, List<Menu> selectedMenus)
        {
            try
            {
                var couponPromo = await _dataContext.CouponPromo
                    .Include(cp => cp.CouponMenus)
                    .FirstOrDefaultAsync(cp => cp.PromoCode == code || cp.CouponCode == code);

                if (couponPromo == null)
                {
                    return (false, "Invalid code");
                }

                if (couponPromo.ExpirationTime <= DateTimeOffset.UtcNow)
                {
                    return (false, "Code has expired");
                }

                // For promos (percentage discounts), no menu validation needed
                if (!string.IsNullOrEmpty(couponPromo.PromoCode))
                {
                    return (true, "Valid promo code");
                }

                // For coupons, validate selected menus
                if (couponPromo.CouponMenus != null && couponPromo.CouponMenus.Any())
                {
                    var validMenus = selectedMenus
                        .Where(m => couponPromo.CouponMenus.Any(cm => cm.Id == m.Id))
                        .ToList();

                    if (!validMenus.Any())
                    {
                        return (false, "No valid menu items selected for this coupon");
                    }

                    if (validMenus.Count < couponPromo.CouponItemQuantity)
                    {
                        return (false, $"Minimum {couponPromo.CouponItemQuantity} items required for this coupon");
                    }
                }

                return (true, "Valid coupon code");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon/promo");
                return (false, "An error occurred while validating the code");
            }
        }
        #endregion

        #region Helper Methods
        private async Task<User?> ValidateManager(string managerEmail)
        {
            return await _dataContext.User
                .FirstOrDefaultAsync(u => u.UserEmail == managerEmail &&
                                        u.UserRole == UserRole.Manager.ToString() &&
                                        u.IsActive);
        }
        #endregion
    }
}
