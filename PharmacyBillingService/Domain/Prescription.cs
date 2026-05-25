using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacyBillingService.Domain
{
    public class Prescription
    {
        [Key]
        public Guid Id { get; set; } // Can map to the ID created by Medical Record Service

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DoctorName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = StatusApproved; // Default: Approved (if stock was OK), Draft (if stock issues), Dispensed (paid & given to patient)

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

        // Status Constants
        public const string StatusDraft = "Draft";
        public const string StatusApproved = "Approved";
        public const string StatusDispensed = "Dispensed";
    }
}
