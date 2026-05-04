using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Pages.Account
{
    /// <summary>
    /// Modelo de página para o registo de novos utilizadores (Self-Service Registration).
    /// Permite que potenciais clientes criem a sua conta antes de efetuarem o login.
    /// </summary>
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RegisterModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // --- Atributos de Validação do Formulário ---

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um endereço de email válido.")]
        [Display(Name = "Endereço de Email")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Phone(ErrorMessage = "Número de telefone inválido.")]
        [Display(Name = "Telefone (Opcional)")]
        public string? Telefone { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Armazena mensagens de erro provenientes da API para exibir na interface.
        /// </summary>
        public string? Erro { get; set; }

        public void OnGet() { }

        /// <summary>
        /// Processa a submissão do formulário de registo.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Verifica as validações definidas nos atributos (Data Annotations)
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("GymAPI");

            // Preparação do objeto anónimo para serialização JSON
            var body = new { Nome, Email, Telefone, Password };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;
            try
            {
                // Chamada assíncrona ao endpoint de registo da API
                response = await client.PostAsync("api/auth/register", content);
            }
            catch (HttpRequestException)
            {
                Erro = "Não foi possível estabelecer ligação com o servidor da API.";
                return Page();
            }

            // Tratamento de conflito de negócio: Email já existente
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Erro = "Este endereço de email já se encontra registado no nosso sistema.";
                return Page();
            }

            // Verificação genérica de sucesso (Status 2xx)
            if (!response.IsSuccessStatusCode)
            {
                Erro = "Ocorreu um erro inesperado ao criar a sua conta. Por favor, tente mais tarde.";
                return Page();
            }

            // Utilização de TempData para persistir a mensagem de sucesso após o redirecionamento
            TempData["Sucesso"] = "A sua conta foi criada com sucesso! Já pode efetuar o login.";

            return RedirectToPage("/Account/Login");
        }
    }
}