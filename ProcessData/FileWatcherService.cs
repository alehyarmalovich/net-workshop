using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessData
{
    public class FileWatcherService : BackgroundService
    {
        private readonly string _inputDirectory;
        private readonly string _processedDirectory;
        private readonly string _connectionString;
        private readonly ILogger<FileWatcherService> _logger;
        private FileSystemWatcher _fileWatcher;
        private readonly ConcurrentDictionary<string, bool> _processedFiles;

        public FileWatcherService(IConfiguration configuration, ILogger<FileWatcherService> logger)
        {
            _inputDirectory = configuration["GenerationFolder"];
            _processedDirectory = configuration["ProcessedFolder"];
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _processedFiles = new ConcurrentDictionary<string, bool>();

            if (!Directory.Exists(_inputDirectory))
            {
                Directory.CreateDirectory(_inputDirectory);
            }

            if (!Directory.Exists(_processedDirectory))
            {
                Directory.CreateDirectory(_processedDirectory);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _fileWatcher = new FileSystemWatcher(_inputDirectory, "*.json")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _fileWatcher.Created += OnCreated;
            _fileWatcher.Changed += OnChanged;
            _fileWatcher.EnableRaisingEvents = true;

            _logger.LogInformation("FileWatcherService started, watching directory: {Directory}", _inputDirectory);

            return Task.CompletedTask;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File created: {FilePath}", e.FullPath);
            ProcessFile(e.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File changed: {FilePath}", e.FullPath);
            ProcessFile(e.FullPath);
        }

        private void ProcessFile(string filePath)
        {
            if (_processedFiles.ContainsKey(filePath))
            {
                _logger.LogInformation("File already processed: {FilePath}", filePath);
                return;
            }

            try
            {
                _logger.LogInformation("Processing file: {FilePath}", filePath);

                var jsonData = File.ReadAllText(filePath);
                var userData = JsonSerializer.Deserialize<UserData>(jsonData);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Users (Username, Email, CreatedAt) VALUES (@Username, @Email, @CreatedAt)";
                    command.Parameters.AddWithValue("@Username", userData.Username);
                    command.Parameters.AddWithValue("@Email", userData.Email);
                    command.Parameters.AddWithValue("@CreatedAt", userData.CreatedAt);
                    command.ExecuteNonQuery();
                }

                // Move the file to the processed directory after processing
                var processedFilePath = Path.Combine(_processedDirectory, Path.GetFileName(filePath));
                File.Move(filePath, processedFilePath);

                _processedFiles[filePath] = true;

                _logger.LogInformation("Processed and moved file: {FilePath} to {ProcessedFilePath}", filePath, processedFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
            }
        }

        public override void Dispose()
        {
            _fileWatcher?.Dispose();
            base.Dispose();
        }

        public class UserData
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
