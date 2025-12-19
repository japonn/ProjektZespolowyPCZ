using WorkshopManager.Model.DataModels;

namespace WorkshopManager.ViewModels;

public class OrderReportViewModel
{
    public int OrderId { get; set; }
    public string ClientFullName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string MechanicFullName { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public RepairOrderStatusValue Status { get; set; }

    // Daty
    public DateTime SubmissionDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Wycena początkowa
    public decimal? EntryEstimatedCostNetto { get; set; }
    public int? VatRate { get; set; }
    public decimal? EntryEstimatedCostBrutto { get; set; }

    // Naprawy
    public List<RepairTaskItem> RepairTasks { get; set; } = new();

    // Koszty dodatkowe
    public List<AdditionalCostItem> AdditionalCosts { get; set; } = new();

    // Podsumowanie
    public decimal TotalNettoRepairTasks { get; set; }
    public decimal TotalNettoAdditionalCosts { get; set; }
    public decimal TotalNetto { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalBrutto { get; set; }
}

public class RepairTaskItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public bool IsCompleted { get; set; }
}

public class AdditionalCostItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime AddedDate { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedDate { get; set; }
}
