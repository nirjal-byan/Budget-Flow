using System.Text.Json;
using BudgetFlow.Models;

namespace BudgetFlow.Services
{
    public class UserService
    {
        private readonly string _filePath = "users.json";
        private List<User> _users = new();

        public UserService()
        {
            LoadUsers();
        }

        public bool Register(string username, string password, string currency)
        {
            if (_users.Any(u => u.Username == username))
                return false;

            _users.Add(new User { Username = username, Password = password, Currency = currency });
            SaveUsers();
            return true;
        }

        public bool Login(string username, string password)
        {
            return _users.Any(u => u.Username == username && u.Password == password);
        }

        private void LoadUsers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users);
            File.WriteAllText(_filePath, json);
        }
    }
}
