using System;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int Tipo { get; set; } // 0: LanceSuperado, 1: LoteVencido, 2: LeilaoFinalizado, 3: PagamentoPendente

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public bool Lida { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Usuario Usuario { get; set; }

    }
}