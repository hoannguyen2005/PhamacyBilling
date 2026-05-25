using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class PrescriptionItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionId { get; set; }

        [ForeignKey(nameof(PrescriptionId))]
        public Prescription? Prescription { get; set; }

        [Required]
        public Guid MedicineId { get; set; }

        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        [Required]
        [MaxLength(100)]
        public string MedicineName { get; set; } = string.Empty; // Snapshot name

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Snapshot price

        [MaxLength(200)]
        public string? Dosage { get; set; } // e.g. "Sáng 1 viên, tối 1 viên sau ăn"
    }
}
