using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class Lote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LeilaoId { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(255)]
        public string NomeDoador { get; set; }

        public string Descricao { get; set; }

        [Required]
        public decimal ValorInicial { get; set; }

        [Required]
        public decimal ValorMinimo { get; set; }

        public decimal LanceAtual { get; set; } = 0;
        
        public decimal LanceMinimo { get; set; } = 0;

        public int? LanceVencedorId { get; set; }

        [Required]
        public int Status { get; set; } = 0; // 0: Aberto, 1: EmLicitacao, 2: Vendido, 3: NaoVendido

        public int Numero { get; set; } = 0;

        public bool Visivel { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Leilao? Leilao { get; set; }
        public virtual Usuario? LanceVencedor { get; set; }
        public virtual ICollection<Lance> Lances { get; set; }
        public virtual ICollection<Pagamento> Pagamentos { get; set; }
        public virtual ICollection<LoteImagem> Imagens { get; set; }

        public Lote()
        {
            Lances = new HashSet<Lance>();
            Pagamentos = new HashSet<Pagamento>();
            Imagens = new HashSet<LoteImagem>();
        }
    }
}