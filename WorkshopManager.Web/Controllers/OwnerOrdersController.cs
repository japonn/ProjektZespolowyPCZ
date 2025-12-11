using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels.RepairOrders;

namespace WorkshopManager.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OwnerOrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Lista wszystkich zleceń klientów
        public IActionResult Index()
        {
            var orders = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .OrderByDescending(o => o.SubmissionDate)
                .ToList();

            return View(orders);
        }

        // Formularz przydzielania zlecenia do mechanika
        [HttpGet]
        public IActionResult Assign(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var mechanics = _db.Mechanics
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToList();

            var model = new AssignMechanicVM
            {
                OrderId = order.Id,
                SelectedMechanicId = order.MechanicId,
                ClientFullName = $"{order.Client.FirstName} {order.Client.LastName}",
                RegistrationNumber = order.RegistrationNumber,
                IssueDescription = order.EntryIssueDescription,
                Status = order.Status,
                Mechanics = mechanics
                    .Select(m => new MechanicListItemVM
                    {
                        Id = m.Id,
                        FullName = $"{m.FirstName} {m.LastName}"
                    })
                    .ToList()
            };

            return View(model);
        }

        // Zapis przydziału + start naprawy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Assign(AssignMechanicVM model)
        {
            if (!ModelState.IsValid)
            {
                // ponowne załadowanie listy mechaników przy błędach walidacji
                var mechanics = _db.Mechanics
                    .OrderBy(m => m.LastName)
                    .ThenBy(m => m.FirstName)
                    .ToList();

                model.Mechanics = mechanics
                    .Select(m => new MechanicListItemVM
                    {
                        Id = m.Id,
                        FullName = $"{m.FirstName} {m.LastName}"
                    })
                    .ToList();

                return View(model);
            }

            var order = _db.RepairOrders
                .FirstOrDefault(o => o.Id == model.OrderId);

            if (order == null)
            {
                return NotFound();
            }

            // przypisanie mechanika
            order.MechanicId = model.SelectedMechanicId;

            // rozpoczęcie naprawy – ustawiamy status i datę startu
            if (order.Status == RepairOrderStatusValue.PendingEstimate)
            {
                order.Status = RepairOrderStatusValue.InProgress;
                order.StartDate = DateTime.Now; // dla uproszczenia lokalny czas
            }

            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
