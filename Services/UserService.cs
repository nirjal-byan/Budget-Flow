using System.Text.Json;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    // Service to handle user registration, login, and user data persistence
    public class UserService
    {
        private readonly string _filePath = "users.json";
        private List<User> _users = new();

        public UserService()
        {
            LoadUsers();
        }

        // Registers a new user if the username is not already taken
        public bool Register(string username, string password, string currency)
        {
            if (_users.Any(u => u.Username == username))
                return false;

            _users.Add(new User { Username = username, Password = password, Currency = currency });
            SaveUsers();
            return true;
        }

        // Checks if the provided username and password match an existing user
        public bool Login(string username, string password)
        {
            return _users.Any(u => u.Username == username && u.Password == password);
        }

        // Loads the list of users from the JSON file
        private void LoadUsers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
        }

        // Serializes the list of users and saves it to the JSON file
        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users);
            File.WriteAllText(_filePath, json);
        }
    }
}
