using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels.RepairOrders;

namespace WorkshopManager.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class CustomerOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerOrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // FORMULARZ
        public IActionResult New()
        {
            return View(new CreateOrderVM());
        }

        // PRZYJĘCIE FORMULARZA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(CreateOrderVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);
        
            // Pobranie ID zalogowanego użytkownika
            var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
            if (string.IsNullOrEmpty(loggedUserId))
            {
                return Unauthorized();
            }
        
            int clientId = int.Parse(loggedUserId);
        
            var order = new RepairOrder
            {
                ClientId = clientId,
                EntryIssueDescription = vm.IssueDescription,
                RegistrationNumber = vm.PlateNumber,
                SubmissionDate = DateTime.Now,
                Status = RepairOrderStatusValue.Created
            };
        
            _db.RepairOrders.Add(order);
            _db.SaveChanges();
        
            return RedirectToAction("MyOrders");

        }

        // PODGLĄD WYSŁANYCH ZGŁOSZEŃ
        public IActionResult MyOrders()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid == null) return Unauthorized();
            int userId = int.Parse(uid);
            var orders = _db.RepairOrders
                .Where(o => o.ClientId == userId)
                .OrderByDescending(o => o.SubmissionDate)
                .ToList();

            return View(orders);
        }

        // SZCZEGÓŁY ZLECENIA
        public IActionResult Details(int id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid == null) return Unauthorized();
            int userId = int.Parse(uid);

            var order = _db.RepairOrders
                .Include(o => o.Mechanic)
                .Include(o => o.AdditionalCosts.OrderBy(ac => ac.AddedDate))
                .FirstOrDefault(o => o.Id == id && o.ClientId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // AKCEPTACJA WYCENY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveEstimate(int id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid == null) return Unauthorized();
            int userId = int.Parse(uid);

            var order = _db.RepairOrders
                .Include(o => o.AdditionalCosts)
                .FirstOrDefault(o => o.Id == id && o.ClientId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Sprawdzenie czy zlecenie jest w stanie "Do akceptacji"
            if (order.Status == RepairOrderStatusValue.PendingApproval)
            {
                order.Status = RepairOrderStatusValue.Approved;

                // Zaakceptowanie wszystkich niezaakceptowanych kosztów dodatkowych
                var unacceptedCosts = order.AdditionalCosts.Where(ac => !ac.IsAccepted).ToList();
                foreach (var cost in unacceptedCosts)
                {
                    cost.IsAccepted = true;
                    cost.AcceptedDate = DateTime.Now;
                }

                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ODRZUCENIE WYCENY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectEstimate(int id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid == null) return Unauthorized();
            int userId = int.Parse(uid);

            var order = _db.RepairOrders
                .FirstOrDefault(o => o.Id == id && o.ClientId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Sprawdzenie czy zlecenie jest w stanie "Do akceptacji"
            if (order.Status == RepairOrderStatusValue.PendingApproval)
            {
                order.Status = RepairOrderStatusValue.Cancelled;
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
