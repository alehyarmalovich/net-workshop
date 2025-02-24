using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace UserDataGeneration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerationController : ControllerBase
    {
        private readonly string _outputDirectory;
        private readonly IConfiguration _configuration;

        public GenerationController(IConfiguration configuration)
        {
            _configuration = configuration;
            _outputDirectory = _configuration["GenerationFolder"];

            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }
        }

        [HttpPost("generate")]
        public IActionResult Generate()
        {
            var result = GenerateUserData();
            return Ok(result);
        }

        private object GenerateUserData()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outputFilePath = Path.Combine(_outputDirectory, $"user_{timestamp}.json");

            var userData = new
            {
                Username = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var jsonData = JsonSerializer.Serialize(userData);
            System.IO.File.WriteAllText(outputFilePath, jsonData);

            Console.WriteLine($"Generated {outputFilePath} and saved it.");

            return new { status = "success", file = outputFilePath, data = userData };
        }
    }
}
