using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Ashwell_Maintenance.View
{
    public partial class NewFolderPopup : Popup
    {
        public event EventHandler PopupClosed;
        public NewFolderPopup()
        {
            InitializeComponent();
        }

        public void NewFolderCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        public async void NewFolderClicked(object sender, EventArgs e)
        {
            // Check if folder name is empty or consists only of whitespace
            if (string.IsNullOrWhiteSpace(folderName.Text))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Folder name cannot be empty.", "OK");
                return;
            }

            try
            {
                var response = await ApiService.UploadFolderAsync(folderName.Text);

                if (response.IsSuccessStatusCode)
                {
                    this.Close();
                    PopupClosed?.Invoke(this, EventArgs.Empty); // Fire the custom event
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
    }
}
