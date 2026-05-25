using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyBillingService.DTOs
{
    public class MedicineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ActiveIngredient { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }
        public int MinAlertQuantity { get; set; }
        public bool IsLowStock => StockQuantity <= MinAlertQuantity;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMedicineDto
    {
        [Required(ErrorMessage = "Medicine name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Active ingredient name cannot exceed 100 characters")]
        public string? ActiveIngredient { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        public string Unit { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a positive number")]
        public int InitialStock { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Min alert quantity must be a positive number")]
        public int MinAlertQuantity { get; set; } = 10;
    }

    public class UpdateMedicineDto
    {
        [Required(ErrorMessage = "Medicine name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Active ingredient name cannot exceed 100 characters")]
        public string? ActiveIngredient { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        public string Unit { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Min alert quantity must be a positive number")]
        public int MinAlertQuantity { get; set; }
    }

    public class StockUpdateDto
    {
        [Required(ErrorMessage = "Stock quantity change is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
