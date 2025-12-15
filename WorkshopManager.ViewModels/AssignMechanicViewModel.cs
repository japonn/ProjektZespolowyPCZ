using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.ViewModels.RepairOrders
{
    public class AssignMechanicVM
    {
        public int OrderId { get; set; }

        [Display(Name = "Mechanik")]
        [Required(ErrorMessage = "Musisz wybrać mechanika.")]
        public int? SelectedMechanicId { get; set; }

        public string ClientFullName { get; set; } = string.Empty;

        [Display(Name = "Numer rejestracyjny")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Opis usterki")]
        public string IssueDescription { get; set; } = string.Empty;

        [Display(Name = "Status zlecenia")]
        public RepairOrderStatusValue Status { get; set; }

        [Display(Name = "Szacowany koszt naprawy (netto)")]
        [Required(ErrorMessage = "Wycena jest wymagana.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Kwota musi być większa niż 0.")]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Stawka VAT")]
        [Required(ErrorMessage = "Wybierz stawkę VAT.")]
        public int? VatRate { get; set; }

        public IList<MechanicListItemVM> Mechanics { get; set; } = new List<MechanicListItemVM>();
    }

    public class MechanicListItemVM
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}
