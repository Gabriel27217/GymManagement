using GymManagement.Models.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Services
{
    /// <summary>
    /// Serviço que centraliza todas as chamadas HTTP à API REST.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx  = ctx;
        }

        // ── Token JWT da sessão ──────────────────────────────────
        private void SetAuth()
        {
            var token = _ctx.HttpContext?.Session.GetString("JWT")
                     ?? _ctx.HttpContext?.User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent Json<T>(T obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        // ── Auth ─────────────────────────────────────────────────
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var resp = await _http.PostAsync("api/Auth/Login", Json(dto));
            if (!resp.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<AuthResponseDto>(
                await resp.Content.ReadAsStringAsync(), _json);
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var resp = await _http.PostAsync("api/Auth/Register", Json(dto));
            return resp.IsSuccessStatusCode;
        }

        // ── Aulas ────────────────────────────────────────────────
        public async Task<List<AulaListDto>> GetAulasAsync()
        {
            SetAuth();
            var resp = await _http.GetStringAsync("api/Aulas");
            return JsonSerializer.Deserialize<List<AulaListDto>>(resp, _json) ?? new();
        }

        public async Task<List<AulaListDto>> GetProximasAulasAsync()
        {
            return await GetAulasAsync();
        }

        public async Task<AulaListDto?> GetAulaAsync(int id)
        {
            var resp = await _http.GetAsync($"api/Aulas/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<AulaListDto>(
                await resp.Content.ReadAsStringAsync(), _json);
        }

        public async Task<bool> CreateAulaAsync(AulaCreateDto dto)
        {
            SetAuth();
            var resp = await _http.PostAsync("api/Aulas", Json(dto));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAulaAsync(int id, AulaCreateDto dto)
        {
            SetAuth();
            var resp = await _http.PutAsync($"api/Aulas/{id}", Json(dto));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAulaAsync(int id)
        {
            SetAuth();
            var resp = await _http.DeleteAsync($"api/Aulas/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ── Planos de Treino ─────────────────────────────────────
        public async Task<List<PlanoTreinoDto>> GetPlanosAsync()
        {
            var resp = await _http.GetStringAsync("api/PlanosTreino");
            return JsonSerializer.Deserialize<List<PlanoTreinoDto>>(resp, _json) ?? new();
        }

        // ── Inscrições ───────────────────────────────────────────
        public async Task<(bool ok, string msg)> InscreverAsync(int aulaId)
        {
            SetAuth();
            var resp = await _http.PostAsync("api/Inscricoes", Json(aulaId));
            var body = await resp.Content.ReadAsStringAsync();
            var obj  = JsonSerializer.Deserialize<Dictionary<string, string>>(body, _json);
            return (resp.IsSuccessStatusCode, obj?.GetValueOrDefault("mensagem") ?? "");
        }

        public async Task<(bool ok, string msg)> CancelarInscricaoAsync(int aulaId)
        {
            SetAuth();
            var resp = await _http.DeleteAsync($"api/Inscricoes/Aula/{aulaId}");
            var body = await resp.Content.ReadAsStringAsync();
            var obj  = JsonSerializer.Deserialize<Dictionary<string, string>>(body, _json);
            return (resp.IsSuccessStatusCode, obj?.GetValueOrDefault("mensagem") ?? "");
        }

        public async Task<string> GetMinhasInscricoesRawAsync()
        {
            SetAuth();
            return await _http.GetStringAsync("api/Inscricoes/Minhas");
        }
    }
}
