using System;
using System.Threading.Tasks;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Services
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterAsync(RegisterRequest request);
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<UserDto?> GetByIdAsync(Guid id);
    }
}
