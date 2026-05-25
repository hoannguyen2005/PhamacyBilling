using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacyBillingService.DTOs
{
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }

    public class PrescriptionItemDto
    {
        public Guid Id { get; set; }
        public Guid MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string? Dosage { get; set; }
    }

    // DTO mapping to the event 'prescription.created'
    public class PrescriptionCreatedEventDto
    {
        [Required(ErrorMessage = "PrescriptionId is required")]
        public Guid PrescriptionId { get; set; }

        [Required(ErrorMessage = "PatientId is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "PatientName is required")]
        public string PatientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "DoctorId is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "DoctorName is required")]
        public string DoctorName { get; set; } = string.Empty;

        public decimal ClinicFee { get; set; } = 150000; // Default standard clinic fee

        public string? Notes { get; set; }

        [Required(ErrorMessage = "Items list is required")]
        [MinLength(1, ErrorMessage = "Prescription must contain at least one medicine item")]
        public List<PrescriptionCreatedEventItemDto> Items { get; set; } = new();
    }

    public class PrescriptionCreatedEventItemDto
    {
        [Required(ErrorMessage = "MedicineId is required")]
        public Guid MedicineId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Dosage instructions are required")]
        public string Dosage { get; set; } = string.Empty;
    }
}
