using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymManagement.Web.Pages.Account
{
    /// <summary>
    /// Modelo de página responsável por encerrar a sessão do utilizador de forma segura.
    /// Garante que todos os tokens e cookies de identidade são invalidados.
    /// </summary>
    public class LogoutModel : PageModel
    {
        /// <summary>
        /// Processa o pedido de logout (normalmente disparado por um formulário via POST).
        /// </summary>
        /// <returns>Redireciona o utilizador para a página inicial após a limpeza.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Limpar todos os dados armazenados na sessão do servidor (como o Nome e Role)
            HttpContext.Session.Clear();

            // 2. Notificar o middleware de autenticação para invalidar o Cookie de Identity
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Remoção explícita dos cookies no navegador por precaução adicional.
            // Isto garante que, mesmo que o browser retenha dados, os identificadores principais são apagados.
            Response.Cookies.Delete(".AspNetCore.Cookies");
            Response.Cookies.Delete(".AspNetCore.Session");

            // Redirecionar para a página principal (Landing Page)
            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Impede o acesso direto à página de logout via URL (GET).
        /// Por razões de segurança (evitar ataques de logout forçado), o logout deve ser sempre um POST.
        /// </summary>
        public IActionResult OnGet() => RedirectToPage("/Index");
    }
}