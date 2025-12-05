using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.ViewModels
{
    public class RegisterClientViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć co najmniej {2} znaków i maksymalnie {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła nie są identyczne.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Imię jest wymagane")]
        [StringLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [StringLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu")]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }
    }
}
