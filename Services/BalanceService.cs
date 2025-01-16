using BudgetFlow.Models;
using System.Threading.Tasks;
namespace BudgetFlow.Services
{
    // Service to handle balance calculations including current balance, pending debts, and available balance
    public class BalanceService
    {
        private readonly AddTransactionService _transactionService;
        private readonly AddDebtService _debtService;

        public BalanceService(AddTransactionService transactionService, AddDebtService debtService)
        {
            _transactionService = transactionService;
            _debtService = debtService;
        }

        // Calculates the current balance based on transactions (optional date range filter)
        public async Task<decimal> GetCurrentBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();

            // Filter transactions based on provided date range
            if (startDate.HasValue)
                transactions = transactions.Where(t => t.Date >= startDate.Value).ToList();
            if (endDate.HasValue)
                transactions = transactions.Where(t => t.Date <= endDate.Value).ToList();

            decimal balance = 0;

            // Iterate over transactions to calculate balance based on type (Credit/Debit)
            foreach (var transaction in transactions)
            {
                if (transaction.Type == "Credit")
                    balance += transaction.Amount;
                else if (transaction.Type == "Debit")
                    balance -= transaction.Amount;
            }
            return balance;
        }

        // GetTotalPendingDebtsAsync: Calculates the total amount of pending debts (optional date range filter)
        public async Task<decimal> GetTotalPendingDebtsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var pendingDebts = await _debtService.GetPendingDebtsAsync();

            if (startDate.HasValue)
                pendingDebts = pendingDebts.Where(d => d.DueDate >= startDate.Value).ToList();
            if (endDate.HasValue)
                pendingDebts = pendingDebts.Where(d => d.DueDate <= endDate.Value).ToList();

            return pendingDebts.Sum(d => d.Amount);
        }

        // Calculates the available balance by subtracting total pending debts from the current balance
        public async Task<decimal> GetAvailableBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentBalance = await GetCurrentBalanceAsync(startDate, endDate);
            var pendingDebts = await GetTotalPendingDebtsAsync(startDate, endDate);
            return currentBalance - pendingDebts;
        }

        // Checks if the current balance is sufficient to cover the specified amount
        public async Task<bool> HasSufficientBalanceAsync(decimal amount)
        {
            var currentBalance = await GetCurrentBalanceAsync();
            return currentBalance >= amount;
        }

        // Returns a summary of the user's financial status (current balance, total pending debts, available balance)
        public async Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return new DashboardSummary
            {
                CurrentBalance = await GetCurrentBalanceAsync(startDate, endDate),
                TotalPendingDebts = await GetTotalPendingDebtsAsync(startDate, endDate),
                AvailableBalance = await GetAvailableBalanceAsync(startDate, endDate)
            };
        }
    }

    // Class to represent a dashboard summary with current balance, total pending debts, and available balance
    public class DashboardSummary
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalPendingDebts { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}