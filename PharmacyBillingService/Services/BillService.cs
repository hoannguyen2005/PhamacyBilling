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
    public class BillService : IBillService
    {
        private readonly PharmacyDbContext _context;

        public BillService(PharmacyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BillDto>> GetAllAsync(string? status = null)
        {
            var query = _context.Bills
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                    .ThenInclude(p => p.ReceivedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.PaymentStatus.ToLower() == status.ToLower());
            }

            var bills = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bills.Select(MapToDto);
        }

        public async Task<BillDto?> GetByIdAsync(Guid id)
        {
            var bill = await _context.Bills
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                    .ThenInclude(p => p.ReceivedBy)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return null;

            return MapToDto(bill);
        }

        public async Task<BillDto> CreateBillFromPrescriptionAsync(Prescription prescription, decimal clinicFee)
        {
            // Calculate medicine fee
            decimal medicineFee = prescription.Items.Sum(item => item.Quantity * item.UnitPrice);
            decimal totalAmount = clinicFee + medicineFee;

            var bill = new Bill
            {
                PrescriptionId = prescription.Id,
                PatientId = prescription.PatientId,
                PatientName = prescription.PatientName,
                ClinicFee = clinicFee,
                MedicineFee = medicineFee,
                TotalAmount = totalAmount,
                PaymentStatus = Bill.StatusPending,
                CreatedAt = DateTime.UtcNow
            };

            var billItems = new List<BillItem>();

            // Add clinic fee line item if it's greater than 0
            if (clinicFee > 0)
            {
                billItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Description = "Phí khám lâm sàng",
                    Amount = clinicFee
                });
            }

            // Add medicine line items
            foreach (var item in prescription.Items)
            {
                billItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Description = $"Thuốc: {item.MedicineName} ({item.Quantity} {item.Dosage})",
                    Amount = item.Quantity * item.UnitPrice
                });
            }

            bill.BillItems = billItems;

            _context.Bills.Add(bill);
            _context.BillItems.AddRange(billItems);
            // Notice: Prescription status updates and stock deductions were handled before calling this method.
            // No context save here as this is usually called inside the same transaction of Prescription creation.
            return MapToDto(bill);
        }

        public async Task<BillDto> CreateManualBillAsync(ManualBillRequestDto request)
        {
            decimal itemsTotal = request.CustomItems.Sum(item => item.Amount);
            decimal totalAmount = request.ClinicFee + itemsTotal;

            var bill = new Bill
            {
                PatientId = request.PatientId,
                PatientName = request.PatientName,
                ClinicFee = request.ClinicFee,
                MedicineFee = itemsTotal,
                TotalAmount = totalAmount,
                PaymentStatus = Bill.StatusPending,
                CreatedAt = DateTime.UtcNow
            };

            var billItems = new List<BillItem>();

            if (request.ClinicFee > 0)
            {
                billItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Description = "Phí khám lâm sàng",
                    Amount = request.ClinicFee
                });
            }

            foreach (var item in request.CustomItems)
            {
                billItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Description = item.Description,
                    Amount = item.Amount
                });
            }

            bill.BillItems = billItems;

            _context.Bills.Add(bill);
            _context.BillItems.AddRange(billItems);
            await _context.SaveChangesAsync();

            return MapToDto(bill);
        }

        public async Task<PaymentResponseDto> PayBillAsync(Guid id, PaymentRequestDto request, Guid receivedById)
        {
            var bill = await _context.Bills
                .Include(b => b.Prescription)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                return new PaymentResponseDto { Success = false, Message = "Hóa đơn không tồn tại." };
            }

            if (bill.PaymentStatus == Bill.StatusPaid)
            {
                return new PaymentResponseDto 
                { 
                    Success = false, 
                    Message = "Hóa đơn này đã được thanh toán trước đó.",
                    BillId = bill.Id,
                    TotalPaid = bill.TotalAmount,
                    CurrentStatus = bill.PaymentStatus
                };
            }

            if (bill.PaymentStatus == Bill.StatusCancelled)
            {
                return new PaymentResponseDto 
                { 
                    Success = false, 
                    Message = "Hóa đơn này đã bị hủy, không thể thanh toán.",
                    BillId = bill.Id,
                    TotalPaid = 0,
                    CurrentStatus = bill.PaymentStatus
                };
            }

            // Create payment transaction
            var payment = new Payment
            {
                BillId = bill.Id,
                AmountPaid = bill.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                TransactionCode = request.TransactionCode,
                PaymentDate = DateTime.UtcNow,
                ReceivedById = receivedById
            };

            bill.PaymentStatus = Bill.StatusPaid;
            bill.PaidAt = DateTime.UtcNow;
            _context.Payments.Add(payment);

            // If this bill was created from an approved prescription, transition the prescription status to 'Dispensed' (Xuất thuốc)
            if (bill.PrescriptionId.HasValue && bill.Prescription != null)
            {
                bill.Prescription.Status = Prescription.StatusDispensed;
            }

            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Success = true,
                Message = "Thanh toán hóa đơn và xuất kho thuốc thành công.",
                BillId = bill.Id,
                TotalPaid = bill.TotalAmount,
                CurrentStatus = bill.PaymentStatus
            };
        }

        private static BillDto MapToDto(Bill b)
        {
            return new BillDto
            {
                Id = b.Id,
                PrescriptionId = b.PrescriptionId,
                PatientId = b.PatientId,
                PatientName = b.PatientName,
                ClinicFee = b.ClinicFee,
                MedicineFee = b.MedicineFee,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                CreatedAt = b.CreatedAt,
                PaidAt = b.PaidAt,
                BillItems = b.BillItems.Select(bi => new BillItemDto
                {
                    Id = bi.Id,
                    Description = bi.Description,
                    Amount = bi.Amount
                }).ToList(),
                Payments = b.Payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    AmountPaid = p.AmountPaid,
                    PaymentMethod = p.PaymentMethod,
                    TransactionCode = p.TransactionCode,
                    PaymentDate = p.PaymentDate,
                    ReceivedById = p.ReceivedById,
                    ReceivedByName = p.ReceivedBy?.FullName ?? "Staff"
                }).ToList()
            };
        }
    }
}
