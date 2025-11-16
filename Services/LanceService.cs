using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;
using Leiloapp.Models.Entities;
using Leiloapp.Services.Interfaces;

namespace Leiloapp.Services
{
    public class LanceService : ILanceService
    {
        private readonly ApplicationDbContext _context;

        public LanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Lance> RealizarLanceAsync(Lance lance)
        {
            // Validar o lance antes de salvar
            var isValid = await ValidarLanceAsync(lance);
            if (!isValid)
                throw new InvalidOperationException("Lance inválido");

            // Obter o lote e atualizar o valor atual
            var lote = await _context.Lotes.FindAsync(lance.LoteId);
            if (lote == null)
                throw new InvalidOperationException("Lote não encontrado");

            // Marcar lances anteriores como não vencedores
            var lancesAnteriores = await _context.Lances
                .Where(l => l.LoteId == lance.LoteId && l.Vencedor)
                .ToListAsync();

            foreach (var lanceAnterior in lancesAnteriores)
            {
                lanceAnterior.Vencedor = false;
            }

            // Definir este lance como vencedor
            lance.Vencedor = true;
            lance.DataHora = DateTime.UtcNow;

            // Atualizar o lote
            lote.LanceAtual = lance.Valor;
            lote.Status = 1;

            _context.Lances.Add(lance);
            await _context.SaveChangesAsync();

            return lance;
        }

        public async Task<IEnumerable<Lance>> ObterLancesPorLoteAsync(int loteId)
        {
            return await _context.Lances
                .Include(l => l.Usuario)
                .Where(l => l.LoteId == loteId)
                .OrderByDescending(l => l.DataHora)
                .ToListAsync();
        }

        public async Task<Lance> ObterLanceVencedorAsync(int loteId)
        {
            return await _context.Lances
                .Include(l => l.Usuario)
                .Where(l => l.LoteId == loteId && l.Vencedor)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ValidarLanceAsync(Lance lance)
        {
            // Verificar se o lote existe e está ativo
            var lote = await _context.Lotes.FindAsync(lance.LoteId);
            if (lote == null || (lote.Status != 1 && lote.Status != 0))
                return false;

            // Verificar se o usuário existe e está aprovado
            var usuario = await _context.Users.FindAsync(lance.UsuarioId);
            if (usuario == null || !usuario.Aprovado || !usuario.Ativo)
                return false;

            // Verificar se o valor é maior que o valor atual
            if (lance.Valor <= lote.LanceAtual)
                return false;

            // Verificar se o valor é maior ou igual ao valor mínimo
            if (lance.Valor < lote.ValorMinimo)
                return false;

            // Verificar se não há lances simultâneos com valor maior
            var lanceMaisRecente = await _context.Lances
                .Where(l => l.LoteId == lance.LoteId)
                .OrderByDescending(l => l.DataHora)
                .FirstOrDefaultAsync();

            if (lanceMaisRecente != null && lance.Valor <= lanceMaisRecente.Valor)
                return false;

            return true;
        }

        public async Task<decimal> ObterValorMinimoProximoLanceAsync(int loteId)
        {
            var lote = await _context.Lotes.FindAsync(loteId);
            if (lote == null)
                return 0;

            // Se não houver lances, retornar o valor inicial
            var lanceMaisRecente = await _context.Lances
                .Where(l => l.LoteId == loteId)
                .OrderByDescending(l => l.Valor)
                .FirstOrDefaultAsync();

            if (lanceMaisRecente == null)
                return lote.ValorInicial;

            // Incremento mínimo de 5% ou R$ 10,00, o que for maior
            var incrementoMinimo = Math.Max(lanceMaisRecente.Valor * 0.05m, 10.00m);
            return lanceMaisRecente.Valor + incrementoMinimo;
        }

        public async Task<IEnumerable<Lance>> ObterLancesPorUsuarioAsync(int usuarioId)
        {
            return await _context.Lances
                .Include(l => l.Lote)
                .ThenInclude(lt => lt.Leilao)
                .Where(l => l.UsuarioId == usuarioId)
                .OrderByDescending(l => l.DataHora)
                .ToListAsync();
        }
    }
}