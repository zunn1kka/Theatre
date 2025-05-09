using Theatre.Core.Models;

namespace Theatre.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserById(int userId);
        Task<decimal> GetBalance(int userId);
        Task AddToBalance(int userId, decimal amount);
    }
}
