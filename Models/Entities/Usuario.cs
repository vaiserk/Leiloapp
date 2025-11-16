using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Leiloapp.Models.Entities
{
    public class Usuario : IdentityUser<int>
    {
        [Required]
        [StringLength(14)]
        public string CPF { get; set; }

        [Required]
        [StringLength(255)]
        public string Nome { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [Required]
        public int TipoUsuario { get; set; } // 0: Visitante, 1: Comprador, 2: Leiloeiro, 3: Administrador

        public bool Ativo { get; set; } = true;

        public bool Aprovado { get; set; } = false;

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public DateTime? DataNascimento { get; set; }
        
        public string? Endereco { get; set; }

        // Navigation properties
        public virtual ICollection<Lance> Lances { get; set; }
        public virtual ICollection<Leilao> LeiloesAdministrados { get; set; }
        public virtual ICollection<Lote> LotesVencidos { get; set; }
        public virtual ICollection<Pagamento> Pagamentos { get; set; }
        public virtual ICollection<Notificacao> Notificacoes { get; set; }

        public Usuario()
        {
            Lances = new HashSet<Lance>();
            LeiloesAdministrados = new HashSet<Leilao>();
            LotesVencidos = new HashSet<Lote>();
            Pagamentos = new HashSet<Pagamento>();
            Notificacoes = new HashSet<Notificacao>();
        }
    }
}