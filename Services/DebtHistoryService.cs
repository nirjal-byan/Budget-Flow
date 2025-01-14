using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    public class DebtHistoryService
    {
        private readonly AddDebtService _addDebtService;

        public DebtHistoryService(AddDebtService addDebtService)
        {
            _addDebtService = addDebtService;
        }

        public async Task<List<Debt>> GetFilteredDebtsAsync(string searchTerm, string tag)
        {
            var debts = await _addDebtService.GetUserDebtsAsync();

            return debts
                .Where(d => (string.IsNullOrWhiteSpace(searchTerm) ||
                            d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            d.Notes.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                            (tag == "All" || d.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<List<string>> GetDistinctTagsAsync()
        {
            var debts = await _addDebtService.GetUserDebtsAsync();

            return debts
                .Select(d => d.Tag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();
        }
    }
}
