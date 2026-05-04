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
    /// <summary>
    /// Gere o processo de autenticação dos utilizadores.
    /// Faz a ponte entre as credenciais do utilizador, a validação na API e a criação da sessão local.
    /// </summary>
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Modelo de dados capturado pelo formulário de Login.
        /// </summary>
        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public string? MensagemErro { get; set; }

        /// <summary>
        /// Verifica se o utilizador já está autenticado. Se sim, redireciona conforme o cargo (Role).
        /// </summary>
        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = HttpContext.Session.GetString("Role");
                return RedirecionarPorCargo(role);
            }
            ViewData["ReturnUrl"] = returnUrl;
            return Page();
        }

        /// <summary>
        /// Processa a submissão do formulário, valida na API e cria o Cookie de autenticação.
        /// </summary>
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
                // Enviar credenciais para o endpoint de login da API
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

            // Extração manual para garantir compatibilidade com PascalCase ou camelCase da API
            string? token = null, nome = null, role = null;
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "token": token = prop.Value.GetString(); break;
                    case "nome": nome = prop.Value.GetString(); break;
                    case "role": role = prop.Value.GetString(); break;
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                MensagemErro = "Resposta inválida da API. Tente novamente.";
                return Page();
            }

            nome ??= "Utilizador";
            role ??= "Cliente";

            // 1. Guardar dados essenciais na Sessão para acesso rápido no Frontend
            HttpContext.Session.SetString("JWT", token);
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("NomeUtilizador", nome);

            // 2. Descodificar o JWT para extrair as Claims (permissões/dados) originais
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claims = jwt.Claims.ToList();

            // Garantir que Claims padrão do ASP.NET estão presentes para o Cookie
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
                claims.Add(new Claim(ClaimTypes.Name, nome));

            if (!claims.Any(c => c.Type == ClaimTypes.Role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Guardar o próprio JWT como Claim para que possa ser recuperado se a Sessão expirar
            claims.Add(new Claim("jwt", token));

            // 3. Criar a identidade baseada em Cookies (necessário para [Authorize] nas Razor Pages)
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            // Redirecionamento inteligente
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirecionarPorCargo(role);
        }

        private IActionResult RedirecionarPorCargo(string? role)
        {
            return role == "Admin"
                ? RedirectToPage("/Dashboard")
                : RedirectToPage("/MinhasInscricoes");
        }

        /// <summary>
        /// Classe DTO para capturar os dados do formulário de login.
        /// </summary>
        public class LoginRequest
        {
            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "O email é obrigatório")]
            [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Introduza um email válido")]
            public string Email { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "A password é obrigatória")]
            public string Password { get; set; } = string.Empty;
        }
    }
}