using Microsoft.EntityFrameworkCore;
using WorkshopManager.DAL.EF;
using WorkshopManager.Model.DataModels;
using WorkshopManager.ViewModels;

namespace WorkshopManager.Services;

public interface IMechanicStatisticsService
{
    Task<MechanicStatisticsViewModel> GenerateStatisticsAsync(GenerateStatisticsRequest request);
    Task<MechanicsComparisonViewModel> CompareMechanicsAsync(CompareMechanicsRequest request);
}

public class MechanicStatisticsService : IMechanicStatisticsService
{
    private readonly ApplicationDbContext _context;

    public MechanicStatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MechanicStatisticsViewModel> GenerateStatisticsAsync(GenerateStatisticsRequest request)
    {
        var mechanic = await _context.Mechanics.FindAsync(request.MechanicId);
        if (mechanic == null)
        {
            throw new ArgumentException($"Mechanic with ID {request.MechanicId} not found.");
        }

        // Określ zakres dat
        var (startDate, endDate, periodName) = GetDateRange(request.Period, request.CustomStartDate, request.CustomEndDate);

        // Pobierz zlecenia mechanika w danym okresie
        var orders = await _context.RepairOrders
            .Include(o => o.Tasks)
            .Include(o => o.AdditionalCosts)
            .Where(o => o.MechanicId == request.MechanicId &&
                       o.SubmissionDate >= startDate &&
                       o.SubmissionDate <= endDate)
            .ToListAsync();

        var statistics = new MechanicStatisticsViewModel
        {
            MechanicId = mechanic.Id,
            MechanicFullName = $"{mechanic.FirstName} {mechanic.LastName}",
            StartDate = startDate,
            EndDate = endDate,
            Period = periodName,

            // Statystyki ogólne
            TotalOrders = orders.Count(),
            CompletedOrders = orders.Count(o => o.Status == RepairOrderStatusValue.Completed),
            CancelledOrders = orders.Count(o => o.Status == RepairOrderStatusValue.Cancelled),
            InProgressOrders = orders.Count(o => o.Status == RepairOrderStatusValue.InProgress),
            TotalEarnings = CalculateTotalEarnings(orders)
        };

        // Statystyki według statusu
        statistics.StatusBreakdown = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusStatistic
            {
                Status = GetStatusName(g.Key),
                Count = g.Count(),
                TotalEarnings = CalculateTotalEarnings(g.ToList())
            })
            .ToList();

        // Statystyki dzienne
        statistics.DailyStatistics = orders
            .GroupBy(o => o.SubmissionDate.Date)
            .Select(g => new DailyStatistic
            {
                Date = g.Key,
                OrdersCount = g.Count(),
                CompletedCount = g.Count(o => o.Status == RepairOrderStatusValue.Completed),
                Earnings = CalculateTotalEarnings(g.ToList())
            })
            .OrderBy(d => d.Date)
            .ToList();

