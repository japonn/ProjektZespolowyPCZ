using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels;


namespace WorkshopManager.Web.Areas.Identity.Pages.Account
{
    public class RegisterClientModel : PageModel
    {
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly IUserStore<User> _userStore;
		private readonly IUserEmailStore<User> _emailStore;
		private readonly ILogger<RegisterClientModel> _logger;

		public RegisterClientModel(
			UserManager<User> userManager,
			RoleManager<Role> roleManager,
			IUserStore<User> userStore,
			SignInManager<User> signInManager,
			ILogger<RegisterClientModel> logger)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_userStore = userStore;
			_emailStore = GetEmailStore();
			_signInManager = signInManager;
			_logger = logger;
		}

		[BindProperty]
		public RegisterClientViewModel Input { get; set; } = new RegisterClientViewModel();
        public void OnGet()
        {
			
        }

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			// Sprawdzenie czy email już istnieje
			var existingUser = await _userManager.FindByEmailAsync(Input.Email);
			if (existingUser != null)
			{
				ModelState.AddModelError(string.Empty, "Użytkownik z tym adresem email już istnieje.");
				return Page();
			}

			// Utworzenie roli Client jeśli nie istnieje
			if (!await _roleManager.RoleExistsAsync(RoleValue.Client.ToString()))
			{
				var clientRole = new Role { Name = RoleValue.Client.ToString() };
				await _roleManager.CreateAsync(clientRole);
				_logger.LogInformation("Utworzono rolę Client");
			}

			// Utworzenie użytkownika
			var client = CreateClient();
			client.FirstName = Input.FirstName;
			client.LastName = Input.LastName;
			client.PhoneNumber = Input.PhoneNumber;

			await _userStore.SetUserNameAsync(client, Input.Email, CancellationToken.None);
			await _emailStore.SetEmailAsync(client, Input.Email, CancellationToken.None);

			var result = await _userManager.CreateAsync(client, Input.Password);

			if (result.Succeeded)
			{
				_logger.LogInformation("Utworzono konto użytkownika: {Email}", Input.Email);

				// Przypisanie roli Client
				await _userManager.AddToRoleAsync(client, RoleValue.Client.ToString());

				// Automatyczne zalogowanie
				await _signInManager.SignInAsync(client, isPersistent: false);

				return RedirectToPage("/ClientPages/Index");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return Page();
		}

		private Model.DataModels.Client CreateClient()
		{
			try
			{
				return Activator.CreateInstance<Model.DataModels.Client>();
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
