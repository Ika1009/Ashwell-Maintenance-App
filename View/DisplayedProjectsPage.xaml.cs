namespace Ashwell_Maintenance.View;
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
    public ObservableCollection<Folder> Folders = new();
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
        this.ShowPopup(new NewFolderPopup());
        await LoadFolders();
    }
    private async Task LoadFolders()
    {
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

                FoldersListView.ItemsSource ??= Folders;
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
}