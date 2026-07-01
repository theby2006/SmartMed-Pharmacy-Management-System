using System.Collections.Generic;
using SmartMed.Models.Entities;
using SmartMed.Models.Results;

namespace SmartMed.BLL.Interfaces
{
    public interface IUserService
    {
        OperationResult<List<User>> GetAllUsers();
        OperationResult<User> GetUserById(int id);
        OperationResult<List<User>> SearchUsers(string keyword);
        OperationResult<int> CreateUser(User user, string password);
        OperationResult UpdateUser(User user);
        OperationResult ResetPassword(int userId, string newPassword);
        OperationResult ActivateUser(int userId);
        OperationResult DeactivateUser(int userId);
    }
}
