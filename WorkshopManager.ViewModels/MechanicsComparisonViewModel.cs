namespace WorkshopManager.ViewModels;

public class MechanicsComparisonViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public List<MechanicComparisonItem> Mechanics { get; set; } = new();
}

public class MechanicComparisonItem
{
    public int MechanicId { get; set; }
    public string MechanicFullName { get; set; } = string.Empty;
    public DateTime EmploymentDate { get; set; }

    // Statystyki
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int InProgressOrders { get; set; }
    public decimal TotalEarnings { get; set; }

    // Wskaźniki wydajności
    public decimal CompletionRate { get; set; } // % zakończonych
    public decimal AverageEarningsPerOrder { get; set; }
    public int DaysWorking { get; set; } // dni w okresie
}

public class CompareMechanicsRequest
{
    public List<int> MechanicIds { get; set; } = new();
    public string Period { get; set; } = "month";
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
}
