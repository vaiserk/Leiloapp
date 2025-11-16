using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leiloapp.Models.Entities;

namespace Leiloapp.Services.Interfaces
{
    public interface ILeilaoService
    {
        Task<IEnumerable<Leilao>> ObterLeiloesAtivosAsync();
        Task<Leilao> ObterLeilaoPorIdAsync(int id);
        Task<Leilao> CriarLeilaoAsync(Leilao leilao);
        Task<Leilao> AtualizarLeilaoAsync(Leilao leilao);
        Task<bool> ExcluirLeilaoAsync(int id);
        Task<IEnumerable<Lote>> ObterLotesPorLeilaoAsync(int leilaoId);
        Task<Lote> AdicionarLoteAsync(Lote lote);
        Task<Lote> AtualizarLoteAsync(Lote lote);
        Task<bool> ExcluirLoteAsync(int id);
        Task<bool> IniciarLeilaoAsync(int leilaoId);
        Task<bool> FinalizarLeilaoAsync(int leilaoId);
        Task<decimal> ObterValorArrecadadoAsync(int leilaoId);
    }
}