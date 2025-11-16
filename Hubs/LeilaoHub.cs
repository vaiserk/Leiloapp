using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Leiloapp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;

namespace Leiloapp.Hubs
{
    public class LeilaoHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public LeilaoHub(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [AllowAnonymous]
        public async Task EntrarLeilao(string leilaoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"leilao-{leilaoId}");
            await Clients.Group($"leilao-{leilaoId}").SendAsync("UsuarioEntrou", Context.User.Identity.Name);
        }

        [AllowAnonymous]
        public async Task SairLeilao(string leilaoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"leilao-{leilaoId}");
            await Clients.Group($"leilao-{leilaoId}").SendAsync("UsuarioSaiu", Context.User.Identity.Name);
        }

        [AllowAnonymous]
        public async Task EntrarLote(string loteId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"lote-{loteId}");
        }

        [AllowAnonymous]
        public async Task SairLote(string loteId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lote-{loteId}");
        }

        [Authorize]
        public async Task EnviarLance(string leilaoId, string loteId, decimal valor, string nomeUsuario)
        {
            // Broadcast para todos no grupo do leilão
            await Clients.Group($"leilao-{leilaoId}")
                .SendAsync("NovoLance", new 
                { 
                    loteId, 
                    valor, 
                    usuario = nomeUsuario, 
                    timestamp = DateTime.UtcNow 
                });
        }

        [Authorize]
        public async Task AtualizarLote(string leilaoId, string loteId, string status, decimal valorAtual, string nomeUsuario)
        {
            await Clients.Group($"leilao-{leilaoId}")
                .SendAsync("LoteAtualizado", new 
                { 
                    loteId, 
                    status, 
                    valorAtual, 
                    usuario = nomeUsuario,
                    timestamp = DateTime.UtcNow 
                });
        }

        [Authorize]
        public async Task NotificarLanceSuperado(string usuarioId, string loteId, decimal novoValor)
        {
            await Clients.User(usuarioId).SendAsync("LanceSuperado", new 
            { 
                loteId, 
                novoValor,
                timestamp = DateTime.UtcNow 
            });
        }

        [Authorize]
        public async Task AtualizarTempoRestante(string leilaoId, string loteId, int tempoRestanteSegundos)
        {
            await Clients.Group($"lote-{loteId}")
                .SendAsync("TempoRestanteAtualizado", new 
                { 
                    loteId, 
                    tempoRestanteSegundos,
                    timestamp = DateTime.UtcNow 
                });
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}