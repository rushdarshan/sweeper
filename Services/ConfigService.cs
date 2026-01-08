using System;
using System.IO;
using System.Text.Json;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Services
{
    /// <summary>
    /// Handles loading and saving application configuration
    /// </summary>
    public class ConfigService
    {
        private readonly string _configPath;

        public ConfigService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ScreenshotSweeper");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, Helpers.Constants.CONFIG_FILE_NAME);
        }

        /// <summary>
        /// Loads configuration from disk, returns default if not found
        /// </summary>
        public AppConfig LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return AppConfig.Default();

                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                return config ?? AppConfig.Default();
            }
            catch
            {
                return AppConfig.Default();
            }
        }

        /// <summary>
        /// Saves configuration to disk
        /// </summary>
        public void SaveConfig(AppConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
