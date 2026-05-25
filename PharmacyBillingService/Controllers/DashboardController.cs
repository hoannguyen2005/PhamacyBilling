using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyBillingService.Data;
using PharmacyBillingService.Domain;
using PharmacyBillingService.DTOs;

namespace PharmacyBillingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly PharmacyDbContext _context;

        public DashboardController(PharmacyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves aggregated administrative statistics (Revenue, Invoices count, Inventory alerts).
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardStatsDto))]
        public async Task<IActionResult> GetStats()
        {
            // Revenue aggregations
            decimal totalRevenue = await _context.Bills
                .Where(b => b.PaymentStatus == Bill.StatusPaid)
                .SumAsync(b => b.TotalAmount);

            int paidCount = await _context.Bills
                .CountAsync(b => b.PaymentStatus == Bill.StatusPaid);

            int pendingCount = await _context.Bills
                .CountAsync(b => b.PaymentStatus == Bill.StatusPending);

            int activeMedicines = await _context.Medicines
                .CountAsync(m => m.IsActive);

            int outOfStock = await _context.MedicineStocks
                .CountAsync(ms => ms.Quantity == 0);

            int lowStock = await _context.MedicineStocks
                .CountAsync(ms => ms.Quantity <= ms.MinAlertQuantity && ms.Quantity > 0);

            // Fetch recent payments
            var recentPayments = await _context.Payments
                .Include(p => p.Bill)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentPaymentDto
                {
                    BillId = p.BillId,
                    PatientName = p.Bill != null ? p.Bill.PatientName : "N/A",
                    Amount = p.AmountPaid,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            var stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                PaidBillsCount = paidCount,
                PendingBillsCount = pendingCount,
                ActiveMedicinesCount = activeMedicines,
                OutOfStockCount = outOfStock,
                LowStockAlertsCount = lowStock,
                RecentPayments = recentPayments
            };

            return Ok(stats);
        }

        /// <summary>
        /// Generates daily billing performance split by clinic fees and medicine costs for the past 7 days.
        /// </summary>
        [HttpGet("revenue-chart")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RevenueChartDto))]
        public async Task<IActionResult> GetRevenueChart()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            // Fetch paid bills in the last 7 days
            var paidBills = await _context.Bills
                .Where(b => b.PaymentStatus == Bill.StatusPaid && b.PaidAt >= startDate)
                .ToListAsync();

            var points = new List<RevenueChartPointDto>();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var dailyList = paidBills.Where(b => b.PaidAt.HasValue && b.PaidAt.Value.Date == date.Date).ToList();

                points.Add(new RevenueChartPointDto
                {
                    DateLabel = date.ToString("dd/MM"), // e.g. "25/05"
                    ClinicRevenue = dailyList.Sum(b => b.ClinicFee),
                    MedicineRevenue = dailyList.Sum(b => b.MedicineFee)
                });
            }

            return Ok(new RevenueChartDto { DailyRevenue = points });
        }
    }
}
