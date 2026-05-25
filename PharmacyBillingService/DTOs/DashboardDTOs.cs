using System;
using System.Collections.Generic;

namespace PharmacyBillingService.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int PaidBillsCount { get; set; }
        public int PendingBillsCount { get; set; }
        public int ActiveMedicinesCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockAlertsCount { get; set; }
        public List<RecentPaymentDto> RecentPayments { get; set; } = new();
    }

    public class RecentPaymentDto
    {
        public Guid BillId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public class RevenueChartDto
    {
        public List<RevenueChartPointDto> DailyRevenue { get; set; } = new();
    }

    public class RevenueChartPointDto
    {
        public string DateLabel { get; set; } = string.Empty; // e.g. "2026-05-25"
        public decimal ClinicRevenue { get; set; }
        public decimal MedicineRevenue { get; set; }
        public decimal TotalRevenue => ClinicRevenue + MedicineRevenue;
    }
}
