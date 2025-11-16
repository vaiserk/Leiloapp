using System;
using System.ComponentModel.DataAnnotations;

namespace Leiloapp.Models.Entities
{
    public class Pagamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoteId { get; set; }

        [Required]
        public int CompradorId { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public int MetodoPagamento { get; set; } // 0: Boleto, 1: Cartao, 2: PIX

        [Required]
        public int Status { get; set; } = 0; // 0: Pendente, 1: Pago, 2: Vencido, 3: Cancelado

        public DateTime? DataPagamento { get; set; }
        
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Lote Lote { get; set; }
        public virtual Usuario Comprador { get; set; }
    }
}