using System.ComponentModel.DataAnnotations;

namespace VendorX.ViewModels
{
    public class DailyReportViewModel
    {
        public DateTime ReportDate { get; set; } = DateTime.Today;
        public decimal TotalSales { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalBakiAmount { get; set; }
        public List<TransactionSummary> Transactions { get; set; } = new();
    }

    public class BakiReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDue { get; set; }
        public decimal TotalSettled { get; set; }
        public List<CustomerBakiSummary> CustomerBakis { get; set; } = new();
    }

    public class ExpenseReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<ExpenseSummary> Expenses { get; set; } = new();
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    }

    public class ProfitExpenseReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class TransactionSummary
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
    }

    public class CustomerBakiSummary
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal DueAmount { get; set; }
        public decimal SettledAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ExpenseSummary
    {
        public string ExpenseName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
    }
}
