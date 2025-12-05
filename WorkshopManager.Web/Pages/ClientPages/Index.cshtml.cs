using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WorkshopManager.Web.Pages.ClientPages
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
