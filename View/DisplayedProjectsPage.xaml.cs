namespace Ashwell_Maintenance.View;

using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

public partial class DisplayedProjectsPage : ContentPage
{
    bool projectComplete = true;
    public async void JobsBack(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    public List<Folder> Folders = new();
    readonly bool finishedProjects;
    public DisplayedProjectsPage(bool finished)
	{
		InitializeComponent();
        finishedProjects = finished;
        if (finished)
        {
            title.Text = "Completed Jobs";
            icon.Source = "completed_jobs.png";
            newFolder.IsVisible = false;
        }
        else
        {
            projectComplete = false;
            title.Text = "Incomplete Jobs";
            icon.Source = "incompleted_jobs.png";
            newFolder.IsVisible = true;
        }
        _ = LoadFolders();
    }
    public async void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;
        string folderName = Folders.First(x => x.Id == folderId).Name;
        await Navigation.PushAsync(new DisplayedReportsPage(folderId, folderName, projectComplete));
    }
    public async void NewFolder(object sender, EventArgs e)
    {
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (folderName == null) // User clicked Cancel
            return;

        try
        {
            var response = await ApiService.UploadFolderAsync(folderName);

            if (response.IsSuccessStatusCode)
            {
                // Load folders after successful upload
                await LoadFolders();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                string errorMessage;
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                        errorMessage = errorObj.ContainsKey("error") ? errorObj["error"] : "An unknown error occurred.";
                    }
                    catch
                    {
                        errorMessage = "An unknown error occurred.";
                    }
                }
                else
                    errorMessage = "Internal server error.";

                await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            // Handle other potential exceptions like network errors, timeouts, etc.
            await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
    private async Task LoadFolders()
    {
        FoldersListView.ItemsSource = null;
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        
        try
        {
            HttpResponseMessage response = await ApiService.GetAllFoldersAsync();
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to load folders.", "OK");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();

            JsonDocument jsonDocument = JsonDocument.Parse(json);
            if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataArray))
            {
                Folders.Clear();
                foreach (var element in dataArray.EnumerateArray())
                {
                    bool isFinished = !string.IsNullOrEmpty(element.GetProperty("signature1").GetString()) &&
                                      !string.IsNullOrEmpty(element.GetProperty("signature2").GetString());

                    // Add folders based on the finishedProjects flag
                    if (finishedProjects == isFinished)
                    {
                        Folders.Add(new Folder
                        {
                            Id = element.GetProperty("folder_id").GetString(),
                            Name = element.GetProperty("folder_name").GetString(),
                            Timestamp = element.GetProperty("created_at").GetString(),
                            Signature1 = element.GetProperty("signature1").GetString(),
                            Signature2 = element.GetProperty("signature2").GetString()
                        });
                    }
                }

                FoldersListView.ItemsSource = Folders;
            }
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Failed to parse the received data. Details: {jsonEx.Message}", "OK");
        }
        catch (FormatException formatEx)
        {
            await DisplayAlert("Error", $"Failed to format the date. Details: {formatEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unknown error occurred. Details: {ex.Message}", "OK");
        }
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
    }
    private async void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                string searchText = e.NewTextValue;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search text is empty, load all folders
                    if (FoldersListView != null && Folders != null)
                    {
                        FoldersListView.ItemsSource = null;
                        FoldersListView.ItemsSource = Folders;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Folders or FoldersListView is null.", "OK");
                    }
                }
                else
                {
                    // Filter folders based on search text
                    if (Folders != null)
                    {
                        List<Folder> filteredFolders = new List<Folder>(Folders.Where(folder => folder.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
                        // Update the ItemsSource with filtered folders
                        if (FoldersListView != null)
                        {
                            FoldersListView.ItemsSource = null;
                            FoldersListView.ItemsSource = filteredFolders;
                        }
                        else
                        {
                            await DisplayAlert("Error", "FoldersListView is null.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Folders is null.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");

            }
        });


    }

}