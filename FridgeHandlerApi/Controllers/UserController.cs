using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using FridgeHandler.Data.Models;
using FridgeHandlerApi.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FridgeHandlerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.PhoneNumber
            });
        }

        
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            // Check old password
            if (!await _userManager.CheckPasswordAsync(user, request.OldPassword))
                return BadRequest("Current password is incorrect.");
            
            if (request.NewPassword.Length < 8 || 
                !request.NewPassword.Any(char.IsDigit) ||
                !request.NewPassword.Any(char.IsUpper))
            {
                return BadRequest("Password must be at least 8 characters long and include an uppercase letter and a number.");
            }
            
            // Update to new password
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            await SendPasswordChangeConfirmationEmail(user.Email);
            return Ok(new { message = "Password updated successfully" });
        }

        private async Task SendPasswordChangeConfirmationEmail(string email)
        {
            var smtpClient = new SmtpClient("smtp.yourprovider.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("your@email.com", "yourEmailPassword"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your@email.com", "AI Fridge Handler"),
                Subject = "Your Password Was Changed",
                Body = $"Hi,\n\nYour password was successfully changed on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.\n\nIf this wasn’t you, please contact support immediately.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            // Update username
            if (!string.IsNullOrWhiteSpace(request.Username))
                user.UserName = request.Username;

            // Update password (optional)
            IdentityResult passResult = IdentityResult.Success;
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                passResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded || !passResult.Succeeded)
            {
                var errors = updateResult.Errors.Concat(passResult.Errors).Select(e => e.Description);
                return BadRequest(new { message = "Failed to update account", errors });
            }

            return Ok(new { message = "Account updated successfully" });
        }
    }
}