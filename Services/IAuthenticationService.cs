using System.Threading.Tasks;
using PizzaHotOnion.DTOs;

namespace PizzaHotOnion.Services
{
  public interface IAuthenticationService
  {
    Task<bool> SignIn(string login, string password);
    Task SignUp(RegisterUserDTO registerUserDTO);
    Task ChangePassword(ChangePasswordDTO changePasswordDTO);
    Task<UserProfileDTO> GetUserProfileByEmail(string email);
    Task UpdateUserProfile(UserProfileDTO userProfileDTO);
    Task SaveResetCode(string email, string code);
    Task<bool> SetNewPassword(string email, string code, string newPassword);
  }
}
