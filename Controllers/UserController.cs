using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaHotOnion.DTOs;
using PizzaHotOnion.Entities;
using PizzaHotOnion.Infrastructure;
using PizzaHotOnion.Infrastructure.Security;
using PizzaHotOnion.Repositories;
using PizzaHotOnion.Services;

namespace PizzaHotOnion.Controllers
{
  [BusinessExceptionFilter]
  [Produces("application/json")]
  [Route("api/[controller]")]
  [Authorize]
  public class UserController : Controller
  {
    private readonly IAuthenticationService authenticationService;
    private readonly IEmailService emailService;

    public UserController(IAuthenticationService authenticationService, IEmailService emailService)
    {
      this.authenticationService = authenticationService;
      this.emailService = emailService;
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO)
    {
      await this.authenticationService.SignUp(registerUserDTO);
      return NoContent();
    }

    [HttpPost("changepassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
    {
      await this.authenticationService.ChangePassword(changePasswordDTO);
      return NoContent();
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserProfile(string email)
    {
      if (string.IsNullOrEmpty(email))
        return BadRequest("Cannot get user profile because e-mail is empty");

      var userProfile = await this.authenticationService.GetUserProfileByEmail(email);
      if (userProfile == null)
        return NotFound("Cannot get user profile because user does not exist");

      return new ObjectResult(userProfile);
    }

    [HttpPut("profile/{email}")]
    public async Task<IActionResult> UpdateUserProfile(string email, [FromBody] UserProfileDTO userProfileDTO)
    {
      await this.authenticationService.UpdateUserProfile(userProfileDTO);
      return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestDTO request)
    {
      if (request == null || string.IsNullOrWhiteSpace(request.Email))
      {
        return BadRequest("Cannot reset password");
      }

      var userProfile = await this.authenticationService.GetUserProfileByEmail(request.Email);
      if (userProfile == null)
        return BadRequest("Cannot reset password");

      string code = Guid.NewGuid().ToString(); ;
      await this.authenticationService.SaveResetCode(request.Email, code);
      var baseUrl = string.Format("{0}://{1}", this.Request.Scheme, this.Request.Host.Value.ToString());
      string bodyTemplate =
@"Someone (possibly you) have just requested resetting password. If you want to reset your password please visit the link (valid for the next 15 min)
{0}/api/user/resetpassword/email/{1}/code/{2}

Otherwise you may ignore this email.
";

      this.emailService.Send(request.Email,
           "Hot Onion - Reset password request",
           string.Format(bodyTemplate,
             baseUrl,
             request.Email,
             code));

      return NoContent();

    }

    [AllowAnonymous]
    [HttpGet("resetpassword/email/{email}/code/{code}")]
    [Produces("text/html")]
    public async Task ResetPasswordAction(string email, string code)
    {
      //TODO: this method writes directly to the response without any HTML and CSS code. Later on could be rewritten or redirected to frontend
      if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
      {
        await WriteBadRequest("Cannot reset password - invalid data");
        return;
      }

      var userProfile = await this.authenticationService.GetUserProfileByEmail(email);
      if (userProfile == null)
      {
        await WriteBadRequest("Cannot reset password - user not exists");
        return;
      }

      var newPassword = GeneratePassword();

      var result = await this.authenticationService.SetNewPassword(email, code, newPassword);
      if (result)
      {
        var baseUrl = string.Format("{0}://{1}/login", this.Request.Scheme, this.Request.Host.Value.ToString());
        string body = @"Your password has been changed. Please login to your account and change it.
New password: {0}
URL: {1}
";
        this.emailService.Send(email, "Hot Onion - Password changed!", string.Format(body, newPassword, baseUrl));

        var bytes = Encoding.UTF8.GetBytes("Your new password has been sent to your e-mail. Please check your mailbox.");
        Response.ContentType = "text/html";
        Response.StatusCode = 200;
        Response.ContentLength = bytes.Length;
        await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
      }
      else
      {
        var bytes = Encoding.UTF8.GetBytes("Reset password FAILED. The code was expired? Please try again.");
        Response.ContentType = "text/html";
        Response.StatusCode = 200;
        Response.ContentLength = bytes.Length;
        await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);

      }
    }

    private async Task WriteBadRequest(string message)
    {
      var bytes = Encoding.UTF8.GetBytes(message);
      Response.ContentType = "text/html";
      Response.StatusCode = 400;
      Response.ContentLength = bytes.Length;
      await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);

    }

    private string GeneratePassword()
    {
      const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
      StringBuilder res = new StringBuilder();
      Random rnd = new Random();
      var length = 10;
      while (0 < length--)
      {
        res.Append(valid[rnd.Next(valid.Length)]);
      }
      return res.ToString();

    }
  }
}
