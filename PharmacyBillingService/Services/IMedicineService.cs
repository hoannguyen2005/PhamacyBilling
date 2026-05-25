using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Services
{
    public interface IMedicineService
    {
        Task<IEnumerable<MedicineDto>> GetAllAsync(string? search = null);
        Task<MedicineDto?> GetByIdAsync(Guid id);
        Task<MedicineDto> CreateAsync(CreateMedicineDto request);
        Task<MedicineDto?> UpdateAsync(Guid id, UpdateMedicineDto request);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ReplenishStockAsync(Guid id, int quantity);
    }
}
