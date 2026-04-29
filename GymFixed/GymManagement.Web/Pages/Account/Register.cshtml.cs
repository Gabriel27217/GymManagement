using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RegisterModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty, Required, Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty, Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [BindProperty, Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [BindProperty, Required, MinLength(6), Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public string? Erro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("GymAPI");

            var body = new { Nome, Email, Telefone, Password };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("api/auth/register", content);
            }
            catch
            {
                Erro = "Não foi possível ligar à API. Verifique se o servidor está a correr.";
                return Page();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Erro = "Este email já está registado.";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                Erro = "Erro ao criar conta. Tente novamente.";
                return Page();
            }

            TempData["Sucesso"] = "Conta criada com sucesso! Podes fazer login agora.";
            return RedirectToPage("/Account/Login");
        }
    }
}