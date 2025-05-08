using EBISX_POS.API.Helper;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Utils;
using EBISX_POS.API.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EBISX_POS.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            try
            {
                Debug.WriteLine("Starting database seeding...");
                using (var context = new DataContext(
                    serviceProvider.GetRequiredService<DbContextOptions<DataContext>>()))
                {
                    // Get FilePaths configuration from DI container
                    var filePaths = serviceProvider.GetRequiredService<IOptions<FilePaths>>().Value;
                    
                    // If paths are empty, set default paths
                    if (string.IsNullOrEmpty(filePaths.ImagePath))
                    {
                        filePaths.ImagePath = Path.Combine(AppContext.BaseDirectory, "Images");
                        Debug.WriteLine($"Setting default ImagePath: {filePaths.ImagePath}");
                    }
                    
                    if (string.IsNullOrEmpty(filePaths.BackUp))
                    {
                        filePaths.BackUp = Path.Combine(AppContext.BaseDirectory, "Backups");
                        Debug.WriteLine($"Setting default BackUp: {filePaths.BackUp}");
                    }

                    // Ensure paths are absolute
                    filePaths.ImagePath = Path.GetFullPath(filePaths.ImagePath);
                    filePaths.BackUp = Path.GetFullPath(filePaths.BackUp);

                    Debug.WriteLine($"Final paths:");
                    Debug.WriteLine($"ImagePath: {filePaths.ImagePath}");
                    Debug.WriteLine($"BackUp: {filePaths.BackUp}");

                    // Ensure directories exist
                    if (!Directory.Exists(filePaths.ImagePath))
                    {
                        Debug.WriteLine($"Creating image directory: {filePaths.ImagePath}");
                        Directory.CreateDirectory(filePaths.ImagePath);
                    }

                    if (!Directory.Exists(filePaths.BackUp))
                    {
                        Debug.WriteLine($"Creating backup directory: {filePaths.BackUp}");
                        Directory.CreateDirectory(filePaths.BackUp);
                    }

                    // Only seed if the database is completely empty
                    if (!context.Database.CanConnect() || !context.User.Any())
                    {
                        Debug.WriteLine("Database is empty, starting seeding process...");

                        var users = new User[]
                        {
                            new User { UserEmail = "user1@example.com", UserFName = "John", UserLName = "Doe", UserRole = "Manager" },
                            new User { UserEmail = "user2@example.com", UserFName = "Jane", UserLName = "Doe", UserRole = "Cashier" },
                            new User { UserEmail = "user3@example.com", UserFName = "Alice", UserLName = "Smith", UserRole = "Manager" },
                            new User { UserEmail = "user4@example.com", UserFName = "Bob", UserLName = "Brown", UserRole = "RGM" },
                            new User { UserEmail = "user5@example.com", UserFName = "Charlie", UserLName = "Davis", UserRole = "Cashier" },
                            new User { UserEmail = "user7@example.com", UserFName = "Eve", UserLName = "Foster", UserRole = "Cashier" },
                            new User { UserEmail = "user8@example.com", UserFName = "Frank", UserLName = "Green", UserRole = "Cashier" },
                            new User { UserEmail = "user9@example.com", UserFName = "Grace", UserLName = "Hill", UserRole = "Cashier" },
                            new User { UserEmail = "user10@example.com", UserFName = "Hank", UserLName = "Ivy", UserRole = "Manager" }
                        };
                        context.User.AddRange(users);
                        Debug.WriteLine("Added users to database");

                        var drinkTypes = new DrinkType[] {
                            new DrinkType { DrinkTypeName = "Hot" },
                            new DrinkType { DrinkTypeName = "Cold" },
                        };
                        context.DrinkType.AddRange(drinkTypes);
                        Debug.WriteLine("Added drink types to database");

                        var saleTypes = new SaleType[]
                        {
                            new SaleType { Name = "GCASH", Account = "A/R - GCASH", Type = "CHARGE" },
                            new SaleType { Name = "PAYMAYA", Account = "A/R - PAYMAYA", Type = "CHARGE" },
                            new SaleType { Name = "FOOD PANDA", Account = "A/R - FOOD PANDA", Type = "CHARGE" },
                            new SaleType { Name = "GRAB", Account = "A/R - FOOD PANDA", Type = "CHARGE" },
                            new SaleType { Name = "GIFT CHEQUE", Account = "A/R - PRODUCT GC", Type = "CHARGE" },
                            new SaleType { Name = "DEBIT", Account = "A/R - DEBIT", Type = "CHARGE" },
                            new SaleType { Name = "CREDIT", Account = "A/R - CREDIT", Type = "CHARGE" },
                        };

                        context.SaleType.AddRange(saleTypes);
                        Debug.WriteLine("Added sale types to database");

                        var addOnTypes = new AddOnType[]
                        {
                            new AddOnType{ AddOnTypeName = "Sides" },
                            new AddOnType{ AddOnTypeName = "Desserts" },
                        };
                        context.AddOnType.AddRange(addOnTypes);
                        Debug.WriteLine("Added add-on types to database");

                        var categories = new Category[]
                        {
                            new Category { CtgryName = "Burger" },
                            new Category { CtgryName = "Spaghetti" },
                            new Category { CtgryName = "Chickensad" },
                            new Category { CtgryName = "Sandwich" },
                            new Category { CtgryName = "Drinks" },
                            new Category { CtgryName = "Desserts" },
                            new Category { CtgryName = "Fries" },
                        };
                        context.Category.AddRange(categories);
                        Debug.WriteLine("Added categories to database");

                        // Save changes to get IDs for categories
                        await context.SaveChangesAsync();
                        Debug.WriteLine("Saved initial data to get IDs");

                        var menus = new Menu[]
                        {
                            // Burgers
                            new Menu { MenuName = "Cheeseburger", MenuPrice = 149.00m, Category = categories[0], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Cheeseburger", MenuPrice = 159.00m, Category = categories[0] },
                            new Menu { MenuName = "Burger Ka Sakin", MenuPrice = 149.00m, Category = categories[0], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Burger Ka Sakin", MenuPrice = 159.00m, Category = categories[0] },
                            new Menu { MenuName = "Cheese", MenuPrice = 25.00m, Category = categories[0], IsAddOn = true }, // Addon
                            new Menu { MenuName = "Bacon", MenuPrice = 35.00m, Category = categories[0], IsAddOn = true },  // Addon

                            // Spaghetti
                            new Menu { MenuName = "Spaghetti Bolognese", MenuPrice = 179.00m, Category = categories[1], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Spaghetti Bolognese", MenuPrice = 189.00m, Category = categories[1] },
                            new Menu { MenuName = "Spaghetti", MenuPrice = 159.00m, Category = categories[1], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Spaghetti", MenuPrice = 169.00m, Category = categories[1] },
                            new Menu { MenuName = "Spaghetti w/ Chickensad", MenuPrice = 159.00m, Category = categories[1], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Spaghetti w/ Chickensad", MenuPrice = 169.00m, Category = categories[1] },

                            // Chickensad
                            new Menu { MenuName = "Chickensad", MenuPrice = 159.00m, Category = categories[2], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Chickensad", MenuPrice = 169.00m, Category = categories[2] },
                            new Menu { MenuName = "Chicken Sandwich", MenuPrice = 159.00m, Category = categories[2], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Chicken Sandwich", MenuPrice = 169.00m, Category = categories[2] },
                            new Menu { MenuName = "Grilled Chicken", MenuPrice = 249.00m, Category = categories[2], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Grilled Chicken", MenuPrice = 259.00m, Category = categories[2] },
                            new Menu { MenuName = "Rice", MenuPrice = 35.00m, Category = categories[2], HasDrink = false, HasAddOn = false },

                            // Sandwich
                            new Menu { MenuName = "Club Sandwich", MenuPrice = 159.00m, Category = categories[3], HasDrink = false, HasAddOn = false },
                            new Menu { MenuName = "Club Sandwich", MenuPrice = 169.00m, Category = categories[3] },

                            // Drinks
                            new Menu { MenuName = "Coke", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Coke", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Coke", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Sprite", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Sprite", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Sprite", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Royal", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Royal", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Royal", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Tea", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Tea", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Tea", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[1], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Kape Letse", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Kape Letse", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Kape Letse", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Pinaig na Mais", MenuPrice = 55.00m, Category = categories[4], Size = MenuSize.R.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Pinaig na Mais", MenuPrice = 65.00m, Category = categories[4], Size = MenuSize.M.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },
                            new Menu { MenuName = "Pinaig na Mais", MenuPrice = 75.00m, Category = categories[4], Size = MenuSize.L.ToString(), DrinkType = drinkTypes[0], HasDrink = false, HasAddOn = false, IsAddOn = true },

                            // Desserts
                            new Menu { MenuName = "Ice Cream", MenuPrice = 75.00m, Category = categories[5], Size = MenuSize.R.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },
                            new Menu { MenuName = "Ice Cream", MenuPrice = 85.00m, Category = categories[5], Size = MenuSize.M.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },
                            new Menu { MenuName = "Ice Cream", MenuPrice = 95.00m, Category = categories[5], Size = MenuSize.L.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },
                            new Menu { MenuName = "Durian Pie", MenuPrice = 75.00m, Category = categories[5], Size = MenuSize.R.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },
                            new Menu { MenuName = "Durian Pie", MenuPrice = 85.00m, Category = categories[5], Size = MenuSize.M.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },
                            new Menu { MenuName = "Durian Pie", MenuPrice = 95.00m, Category = categories[5], Size = MenuSize.L.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[1] },

                            // Fries
                            new Menu { MenuName = "Rice", MenuPrice = 35.00m, Category = categories[6], HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[0] },
                            new Menu { MenuName = "Bisaya Fries", MenuPrice = 79.00m, Category = categories[6], Size = MenuSize.R.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[0] },
                            new Menu { MenuName = "Bisaya Fries", MenuPrice = 99.00m, Category = categories[6], Size = MenuSize.M.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[0] },
                            new Menu { MenuName = "Bisaya Fries", MenuPrice = 119.00m, Category = categories[6], Size = MenuSize.L.ToString(), HasDrink = false, HasAddOn = false, IsAddOn = true, AddOnType = addOnTypes[0] },
                        };

                        // Download and set images for each menu item
                        foreach (var menu in menus)
                        {
                            try
                            {
                                Debug.WriteLine($"Downloading image for menu item: {menu.MenuName}");
                                menu.MenuImagePath = await ImageHelper.DownloadAndSaveImageAsync(
                                    "https://ebisx.com/assets/img/items/1700209804370249939_701199564837023_606470247253043161_n.png",
                                    filePaths.ImagePath
                                );
                                Debug.WriteLine($"Successfully set image path for {menu.MenuName}: {menu.MenuImagePath}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error downloading image for {menu.MenuName}: {ex.Message}");
                                // Set a default image path
                                menu.MenuImagePath = Path.Combine(filePaths.ImagePath, "default_menu.png");
                                Debug.WriteLine($"Set default image path for {menu.MenuName}: {menu.MenuImagePath}");
                            }
                        }

                        context.Menu.AddRange(menus);
                        Debug.WriteLine("Added menus to database");

                        var couponPromos = new CouponPromo[]
                        {
                            // Promo type entries: percentage discount (values remain as percentages)
                            new CouponPromo
                            {
                                Description = "Summer Sale: 15% off on all orders",
                                PromoCode = "SUMMER15",
                                CouponCode = null,
                                PromoAmount = 15.0m,
                                CouponMenus = null, // No menus for promo type
                                ExpirationTime = DateTimeOffset.UtcNow.AddMonths(1),
                            },
                            new CouponPromo
                            {
                                Description = "Winter Discount: 10% off storewide",
                                PromoCode = "WINTER10",
                                CouponCode = null,
                                PromoAmount = 10.0m,
                                CouponMenus = null,
                                ExpirationTime = DateTimeOffset.UtcNow.AddMonths(2),
                            },
                            new CouponPromo
                            {
                                Description = "Spring Promo: 20% off for new customers",
                                PromoCode = "SPRING20",
                                CouponCode = null,
                                PromoAmount = 20.0m,
                                CouponMenus = null,
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(45),
                            },
                            new CouponPromo
                            {
                                Description = "Exclusive Online Promo: 25% off on select items",
                                PromoCode = "ONLINE25",
                                CouponCode = null,
                                PromoAmount = 25.0m,
                                CouponMenus = null,
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
                            },
                            new CouponPromo
                            {
                                Description = "Limited Time Promo: 5% off your purchase",
                                PromoCode = "LIMITED5",
                                CouponCode = null,
                                PromoAmount = 5.0m,
                                CouponMenus = null,
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(10),
                            },

                            // Coupon type entries: fixed discount amounts converted to PHP values
                            new CouponPromo
                            {
                                Description = "Free Cheeseburger Coupon",
                                PromoCode = null,
                                CouponCode = "FREE_CHEESE",
                                PromoAmount = 149.00m, // Converted from 4.99 * 30
                                CouponItemQuantity = 6,
                                CouponMenus = new List<Menu> { menus[0] },
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(20),
                            },
                            new CouponPromo
                            {
                                Description = "Discount Coupon: ?30 off on Cheeseburger (second price)",
                                PromoCode = null,
                                CouponCode = "DISC_CHEESE",
                                PromoAmount = 30.00m, // Converted from 1.00 * 30
                                CouponItemQuantity = 3,
                                CouponMenus = new List<Menu> { menus[1] },
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(25),
                            },
                            new CouponPromo
                            {
                                Description = "Combo Coupon: Burger Ka Sakin with free add-on",
                                PromoCode = null,
                                CouponCode = "BKS_COUPON",
                                PromoAmount = 75.00m, // Converted from 2.50 * 30
                                CouponItemQuantity = 1,
                                CouponMenus = new List<Menu> { menus[2], menus[3] },
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
                            },
                            new CouponPromo
                            {
                                Description = "Discount Coupon: ?45 off on Bacon",
                                PromoCode = null,
                                CouponCode = "BACON_SAVE",
                                CouponItemQuantity = 2,
                                PromoAmount = 45.00m, // Converted from 1.49 * 30 (rounded)
                                CouponMenus = new List<Menu> { menus[5], menus[3] },
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(35),
                            },
                            new CouponPromo
                            {
                                Description = "Special Coupon: Free Club Sandwich",
                                PromoCode = null,
                                CouponCode = "FREE_CLUB",
                                CouponItemQuantity = 2,
                                PromoAmount = 179.00m, // Converted from 5.99 * 30 (rounded)
                                CouponMenus = new List<Menu> { menus[18], menus[19] },
                                ExpirationTime = DateTimeOffset.UtcNow.AddDays(40),
                            }
                        };

                        context.CouponPromo.AddRange(couponPromos);
                        Debug.WriteLine("Added coupon promos to database");

                        await context.SaveChangesAsync();
                        Debug.WriteLine("Database seeding completed successfully");
                    }
                    else
                    {
                        Debug.WriteLine("Database already contains data, skipping seeding");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database seeding: {ex}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
