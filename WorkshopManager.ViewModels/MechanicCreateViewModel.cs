using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.ViewModels
{
    public class MechanicCreateViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; } = string.Empty;
    }
}
