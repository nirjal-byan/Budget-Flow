using BudgetFlow.Models;
using System.Threading.Tasks;

namespace BudgetFlow.Services
{
    public class BalanceService
    {
        private readonly AddTransactionService _transactionService;
        private readonly AddDebtService _debtService;

        public BalanceService(AddTransactionService transactionService, AddDebtService debtService)
        {
            _transactionService = transactionService;
            _debtService = debtService;
        }

        public async Task<decimal> GetCurrentBalanceAsync()
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();
            decimal balance = 0;

            foreach (var transaction in transactions)
            {
                if (transaction.Type == "Credit")
                    balance += transaction.Amount;
                else if (transaction.Type == "Debit")
                    balance -= transaction.Amount;
            }

            return balance;
        }

        public async Task<decimal> GetTotalPendingDebtsAsync()
        {
            var pendingDebts = await _debtService.GetPendingDebtsAsync();
            return pendingDebts.Sum(d => d.Amount);
        }

        public async Task<decimal> GetAvailableBalanceAsync()
        {
            var currentBalance = await GetCurrentBalanceAsync();
            var pendingDebts = await GetTotalPendingDebtsAsync();
            return currentBalance - pendingDebts;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            return new DashboardSummary
            {
                CurrentBalance = await GetCurrentBalanceAsync(),
                TotalPendingDebts = await GetTotalPendingDebtsAsync(),
                AvailableBalance = await GetAvailableBalanceAsync()
            };
        }
    }

    public class DashboardSummary
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalPendingDebts { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}