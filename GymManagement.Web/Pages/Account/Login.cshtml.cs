using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
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
                return RedirectToPage("/Dashboard");

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

            var token = doc.RootElement.GetProperty("token").GetString()!;
            var nome = doc.RootElement.GetProperty("nome").GetString() ?? "";
            var role = doc.RootElement.GetProperty("role").GetString() ?? "";

            // Guardar JWT, Role e Nome na sessão (para o _Layout e ApiService)
            HttpContext.Session.SetString("JWT", token);
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("NomeUtilizador", nome);

            // Extrair claims do JWT para autenticar o cookie
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = jwt.Claims.ToList();

            // Garantir claim de Name legível
            var nameClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
                         ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (nameClaim != null && !claims.Any(c => c.Type == ClaimTypes.Name))
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Dashboard");
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