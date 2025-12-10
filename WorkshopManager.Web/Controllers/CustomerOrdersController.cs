using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                Status = RepairOrderStatusValue.PendingEstimate
            };
        
            _db.RepairOrders.Add(order);
            _db.SaveChanges();
        
            return RedirectToAction("Index", "Client");
        }

    }
}
