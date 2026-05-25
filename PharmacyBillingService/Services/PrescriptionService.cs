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
    public class PrescriptionService : IPrescriptionService
    {
        private readonly PharmacyDbContext _context;
        private readonly IBillService _billService;

        public PrescriptionService(PharmacyDbContext context, IBillService billService)
        {
            _context = context;
            _billService = billService;
        }

        public async Task<IEnumerable<PrescriptionDto>> GetAllAsync(string? status = null)
        {
            var query = _context.Prescriptions
                .Include(p => p.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status.ToLower() == status.ToLower());
            }

            var prescriptions = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return prescriptions.Select(MapToDto);
        }

        public async Task<PrescriptionDto?> GetByIdAsync(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return null;

            return MapToDto(prescription);
        }

        public async Task<PrescriptionDto> ProcessPrescriptionEventAsync(PrescriptionCreatedEventDto eventDto)
        {
            // Idempotency check: verify if the prescription is already handled
            var existingPrescription = await _context.Prescriptions
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == eventDto.PrescriptionId);

            if (existingPrescription != null)
            {
                return MapToDto(existingPrescription);
            }

            // Create prescription object
            var prescription = new Prescription
            {
                Id = eventDto.PrescriptionId,
                PatientId = eventDto.PatientId,
                PatientName = eventDto.PatientName,
                DoctorId = eventDto.DoctorId,
                DoctorName = eventDto.DoctorName,
                CreatedDate = DateTime.UtcNow,
                Notes = eventDto.Notes
            };

            var prescriptionItems = new List<PrescriptionItem>();
            bool hasStockIssue = false;

            // Validate all items
            foreach (var itemDto in eventDto.Items)
            {
                var medicine = await _context.Medicines
                    .Include(m => m.Stock)
                    .FirstOrDefaultAsync(m => m.Id == itemDto.MedicineId);

                if (medicine == null || !medicine.IsActive || medicine.Stock == null || medicine.Stock.Quantity < itemDto.Quantity)
                {
                    hasStockIssue = true;
                }

                prescriptionItems.Add(new PrescriptionItem
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = itemDto.MedicineId,
                    MedicineName = medicine?.Name ?? "Unknown Medicine",
                    Quantity = itemDto.Quantity,
                    UnitPrice = medicine?.Price ?? 0,
                    Dosage = itemDto.Dosage
                });
            }

            if (hasStockIssue)
            {
                // Save prescription as Draft due to stock insufficiency, do NOT deduct inventory
                prescription.Status = Prescription.StatusDraft;
                prescription.Notes = "[STOCK ISSUES DETECTED] " + (prescription.Notes ?? "");
            }
            else
            {
                // Stock is sufficient, approve prescription and deduct inventory
                prescription.Status = Prescription.StatusApproved;

                foreach (var item in prescriptionItems)
                {
                    var stock = await _context.MedicineStocks.FirstAsync(s => s.MedicineId == item.MedicineId);
                    stock.Quantity -= item.Quantity;
                    stock.LastUpdated = DateTime.UtcNow;
                }
            }

            prescription.Items = prescriptionItems;
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Generate the Bill automatically if prescription status is Approved
            if (prescription.Status == Prescription.StatusApproved)
            {
                await _billService.CreateBillFromPrescriptionAsync(prescription, eventDto.ClinicFee);
            }

            return MapToDto(prescription);
        }

        private static PrescriptionDto MapToDto(Prescription p)
        {
            return new PrescriptionDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                PatientName = p.PatientName,
                DoctorId = p.DoctorId,
                DoctorName = p.DoctorName,
                CreatedDate = p.CreatedDate,
                Status = p.Status,
                Notes = p.Notes,
                Items = p.Items.Select(i => new PrescriptionItemDto
                {
                    Id = i.Id,
                    MedicineId = i.MedicineId,
                    MedicineName = i.MedicineName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Dosage = i.Dosage
                }).ToList()
            };
        }
    }
}
