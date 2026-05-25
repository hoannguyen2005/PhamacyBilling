using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class BillItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required]
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty; // e.g., "Phí khám bệnh chuyên khoa", "Paracetamol 500mg (10 viên)"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}
