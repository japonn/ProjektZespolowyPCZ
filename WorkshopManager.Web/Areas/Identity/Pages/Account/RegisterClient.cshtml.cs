using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;


namespace WorkshopManager.Web.Areas.Identity.Pages.Account
{
    public class RegisterClientModel : PageModel
    {
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly IUserStore<User> _userStore;
		private readonly IUserEmailStore<User> _emailStore;
		private readonly ILogger<RegisterModel> _logger;
		private readonly IEmailSender _emailSender;

		public RegisterClientModel(
			UserManager<User> userManager,
			IUserStore<User> userStore,
			SignInManager<User> signInManager,
			ILogger<RegisterModel> logger,
			IEmailSender emailSender)
		{
			_userManager = userManager;
			_userStore = userStore;
			_emailStore = GetEmailStore();
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public class InputModel
		{
			[Required]
			[EmailAddress]
			[Display(Name = "Email")]
			public string Email { get; set; } = String.Empty;

			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "Password")]
			public string Password { get; set; } = String.Empty;

			[DataType(DataType.Password)]
			[Display(Name = "Confirm password")]
			[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
			public string ConfirmPassword { get; set; } = String.Empty;
		}
        public void OnGet()
        {
			
        }

		public async Task<IActionResult> OnPostAsync()
		{
			if(ModelState.IsValid)
			{
				var client = CreateClient();
				await _userStore.SetUserNameAsync(client, Input.Email, CancellationToken.None);
				await _emailStore.SetEmailAsync(client, Input.Email, CancellationToken.None);
				var result = _userManager.CreateAsync(client, Input.Password).Result;

				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(client, RoleValue.Client.ToString());
					await _emailSender.SendEmailAsync(Input.Email, "WorkshopManager registration", "Your account was successfully registered !");
					await _signInManager.SignInAsync(client, isPersistent: false);
					return RedirectToAction("Index", "ClientRepair");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return Page();
		}

		private Client CreateClient()
		{
			try
			{
				return Activator.CreateInstance<Client>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
					$"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
					$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
			}
		}

		private IUserEmailStore<User> GetEmailStore()
		{
			if (!_userManager.SupportsUserEmail)
			{
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<User>)_userStore;
		}
	}
}
