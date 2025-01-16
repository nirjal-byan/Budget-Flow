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

        public async Task DeleteTransactionAsync(int transactionId)
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to delete transactions");

            var transactions = await LoadTransactionsAsync();
            var transactionToDelete = transactions.FirstOrDefault(t => t.Id == transactionId && t.UserId == _authService.CurrentUser!.Id);

            if (transactionToDelete == null)
                throw new InvalidOperationException("Transaction not found or does not belong to the user");

            transactions.Remove(transactionToDelete);
            await SaveTransactionsAsync(transactions);
        }

        public async Task UpdateTransactionAsync(Transaction updatedTransaction)
        {
            if (!_authService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be logged in to update transactions");

            var transactions = await LoadTransactionsAsync();
            var transactionToUpdate = transactions.FirstOrDefault(t => t.Id == updatedTransaction.Id && t.UserId == _authService.CurrentUser!.Id);

            if (transactionToUpdate == null)
                throw new InvalidOperationException("Transaction not found or does not belong to the user");

            transactionToUpdate.Title = updatedTransaction.Title;
            transactionToUpdate.Amount = updatedTransaction.Amount;
            transactionToUpdate.Type = updatedTransaction.Type;
            transactionToUpdate.Tag = updatedTransaction.Tag;
            transactionToUpdate.Date = updatedTransaction.Date;
            transactionToUpdate.Notes = updatedTransaction.Notes;

            await SaveTransactionsAsync(transactions);
        }
    }
}
