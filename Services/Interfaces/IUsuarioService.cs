using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leiloapp.Models.Entities;

namespace Leiloapp.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario> ObterUsuarioPorIdAsync(int id);
        Task<Usuario> ObterUsuarioPorCPFAsync(string cpf);
        Task<Usuario> ObterUsuarioPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObterUsuariosPendentesAprovacaoAsync();
        Task<bool> AprovarUsuarioAsync(int usuarioId);
        Task<bool> RejeitarUsuarioAsync(int usuarioId);
        Task<bool> AtualizarPerfilUsuarioAsync(int usuarioId, int tipoUsuario);
        Task<IEnumerable<Usuario>> ObterParticipantesAsync();
        Task<IEnumerable<Usuario>> ObterAdministradoresAsync();
    }
}