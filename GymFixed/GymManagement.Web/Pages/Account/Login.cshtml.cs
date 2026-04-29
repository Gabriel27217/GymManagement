using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public string? MensagemErro { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole("Admin")
                    ? RedirectToPage("/Dashboard")
                    : RedirectToPage("/MinhasInscricoes");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("GymAPI");
            var content = new StringContent(
                JsonSerializer.Serialize(Input),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("api/auth/login", content);
            }
            catch
            {
                MensagemErro = "Não foi possível ligar à API. Verifique se o servidor está a correr.";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Email ou password incorretos.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string? token = null, nome = null, role = null, email = null, userId = null;
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "token": token = prop.Value.GetString(); break;
                    case "nome":  nome  = prop.Value.GetString(); break;
                    case "role":  role  = prop.Value.GetString(); break;
                    case "email": email = prop.Value.GetString(); break;
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                MensagemErro = "Resposta inválida da API. Tente novamente.";
                return Page();
            }

            nome  ??= "";
            role  ??= "";
            email ??= "";

            // Guardar na sessão
            HttpContext.Session.SetString("JWT",             token);
            HttpContext.Session.SetString("Role",            role);
            HttpContext.Session.SetString("NomeUtilizador",  nome);

            // ── Construir os claims manualmente a partir da resposta da API ──
            // NÃO usar JwtSecurityTokenHandler.ReadJwtToken porque ele remapeia
            // ClaimTypes.Role para o tipo curto "role", quebrando User.IsInRole().
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  nome),
                new Claim(ClaimTypes.Email, email),
                // CRÍTICO: usar ClaimTypes.Role para que User.IsInRole() funcione
                new Claim(ClaimTypes.Role,  role),
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent    = false,
                    ExpiresUtc      = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return role == "Admin"
                ? RedirectToPage("/Dashboard")
                : RedirectToPage("/MinhasInscricoes");
        }

        public class LoginRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.EmailAddress]
            public string Email { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Required]
            public string Password { get; set; } = string.Empty;
        }
    }
}
