using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    // Service to handle debt history operations including filtering, retrieving distinct tags, and fetching top pending debts by amount
    public class DebtHistoryService
    {
        private readonly AddDebtService _addDebtService;

        public DebtHistoryService(AddDebtService addDebtService)
        {
            _addDebtService = addDebtService;
        }

        // Filters debts based on a search term (title or notes) and a specific tag (or all tags)
        public async Task<List<Debt>> GetFilteredDebtsAsync(string searchTerm, string tag)
        {
            var debts = await _addDebtService.GetUserDebtsAsync();

            // Filter debts based on search term and tag
            return debts
                .Where(d => (string.IsNullOrWhiteSpace(searchTerm) ||
                            d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            d.Notes.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                            (tag == "All" || d.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // Retrieves a list of distinct tags from the user's debts
        public async Task<List<string>> GetDistinctTagsAsync()
        {
            var debts = await _addDebtService.GetUserDebtsAsync();

            // Extract distinct tags from the debts, excluding null or whitespace tags
            return debts
                .Select(d => d.Tag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();
        }

        // Retrieves the top 'count' pending debts ordered by the highest remaining amount, with optional date filters
        public async Task<List<Debt>> GetTopPendingDebtsByAmountAsync(int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            var debts = await _addDebtService.GetUserDebtsAsync();

            // Filter debts by start and end date if provided
            if (startDate.HasValue)
                debts = debts.Where(d => d.DueDate >= startDate.Value).ToList();
            if (endDate.HasValue)
                debts = debts.Where(d => d.DueDate <= endDate.Value).ToList();

            // Retrieve top 'count' pending debts ordered by remaining amount in descending order
            return debts
                .Where(d => d.Status == "Pending")
                .OrderByDescending(d => d.RemainingAmount)
                .Take(count)
                .ToList();
        }
    }
}
