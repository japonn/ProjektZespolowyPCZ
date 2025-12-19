using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels.RepairOrders;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly WorkshopManager.Services.IPdfGeneratorService _pdfGenerator;

        public OwnerOrdersController(ApplicationDbContext db, WorkshopManager.Services.IPdfGeneratorService pdfGenerator)
        {
            _db = db;
            _pdfGenerator = pdfGenerator;
        }

        // Lista wszystkich zleceń klientów z filtrowaniem
        public IActionResult Index(int? mechanicId, int? status)
        {
            var ordersQuery = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .Include(o => o.AdditionalCosts)
                .AsQueryable();

            // Filtrowanie po mechaniku
            if (mechanicId.HasValue && mechanicId.Value > 0)
            {
                ordersQuery = ordersQuery.Where(o => o.MechanicId == mechanicId.Value);
            }

            // Filtrowanie po statusie
            if (status.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => (int)o.Status == status.Value);
            }

            var orders = ordersQuery
                .OrderByDescending(o => o.SubmissionDate)
                .ToList();

            // Przekazanie listy mechaników do dropdownu
            ViewBag.Mechanics = _db.Mechanics
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .Select(m => new MechanicListItemVM
                {
                    Id = m.Id,
                    FullName = $"{m.FirstName} {m.LastName}"
                })
                .ToList();

            ViewBag.SelectedMechanicId = mechanicId;
            ViewBag.SelectedStatus = status;

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

        // Formularz zmiany statusu zlecenia
        [HttpGet]
        public IActionResult ChangeStatus(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var model = new ChangeStatusVM
            {
                OrderId = order.Id,
                ClientFullName = $"{order.Client.FirstName} {order.Client.LastName}",
                RegistrationNumber = order.RegistrationNumber,
                IssueDescription = order.EntryIssueDescription,
                CurrentStatus = order.Status,
                NewStatus = order.Status
            };

            return View(model);
        }

        // Zapis zmiany statusu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(ChangeStatusVM model)
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

            order.Status = model.NewStatus;

            // Ustawienie dat w zależności od statusu
            if (model.NewStatus == RepairOrderStatusValue.InProgress && order.StartDate == null)
            {
                order.StartDate = DateTime.Now;
            }
            else if (model.NewStatus == RepairOrderStatusValue.Completed && order.EndDate == null)
            {
                order.EndDate = DateTime.Now;
            }

            _db.SaveChanges();

            TempData["Success"] = "Status zlecenia został zmieniony.";
            return RedirectToAction(nameof(Index));
        }

        // Szczegółowy podgląd zlecenia
        [HttpGet]
        public IActionResult Details(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .Include(o => o.AdditionalCosts.OrderBy(ac => ac.AddedDate))
                .Include(o => o.Tasks)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Raport zakończonego zlecenia
        [HttpGet]
        public IActionResult Report(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .Include(o => o.AdditionalCosts.OrderBy(ac => ac.AddedDate))
                .Include(o => o.Tasks)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Raport dostępny tylko dla zakończonych zleceń
            if (order.Status != RepairOrderStatusValue.Completed)
            {
                TempData["Error"] = "Raport dostępny tylko dla zakończonych zleceń.";
                return RedirectToAction(nameof(Index));
            }

            // Przygotowanie modelu raportu
            var entryNetto = order.EntryEstimatedCost ?? 0;
            var vatRate = order.VatRate ?? 0;
            var entryVat = (entryNetto * vatRate) / 100;
            var entryBrutto = entryNetto + entryVat;

            var tasksNetto = order.Tasks?.Sum(t => t.Cost) ?? 0;
            var additionalCostsNetto = order.AdditionalCosts?
                .Where(ac => ac.IsAccepted)
                .Sum(ac => ac.Cost) ?? 0;

            var totalNetto = entryNetto + tasksNetto + additionalCostsNetto;
            var totalVat = (totalNetto * vatRate) / 100;
            var totalBrutto = totalNetto + totalVat;

            var model = new OrderReportViewModel
            {
                OrderId = order.Id,
                ClientFullName = $"{order.Client.FirstName} {order.Client.LastName}",
                RegistrationNumber = order.RegistrationNumber,
                MechanicFullName = order.Mechanic != null
                    ? $"{order.Mechanic.FirstName} {order.Mechanic.LastName}"
                    : "Nieprzydzielony",
                IssueDescription = order.EntryIssueDescription,
                Status = order.Status,
                SubmissionDate = order.SubmissionDate,
                StartDate = order.StartDate,
                EndDate = order.EndDate,

                EntryEstimatedCostNetto = entryNetto,
                VatRate = vatRate,
                EntryEstimatedCostBrutto = entryBrutto,

                RepairTasks = order.Tasks?.Select(t => new RepairTaskItem
                {
                    Description = t.Description ?? "Brak opisu",
                    Cost = t.Cost,
                    IsCompleted = t.IsCompleted
                }).ToList() ?? new List<RepairTaskItem>(),

                AdditionalCosts = order.AdditionalCosts?.Select(ac => new AdditionalCostItem
                {
                    Description = ac.Description,
                    Cost = ac.Cost,
                    AddedDate = ac.AddedDate,
                    IsAccepted = ac.IsAccepted,
                    AcceptedDate = ac.AcceptedDate
                }).ToList() ?? new List<AdditionalCostItem>(),

                TotalNettoRepairTasks = tasksNetto,
                TotalNettoAdditionalCosts = additionalCostsNetto,
                TotalNetto = totalNetto,
                TotalVat = totalVat,
                TotalBrutto = totalBrutto
            };

            return View(model);
        }

        // Pobieranie raportu jako PDF
        [HttpGet]
        public IActionResult DownloadReportPdf(int id)
        {
            var order = _db.RepairOrders
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .Include(o => o.AdditionalCosts.OrderBy(ac => ac.AddedDate))
                .Include(o => o.Tasks)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // PDF dostępny tylko dla zakończonych zleceń
            if (order.Status != RepairOrderStatusValue.Completed)
            {
                TempData["Error"] = "PDF dostępny tylko dla zakończonych zleceń.";
                return RedirectToAction(nameof(Index));
            }

            // Przygotowanie modelu raportu (ten sam kod co w Report)
            var entryNetto = order.EntryEstimatedCost ?? 0;
            var vatRate = order.VatRate ?? 0;
            var entryVat = (entryNetto * vatRate) / 100;
            var entryBrutto = entryNetto + entryVat;

            var tasksNetto = order.Tasks?.Sum(t => t.Cost) ?? 0;
            var additionalCostsNetto = order.AdditionalCosts?
                .Where(ac => ac.IsAccepted)
                .Sum(ac => ac.Cost) ?? 0;

            var totalNetto = entryNetto + tasksNetto + additionalCostsNetto;
            var totalVat = (totalNetto * vatRate) / 100;
            var totalBrutto = totalNetto + totalVat;

            var model = new OrderReportViewModel
            {
                OrderId = order.Id,
                ClientFullName = $"{order.Client.FirstName} {order.Client.LastName}",
                RegistrationNumber = order.RegistrationNumber,
                MechanicFullName = order.Mechanic != null
                    ? $"{order.Mechanic.FirstName} {order.Mechanic.LastName}"
                    : "Nieprzydzielony",
                IssueDescription = order.EntryIssueDescription,
                Status = order.Status,
                SubmissionDate = order.SubmissionDate,
                StartDate = order.StartDate,
                EndDate = order.EndDate,

                EntryEstimatedCostNetto = entryNetto,
                VatRate = vatRate,
                EntryEstimatedCostBrutto = entryBrutto,

                RepairTasks = order.Tasks?.Select(t => new RepairTaskItem
                {
                    Description = t.Description ?? "Brak opisu",
                    Cost = t.Cost,
                    IsCompleted = t.IsCompleted
                }).ToList() ?? new List<RepairTaskItem>(),

                AdditionalCosts = order.AdditionalCosts?.Select(ac => new AdditionalCostItem
                {
                    Description = ac.Description,
                    Cost = ac.Cost,
                    AddedDate = ac.AddedDate,
                    IsAccepted = ac.IsAccepted,
                    AcceptedDate = ac.AcceptedDate
                }).ToList() ?? new List<AdditionalCostItem>(),

                TotalNettoRepairTasks = tasksNetto,
                TotalNettoAdditionalCosts = additionalCostsNetto,
                TotalNetto = totalNetto,
                TotalVat = totalVat,
                TotalBrutto = totalBrutto
            };

            // Generuj PDF
            var pdfBytes = _pdfGenerator.GenerateOrderReportPdf(model);
            var fileName = $"Raport_Zlecenia_{order.Id}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
