using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymManagement.Web.Pages.Erro
{
    [AllowAnonymous]
    public class AcessoModel : PageModel
    {
        public void OnGet() { }
    }
}
