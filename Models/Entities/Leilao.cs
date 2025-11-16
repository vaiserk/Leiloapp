using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class Leilao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LeiloeiroId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; }

        public string Descricao { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Required]
        public int Status { get; set; } = 0;

        public decimal ValorArrecadado { get; set; } = 0;

        [StringLength(500)]
        public string? ImagemCapa { get; set; }

        public string? Local { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Usuario? Leiloeiro { get; set; }
        public virtual ICollection<Lote> Lotes { get; set; }

        public Leilao()
        {
            Lotes = new HashSet<Lote>();
        }
    }
}