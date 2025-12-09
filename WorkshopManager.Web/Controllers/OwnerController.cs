using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        private readonly UserManager<User> _userManager;

        public OwnerController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Clients()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(RoleValue.Client.ToString());

            var model = usersInRole
                .Select(u => new ClientListItemViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            return View(model);
        }
    }
}
