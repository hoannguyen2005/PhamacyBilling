using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyBillingService.Data;
using PharmacyBillingService.Domain;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly PharmacyDbContext _context;

        public MedicineService(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicineDto>> GetAllAsync(string? search = null)
        {
            var query = _context.Medicines
                .Include(m => m.Stock)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(lowerSearch) 
                                      || (m.ActiveIngredient != null && m.ActiveIngredient.ToLower().Contains(lowerSearch)));
            }

            var medicines = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            return medicines.Select(MapToDto);
        }

        public async Task<MedicineDto?> GetByIdAsync(Guid id)
        {
            var medicine = await _context.Medicines
                .Include(m => m.Stock)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null) return null;

            return MapToDto(medicine);
        }

        public async Task<MedicineDto> CreateAsync(CreateMedicineDto request)
        {
            var medicine = new Medicine
            {
                Name = request.Name,
                ActiveIngredient = request.ActiveIngredient,
                Unit = request.Unit,
                Price = request.Price,
                IsActive = true
            };

            var stock = new MedicineStock
            {
                MedicineId = medicine.Id,
                Quantity = request.InitialStock,
                MinAlertQuantity = request.MinAlertQuantity,
                LastUpdated = DateTime.UtcNow
            };

            medicine.Stock = stock;

            _context.Medicines.Add(medicine);
            _context.MedicineStocks.Add(stock);
            await _context.SaveChangesAsync();

            return MapToDto(medicine);
        }

        public async Task<MedicineDto?> UpdateAsync(Guid id, UpdateMedicineDto request)
        {
            var medicine = await _context.Medicines
                .Include(m => m.Stock)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null) return null;

            medicine.Name = request.Name;
            medicine.ActiveIngredient = request.ActiveIngredient;
            medicine.Unit = request.Unit;
            medicine.Price = request.Price;
            medicine.IsActive = request.IsActive;

            if (medicine.Stock != null)
            {
                medicine.Stock.MinAlertQuantity = request.MinAlertQuantity;
                medicine.Stock.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(medicine);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return false;

            // Soft delete by disabling active flag so historical prescriptions remain valid
            medicine.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReplenishStockAsync(Guid id, int quantity)
        {
            var stock = await _context.MedicineStocks.FirstOrDefaultAsync(s => s.MedicineId == id);
            if (stock == null) return false;

            stock.Quantity += quantity;
            stock.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static MedicineDto MapToDto(Medicine m)
        {
            return new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                ActiveIngredient = m.ActiveIngredient,
                Unit = m.Unit,
                Price = m.Price,
                IsActive = m.IsActive,
                StockQuantity = m.Stock?.Quantity ?? 0,
                MinAlertQuantity = m.Stock?.MinAlertQuantity ?? 10,
                CreatedAt = m.CreatedAt
            };
        }
    }
}
