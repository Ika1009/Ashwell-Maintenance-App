using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;

namespace Ashwell_Maintenance
{
    public static class FolderManager
    {
        private static string OfflineFolderQueuePath => Path.Combine(FileSystem.AppDataDirectory, "PendingFolders.json");

        public static async Task<Folder> CreateFolderAsync(string folderName)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var response = await ApiService.UploadFolderAsync(folderName);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JsonDocument.Parse(content);
                        var folderId = json.RootElement.GetProperty("folder_id").GetString();

                        return new Folder
                        {
                            Id = folderId,
                            Name = folderName
                        };
                    }
                }
                catch
                {
                    // fallback to offline
                }
            }

            // No internet or failed — save locally
            var folder = new Folder
            {
                Id = null,
                Name = folderName
            };

            await QueueFolderForLaterUpload(folder);
            return folder;
        }

        public static async Task LoadFoldersAsync(ObservableCollection<Folder> folders, ListView listView = null)
        {
            try
            {
                var response = await ApiService.GetAllFoldersAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to load folders.", "OK");
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(json);

                if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataArray))
                {
                    folders.Clear();
                    foreach (var element in dataArray.EnumerateArray())
                    {
                        folders.Add(new Folder
                        {
                            Id = element.GetProperty("folder_id").GetString(),
                            Name = element.GetProperty("folder_name").GetString(),
                            Timestamp = element.GetProperty("created_at").GetString(),
                            Signature1 = element.GetProperty("signature1").GetString(),
                            Signature2 = element.GetProperty("signature2").GetString()
                        });
                    }

                    if (listView != null)
                        listView.ItemsSource ??= folders;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Loading folders failed: {ex.Message}", "OK");
            }
        }

        public static async Task EditFolderAsync(Folder folder, ObservableCollection<Folder> folders)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await App.Current.MainPage.DisplayAlert("Offline", "You cannot rename or delete folders while offline.", "OK");
                return;
            }

            var folderId = folder.Id;
            var folderName = folder.Name;
            var oldName = folderName;

            if (CurrentUser.IsAdmin)
            {
                folderName = await Shell.Current.DisplayPromptAsync("Edit Folder", "Rename or delete folder", "RENAME", "DELETE", null, -1, null, folderName);
            }
            else
            {
                folderName = await Shell.Current.DisplayPromptAsync("Edit Folder", "Rename folder", "RENAME", "Cancel", null, -1, null, folderName);
            }

            if (folderName == null && CurrentUser.IsAdmin)
            {
                var confirm = await App.Current.MainPage.DisplayAlert("Delete", "Are you sure?", "Yes", "Cancel");
                if (confirm)
                {
                    var deleteResp = await ApiService.DeleteFolderAsync(folderId);
                    if (deleteResp.IsSuccessStatusCode)
                    {
                        await LoadFoldersAsync(folders);
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to delete folder", "OK");
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(folderName) && folderName != oldName)
            {
                var updateResp = await ApiService.RenameFolderAsync(folderId, folderName);
                if (!updateResp.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to rename folder", "OK");
                }
                else
                {
                    await LoadFoldersAsync(folders);
                }
            }
        }

        private static async Task QueueFolderForLaterUpload(Folder folder)
        {
            List<Folder> pending = new();
            if (File.Exists(OfflineFolderQueuePath))
            {
                string json = await File.ReadAllTextAsync(OfflineFolderQueuePath);
                pending = JsonSerializer.Deserialize<List<Folder>>(json) ?? new List<Folder>();
            }

            pending.Add(folder);

            string updatedJson = JsonSerializer.Serialize(pending, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(OfflineFolderQueuePath, updatedJson);
        }
    }
}
