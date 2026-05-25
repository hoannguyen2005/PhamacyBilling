using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        [MaxLength(30)]
        public string PaymentMethod { get; set; } = MethodCash; // Cash / BankTransfer / POS

        [MaxLength(100)]
        public string? TransactionCode { get; set; } // Custom bank ref code, POS receipt code

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid ReceivedById { get; set; }

        [ForeignKey(nameof(ReceivedById))]
        public User? ReceivedBy { get; set; }

        // Method Constants
        public const string MethodCash = "Cash";
        public const string MethodBankTransfer = "BankTransfer";
        public const string MethodPOS = "POS";
    }
}
