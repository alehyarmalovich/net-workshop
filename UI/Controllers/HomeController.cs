using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly string _userGenerationUrl;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
            _userGenerationUrl = configuration["UserGenerationUrl"] ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IActionResult> Index()
        {
            var users = await GetUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateUser()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(_userGenerationUrl, null);
                response.EnsureSuccessStatusCode();
            }

            return RedirectToAction("Index");
        }

        private async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT * FROM Users", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32("Id"),
                            Username = reader.GetString("Username"),
                            Email = reader.GetString("Email"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        });
                    }
                }
            }

            return users;
        }

        public new class User
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
