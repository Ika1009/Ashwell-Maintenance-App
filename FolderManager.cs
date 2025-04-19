using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Ashwell_Maintenance
{
    public static class FolderManager
    {
        // Path to offline queue of new-folder requests
        private static readonly string OfflineQueuePath =
            Path.Combine(FileSystem.AppDataDirectory, "PendingFolders.json");

        /// <summary>
        /// Creates a folder on the server if online, otherwise queues it for later.
        /// </summary>
        public static async Task<Folder> CreateFolderAsync(string folderName)
        {
            // 1) Try server
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

                        return new Folder
                        {
                            Id = id,
                            Name = folderName,
                            Timestamp = DateTime.UtcNow.ToString("o")
                        };
                    }
                }
                catch
                {
                    // network/server error → fall back
                }
            }

            // 2) Offline or failure: queue and return placeholder
            var offline = new Folder
            {
                Id = null,
                Name = folderName,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
            await QueueFolderForLaterUpload(offline);
            return offline;
        }

        /// <summary>
        /// Loads folders into the given collection:
        /// - If online, fetches server list
        /// - Always appends any offline‑queued folders so they appear in the UI
        /// </summary>
        public static async Task LoadFoldersAsync(
            ObservableCollection<Folder> folders,
            ListView listView = null)
        {
            folders.Clear();
            bool loadedFromServer = false;

            // Attempt server fetch
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var resp = await ApiService.GetAllFoldersAsync();
                    if (resp.IsSuccessStatusCode)
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
                            loadedFromServer = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Loading folders failed: {ex.Message}", "OK");
                }
            }

            // Append offline‑queued items
            if (File.Exists(OfflineQueuePath))
            {
                try
                {
                    string pendingJson = await File.ReadAllTextAsync(OfflineQueuePath);
                    var pending = JsonSerializer
                        .Deserialize<List<Folder>>(pendingJson)
                        ?? new List<Folder>();

                    foreach (var q in pending)
                        folders.Add(q);
                }
                catch
                {
                    // ignore corrupt queue
                }
            }

            // Notify if we never got server data
            if (!loadedFromServer)
            {
                await Shell.Current.DisplayAlert(
                    "Offline",
                    "Could not load server folders. Showing only offline‑created ones.",
                    "OK");
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
                await Shell.Current.DisplayAlert(
                    "Offline",
                    "You cannot rename or delete folders while offline.",
                    "OK");
                return;
            }

            string oldName = folder.Name;
            string newName = oldName;

            if (CurrentUser.IsAdmin)
            {
                newName = await Shell.Current.DisplayPromptAsync(
                    "Edit Folder",
                    "Rename or delete folder",
                    "RENAME",
                    "DELETE",
                    null, -1, null, oldName);
            }
            else
            {
                newName = await Shell.Current.DisplayPromptAsync(
                    "Edit Folder",
                    "Rename folder",
                    "RENAME",
                    "Cancel",
                    null, -1, null, oldName);
            }

            // DELETE?
            if (newName == null && CurrentUser.IsAdmin)
            {
                bool ok = await Shell.Current.DisplayAlert(
                    "Delete Folder",
                    "This folder will be deleted permanently.",
                    "OK", "Cancel");
                if (ok)
                {
                    var delResp = await ApiService.DeleteFolderAsync(folder.Id);
                    if (delResp.IsSuccessStatusCode)
                        await LoadFoldersAsync(folders);
                    else
                        await Shell.Current.DisplayAlert("Error", "Failed to delete folder", "OK");
                }
                return;
            }

            // RENAME?
            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                var renameResp = await ApiService.RenameFolderAsync(folder.Id, newName);
                if (renameResp.IsSuccessStatusCode)
                    await LoadFoldersAsync(folders);
                else
                    await Shell.Current.DisplayAlert("Error", "Failed to rename folder", "OK");
            }
        }

        /// <summary>
        /// Retries creating any folders in the offline queue. Stops at first failure.
        /// </summary>
        public static async Task RetryPendingFoldersAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return;

            if (!File.Exists(OfflineQueuePath))
                return;

            List<Folder> pending;
            try
            {
                string json = await File.ReadAllTextAsync(OfflineQueuePath);
                pending = JsonSerializer.Deserialize<List<Folder>>(json) ?? new List<Folder>();
            }
            catch
            {
                return;
            }

            var stillPending = new List<Folder>();

            foreach (var folder in pending)
            {
                try
                {
                    // Attempt server create
                    var resp = await ApiService.UploadFolderAsync(folder.Name);
                    if (!resp.IsSuccessStatusCode)
                        throw new HttpRequestException();

                    // if success, continue to next
                }
                catch
                {
                    // on first failure, keep this and all remaining
                    stillPending.AddRange(pending.Skip(stillPending.Count));
                    break;
                }
            }

            // overwrite or delete queue file
            try
            {
                if (stillPending.Any())
                {
                    string updated = JsonSerializer.Serialize(stillPending,
                        new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(OfflineQueuePath, updated);
                }
                else
                {
                    File.Delete(OfflineQueuePath);
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Appends a folder to the offline queue.
        /// </summary>
        private static async Task QueueFolderForLaterUpload(Folder folder)
        {
            var pending = new List<Folder>();
            if (File.Exists(OfflineQueuePath))
            {
                string existing = await File.ReadAllTextAsync(OfflineQueuePath);
                pending = JsonSerializer.Deserialize<List<Folder>>(existing)
                          ?? new List<Folder>();
            }

            pending.Add(folder);

            string json = JsonSerializer.Serialize(pending,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(OfflineQueuePath, json);
        }
    }
}
