using System;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class LoteImagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoteId { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; }

        [StringLength(200)]
        public string? Descricao { get; set; }

        public int Ordem { get; set; } = 0;

        public bool Principal { get; set; } = false;

        public DateTime DataUpload { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Lote Lote { get; set; }
    }
}