        return statistics;
    }

    private (DateTime StartDate, DateTime EndDate, string PeriodName) GetDateRange(
        string period,
        DateTime? customStart,
        DateTime? customEnd)
    {
        var now = DateTime.Now;

        if (customStart.HasValue && customEnd.HasValue)
        {
            return (customStart.Value, customEnd.Value, "Niestandardowy okres");
        }

        return period.ToLower() switch
        {
            "day" => (now.Date, now.Date.AddDays(1).AddSeconds(-1), "Dzień"),
            "week" => (now.AddDays(-(int)now.DayOfWeek).Date, now.AddDays(6 - (int)now.DayOfWeek).Date.AddDays(1).AddSeconds(-1), "Tydzień"),
            "month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1), "Miesiąc"),
            "year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31, 23, 59, 59), "Rok"),
            _ => (now.AddMonths(-1), now, "Ostatni miesiąc")
        };
    }

    private decimal CalculateTotalEarnings(List<RepairOrder> orders)
    {
        decimal total = 0;

        foreach (var order in orders.Where(o => o.Status == RepairOrderStatusValue.Completed))
        {
            // Główny koszt zlecenia (wstępna wycena)
            total += order.EntryEstimatedCost ?? 0;

            // Suma zaakceptowanych dodatkowych kosztów
            total += order.AdditionalCosts?
                .Where(ac => ac.IsAccepted)
                .Sum(ac => ac.Cost) ?? 0;

            // Jeśli są tasks i mają koszty, dodaj je też
            var tasksCost = order.Tasks?.Sum(t => t.Cost) ?? 0;
            if (tasksCost > 0)
            {
                total += tasksCost;
            }
        }

        return total;
    }

    private string GetStatusName(RepairOrderStatusValue status)
    {
        return status switch
        {
            RepairOrderStatusValue.Created => "Utworzone",
            RepairOrderStatusValue.PendingApproval => "Do akceptacji",
            RepairOrderStatusValue.Approved => "Zaakceptowane",
            RepairOrderStatusValue.InProgress => "W realizacji",
            RepairOrderStatusValue.ReadyForPickup => "Gotowe do odbioru",
            RepairOrderStatusValue.Completed => "Zakończone",
            RepairOrderStatusValue.Cancelled => "Anulowane",
            _ => status.ToString()
        };
    }

    public async Task<MechanicsComparisonViewModel> CompareMechanicsAsync(CompareMechanicsRequest request)
    {
        if (request.MechanicIds == null || !request.MechanicIds.Any())
        {
            throw new ArgumentException("Należy wybrać przynajmniej jednego mechanika do porównania.");
        }

        // Określ zakres dat
        var (startDate, endDate, periodName) = GetDateRange(request.Period, request.CustomStartDate, request.CustomEndDate);

        // Pobierz mechaników
        var mechanics = await _context.Mechanics
            .Where(m => request.MechanicIds.Contains(m.Id))
            .ToListAsync();

        if (!mechanics.Any())
        {
            throw new ArgumentException("Nie znaleziono wybranych mechaników.");
        }

        var comparison = new MechanicsComparisonViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Period = periodName,
            Mechanics = new List<MechanicComparisonItem>()
        };

        foreach (var mechanic in mechanics.OrderBy(m => m.LastName).ThenBy(m => m.FirstName))
        {
            // Pobierz zlecenia mechanika w danym okresie
            var orders = await _context.RepairOrders
                .Include(o => o.Tasks)
                .Include(o => o.AdditionalCosts)
                .Where(o => o.MechanicId == mechanic.Id &&
                           o.SubmissionDate >= startDate &&
                           o.SubmissionDate <= endDate)
                .ToListAsync();

            var completedCount = orders.Count(o => o.Status == RepairOrderStatusValue.Completed);
            var totalEarnings = CalculateTotalEarnings(orders);
            var totalOrders = orders.Count();

            var item = new MechanicComparisonItem
            {
                MechanicId = mechanic.Id,
                MechanicFullName = $"{mechanic.FirstName} {mechanic.LastName}",
                EmploymentDate = mechanic.EmploymentDate,
                TotalOrders = totalOrders,
                CompletedOrders = completedCount,
                CancelledOrders = orders.Count(o => o.Status == RepairOrderStatusValue.Cancelled),
                InProgressOrders = orders.Count(o => o.Status == RepairOrderStatusValue.InProgress),
                TotalEarnings = totalEarnings,
                CompletionRate = totalOrders > 0 ? (decimal)completedCount / totalOrders * 100 : 0,
                AverageEarningsPerOrder = completedCount > 0 ? totalEarnings / completedCount : 0,
                DaysWorking = (int)(endDate - startDate).TotalDays + 1
            };

            comparison.Mechanics.Add(item);
        }

        // Sortuj według zarobków (malejąco)
        comparison.Mechanics = comparison.Mechanics.OrderByDescending(m => m.TotalEarnings).ToList();

        return comparison;
    }
}
