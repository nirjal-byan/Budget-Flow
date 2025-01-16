using BudgetFlow.Models;
using System.Text.Json;

namespace BudgetFlow.Services
{
    // Service to handle user authentication (login, logout, registration)
    public class AuthenticationService
    {
        private User? _currentUser;
        private readonly string _filePath;
        public event Action? OnAuthenticationStateChanged;

        public AuthenticationService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "users.json");
        }

        public bool IsAuthenticated => _currentUser != null;
        public User? CurrentUser => _currentUser;

        // Loads the list of users from the JSON file
        private async Task<List<User>> LoadUsersAsync()
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        // Saves the list of users into the JSON file
        private async Task SaveUsersAsync(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        // Attempts to log in a user by checking their username and password
        public async Task<bool> LoginAsync(string username, string password)
        {
            var users = await LoadUsersAsync();
            var user = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password.Equals(password));

            if (user != null)
            {
                _currentUser = user;
                OnAuthenticationStateChanged?.Invoke();
                return true;
            }

            return false;
        }

        // Registers a new user with a username, password, and currency
        public async Task<(bool success, string message)> RegisterAsync(string username, string password, string currency)
        {
            var users = await LoadUsersAsync();

            // Check if the username already exists
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return (false, "Username already exists");

            var newUser = new User
            {
                Username = username,
                Password = password,
                Currency = currency
            };

            users.Add(newUser);
            await SaveUsersAsync(users);

            return (true, "Registration successful");
        }

        // Logs out the current user
        public async Task LogoutAsync()
        {
            _currentUser = null;
            OnAuthenticationStateChanged?.Invoke();
        }
    }
}
