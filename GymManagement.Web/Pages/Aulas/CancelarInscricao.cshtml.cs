using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace GymManagement.Web.Pages.Aulas
{
    [Authorize]
    public class CancelarInscricaoModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CancelarInscricaoModel(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty(SupportsGet = true)]
        public int InscricaoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int AulaId { get; set; }

        public string NomeAula { get; set; } = string.Empty;

        private HttpClient CreateClient()
        {
            var client = _clientFactory.CreateClient("GymAPI");
            // FIX: chave correta é "JWT", não "JwtToken"
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT")
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (InscricaoId <= 0 && AulaId <= 0)
                // FIX: caminho correto é /MinhasInscricoes, não /MinhasInscricoes/Index
                return RedirectToPage("/MinhasInscricoes");

            var client = CreateClient();
            var response = await client.GetAsync($"api/Aulas/{AulaId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                // FIX: leitura case-insensitive do campo nome
                foreach (var prop in doc.RootElement.EnumerateObject())
                    if (prop.Name.Equals("nome", StringComparison.OrdinalIgnoreCase))
                    { NomeAula = prop.Value.GetString() ?? "Aula"; break; }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = CreateClient();
            // FIX: API endpoint é DELETE /api/Inscricoes/Aula/{aulaId}
            var response = await client.DeleteAsync($"api/Inscricoes/Aula/{AulaId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Não foi possível cancelar a inscrição. Tente novamente.";
            }
            else
            {
                TempData["Sucesso"] = "Inscrição cancelada com sucesso.";
            }

            // FIX: caminho correto
            return RedirectToPage("/MinhasInscricoes");
        }
    }
}
