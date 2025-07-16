using System.Text.Json;
using AudioWorkstation.Core.Models;

namespace AudioWorkstation.Core.Services;

public interface IProjectManager
{
    Task<bool> SaveProjectAsync(string filePath, AudioProject project);
    Task<AudioProject?> LoadProjectAsync(string filePath);
    AudioProject CreateNewProject(string name);
}

public class AudioProject
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public List<AudioChannelData> Channels { get; set; } = new();
    public EqualizerSettings MasterEqualizer { get; set; } = new();
    public float MasterVolume { get; set; } = 1.0f;
    public bool IsMasterMuted { get; set; } = false;
    public DuckingSettings DuckingSettings { get; set; } = new();
}

public class AudioChannelData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty; // "MP3", "YouTube", "Live"
    public string SourcePath { get; set; } = string.Empty;
    public float Volume { get; set; } = 1.0f;
    public bool IsMuted { get; set; } = false;
    public EqualizerSettings Equalizer { get; set; } = new();
    public List<EffectData> Effects { get; set; } = new();
}

public class EffectData
{
    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ProjectManager : IProjectManager
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ProjectManager()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> SaveProjectAsync(string filePath, AudioProject project)
    {
        try
        {
            project.LastModified = DateTime.Now;
            var json = JsonSerializer.Serialize(project, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error saving project: {ex.Message}");
            return false;
        }
    }

    public async Task<AudioProject?> LoadProjectAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<AudioProject>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading project: {ex.Message}");
            return null;
        }
    }

    public AudioProject CreateNewProject(string name)
    {
        return new AudioProject
        {
            Name = name,
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };
    }
}