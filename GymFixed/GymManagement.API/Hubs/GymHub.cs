using Microsoft.AspNetCore.SignalR;

namespace GymManagement.API.Hubs
{
    /// <summary>
    /// Hub SignalR para notificações em tempo real.
    /// Envia alertas quando: aula fica lotada, vagas libertadas, novo plano criado.
    /// </summary>
    public class GymHub : Hub
    {
        // Notifica todos os clientes que uma aula ficou lotada
        public async Task AulaLotada(string nomeAula)
            => await Clients.All.SendAsync("AulaLotada", nomeAula);

        // Notifica que ficaram vagas disponíveis numa aula
        public async Task VagasLiberadas(string nomeAula, int vagas)
            => await Clients.All.SendAsync("VagasLiberadas", nomeAula, vagas);

        // Notifica a criação de um novo plano de treino
        public async Task NovoPlano(string nomePlano)
            => await Clients.All.SendAsync("NovoPlano", nomePlano);

        // Notifica entrada/saída de cliente no ginásio (para dashboard admin)
        public async Task AtualizarFrequencias()
            => await Clients.All.SendAsync("AtualizarFrequencias");
    }
}
