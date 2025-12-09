using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels;
using Microsoft.EntityFrameworkCore;


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

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var mechanics = await _context.Mechanics
            .Select(m => new MechanicCreateViewModel
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName
            })
            .ToListAsync();
            return View(mechanics);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mechanic = await _context.Mechanics.FindAsync(id);
            if (mechanic == null)
            {
                return NotFound();
            }

            var model = new MechanicCreateViewModel
            {
                Id = mechanic.Id,
                FirstName = mechanic.FirstName,
                LastName = mechanic.LastName
            };

                return View(model);
        }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(MechanicCreateViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var mechanic = await _context.Mechanics.FindAsync(model.Id);
                if (mechanic == null)
                {
                    return NotFound();
                }

                mechanic.FirstName = model.FirstName;
                mechanic.LastName = model.LastName;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(List));
            }

            [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
                var mechanic = await _context.Mechanics.FindAsync(id);
                if (mechanic == null)
                {
                    return NotFound();
                }
            
                var model = new MechanicCreateViewModel
                {
                    Id = mechanic.Id,
                    FirstName = mechanic.FirstName,
                    LastName = mechanic.LastName
                };
            
                return View(model);
            }
            
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(MechanicCreateViewModel model)
            {
                var mechanic = await _context.Mechanics.FindAsync(model.Id);
                if (mechanic == null)
                {
                    return NotFound();
                }
            
                _context.Mechanics.Remove(mechanic);
                await _context.SaveChangesAsync();
            
                return RedirectToAction(nameof(List));
            }

    }
}
