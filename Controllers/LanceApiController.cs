using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;
using Leiloapp.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Leiloapp.Hubs;

namespace Leiloapp.Controllers
{
    [Route("api/lance")]
    [ApiController]
    [Authorize]
    public class LanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IHubContext<LeilaoHub> _hub;

        public LanceApiController(ApplicationDbContext context, UserManager<Usuario> userManager, IHubContext<LeilaoHub> hub)
        {
            _context = context;
            _userManager = userManager;
            _hub = hub;
        }

        [HttpPost("dar-lance")]
        public async Task<IActionResult> DarLance([FromBody] LanceRequest request)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return Unauthorized(new { success = false, message = "Usuário não autenticado" });
                }

                var lote = await _context.Lotes
                    .Include(l => l.Lances)
                    .Include(l => l.Leilao)
                    .FirstOrDefaultAsync(l => l.Id == request.LoteId);

                if (lote == null)
                {
                    return NotFound(new { success = false, message = "Lote não encontrado" });
                }

                var ativoStatus = lote.Leilao.Status == 0 || lote.Leilao.Status == 1;
                var loteAtivo = lote.Status == 1;
                if (!(ativoStatus && loteAtivo))
                {
                    return BadRequest(new { success = false, message = "Este leilão não está ativo" });
                }

                // Validar valor do lance
                var lanceMinimo = CalcularLanceMinimo(lote);
                if (request.Valor < lanceMinimo)
                {
                    return BadRequest(new { success = false, message = $"O lance mínimo é R$ {lanceMinimo:F2}" });
                }

                // Criar novo lance
                var novoLance = new Lance
                {
                    LoteId = request.LoteId,
                    UsuarioId = usuario.Id,
                    Valor = request.Valor,
                    DataHora = DateTime.UtcNow,
                    Vencedor = true
                };

                // Atualizar lote
                lote.LanceAtual = request.Valor;
                lote.LanceMinimo = CalcularLanceMinimo(lote, request.Valor);
                lote.LanceVencedorId = usuario.Id;
                lote.AtualizadoEm = DateTime.UtcNow;

                // Marcar lances anteriores como não vencedores
                foreach (var lanceAnterior in lote.Lances.Where(l => l.Vencedor))
                {
                    lanceAnterior.Vencedor = false;
                }

                _context.Lances.Add(novoLance);
                await _context.SaveChangesAsync();

                await _hub.Clients.Group($"leilao-{lote.LeilaoId}").SendAsync("NovoLance", new
                {
                    loteId = lote.Id.ToString(),
                    valor = request.Valor,
                    usuario = usuario.Nome,
                    timestamp = DateTime.UtcNow
                });

                return Ok(new 
                { 
                    success = true, 
                    message = "Lance realizado com sucesso!",
                    lance = new 
                    {
                        valor = request.Valor,
                        usuario = usuario.Nome,
                        dataHora = novoLance.DataHora
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao processar lance: " + ex.Message });
            }
        }

        [HttpPost("abrir-lote")]
        public async Task<IActionResult> AbrirLote([FromBody] LoteActionRequest request)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized(new { success = false, message = "Usuário não autenticado" });
            if (usuario.TipoUsuario != 2) return Forbid();

            var lote = await _context.Lotes.Include(l=>l.Leilao).FirstOrDefaultAsync(l => l.Id == request.LoteId);
            if (lote == null) return NotFound(new { success = false, message = "Lote não encontrado" });

            lote.Status = 1;
            lote.AtualizadoEm = DateTime.UtcNow;
            if (lote.LanceAtual <= 0) lote.LanceMinimo = CalcularLanceMinimo(lote, lote.ValorInicial);
            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"leilao-{lote.LeilaoId}").SendAsync("LoteAberto", new { loteId = lote.Id.ToString() });
            return Ok(new { success = true });
        }

        [HttpPost("finalizar-lote")]
        public async Task<IActionResult> FinalizarLote([FromBody] FinalizarLoteRequest request)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Unauthorized(new { success = false, message = "Usuário não autenticado" });
            if (usuario.TipoUsuario != 2) return Forbid();

            var lote = await _context.Lotes.Include(l=>l.Lances).FirstOrDefaultAsync(l => l.Id == request.LoteId);
            if (lote == null) return NotFound(new { success = false, message = "Lote não encontrado" });

            lote.Status = request.Vendido ? 2 : 3;
            lote.AtualizadoEm = DateTime.UtcNow;
            if (request.Vendido && lote.Lances.Any())
            {
                var ultimo = lote.Lances.OrderByDescending(l => l.DataHora).First();
                lote.LanceVencedorId = ultimo.UsuarioId;
            }
            await _context.SaveChangesAsync();

            // Atualizar valor arrecadado do leilão
            var totalArrecadado = await _context.Lotes
                .Where(l => l.LeilaoId == lote.LeilaoId && l.Status == 2)
                .SumAsync(l => l.LanceAtual);
            var leilao = await _context.Leiloes.FirstOrDefaultAsync(l => l.Id == lote.LeilaoId);
            if (leilao != null)
            {
                leilao.ValorArrecadado = totalArrecadado;
                await _context.SaveChangesAsync();
            }

            string? vencedorNome = null;
            decimal valorFinal = lote.LanceAtual;
            if (request.Vendido && lote.Lances.Any())
            {
                var ultimo = lote.Lances.OrderByDescending(l => l.DataHora).First();
                vencedorNome = (await _context.Users.FirstOrDefaultAsync(u => u.Id == ultimo.UsuarioId))?.Nome;
            }
            await _hub.Clients.Group($"leilao-{lote.LeilaoId}").SendAsync("LoteFinalizado", new { loteId = lote.Id.ToString(), vendido = request.Vendido, vencedor = vencedorNome, valorFinal });
            await _hub.Clients.Group($"leilao-{lote.LeilaoId}").SendAsync("LeilaoAtualizado", new { leilaoId = lote.LeilaoId.ToString(), valorArrecadado = totalArrecadado });
            return Ok(new { success = true });
        }

        private decimal CalcularLanceMinimo(Lote lote, decimal? valorAtual = null)
        {
            var valorBase = valorAtual ?? lote.LanceAtual;
            if (valorBase == 0)
            {
                return lote.ValorInicial;
            }

            // Calcular aumento mínimo: 5% ou R$ 10,00, o que for maior
            var aumentoPercentual = valorBase * 0.05m;
            var aumentoMinimo = Math.Max(aumentoPercentual, 10.00m);
            
            return valorBase + aumentoMinimo;
        }

    }

    public class LanceRequest
    {
        public int LoteId { get; set; }
        public decimal Valor { get; set; }
    }

    public class LoteActionRequest
    {
        public int LoteId { get; set; }
    }

    public class FinalizarLoteRequest
    {
        public int LoteId { get; set; }
        public bool Vendido { get; set; }
    }
}