using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Data;
using Leiloapp.Models.Entities;
using Leiloapp.Services.Interfaces;

namespace Leiloapp.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public UsuarioService(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(int id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<Usuario> ObterUsuarioPorCPFAsync(string cpf)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.CPF == cpf);
        }

        public async Task<Usuario> ObterUsuarioPorEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<Usuario>> ObterUsuariosPendentesAprovacaoAsync()
        {
            return await _context.Users
                .Where(u => !u.Aprovado && u.TipoUsuario == 1)
                .OrderBy(u => u.DataCadastro)
                .ToListAsync();
        }

        public async Task<bool> AprovarUsuarioAsync(int usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null)
                return false;

            usuario.Aprovado = true;
            var result = await _userManager.UpdateAsync(usuario);

            return result.Succeeded;
        }

        public async Task<bool> RejeitarUsuarioAsync(int usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null)
                return false;

            // Remover usuário rejeitado
            var result = await _userManager.DeleteAsync(usuario);
            return result.Succeeded;
        }

        public async Task<bool> AtualizarPerfilUsuarioAsync(int usuarioId, int tipoUsuario)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null)
                return false;

            if (tipoUsuario < 0 || tipoUsuario > 3)
                return false;

            usuario.TipoUsuario = tipoUsuario;
            var result = await _userManager.UpdateAsync(usuario);

            return result.Succeeded;
        }

        public async Task<IEnumerable<Usuario>> ObterParticipantesAsync()
        {
            return await _context.Users
                .Where(u => u.TipoUsuario == 1 && u.Aprovado)
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObterAdministradoresAsync()
        {
            return await _context.Users
                .Where(u => u.TipoUsuario == 3 || u.TipoUsuario == 2)
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }
    }
}