using System.Text.Json;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    public class AddTransactionService
    {
        private readonly string _filePath;
        private readonly AuthenticationService _authService;

        public AddTransactionService(AuthenticationService authService)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "transactions.json");
            _authService = authService;
        }

        private async Task<List<Transaction>> LoadTransactionsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Transaction>();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
        }

        private async Task SaveTransactionsAsync(List<Transaction> transactions)
        {
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync()
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to view transactions");

            var transactions = await LoadTransactionsAsync();
            return transactions.Where(t => t.UserId == _authService.CurrentUser!.Id).ToList();
        }

        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to add transactions");

            var transactions = await LoadTransactionsAsync();

            transaction.UserId = _authService.CurrentUser!.Id;
            transaction.Id = transactions.Count > 0 ? transactions.Max(t => t.Id) + 1 : 1;
            transaction.Date = transaction.Date ?? DateTime.UtcNow;

            transactions.Add(transaction);
            await SaveTransactionsAsync(transactions);

            return transaction;
        }
    }
}