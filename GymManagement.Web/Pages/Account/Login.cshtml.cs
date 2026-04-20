using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _api;
        public LoginModel(ApiService api) => _api = api;

        [BindProperty, Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [BindProperty, Required, Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public string Erro { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var resultado = await _api.LoginAsync(new LoginDto { Email = Email, Password = Password });

            if (resultado == null)
            {
                Erro = "Email ou password incorretos.";
                return Page();
            }

            HttpContext.Session.SetString("JWT",              resultado.Token);
            HttpContext.Session.SetString("NomeUtilizador",  resultado.Nome);
            HttpContext.Session.SetString("EmailUtilizador", resultado.Email);
            HttpContext.Session.SetString("Role",             resultado.Role);

            TempData["Sucesso"] = $"Bem-vindo(a), {resultado.Nome}!";
            return RedirectToPage("/Index");
        }
    }

    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }

    public class RegisterModel : PageModel
    {
        private readonly ApiService _api;
        public RegisterModel(ApiService api) => _api = api;

        [BindProperty, Required, StringLength(100), Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty, Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [BindProperty, Required, MinLength(6), Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [BindProperty, Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        public string Erro { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var ok = await _api.RegisterAsync(new RegisterDto
            {
                Nome = Nome, Email = Email, Password = Password, Telefone = Telefone
            });

            if (!ok)
            {
                Erro = "Este email já está registado ou ocorreu um erro.";
                return Page();
            }

            TempData["Sucesso"] = "Conta criada com sucesso! Podes agora iniciar sessão.";
            return RedirectToPage("/Account/Login");
        }
    }
}
