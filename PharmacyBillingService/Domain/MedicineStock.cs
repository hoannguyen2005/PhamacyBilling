using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyBillingService.Domain
{
    public class MedicineStock
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MedicineId { get; set; }

        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int MinAlertQuantity { get; set; } = 10;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
