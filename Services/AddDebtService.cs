using System.Text.Json;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    public class AddDebtService
    {
        private readonly string _filePath;
        private readonly AuthenticationService _authService;

        public AddDebtService(AuthenticationService authService)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "debts.json");
            _authService = authService;
        }

        private async Task<List<Debt>> LoadDebtsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Debt>();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
        }

        private async Task SaveDebtsAsync(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<Debt>> GetUserDebtsAsync()
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to view debts");

            var debts = await LoadDebtsAsync();
            return debts.Where(d => d.UserId == _authService.CurrentUser!.Id).ToList();
        }

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

        public async Task<List<Debt>> GetPendingDebtsAsync()
        {
            var userDebts = await GetUserDebtsAsync();
            return userDebts.Where(d => d.Status == "Pending").ToList();
        }
    }
}