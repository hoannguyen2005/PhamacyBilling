using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacyBillingService.DTOs
{
    public class BillDto
    {
        public Guid Id { get; set; }
        public Guid? PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal ClinicFee { get; set; }
        public decimal MedicineFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<BillItemDto> BillItems { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class BillItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid ReceivedById { get; set; }
        public string ReceivedByName { get; set; } = string.Empty;
    }

    public class ManualBillRequestDto
    {
        [Required(ErrorMessage = "PatientId is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "PatientName is required")]
        [StringLength(100, ErrorMessage = "PatientName cannot exceed 100 characters")]
        public string PatientName { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Clinic fee must be positive")]
        public decimal ClinicFee { get; set; } = 150000;

        [Required(ErrorMessage = "Itemized descriptions are required")]
        [MinLength(1, ErrorMessage = "At least one item details must be specified")]
        public List<ManualBillItemRequestDto> CustomItems { get; set; } = new();
    }

    public class ManualBillItemRequestDto
    {
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
    }

    public class PaymentRequestDto
    {
        [Required(ErrorMessage = "PaymentMethod is required")]
        [RegularExpression("^(Cash|BankTransfer|POS)$", ErrorMessage = "PaymentMethod must be 'Cash', 'BankTransfer', or 'POS'")]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? TransactionCode { get; set; }
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid BillId { get; set; }
        public decimal TotalPaid { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
    }
}
