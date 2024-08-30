using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class PressurisationUnitReportPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public PressurisationUnitReportPage()
	{
		InitializeComponent();
	}

    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;

        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }

    private async Task UploadReport(Folder folder, Dictionary<string, string> report)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        PressurisationUnitReportBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.PressurisationUnitReport, reportName, folder.Id, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully uploaded new sheet.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to upload report.", "OK");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"HTTP request error. Details: {httpEx.Message}", "OK");
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Failed to parse the received data. Details: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unknown error occurred. Details: {ex.Message}", "OK");
        }

        if (!string.IsNullOrEmpty(folder.Signature1) && !string.IsNullOrEmpty(folder.Signature2))
        {
            try
            {
                byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");
                if (signature1 == null || signature2 == null)
                    throw new Exception("Couldn't retrieve signatures");

                byte[] pdfData = await PdfCreation.PressurisationReport(reportData, signature1, signature2);

                if (pdfData != null)
                {
                    HttpResponseMessage signatureResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);

                    if (!signatureResponse.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Error", $"Failed to upload {reportName} to DropBox with already given signatures.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error processing signatures when uploading file to DropBox: {ex.Message}", "OK");
            }
        }
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
        await Navigation.PopModalAsync();
    }

    public async void NewFolder(object sender, EventArgs e)
    {
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (folderName == null || folderName == "") // User clicked Cancel
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
                // Clear the existing items and add the new ones directly to the ObservableCollection
                Folders.Clear();
                foreach (var element in dataArray.EnumerateArray())
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

                // Check if the ItemsSource is already set
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
    }
    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
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
    public async void PressurisationUnitReportBack(object sender, EventArgs e)
    {
		if (PURSection1.IsVisible)
        {
            PressurisationUnitReportBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (PURSection2.IsVisible)
		{
			PURSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await PURSection1.ScrollToAsync(0, 0, false);
			PURSection1.IsVisible = true;
		}
		else if (PURSection3.IsVisible)
		{
            PURSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await PURSection2.ScrollToAsync(0, 0, false);
            PURSection2.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await PURSection3.ScrollToAsync(0, 0, false);
            PURSection3.IsVisible = true;
        }
    }

    
    public async void PressurisationUnitReportNext1(object sender, EventArgs e)
	{
		PURSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await PURSection2.ScrollToAsync(0, 0, false);
		PURSection2.IsVisible = true;
	}
    
    public async void PressurisationUnitReportNext2(object sender, EventArgs e)
    {
        PURSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await PURSection3.ScrollToAsync(0, 0, false);
        PURSection3.IsVisible = true;
        await LoadFolders();
    }
    public async void PressurisationUnitReportNext3(object sender, EventArgs e)
    {
        PURSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Pressurisation_Unit_Report_{dateTimeString}.pdf";
        reportData = GatherReportData();

        //PdfCreation.PressurisationReport(GatherReportData());
    }
    public void previewPressurisationUnitReportPage(Dictionary<string,string> reportData) 
    {
        // Assuming reportData is a dictionary of string keys and string values

        // Populate the form fields from the dictionary
        if (reportData.TryGetValue("siteNameAndAddress", out var siteNameAndAddress))
            siteName.Text = siteNameAndAddress;

        if (reportData.TryGetValue("totalHeatingSystemRating", out var totalHeatingSystemRating))
            this.totalHeatingSystemRating.Text = totalHeatingSystemRating;

        if (reportData.TryGetValue("numberOfBoilers", out var numberOfBoilers))
            this.numberOfBoilers.Text = numberOfBoilers;

        if (reportData.TryGetValue("flowTemperature", out var flowTemperature))
            this.flowTemperature.Text = flowTemperature;

        if (reportData.TryGetValue("returnTemperature", out var returnTemperature))
            this.returnTemperature.Text = returnTemperature;

        if (reportData.TryGetValue("currentWorkingPressure", out var currentWorkingPressure))
            this.currentWorkingPressure.Text = currentWorkingPressure;

        if (reportData.TryGetValue("safetyValveSetting", out var safetyValveSetting))
            this.safetyValveSetting.Text = safetyValveSetting;

        if (reportData.TryGetValue("unitModel", out var unitModel))
            this.unitModel.Text = unitModel;

        if (reportData.TryGetValue("serialNo", out var serialNo))
            this.serialNo.Text = serialNo;

        if (reportData.TryGetValue("expansionVesselSize", out var expansionVesselSize))
            this.expansionVesselSize.Text = expansionVesselSize;

        if (reportData.TryGetValue("numberOfPressureVessels", out var numberOfPressureVessels))
            this.numberOfPressureVessels.Text = numberOfPressureVessels;

        if (reportData.TryGetValue("setFillPressure", out var setFillPressure))
            this.setFillPressure.Text = setFillPressure;

        if (reportData.TryGetValue("ratedExpansionVesselCharge", out var ratedExpansionVesselCharge))
            this.ratedExpensionVesselCharge.Text = ratedExpansionVesselCharge;

        if (reportData.TryGetValue("highPressureSwitchSetting", out var highPressureSwitchSetting))
            this.highPressureSwitchSetting.Text = highPressureSwitchSetting;

        if (reportData.TryGetValue("lowPressureSwitchSetting", out var lowPressureSwitchSetting))
            this.lowPressureSwitchSetting.Text = lowPressureSwitchSetting;

        if (reportData.TryGetValue("finalSystemPressure", out var finalSystemPressure))
            this.finalSystemPressure.Text = finalSystemPressure;

        if (reportData.TryGetValue("notes", out var notes))
            this.notes.Text = notes;

        if (reportData.TryGetValue("date", out var date))
            this.date.Text = date;
        
        //fale bool

    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());
        string SNameAdress = siteAddress.Text+" "+ siteName.Text;
        reportData.Add("siteNameAndAddress", SNameAdress?? string.Empty);
        reportData.Add("totalHeatingSystemRating", totalHeatingSystemRating.Text ?? string.Empty);
        reportData.Add("numberOfBoilers", numberOfBoilers.Text ?? string.Empty);
        reportData.Add("flowTemperature", flowTemperature.Text ?? string.Empty);
        reportData.Add("returnTemperature", returnTemperature.Text ?? string.Empty);
        reportData.Add("currentWorkingPressure", currentWorkingPressure.Text ?? string.Empty);
        reportData.Add("safetyValveSetting", safetyValveSetting.Text ?? string.Empty);
        reportData.Add("unitModel", unitModel.Text ?? string.Empty);
        reportData.Add("serialNo", serialNo.Text ?? string.Empty);
        reportData.Add("expansionVesselSize", expansionVesselSize.Text ?? string.Empty);
        reportData.Add("numberOfPressureVessels", numberOfPressureVessels.Text ?? string.Empty);
        reportData.Add("setFillPressure", setFillPressure.Text ?? string.Empty);
        reportData.Add("ratedExpansionVesselCharge", ratedExpensionVesselCharge.Text ?? string.Empty);
        reportData.Add("highPressureSwitchSetting", highPressureSwitchSetting.Text ?? string.Empty);
        reportData.Add("lowPressureSwitchSetting", lowPressureSwitchSetting.Text ?? string.Empty);
        reportData.Add("finalSystemPressure", finalSystemPressure.Text ?? string.Empty);
        reportData.Add("notes", notes.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);


        reportData.Add("checkMainWaterSupply", checkMainsWatersSupplyYes.IsChecked.ToString());
        reportData.Add("checkColdFillPressureSet", checkColdFillPressureSetCorrectlyYes.IsChecked.ToString());
        reportData.Add("checkElectricalSupplyWorking", checkElectricalSupplyWorkingYes.IsChecked.ToString());
        reportData.Add("checkFillingLoopDisconnected", checkFillingLoopDisconnectedYes.IsChecked.ToString());
        reportData.Add("checkUnitLeftOperational", checkUnitLeftOperationalYes.IsChecked.ToString());





        return reportData;
    }
}