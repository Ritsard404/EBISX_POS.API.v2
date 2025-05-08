using EBISX_POS.API.Models;

namespace EBISX_POS.API.Services.Interfaces
{
    /// <summary>
    /// Interface for data operations related to users and other entities
    /// </summary>
    public interface IData
    {
        /// <summary>
        /// Adds a new user to the system
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <returns>A tuple containing success status, message, and the list of users</returns>
        Task<(bool isSuccess, string message, List<User> users)> AddUser(User user, string? managerEmail);

        /// <summary>
        /// Retrieves all active users from the system
        /// </summary>
        /// <returns>A list of active users</returns>
        Task<List<User>> GetUsers();

        /// <summary>
        /// Updates an existing user's information
        /// </summary>
        /// <param name="user">The user with updated information</param>
        /// <param name="managerEmail">The email of the manager performing the update</param>
        /// <returns>A tuple containing success status and message</returns>
        Task<(bool isSuccess, string message)> UpdateUser(User user, string managerEmail);

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        /// <param name="userEmail">The email of the user to deactivate</param>
        /// <param name="managerEmail">The email of the manager performing the deactivation</param>
        /// <returns>A tuple containing success status and message</returns>
        Task<(bool isSuccess, string message)> DeactivateUser(string userEmail, string managerEmail);
    }
}
