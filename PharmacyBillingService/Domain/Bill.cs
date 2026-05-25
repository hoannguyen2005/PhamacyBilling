using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class Bill
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? PrescriptionId { get; set; }

        [ForeignKey(nameof(PrescriptionId))]
        public Prescription? Prescription { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClinicFee { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicineFee { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = StatusPending; // Default: Pending

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        // Navigation properties
        public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Status Constants
        public const string StatusPending = "Pending";
        public const string StatusPaid = "Paid";
        public const string StatusCancelled = "Cancelled";
    }
}
