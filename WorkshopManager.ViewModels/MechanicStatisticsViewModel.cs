namespace WorkshopManager.ViewModels;

public class MechanicStatisticsViewModel
{
    public int MechanicId { get; set; }
    public string MechanicFullName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = string.Empty; // "Dzień", "Tydzień", "Miesiąc", "Rok"

    // Statystyki ogólne
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int InProgressOrders { get; set; }
    public decimal TotalEarnings { get; set; }

    // Statystyki szczegółowe
    public List<OrderStatusStatistic> StatusBreakdown { get; set; } = new();
    public List<DailyStatistic> DailyStatistics { get; set; } = new();
}

public class OrderStatusStatistic
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalEarnings { get; set; }
}

public class DailyStatistic
{
    public DateTime Date { get; set; }
    public int OrdersCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal Earnings { get; set; }
}

public class GenerateStatisticsRequest
{
    public int MechanicId { get; set; }
    public string Period { get; set; } = "month"; // day, week, month, year
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
}
