using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AudioWorkstation.UI.Views;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace AudioWorkstation.UI.Services;

public interface IDialogService
{
    Task<string[]?> ShowOpenFileDialogAsync(string title, FilePickerFileType[] fileTypes);
    Task<string?> ShowSaveFileDialogAsync(string title, FilePickerFileType[] fileTypes);
    Task<bool> ShowConfirmationDialogAsync(string title, string message);
    Task ShowMessageDialogAsync(string title, string message);
    Task ShowSettingsDialogAsync();
    Task ShowAboutDialogAsync();
}

public class DialogService : IDialogService
{
    private readonly Window _parentWindow;

    public DialogService(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }

    public async Task<string[]?> ShowOpenFileDialogAsync(string title, FilePickerFileType[] fileTypes)
    {
        var files = await _parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        return files?.Select(f => f.Path.LocalPath).ToArray();
    }

    public async Task<string?> ShowSaveFileDialogAsync(string title, FilePickerFileType[] fileTypes)
    {
        var file = await _parentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = fileTypes
        });

        return file?.Path.LocalPath;
    }

    public async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
        };

        var panel = new StackPanel { Margin = new Thickness(20) };
        
        panel.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 80,
            Height = 30
        };

        bool result = false;
        okButton.Click += (s, e) => { result = true; dialog.Close(); };
        cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;
        await dialog.ShowDialog(_parentWindow);
        return result;
    }

    public async Task ShowMessageDialogAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
        };

        var panel = new StackPanel { Margin = new Thickness(20) };
        
        panel.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            Height = 30,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        okButton.Click += (s, e) => dialog.Close();
        panel.Children.Add(okButton);

        dialog.Content = panel;
        await dialog.ShowDialog(_parentWindow);
    }

    public async Task ShowSettingsDialogAsync()
    {
        var settingsWindow = new SettingsWindow();
        await settingsWindow.ShowDialog(_parentWindow);
    }

    public async Task ShowAboutDialogAsync()
    {
        var aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog(_parentWindow);
    }
}