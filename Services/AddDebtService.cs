using System.Text.Json;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    // Service to manage debt-related operations (CRUD operations on debts)
    public class AddDebtService
    {
        private readonly string _filePath;
        private readonly AuthenticationService _authService;

        // Initializes the file path and authentication service
        public AddDebtService(AuthenticationService authService)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "debts.json");
            _authService = authService;
        }

        // Loads the list of debts from the JSON file
        private async Task<List<Debt>> LoadDebtsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Debt>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
        }

        // Saves the list of debts into the JSON file
        private async Task SaveDebtsAsync(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        // Retrieves the debts associated with the currently authenticated user
        public async Task<List<Debt>> GetUserDebtsAsync()
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to view debts");
            var debts = await LoadDebtsAsync();
            return debts.Where(d => d.UserId == _authService.CurrentUser!.Id).ToList();
        }

        // Retrieves the pending debts associated with the currently authenticated user
        public async Task<List<Debt>> GetPendingDebtsAsync()
        {
            var userDebts = await GetUserDebtsAsync();
            return userDebts.Where(d => d.Status == "Pending").ToList();
        }

        // Adds a new debt for the currently authenticated user
        public async Task<Debt> AddDebtAsync(Debt debt)
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to add debts");
            var debts = await LoadDebtsAsync();
            debt.UserId = _authService.CurrentUser!.Id;
            debt.DueDate = debt.DueDate ?? DateTime.UtcNow;
            debts.Add(debt);
            await SaveDebtsAsync(debts);
            return debt;
        }

        // Updates the debt payment and adjusts the debt amount
        public async Task<Debt> UpdateDebtPaymentAsync(string debtId, decimal paymentAmount)
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to update debts");

            var debts = await LoadDebtsAsync();
            var debt = debts.FirstOrDefault(d => d.Id == debtId && d.UserId == _authService.CurrentUser!.Id);

            if (debt == null)
                throw new KeyNotFoundException("Debt not found");

            if (paymentAmount > debt.RemainingAmount)
                throw new InvalidOperationException("Payment amount exceeds remaining debt");

            debt.AmountPaid += paymentAmount;
            debt.LastPaymentDate = DateTime.UtcNow;

            if (debt.RemainingAmount == 0)
            {
                debt.Status = "Paid";
            }

            await SaveDebtsAsync(debts);
            return debt;
        }

        // Updates the status of debts based on their due date and remaining amount
        public async Task UpdateDebtStatusAsync()
        {
            var debts = await LoadDebtsAsync();
            var currentDate = DateTime.UtcNow;

            // Update the status of debts that are overdue
            foreach (var debt in debts.Where(d => d.Status == "Pending"))
            {
                if (debt.DueDate < currentDate && debt.RemainingAmount > 0)
                {
                    debt.Status = "Overdue";
                }
            }

            await SaveDebtsAsync(debts);
        }
    }
}