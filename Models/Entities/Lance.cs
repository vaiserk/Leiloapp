using System;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class Lance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoteId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public decimal Valor { get; set; }

        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        public bool Vencedor { get; set; } = false;

        // Navigation properties
        public virtual Lote Lote { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}