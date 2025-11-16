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
    public class LeilaoService : ILeilaoService
    {
        private readonly ApplicationDbContext _context;

        public LeilaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Leilao>> ObterLeiloesAtivosAsync()
        {
            return await _context.Leiloes
                .Include(l => l.Lotes)
                .Where(l => l.Status == 0 || l.Status == 1)
                .OrderByDescending(l => l.DataInicio)
                .ToListAsync();
        }

        public async Task<Leilao> ObterLeilaoPorIdAsync(int id)
        {
            return await _context.Leiloes
                .Include(l => l.Lotes)
                .ThenInclude(lt => lt.Imagens)
                .Include(l => l.Leiloeiro)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Leilao> CriarLeilaoAsync(Leilao leilao)
        {
            leilao.CriadoEm = DateTime.UtcNow;
            leilao.AtualizadoEm = DateTime.UtcNow;
            leilao.Status = 0;
            leilao.ValorArrecadado = 0;

            _context.Leiloes.Add(leilao);
            await _context.SaveChangesAsync();

            return leilao;
        }

        public async Task<Leilao> AtualizarLeilaoAsync(Leilao leilao)
        {
            var existingLeilao = await _context.Leiloes.FindAsync(leilao.Id);
            if (existingLeilao == null)
                return null;

            _context.Entry(existingLeilao).CurrentValues.SetValues(leilao);
            await _context.SaveChangesAsync();

            return leilao;
        }

        public async Task<bool> ExcluirLeilaoAsync(int id)
        {
            var leilao = await _context.Leiloes.FindAsync(id);
            if (leilao == null)
                return false;

            if (leilao.Status == 1 || leilao.Status == 2)
                return false;

            var possuiLotes = await _context.Lotes.AnyAsync(l => l.LeilaoId == id);
            if (possuiLotes)
                return false;

            _context.Leiloes.Remove(leilao);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Lote>> ObterLotesPorLeilaoAsync(int leilaoId)
        {
            return await _context.Lotes
                .Include(l => l.Imagens)
                .Include(l => l.Lances)
                .Where(l => l.LeilaoId == leilaoId)
                .OrderBy(l => l.Numero)
                .ToListAsync();
        }

        public async Task<Lote> AdicionarLoteAsync(Lote lote)
        {
            lote.CriadoEm = DateTime.UtcNow;
            lote.AtualizadoEm = DateTime.UtcNow;
            lote.Status = 0;

            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            return lote;
        }

        public async Task<Lote> AtualizarLoteAsync(Lote lote)
        {
            var existingLote = await _context.Lotes.FindAsync(lote.Id);
            if (existingLote == null)
                return null;

            _context.Entry(existingLote).CurrentValues.SetValues(lote);
            await _context.SaveChangesAsync();

            return lote;
        }

        public async Task<bool> ExcluirLoteAsync(int id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null)
                return false;

            if (lote.Status == 2 || lote.Status == 1)
                return false;

            _context.Lotes.Remove(lote);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IniciarLeilaoAsync(int leilaoId)
        {
            var leilao = await _context.Leiloes.FindAsync(leilaoId);
            if (leilao == null)
                return false;

            if (leilao.Status != 0)
                return false;

            leilao.Status = 1;
            leilao.DataInicio = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinalizarLeilaoAsync(int leilaoId)
        {
            var leilao = await _context.Leiloes.FindAsync(leilaoId);
            if (leilao == null)
                return false;

            if (leilao.Status != 1)
                return false;

            leilao.Status = 2;
            leilao.DataFim = DateTime.UtcNow;

            // Atualizar valor arrecadado
            leilao.ValorArrecadado = await ObterValorArrecadadoAsync(leilaoId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> ObterValorArrecadadoAsync(int leilaoId)
        {
            return await _context.Lotes
                .Where(l => l.LeilaoId == leilaoId && l.Status == 2)
                .SumAsync(l => l.LanceAtual);
        }
    }
}