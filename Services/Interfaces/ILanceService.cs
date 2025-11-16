using System;
using System.Threading.Tasks;
using Leiloapp.Models.Entities;

namespace Leiloapp.Services.Interfaces
{
    public interface ILanceService
    {
        Task<Lance> RealizarLanceAsync(Lance lance);
        Task<IEnumerable<Lance>> ObterLancesPorLoteAsync(int loteId);
        Task<Lance> ObterLanceVencedorAsync(int loteId);
        Task<bool> ValidarLanceAsync(Lance lance);
        Task<decimal> ObterValorMinimoProximoLanceAsync(int loteId);
        Task<IEnumerable<Lance>> ObterLancesPorUsuarioAsync(int usuarioId);
    }
}