using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Services
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<PrescriptionDto>> GetAllAsync(string? status = null);
        Task<PrescriptionDto?> GetByIdAsync(Guid id);
        Task<PrescriptionDto> ProcessPrescriptionEventAsync(PrescriptionCreatedEventDto eventDto);
    }
}
