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
                .Include(o => o.AdditionalCosts)
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
                EstimatedCost = order.EntryEstimatedCost,
                VatRate = order.VatRate,
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

            // Sprawdzenie czy wycena się zmieniła
            bool estimateChanged = order.EntryEstimatedCost != model.EstimatedCost ||
                                   order.VatRate != model.VatRate;

            // przypisanie mechanika
            order.MechanicId = model.SelectedMechanicId;

            // zapisanie wyceny
            order.EntryEstimatedCost = model.EstimatedCost;
            order.VatRate = model.VatRate;

            // Zmiana statusu na "Do akceptacji" w następujących przypadkach:
            // 1. Zlecenie jest "Utworzone" - pierwsza wycena
            // 2. Zlecenie jest "Anulowane" - rewycena po odrzuceniu
            // 3. Zlecenie jest "Zaakceptowane" ale wycena się zmieniła - rewycena
            // 4. Zlecenie jest "Do akceptacji" i wycena się zmieniła - aktualizacja wyceny
            if (order.Status == RepairOrderStatusValue.Created ||
                order.Status == RepairOrderStatusValue.Cancelled ||
                (order.Status == RepairOrderStatusValue.Approved && estimateChanged) ||
                (order.Status == RepairOrderStatusValue.PendingApproval && estimateChanged))
            {
                order.Status = RepairOrderStatusValue.PendingApproval;
            }

            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Formularz dodawania kosztów dodatkowych
        [HttpGet]
        public IActionResult AddAdditionalCost(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.AdditionalCosts.OrderBy(ac => ac.AddedDate))
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Koszty dodatkowe można dodać tylko do zleceń z wycenowaną naprawą
            if (!order.EntryEstimatedCost.HasValue || !order.VatRate.HasValue)
            {
                TempData["Error"] = "Nie można dodać kosztów dodatkowych do zlecenia bez wyceny.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AddAdditionalCostVM
            {
                OrderId = order.Id,
                ClientFullName = $"{order.Client.FirstName} {order.Client.LastName}",
                RegistrationNumber = order.RegistrationNumber,
                IssueDescription = order.EntryIssueDescription,
                Status = order.Status,
                OriginalEstimatedCost = order.EntryEstimatedCost,
                VatRate = order.VatRate
            };

            ViewBag.ExistingCosts = order.AdditionalCosts;

            return View(model);
        }

        // Zapis kosztów dodatkowych
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdditionalCost(AddAdditionalCostVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var order = _db.RepairOrders
                .FirstOrDefault(o => o.Id == model.OrderId);

            if (order == null)
            {
                return NotFound();
            }

            // Utworzenie nowego kosztu dodatkowego
            var additionalCost = new AdditionalCost
            {
                RepairOrderId = order.Id,
                Cost = model.AdditionalCost!.Value,
                Description = model.AdditionalCostDescription!,
                AddedDate = DateTime.Now,
                IsAccepted = false
            };

            _db.AdditionalCosts.Add(additionalCost);

            // Jeśli zlecenie było zaakceptowane, wymaga ponownej akceptacji kosztów dodatkowych
            if (order.Status == RepairOrderStatusValue.Approved ||
                order.Status == RepairOrderStatusValue.InProgress)
            {
                order.Status = RepairOrderStatusValue.PendingApproval;
            }

            _db.SaveChanges();

            TempData["Success"] = "Koszty dodatkowe zostały dodane. Klient musi je zaakceptować.";
            return RedirectToAction(nameof(Index));
        }
    }
}
