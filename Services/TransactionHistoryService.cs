using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetFlow.Models;
using BudgetFlow.Services;

namespace BudgetFlow.Services
{
    public class TransactionHistoryService
    {
        private readonly AddTransactionService _transactionService;

        public TransactionHistoryService(AddTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // Get filtered transactions by search term, type, and tag
        public async Task<List<Transaction>> GetFilteredTransactionsAsync(string searchTerm, string transactionType, string tag)
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();

            return transactions
                .Where(t => (string.IsNullOrWhiteSpace(searchTerm) ||
                            t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            t.Tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            t.Type.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                            (transactionType == "All" || t.Type.Equals(transactionType, StringComparison.OrdinalIgnoreCase)) &&
                            (tag == "All" || t.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // Get all unique tags from the transactions
        public async Task<List<string>> GetAllTagsAsync()
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();

            return transactions
                .Select(t => t.Tag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();
        }

        public async Task<List<Transaction>> GetTopTransactionsByAmountAsync(int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();

            if (startDate.HasValue)
                transactions = transactions.Where(t => t.Date >= startDate.Value).ToList();
            if (endDate.HasValue)
                transactions = transactions.Where(t => t.Date <= endDate.Value).ToList();

            return transactions
                .OrderByDescending(t => t.Amount)
                .Take(count)
                .ToList();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await _transactionService.DeleteTransactionAsync(id);
        }
    }
}
