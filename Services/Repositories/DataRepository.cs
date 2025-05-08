using EBISX_POS.API.Data;
using EBISX_POS.API.Models;
using EBISX_POS.API.Models.Utils;
using EBISX_POS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Services.Repositories
{
    public class DataRepository(DataContext _dataContext, ILogger<DataRepository> _logger) : IData
    {
        public async Task<(bool isSuccess, string message, List<User> users)> AddUser(User user, string? managerEmail = null)
        {
            try
            {
                // Validate user data
                var validationContext = new ValidationContext(user);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
                {
                    return (false, string.Join(", ", validationResults.Select(r => r.ErrorMessage)), new List<User>());
                }

                // Check if user already exists
                if (await _dataContext.User.AnyAsync(u => u.UserEmail == user.UserEmail))
                {
                    return (false, "User with this email already exists", new List<User>());
                }

                // Check if this is the first user
                var isFirstUser = !await _dataContext.User.AnyAsync();
                
                // Validate manager email requirement
                if (!isFirstUser)
                {
                    if (string.IsNullOrWhiteSpace(managerEmail))
                    {
                        return (false, "Manager email is required for adding new users", new List<User>());
                    }

                    // Verify manager exists and has appropriate role
                    var manager = await _dataContext.User
                        .FirstOrDefaultAsync(u => u.UserEmail == managerEmail && 
                                                u.UserRole == UserRole.Manager.ToString() && 
                                                u.IsActive);

                    if (manager == null)
                    {
                        return (false, "Unauthorized: Invalid manager credentials", new List<User>());
                    }
                }
                else if (user.UserRole != UserRole.Manager.ToString())
                {
                    return (false, "First user must be a Manager", new List<User>());
                }

                // Set timestamps
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // Add user
                await _dataContext.User.AddAsync(user);

                // Create user log entry
                var userLog = new UserLog
                {
                    Action = isFirstUser ? "Initial System Setup: Added First User (Manager)" : $"Added New User: {user.UserEmail}",
                    CreatedAt = DateTime.UtcNow
                };

                // If not the first user, add manager to log
                if (!isFirstUser)
                {
                    var manager = await _dataContext.User
                        .FirstOrDefaultAsync(u => u.UserEmail == managerEmail && 
                                                u.UserRole == UserRole.Manager.ToString() && 
                                                u.IsActive);
                    userLog.Manager = manager;
                }

                await _dataContext.UserLog.AddAsync(userLog);
                await _dataContext.SaveChangesAsync();

                // Log the action
                _logger.LogInformation("New user added: {Email} by {Action}", user.UserEmail, userLog.Action);

                // Return updated user list
                var users = await _dataContext.User
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.UserFName)
                    .ThenBy(u => u.UserLName)
                    .ToListAsync();

                return (true, isFirstUser ? "Initial Manager user created successfully" : "User added successfully", users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {Email}", user.UserEmail);
                return (false, "An error occurred while adding the user", new List<User>());
            }
        }

        public async Task<List<User>> GetUsers()
        {
            try
            {
                return await _dataContext.User
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.UserFName)
                    .ThenBy(u => u.UserLName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return new List<User>();
            }
        }

        public async Task<(bool isSuccess, string message)> UpdateUser(User user, string managerEmail)
        {
            try
            {
                // Validate user data
                var validationContext = new ValidationContext(user);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
                {
                    return (false, string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
                }

                // Verify manager exists and has appropriate role
                var manager = await _dataContext.User
                    .FirstOrDefaultAsync(u => u.UserEmail == managerEmail && 
                                            u.UserRole == UserRole.Manager.ToString() && 
                                            u.IsActive);

                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get existing user
                var existingUser = await _dataContext.User
                    .FirstOrDefaultAsync(u => u.UserEmail == user.UserEmail);

                if (existingUser == null)
                {
                    return (false, "User not found");
                }

                // Prevent changing the last manager to cashier
                if (existingUser.UserRole == UserRole.Manager.ToString() && user.UserRole == UserRole.Cashier.ToString())
                {
                    var managerCount = await _dataContext.User
                        .CountAsync(u => u.UserRole == UserRole.Manager.ToString() && u.IsActive);

                    if (managerCount <= 1)
                    {
                        return (false, "Cannot change the last manager to cashier role");
                    }
                }

                // Update user properties
                existingUser.UserFName = user.UserFName;
                existingUser.UserLName = user.UserLName;
                existingUser.UserRole = user.UserRole;
                //existingUser.IsActive = user.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Updated user: {user.UserEmail}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                _logger.LogInformation("User updated: {Email} by manager: {ManagerEmail}", 
                    user.UserEmail, managerEmail);

                return (true, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Email}", user.UserEmail);
                return (false, "An error occurred while updating the user");
            }
        }

        public async Task<(bool isSuccess, string message)> DeactivateUser(string userEmail, string managerEmail)
        {
            try
            {
                // Verify manager exists and has appropriate role
                var manager = await _dataContext.User
                    .FirstOrDefaultAsync(u => u.UserEmail == managerEmail && 
                                            u.UserRole == UserRole.Manager.ToString() && 
                                            u.IsActive);

                if (manager == null)
                {
                    return (false, "Unauthorized: Invalid manager credentials");
                }

                // Get user to deactivate
                var user = await _dataContext.User
                    .FirstOrDefaultAsync(u => u.UserEmail == userEmail);

                if (user == null)
                {
                    return (false, "User not found");
                }

                // Check if user is already deactivated
                if (!user.IsActive)
                {
                    return (false, "User is already deactivated");
                }

                // Prevent deactivating the last manager
                if (user.UserRole == UserRole.Manager.ToString())
                {
                    var managerCount = await _dataContext.User
                        .CountAsync(u => u.UserRole == UserRole.Manager.ToString() && u.IsActive);

                    if (managerCount <= 1)
                    {
                        return (false, "Cannot deactivate the last manager");
                    }
                }

                // Deactivate user and update timestamp
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                // Log the action
                _dataContext.UserLog.Add(new UserLog
                {
                    Manager = manager,
                    Action = $"Deactivated user: {userEmail}",
                    CreatedAt = DateTime.UtcNow
                });

                await _dataContext.SaveChangesAsync();
                _logger.LogInformation("User deactivated: {Email} by manager: {ManagerEmail}", 
                    userEmail, managerEmail);

                return (true, "User deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {Email}", userEmail);
                return (false, "An error occurred while deactivating the user");
            }
        }
    }
}
