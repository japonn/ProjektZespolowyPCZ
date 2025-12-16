using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.ViewModels.RepairOrders
{
    public class AddAdditionalCostVM
    {
        public int OrderId { get; set; }

        public string ClientFullName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public RepairOrderStatusValue Status { get; set; }

        [Display(Name = "Opis usterki")]
        public string IssueDescription { get; set; } = string.Empty;

        [Display(Name = "Pierwotna wycena (netto)")]
        public decimal? OriginalEstimatedCost { get; set; }

        [Display(Name = "Stawka VAT")]
        public int? VatRate { get; set; }

        [Display(Name = "Koszt dodatkowy (netto)")]
        [Required(ErrorMessage = "Koszt dodatkowy jest wymagany.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Kwota musi być większa niż 0.")]
        public decimal? AdditionalCost { get; set; }

        [Display(Name = "Opis kosztu dodatkowego")]
        [Required(ErrorMessage = "Opis jest wymagany.")]
        [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków.")]
        public string? AdditionalCostDescription { get; set; }
    }
}
