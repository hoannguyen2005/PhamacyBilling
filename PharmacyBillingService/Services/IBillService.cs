using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyBillingService.Domain;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Services
{
    public interface IBillService
    {
        Task<IEnumerable<BillDto>> GetAllAsync(string? status = null);
        Task<BillDto?> GetByIdAsync(Guid id);
        Task<BillDto> CreateBillFromPrescriptionAsync(Prescription prescription, decimal clinicFee);
        Task<BillDto> CreateManualBillAsync(ManualBillRequestDto request);
        Task<PaymentResponseDto> PayBillAsync(Guid id, PaymentRequestDto request, Guid receivedById);
    }
}
