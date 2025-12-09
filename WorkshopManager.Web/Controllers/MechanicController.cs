using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class MechanicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MechanicController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MechanicCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MechanicCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mechanic = new Mechanic
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                BankAccountNumber = string.Empty,
                EmploymentDate = DateTime.Now
            };

            _context.Mechanics.Add(mechanic);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Owner");
        }
    }
}
