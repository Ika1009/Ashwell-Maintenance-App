using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Ashwell_Maintenance
{
    public static class FolderManager
    {
        /// <summary>
        /// Tries to create the folder on the server (if online). 
        /// On success returns the real Id; on network/server failure returns a placeholder Folder with Id = null.
        /// </summary>
        public static async Task<Folder> CreateFolderAsync(string folderName)
        {
            // If we have connectivity, try server
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var resp = await ApiService.UploadFolderAsync(folderName);
                    if (resp.IsSuccessStatusCode)
                    {
                        var payload = await resp.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(payload);
                        string id = doc.RootElement.GetProperty("folder_id").GetString();
                        string createdAt = doc.RootElement.TryGetProperty("created_at", out var ts)
                                           ? ts.GetString()
                                           : DateTime.UtcNow.ToString("o");

                        return new Folder
                        {
                            Id = id,
                            Name = folderName,
                            Timestamp = createdAt
                        };
                    }
                }
                catch
                {
                    // swallow and fall back to offline
                }
            }

            // Offline or server error → return placeholder
            return new Folder
            {
                Id = null,
                Name = folderName,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
        }

        /// <summary>
        /// Populates the given ObservableCollection with server folders (if online).
        /// If offline or fetch fails, shows an alert and leaves the list empty.
        /// </summary>
        public static async Task LoadFoldersAsync(
            ObservableCollection<Folder> folders,
            ListView listView = null)
        {
            folders.Clear();

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var resp = await ApiService.GetAllFoldersAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to load folders.", "OK");
                    }
                    else
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("data", out var arr))
                        {
                            foreach (var el in arr.EnumerateArray())
                            {
                                folders.Add(new Folder
                                {
                                    Id = el.GetProperty("folder_id").GetString(),
                                    Name = el.GetProperty("folder_name").GetString(),
                                    Timestamp = el.GetProperty("created_at").GetString(),
                                    Signature1 = el.GetProperty("signature1").GetString(),
                                    Signature2 = el.GetProperty("signature2").GetString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Loading folders failed: {ex.Message}", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Offline", "Cannot load folders while offline.", "OK");
            }

            if (listView != null)
                listView.ItemsSource = folders;
        }

        /// <summary>
        /// Renames or deletes an existing folder (only when online).
        /// </summary>
        public static async Task EditFolderAsync(
            Folder folder,
            ObservableCollection<Folder> folders)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Offline", "You cannot rename or delete folders while offline.", "OK");
                return;
            }

            string oldName = folder.Name;
            string newName = oldName;

            // Prompt
            if (CurrentUser.IsAdmin)
            {
                newName = await Shell.Current.DisplayPromptAsync(
                    "Edit Folder", "Rename or delete folder", "RENAME", "DELETE", null, -1, null, oldName);
            }
            else
            {
                newName = await Shell.Current.DisplayPromptAsync(
                    "Edit Folder", "Rename folder", "RENAME", "Cancel", null, -1, null, oldName);
            }

            // DELETE case (admin only)
            if (newName == null && CurrentUser.IsAdmin)
            {
                bool confirmed = await Shell.Current.DisplayAlert(
                    "Delete Folder", "This will delete the folder permanently.", "OK", "Cancel");
                if (confirmed)
                {
                    var delResp = await ApiService.DeleteFolderAsync(folder.Id);
                    if (delResp.IsSuccessStatusCode)
                        await LoadFoldersAsync(folders);
                    else
                        await Shell.Current.DisplayAlert("Error", "Failed to delete folder.", "OK");
                }
                return;
            }

            // RENAME
            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                var renameResp = await ApiService.RenameFolderAsync(folder.Id, newName);
                if (renameResp.IsSuccessStatusCode)
                    await LoadFoldersAsync(folders);
                else
                    await Shell.Current.DisplayAlert("Error", "Failed to rename folder.", "OK");
            }
        }

        /// <summary>
        /// If folder.Id == null, calls CreateFolderAsync to get a real Id (or leave it null if still offline).
        /// Caches the result back into folder.Id.
        /// </summary>
        public static async Task<string> EnsureFolderIdAsync(Folder folder)
        {
            if (!string.IsNullOrEmpty(folder.Id))
                return folder.Id;

            var real = await CreateFolderAsync(folder.Name);
            folder.Id = real.Id;
            return folder.Id;
        }
    }
}
