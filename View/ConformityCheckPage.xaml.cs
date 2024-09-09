using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class ConformityCheckPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    bool previewOnly = false;
    public ConformityCheckPage()
	{
		InitializeComponent();

        checkFluesFittedNA.IsChecked = true;
        checkFluesSupportedNA.IsChecked = true;
        checkFluesInLineNA.IsChecked = true;
        checkFacilitiesNA.IsChecked = true;
        checkFlueGradientsNA.IsChecked = true;
        checkFluesInspectionNA.IsChecked = true;
        checkFlueJointsNA.IsChecked = true;
        checkInterlocksProvidedNA.IsChecked = true;
        checkEmergencyShutOffButtonNA.IsChecked = true;
        checkPlantInterlinkNA.IsChecked = true;
        checkFuelShutOffNA.IsChecked = true;
        checkFuelFirstEntryNA.IsChecked = true;
        checkSystemStopNA.IsChecked = true;
        checkTestAndResetNA.IsChecked = true;
	}
    public ConformityCheckPage(Report report)
    {
        InitializeComponent();
        previewOnly = true;
        PreviewConformityCheckPage(report.ReportData);
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
        ConformityCheckBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ConformityCheck, reportName, folder.Id, report);

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

                byte[] pdfData = await PdfCreation.ConformityCheck(reportData, signature1, signature2);

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

    public async void FolderEdit(object sender, EventArgs e)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        string folderId = (sender as ImageButton).CommandParameter as string;
        string folderName = Folders.First(x => x.Id == folderId).Name;
        string oldFolderName = folderName;

        if (CurrentUser.IsAdmin)
            folderName = await Shell.Current.DisplayPromptAsync("Edit Folder", "Rename or delete folder", "RENAME", "DELETE", null, -1, null, folderName);
        else
            folderName = await Shell.Current.DisplayPromptAsync("Edit Folder", "Rename folder", "RENAME", "Cancel", null, -1, null, folderName);

        if (folderName == null && CurrentUser.IsAdmin) // User clicked Delete
        {
            bool deleteConfirmed = await Shell.Current.DisplayAlert("Delete Folder", "This folder will be deleted", "OK", "Cancel");
            if (deleteConfirmed)
            {
                // Deleting Folder in the Database
                var response = await ApiService.DeleteFolderAsync(folderId);
                if (response.IsSuccessStatusCode)
                {
                    //await DisplayAlert("Success", "Folder deleted successfully", "OK");
                    await LoadFolders();
                }
                else
                {
                    await DisplayAlert("Error", $"Error deleting folder: {response.Content.ReadAsStringAsync().Result}", "OK");
                }
            }
            loadingBG.IsRunning = false;
            loading.IsRunning = false;
            return;
        }
        else if (folderName == oldFolderName || !CurrentUser.IsAdmin && folderName == null)
        {
            loadingBG.IsRunning = false;
            loading.IsRunning = false;
            return;
        }

        // Update the folder name in the database
        var updateResponse = await ApiService.RenameFolderAsync(folderId, folderName);
        if (!updateResponse.IsSuccessStatusCode)
        {
            await DisplayAlert("Error", $"Error updating folder name: {updateResponse.Content.ReadAsStringAsync().Result}", "OK");
        }

        // Update Renamed in the Front End
        Folders.First(x => x.Id == folderId).Name = folderName;
        await LoadFolders();
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
    }

    public async void ConformityCheckBack(object sender, EventArgs e)
	{
		if (CCSection1.IsVisible)
        {
            ConformityCheckBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (CCSection2.IsVisible)
		{
			CCSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection1.ScrollToAsync(0, 0, false);
			CCSection1.IsVisible = true;
		}
		else if (CCSection3.IsVisible)
		{
            CCSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection2.ScrollToAsync(0, 0, false);
            CCSection2.IsVisible = true;
        }
		else if (CCSection4.IsVisible)
		{
            CCSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection3.ScrollToAsync(0, 0, false);
            CCSection3.IsVisible = true;
        }
        else if(CCSection5.IsVisible)
        {
            CCSection5.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection4.ScrollToAsync(0, 0, false);
            CCSection4.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection5.ScrollToAsync(0, 0, false);
            CCSection5.IsVisible = true;
        }
	}

    
    public async void CCNext1(object sender, EventArgs e)
	{
		CCSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection2.ScrollToAsync(0, 0, false);
		CCSection2.IsVisible = true;
	}
    
    public async void CCNext2(object sender, EventArgs e)
    {
        CCSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection3.ScrollToAsync(0, 0, false);
        CCSection3.IsVisible = true;
    }
    
    public async void CCNext3(object sender, EventArgs e)
    {
        CCSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection4.ScrollToAsync(0, 0, false);
        CCSection4.IsVisible = true;
    }
    
    public async void CCNext4(object sender, EventArgs e)
    {
        CCSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection5.ScrollToAsync(0, 0, false);
        CCSection5.IsVisible = true;
        // Do not Show Folders if in preview of PDF page
        if (!previewOnly)
            await LoadFolders();
    }
    public async void CCNext5(object sender, EventArgs e)
    {
        // Do not Show Folders if in preview of PDF page
        if (previewOnly)
            await Navigation.PopModalAsync();

        CCSection5.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Conformity_Check_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.CheckPage(GatherReportData());
    }
    public void PreviewConformityCheckPage(Dictionary<string, string> reportData)
    {
        if (reportData.ContainsKey("uern"))
        {
            uern.Text = reportData["uern"];
        }
        if (reportData.ContainsKey("SheetNo"))
        {
            SheetNo.Text = reportData["SheetNo"];
        }
        if (reportData.ContainsKey("WarningNoticeRefNo"))
        {
            WarningNoticeRefNo.Text = reportData["WarningNoticeRefNo"];
        }
        if (reportData.ContainsKey("nameAndAddressOfPremises"))
        {
            nameAndAddressOfPremises.Text = reportData["nameAndAddressOfPremises"];
        }
        if (reportData.ContainsKey("location"))
        {
            location.Text = reportData["location"];
        }
        if (reportData.ContainsKey("ventilationCalculations"))
        {
            ventilationCalculations.Text = reportData["ventilationCalculations"];
        }
        if (reportData.ContainsKey("existingHighLevel"))
        {
            existingHighLevel.Text = reportData["existingHighLevel"];
        }
        if (reportData.ContainsKey("existingLowLevel"))
        {
            existingLowLevel.Text = reportData["existingLowLevel"];
        }
        if (reportData.ContainsKey("requiredHighLevel"))
        {
            requiredHighLevel.Text = reportData["requiredHighLevel"];
        }
        if (reportData.ContainsKey("requiredLowLevel"))
        {
            requiredLowLevel.Text = reportData["requiredLowLevel"];
        }
        if (reportData.ContainsKey("ventilationChecksComments"))
        {
            ventilationChecksComments.Text = reportData["ventilationChecksComments"];
        }
        if (reportData.ContainsKey("flueChecksComments"))
        {
            flueChecksComments.Text = reportData["flueChecksComments"];
        }
        if (reportData.ContainsKey("emergencyStopButtonComment"))
        {
            emergencyStopButtonComment.Text = reportData["emergencyStopButtonComment"];
        }
        if (reportData.ContainsKey("safetyInterlocksComments"))
        {
            safetyInterlocksComments.Text = reportData["safetyInterlocksComments"];
        }
        if (reportData.ContainsKey("engineersName"))
        {
            engineersName.Text = reportData["engineersName"];
        }
        if (reportData.ContainsKey("contractor"))
        {
            contractor.Text = reportData["contractor"];
        }
        if (reportData.ContainsKey("companyGasSafeRegistrationNo"))
        {
            companyGasSafeRegistrationNo.Text = reportData["companyGasSafeRegistrationNo"];
        }
        if (reportData.ContainsKey("inspectionDate"))
        {
            inspectionDate.Date = DateTime.Parse(reportData["inspectionDate"]);
        }
        if (reportData.ContainsKey("engineersGasSafeIDNo"))
        {
            engineersGasSafeIDNo.Text = reportData["engineersGasSafeIDNo"];
        }
        if (reportData.ContainsKey("clientsName"))
        {
            clientsName.Text = reportData["clientsName"];
        }
        if (reportData.ContainsKey("date"))
        {
            date.Date = DateTime.Parse(reportData["date"]);
        }
        if (reportData.ContainsKey("checkRemedialToWorkRequiredYes"))
            checkRemedialToWorkRequiredYes.IsChecked = bool.Parse(reportData["checkRemedialToWorkRequiredYes"]);

        if (reportData.ContainsKey("checkTestsCompletedSatisfactoryYes"))
            checkTestsCompletedSatisfactoryYes.IsChecked = bool.Parse(reportData["checkTestsCompletedSatisfactoryYes"]);

        if (reportData.ContainsKey("checkExistingLowLevelCM"))
            checkExistingLevelCM.IsChecked = bool.Parse(reportData["checkExistingLowLevelCM"]);

        if (reportData.ContainsKey("checkRequiredHighLevelCM"))
            checkRequiredLevelCM.IsChecked = bool.Parse(reportData["checkRequiredHighLevelCM"]);

        if (reportData.ContainsKey("checkVentilationCorrectlySizedYes"))
            checkVentilationCorrectlySizedYes.IsChecked = bool.Parse(reportData["checkVentilationCorrectlySizedYes"]);

        if (reportData.ContainsKey("checkVentilationAtTheCorrectHeightYes"))
            checkVentilationAtTheCorrectHeightYes.IsChecked = bool.Parse(reportData["checkVentilationAtTheCorrectHeightYes"]);

        if (reportData.ContainsKey("checkVentilationArrangementsYes"))
            checkVentilationArrangementsYes.IsChecked = bool.Parse(reportData["checkVentilationArrangementsYes"]);

        if (reportData.ContainsKey("checkA_ID1"))
            checkA_ID1.IsChecked = bool.Parse(reportData["checkA_ID1"]);

        if (reportData.ContainsKey("checkB_AR1"))
            checkB_AR1.IsChecked = bool.Parse(reportData["checkB_AR1"]);

        if (reportData.ContainsKey("checkC_NCS1"))
            checkC_NCS1.IsChecked = bool.Parse(reportData["checkC_NCS1"]);

        if (reportData.ContainsKey("checkFluesFittedYes"))
            checkFluesFittedYes.IsChecked = bool.Parse(reportData["checkFluesFittedYes"]);

        if (reportData.ContainsKey("checkFluesFittedNo"))
            checkFluesFittedNo.IsChecked = bool.Parse(reportData["checkFluesFittedNo"]);

        if (reportData.ContainsKey("checkFluesSupportedYes"))
            checkFluesSupportedYes.IsChecked = bool.Parse(reportData["checkFluesSupportedYes"]);

        if (reportData.ContainsKey("checkFluesSupportedNo"))
            checkFluesSupportedNo.IsChecked = bool.Parse(reportData["checkFluesSupportedNo"]);

        if (reportData.ContainsKey("checkFluesInLineYes"))
            checkFluesInLineYes.IsChecked = bool.Parse(reportData["checkFluesInLineYes"]);

        if (reportData.ContainsKey("checkFluesInLineNo"))
            checkFluesInLineNo.IsChecked = bool.Parse(reportData["checkFluesInLineNo"]);

        if (reportData.ContainsKey("checkFacilitiesYes"))
            checkFacilitiesYes.IsChecked = bool.Parse(reportData["checkFacilitiesYes"]);

        if (reportData.ContainsKey("checkFacilitiesNo"))
            checkFacilitiesNo.IsChecked = bool.Parse(reportData["checkFacilitiesNo"]);

        if (reportData.ContainsKey("checkFlueGradientsYes"))
            checkFlueGradientsYes.IsChecked = bool.Parse(reportData["checkFlueGradientsYes"]);

        if (reportData.ContainsKey("checkFlueGradientsNo"))
            checkFlueGradientsNo.IsChecked = bool.Parse(reportData["checkFlueGradientsNo"]);

        if (reportData.ContainsKey("checkFluesInspectionYes"))
            checkFluesInspectionYes.IsChecked = bool.Parse(reportData["checkFluesInspectionYes"]);

        if (reportData.ContainsKey("checkFluesInspectionNo"))
            checkFluesInspectionNo.IsChecked = bool.Parse(reportData["checkFluesInspectionNo"]);

        if (reportData.ContainsKey("checkFlueJointsYes"))
            checkFlueJointsYes.IsChecked = bool.Parse(reportData["checkFlueJointsYes"]);

        if (reportData.ContainsKey("checkFlueJointsNo"))
            checkFlueJointsNo.IsChecked = bool.Parse(reportData["checkFlueJointsNo"]);

        if (reportData.ContainsKey("checkA_ID2"))
            checkA_ID2.IsChecked = bool.Parse(reportData["checkA_ID2"]);

        if (reportData.ContainsKey("checkB_AR2"))
            checkB_AR2.IsChecked = bool.Parse(reportData["checkB_AR2"]);

        if (reportData.ContainsKey("checkC_NCS2"))
            checkC_NCS2.IsChecked = bool.Parse(reportData["checkC_NCS2"]);

        if (reportData.ContainsKey("checkInterlocksProvidedYes"))
            checkInterlocksProvidedYes.IsChecked = bool.Parse(reportData["checkInterlocksProvidedYes"]);

        if (reportData.ContainsKey("checkInterlocksProvidedNo"))
            checkInterlocksProvidedNo.IsChecked = bool.Parse(reportData["checkInterlocksProvidedNo"]);

        if (reportData.ContainsKey("checkEmergencyShutOffButtonYes"))
            checkEmergencyShutOffButtonYes.IsChecked = bool.Parse(reportData["checkEmergencyShutOffButtonYes"]);

        if (reportData.ContainsKey("checkEmergencyShutOffButtonNo"))
            checkEmergencyShutOffButtonNo.IsChecked = bool.Parse(reportData["checkEmergencyShutOffButtonNo"]);

        if (reportData.ContainsKey("checkPlantInterlinkYes"))
            checkPlantInterlinkYes.IsChecked = bool.Parse(reportData["checkPlantInterlinkYes"]);

        if (reportData.ContainsKey("checkPlantInterlinkNo"))
            checkPlantInterlinkNo.IsChecked = bool.Parse(reportData["checkPlantInterlinkNo"]);

        if (reportData.ContainsKey("checkFuelShutOffYes"))
            checkFuelShutOffYes.IsChecked = bool.Parse(reportData["checkFuelShutOffYes"]);

        if (reportData.ContainsKey("checkFuelShutOffNo"))
            checkFuelShutOffNo.IsChecked = bool.Parse(reportData["checkFuelShutOffNo"]);

        if (reportData.ContainsKey("checkFuelFirstEntryYes"))
            checkFuelFirstEntryYes.IsChecked = bool.Parse(reportData["checkFuelFirstEntryYes"]);

        if (reportData.ContainsKey("checkFuelFirstEntryNo"))
            checkFuelFirstEntryNo.IsChecked = bool.Parse(reportData["checkFuelFirstEntryNo"]);

        if (reportData.ContainsKey("checkSystemStopYes"))
            checkSystemStopYes.IsChecked = bool.Parse(reportData["checkSystemStopYes"]);

        if (reportData.ContainsKey("checkSystemStopNo"))
            checkSystemStopNo.IsChecked = bool.Parse(reportData["checkSystemStopNo"]);

        if (reportData.ContainsKey("checkTestAndResetYes"))
            checkTestAndResetYes.IsChecked = bool.Parse(reportData["checkTestAndResetYes"]);

        if (reportData.ContainsKey("checkTestAndResetNo"))
            checkTestAndResetNo.IsChecked = bool.Parse(reportData["checkTestAndResetNo"]);

        if (reportData.ContainsKey("checkA_ID3"))
            checkA_ID3.IsChecked = bool.Parse(reportData["checkA_ID3"]);

        if (reportData.ContainsKey("checkB_AR3"))
            checkB_AR3.IsChecked = bool.Parse(reportData["checkB_AR3"]);

        if (reportData.ContainsKey("checkC_NCS3"))
            checkC_NCS3.IsChecked = bool.Parse(reportData["checkC_NCS3"]);

        if (reportData.ContainsKey("checkSystemDosingFacilitiesYes"))
            checkSystemDosingFacilitiesYes.IsChecked = bool.Parse(reportData["checkSystemDosingFacilitiesYes"]);

    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("uern", uern.Text ?? string.Empty);
        reportData.Add("SheetNo", SheetNo.Text ?? string.Empty);
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("nameAndAddressOfPremises", nameAndAddressOfPremises.Text ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        reportData.Add("ventilationCalculations", ventilationCalculations.Text ?? string.Empty);
        //reportData.Add("existingHighLevel", existingHighLevel.Text ?? string.Empty);
        //reportData.Add("existingLowLevel", existingLowLevel.Text ?? string.Empty);
        //reportData.Add("requiredHighLevel", requiredHighLevel.Text ?? string.Empty);
        //reportData.Add("requiredLowLevel", requiredLowLevel.Text ?? string.Empty);
        reportData.Add("existingLowLevel", existingHighLevel.Text ?? string.Empty);
        reportData.Add("existingLowLevel", existingLowLevel.Text ?? string.Empty);
        reportData.Add("requiredHighLevel", requiredHighLevel.Text ?? string.Empty);
        reportData.Add("requiredHighLevel", requiredLowLevel.Text ?? string.Empty);
        reportData.Add("ventilationChecksComments", ventilationChecksComments.Text ?? string.Empty);
        reportData.Add("flueChecksComments", flueChecksComments.Text ?? string.Empty);
        reportData.Add("emergencyStopButtonComment", emergencyStopButtonComment.Text ?? string.Empty);
        reportData.Add("safetyInterlocksComments", safetyInterlocksComments.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("contractor", contractor.Text ?? string.Empty);
        reportData.Add("companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Date.ToString("d/M/yyyy") ?? string.Empty);
        reportData.Add("engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("date", date.Date.ToString("d/M/yyyy") ?? string.Empty);
        


        reportData.Add("checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString());
        //reportData.Add("checkExistingHighLevelCM", checkExistingHighLevelCM.IsChecked.ToString());
        //reportData.Add("checkExistingLowLevelCM", checkExistingLowLevelCM.IsChecked.ToString());
        //reportData.Add("checkRequiredHighLevelCM", checkRequiredHighLevelCM.IsChecked.ToString());
        //reportData.Add("checkRequiredLowLevelCM", checkRequiredLowLevelCM.IsChecked.ToString());
        reportData.Add("checkExistingLowLevelCM", checkExistingLevelCM.IsChecked.ToString());
        reportData.Add("checkRequiredHighLevelCM", checkRequiredLevelCM.IsChecked.ToString());
        reportData.Add("checkVentilationCorrectlySizedYes", checkVentilationCorrectlySizedYes.IsChecked.ToString());
        reportData.Add("checkVentilationAtTheCorrectHeightYes", checkVentilationAtTheCorrectHeightYes.IsChecked.ToString());
        reportData.Add("checkVentilationArrangementsYes", checkVentilationArrangementsYes.IsChecked.ToString());
        reportData.Add("checkA_ID1", checkA_ID1.IsChecked.ToString());
        reportData.Add("checkB_AR1", checkB_AR1.IsChecked.ToString());
        reportData.Add("checkC_NCS1", checkC_NCS1.IsChecked.ToString());
        reportData.Add("checkFluesFittedYes", checkFluesFittedYes.IsChecked.ToString());
        reportData.Add("checkFluesFittedNo", checkFluesFittedNo.IsChecked.ToString());
        reportData.Add("checkFluesSupportedYes", checkFluesSupportedYes.IsChecked.ToString());
        reportData.Add("checkFluesSupportedNo", checkFluesSupportedNo.IsChecked.ToString());
        reportData.Add("checkFluesInLineYes", checkFluesInLineYes.IsChecked.ToString());
        reportData.Add("checkFluesInLineNo", checkFluesInLineNo.IsChecked.ToString());
        reportData.Add("checkFacilitiesYes", checkFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkFacilitiesNo", checkFacilitiesNo.IsChecked.ToString());
        reportData.Add("checkFlueGradientsYes", checkFlueGradientsYes.IsChecked.ToString());
        reportData.Add("checkFlueGradientsNo", checkFlueGradientsNo.IsChecked.ToString());
        reportData.Add("checkFluesInspectionYes", checkFluesInspectionYes.IsChecked.ToString());
        reportData.Add("checkFluesInspectionNo", checkFluesInspectionNo.IsChecked.ToString());
        reportData.Add("checkFlueJointsYes", checkFlueJointsYes.IsChecked.ToString());
        reportData.Add("checkFlueJointsNo", checkFlueJointsNo.IsChecked.ToString());
        reportData.Add("checkA_ID2", checkA_ID2.IsChecked.ToString());
        reportData.Add("checkB_AR2", checkB_AR2.IsChecked.ToString());
        reportData.Add("checkC_NCS2", checkC_NCS2.IsChecked.ToString());
        reportData.Add("checkInterlocksProvidedYes", checkInterlocksProvidedYes.IsChecked.ToString());
        reportData.Add("checkInterlocksProvidedNo", checkInterlocksProvidedNo.IsChecked.ToString());
        reportData.Add("checkEmergencyShutOffButtonYes", checkEmergencyShutOffButtonYes.IsChecked.ToString());
        reportData.Add("checkEmergencyShutOffButtonNo", checkEmergencyShutOffButtonNo.IsChecked.ToString());
        reportData.Add("checkPlantInterlinkYes", checkPlantInterlinkYes.IsChecked.ToString());
        reportData.Add("checkPlantInterlinkNo", checkPlantInterlinkNo.IsChecked.ToString());
        reportData.Add("checkFuelShutOffYes", checkFuelShutOffYes.IsChecked.ToString());
        reportData.Add("checkFuelShutOffNo", checkFuelShutOffNo.IsChecked.ToString());
        reportData.Add("checkFuelFirstEntryYes", checkFuelFirstEntryYes.IsChecked.ToString());
        reportData.Add("checkFuelFirstEntryNo", checkFuelFirstEntryNo.IsChecked.ToString());
        reportData.Add("checkSystemStopYes", checkSystemStopYes.IsChecked.ToString());
        reportData.Add("checkSystemStopNo", checkSystemStopNo.IsChecked.ToString());
        reportData.Add("checkTestAndResetYes", checkTestAndResetYes.IsChecked.ToString());
        reportData.Add("checkTestAndResetNo", checkTestAndResetNo.IsChecked.ToString());
        reportData.Add("checkA_ID3", checkA_ID3.IsChecked.ToString());
        reportData.Add("checkB_AR3", checkB_AR3.IsChecked.ToString());
        reportData.Add("checkC_NCS3", checkC_NCS3.IsChecked.ToString());
        reportData.Add("checkSystemDosingFacilitiesYes", checkSystemDosingFacilitiesYes.IsChecked.ToString());
     





        return reportData;
    }


    // Disjunct Buttons


    public void DisjunctCheckboxes(CheckBox a, CheckBox b, CheckBox c)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;

        a.Color = Colors.Red;
        b.Color = Colors.White;
        c.Color = Colors.White;
    }


    public void CheckFluesFittedYesChanged(object sender, EventArgs e)
    {
        if (checkFluesFittedYes.IsChecked)
            DisjunctCheckboxes(checkFluesFittedYes, checkFluesFittedNo, checkFluesFittedNA);
        else
        {
            checkFluesFittedYes.Color = Colors.White;
            if (!checkFluesFittedNo.IsChecked)
                DisjunctCheckboxes(checkFluesFittedNA, checkFluesFittedYes, checkFluesFittedNo);
        }
    }
    public void CheckFluesFittedNoChanged(object sender, EventArgs e)
    {
        if (checkFluesFittedNo.IsChecked)
            DisjunctCheckboxes(checkFluesFittedNo, checkFluesFittedYes, checkFluesFittedNA);
        else
        {
            checkFluesFittedNo.Color = Colors.White;
            if (!checkFluesFittedYes.IsChecked)
                DisjunctCheckboxes(checkFluesFittedNA, checkFluesFittedYes, checkFluesFittedNo);
        }
    }
    public void CheckFluesFittedNAChanged(object sender, EventArgs e)
    {
        if (checkFluesFittedNA.IsChecked || !checkFluesFittedYes.IsChecked && !checkFluesFittedNo.IsChecked)
            DisjunctCheckboxes(checkFluesFittedNA, checkFluesFittedYes, checkFluesFittedNo);
        else
            checkFluesFittedNA.Color = Colors.White;
    }




    public void CheckFluesSupportedYesChanged(object sender, EventArgs e)
    {
        if (checkFluesSupportedYes.IsChecked)
            DisjunctCheckboxes(checkFluesSupportedYes, checkFluesSupportedNo, checkFluesSupportedNA);
        else
        {
            checkFluesSupportedYes.Color = Colors.White;
            if (!checkFluesSupportedNo.IsChecked)
                DisjunctCheckboxes(checkFluesSupportedNA, checkFluesSupportedYes, checkFluesSupportedNo);
        }
    }
    public void CheckFluesSupportedNoChanged(object sender, EventArgs e)
    {
        if (checkFluesSupportedNo.IsChecked)
            DisjunctCheckboxes(checkFluesSupportedNo, checkFluesSupportedYes, checkFluesSupportedNA);
        else
        {
            checkFluesSupportedNo.Color = Colors.White;
            if (!checkFluesSupportedYes.IsChecked)
                DisjunctCheckboxes(checkFluesSupportedNA, checkFluesSupportedYes, checkFluesSupportedNo);
        }
    }
    public void CheckFluesSupportedNAChanged(object sender, EventArgs e)
    {
        if (checkFluesSupportedNA.IsChecked || !checkFluesSupportedYes.IsChecked && !checkFluesSupportedNo.IsChecked)
            DisjunctCheckboxes(checkFluesSupportedNA, checkFluesSupportedYes, checkFluesSupportedNo);
        else
            checkFluesSupportedNA.Color = Colors.White;
    }




    public void CheckFluesInLineYesChanged(object sender, EventArgs e)
    {
        if (checkFluesInLineYes.IsChecked)
            DisjunctCheckboxes(checkFluesInLineYes, checkFluesInLineNo, checkFluesInLineNA);
        else
        {
            checkFluesInLineYes.Color = Colors.White;
            if (!checkFluesInLineNo.IsChecked)
                DisjunctCheckboxes(checkFluesInLineNA, checkFluesInLineYes, checkFluesInLineNo);
        }
    }
    public void CheckFluesInLineNoChanged(object sender, EventArgs e)
    {
        if (checkFluesInLineNo.IsChecked)
            DisjunctCheckboxes(checkFluesInLineNo, checkFluesInLineYes, checkFluesInLineNA);
        else
        {
            checkFluesInLineNo.Color = Colors.White;
            if (!checkFluesInLineYes.IsChecked)
                DisjunctCheckboxes(checkFluesInLineNA, checkFluesInLineYes, checkFluesInLineNo);
        }
    }
    public void CheckFluesInLineNAChanged(object sender, EventArgs e)
    {
        if (checkFluesInLineNA.IsChecked || !checkFluesInLineYes.IsChecked && !checkFluesInLineNo.IsChecked)
            DisjunctCheckboxes(checkFluesInLineNA, checkFluesInLineYes, checkFluesInLineNo);
        else
            checkFluesInLineNA.Color = Colors.White;
    }




    public void CheckFacilitiesYesChanged(object sender, EventArgs e)
    {
        if (checkFacilitiesYes.IsChecked)
            DisjunctCheckboxes(checkFacilitiesYes, checkFacilitiesNo, checkFacilitiesNA);
        else
        {
            checkFacilitiesYes.Color = Colors.White;
            if (!checkFacilitiesNo.IsChecked)
                DisjunctCheckboxes(checkFacilitiesNA, checkFacilitiesYes, checkFacilitiesNo);
        }
    }
    public void CheckFacilitiesNoChanged(object sender, EventArgs e)
    {
        if (checkFacilitiesNo.IsChecked)
            DisjunctCheckboxes(checkFacilitiesNo, checkFacilitiesYes, checkFacilitiesNA);
        else
        {
            checkFacilitiesNo.Color = Colors.White;
            if (!checkFacilitiesYes.IsChecked)
                DisjunctCheckboxes(checkFacilitiesNA, checkFacilitiesYes, checkFacilitiesNo);
        }
    }
    public void CheckFacilitiesNAChanged(object sender, EventArgs e)
    {
        if (checkFacilitiesNA.IsChecked || !checkFacilitiesYes.IsChecked && !checkFacilitiesNo.IsChecked)
            DisjunctCheckboxes(checkFacilitiesNA, checkFacilitiesYes, checkFacilitiesNo);
        else
            checkFacilitiesNA.Color = Colors.White;
    }




    public void CheckFlueGradientsYesChanged(object sender, EventArgs e)
    {
        if (checkFlueGradientsYes.IsChecked)
            DisjunctCheckboxes(checkFlueGradientsYes, checkFlueGradientsNo, checkFlueGradientsNA);
        else
        {
            checkFlueGradientsYes.Color = Colors.White;
            if (!checkFlueGradientsNo.IsChecked)
                DisjunctCheckboxes(checkFlueGradientsNA, checkFlueGradientsYes, checkFlueGradientsNo);
        }
    }
    public void CheckFlueGradientsNoChanged(object sender, EventArgs e)
    {
        if (checkFlueGradientsNo.IsChecked)
            DisjunctCheckboxes(checkFlueGradientsNo, checkFlueGradientsYes, checkFlueGradientsNA);
        else
        {
            checkFlueGradientsNo.Color = Colors.White;
            if (!checkFlueGradientsYes.IsChecked)
                DisjunctCheckboxes(checkFlueGradientsNA, checkFlueGradientsYes, checkFlueGradientsNo);
        }
    }
    public void CheckFlueGradientsNAChanged(object sender, EventArgs e)
    {
        if (checkFlueGradientsNA.IsChecked || !checkFlueGradientsYes.IsChecked && !checkFlueGradientsNo.IsChecked)
            DisjunctCheckboxes(checkFlueGradientsNA, checkFlueGradientsYes, checkFlueGradientsNo);
        else
            checkFlueGradientsNA.Color = Colors.White;
    }




    public void CheckFluesInspectionYesChanged(object sender, EventArgs e)
    {
        if (checkFluesInspectionYes.IsChecked)
            DisjunctCheckboxes(checkFluesInspectionYes, checkFluesInspectionNo, checkFluesInspectionNA);
        else
        {
            checkFluesInspectionYes.Color = Colors.White;
            if (!checkFluesInspectionNo.IsChecked)
                DisjunctCheckboxes(checkFluesInspectionNA, checkFluesInspectionYes, checkFluesInspectionNo);
        }
    }
    public void CheckFluesInspectionNoChanged(object sender, EventArgs e)
    {
        if (checkFluesInspectionNo.IsChecked)
            DisjunctCheckboxes(checkFluesInspectionNo, checkFluesInspectionYes, checkFluesInspectionNA);
        else
        {
            checkFluesInspectionNo.Color = Colors.White;
            if (!checkFluesInspectionYes.IsChecked)
                DisjunctCheckboxes(checkFluesInspectionNA, checkFluesInspectionYes, checkFluesInspectionNo);
        }
    }
    public void CheckFluesInspectionNAChanged(object sender, EventArgs e)
    {
        if (checkFluesInspectionNA.IsChecked || !checkFluesInspectionYes.IsChecked && !checkFluesInspectionNo.IsChecked)
            DisjunctCheckboxes(checkFluesInspectionNA, checkFluesInspectionYes, checkFluesInspectionNo);
        else
            checkFluesInspectionNA.Color = Colors.White;
    }




    public void CheckFlueJointsYesChanged(object sender, EventArgs e)
    {
        if (checkFlueJointsYes.IsChecked)
            DisjunctCheckboxes(checkFlueJointsYes, checkFlueJointsNo, checkFlueJointsNA);
        else
        {
            checkFlueJointsYes.Color = Colors.White;
            if (!checkFlueJointsNo.IsChecked)
                DisjunctCheckboxes(checkFlueJointsNA, checkFlueJointsYes, checkFlueJointsNo);
        }
    }
    public void CheckFlueJointsNoChanged(object sender, EventArgs e)
    {
        if (checkFlueJointsNo.IsChecked)
            DisjunctCheckboxes(checkFlueJointsNo, checkFlueJointsYes, checkFlueJointsNA);
        else
        {
            checkFlueJointsNo.Color = Colors.White;
            if (!checkFlueJointsYes.IsChecked)
                DisjunctCheckboxes(checkFlueJointsNA, checkFlueJointsYes, checkFlueJointsNo);
        }
    }
    public void CheckFlueJointsNAChanged(object sender, EventArgs e)
    {
        if (checkFlueJointsNA.IsChecked || !checkFlueJointsYes.IsChecked && !checkFlueJointsNo.IsChecked)
            DisjunctCheckboxes(checkFlueJointsNA, checkFlueJointsYes, checkFlueJointsNo);
        else
            checkFlueJointsNA.Color = Colors.White;
    }




    public void CheckInterlocksProvidedYesChanged(object sender, EventArgs e)
    {
        if (checkInterlocksProvidedYes.IsChecked)
            DisjunctCheckboxes(checkInterlocksProvidedYes, checkInterlocksProvidedNo, checkInterlocksProvidedNA);
        else
        {
            checkInterlocksProvidedYes.Color = Colors.White;
            if (!checkInterlocksProvidedNo.IsChecked)
                DisjunctCheckboxes(checkInterlocksProvidedNA, checkInterlocksProvidedYes, checkInterlocksProvidedNo);
        }
    }
    public void CheckInterlocksProvidedNoChanged(object sender, EventArgs e)
    {
        if (checkInterlocksProvidedNo.IsChecked)
            DisjunctCheckboxes(checkInterlocksProvidedNo, checkInterlocksProvidedYes, checkInterlocksProvidedNA);
        else
        {
            checkInterlocksProvidedNo.Color = Colors.White;
            if (!checkInterlocksProvidedYes.IsChecked)
                DisjunctCheckboxes(checkInterlocksProvidedNA, checkInterlocksProvidedYes, checkInterlocksProvidedNo);
        }
    }
    public void CheckInterlocksProvidedNAChanged(object sender, EventArgs e)
    {
        if (checkInterlocksProvidedNA.IsChecked || !checkInterlocksProvidedYes.IsChecked && !checkInterlocksProvidedNo.IsChecked)
            DisjunctCheckboxes(checkInterlocksProvidedNA, checkInterlocksProvidedYes, checkInterlocksProvidedNo);
        else
            checkInterlocksProvidedNA.Color = Colors.White;
    }




    public void CheckEmergencyShutOffButtonYesChanged(object sender, EventArgs e)
    {
        if (checkEmergencyShutOffButtonYes.IsChecked)
            DisjunctCheckboxes(checkEmergencyShutOffButtonYes, checkEmergencyShutOffButtonNo, checkEmergencyShutOffButtonNA);
        else
        {
            checkEmergencyShutOffButtonYes.Color = Colors.White;
            if (!checkEmergencyShutOffButtonNo.IsChecked)
                DisjunctCheckboxes(checkEmergencyShutOffButtonNA, checkEmergencyShutOffButtonYes, checkEmergencyShutOffButtonNo);
        }
    }
    public void CheckEmergencyShutOffButtonNoChanged(object sender, EventArgs e)
    {
        if (checkEmergencyShutOffButtonNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyShutOffButtonNo, checkEmergencyShutOffButtonYes, checkEmergencyShutOffButtonNA);
        else
        {
            checkEmergencyShutOffButtonNo.Color = Colors.White;
            if (!checkEmergencyShutOffButtonYes.IsChecked)
                DisjunctCheckboxes(checkEmergencyShutOffButtonNA, checkEmergencyShutOffButtonYes, checkEmergencyShutOffButtonNo);
        }
    }
    public void CheckEmergencyShutOffButtonNAChanged(object sender, EventArgs e)
    {
        if (checkEmergencyShutOffButtonNA.IsChecked || !checkEmergencyShutOffButtonYes.IsChecked && !checkEmergencyShutOffButtonNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyShutOffButtonNA, checkEmergencyShutOffButtonYes, checkEmergencyShutOffButtonNo);
        else
            checkEmergencyShutOffButtonNA.Color = Colors.White;
    }




    public void CheckPlantInterlinkYesChanged(object sender, EventArgs e)
    {
        if (checkPlantInterlinkYes.IsChecked)
            DisjunctCheckboxes(checkPlantInterlinkYes, checkPlantInterlinkNo, checkPlantInterlinkNA);
        else
        {
            checkPlantInterlinkYes.Color = Colors.White;
            if (!checkPlantInterlinkNo.IsChecked)
                DisjunctCheckboxes(checkPlantInterlinkNA, checkPlantInterlinkYes, checkPlantInterlinkNo);
        }
    }
    public void CheckPlantInterlinkNoChanged(object sender, EventArgs e)
    {
        if (checkPlantInterlinkNo.IsChecked)
            DisjunctCheckboxes(checkPlantInterlinkNo, checkPlantInterlinkYes, checkPlantInterlinkNA);
        else
        {
            checkPlantInterlinkNo.Color = Colors.White;
            if (!checkPlantInterlinkYes.IsChecked)
                DisjunctCheckboxes(checkPlantInterlinkNA, checkPlantInterlinkYes, checkPlantInterlinkNo);
        }
    }
    public void CheckPlantInterlinkNAChanged(object sender, EventArgs e)
    {
        if (checkPlantInterlinkNA.IsChecked || !checkPlantInterlinkYes.IsChecked && !checkPlantInterlinkNo.IsChecked)
            DisjunctCheckboxes(checkPlantInterlinkNA, checkPlantInterlinkYes, checkPlantInterlinkNo);
        else
            checkPlantInterlinkNA.Color = Colors.White;
    }




    public void CheckFuelShutOffYesChanged(object sender, EventArgs e)
    {
        if (checkFuelShutOffYes.IsChecked)
            DisjunctCheckboxes(checkFuelShutOffYes, checkFuelShutOffNo, checkFuelShutOffNA);
        else
        {
            checkFuelShutOffYes.Color = Colors.White;
            if (!checkFuelShutOffNo.IsChecked)
                DisjunctCheckboxes(checkFuelShutOffNA, checkFuelShutOffYes, checkFuelShutOffNo);
        }
    }
    public void CheckFuelShutOffNoChanged(object sender, EventArgs e)
    {
        if (checkFuelShutOffNo.IsChecked)
            DisjunctCheckboxes(checkFuelShutOffNo, checkFuelShutOffYes, checkFuelShutOffNA);
        else
        {
            checkFuelShutOffNo.Color = Colors.White;
            if (!checkFuelShutOffYes.IsChecked)
                DisjunctCheckboxes(checkFuelShutOffNA, checkFuelShutOffYes, checkFuelShutOffNo);
        }
    }
    public void CheckFuelShutOffNAChanged(object sender, EventArgs e)
    {
        if (checkFuelShutOffNA.IsChecked || !checkFuelShutOffYes.IsChecked && !checkFuelShutOffNo.IsChecked)
            DisjunctCheckboxes(checkFuelShutOffNA, checkFuelShutOffYes, checkFuelShutOffNo);
        else
            checkFuelShutOffNA.Color = Colors.White;
    }




    public void CheckFuelFirstEntryYesChanged(object sender, EventArgs e)
    {
        if (checkFuelFirstEntryYes.IsChecked)
            DisjunctCheckboxes(checkFuelFirstEntryYes, checkFuelFirstEntryNo, checkFuelFirstEntryNA);
        else
        {
            checkFuelFirstEntryYes.Color = Colors.White;
            if (!checkFuelFirstEntryNo.IsChecked)
                DisjunctCheckboxes(checkFuelFirstEntryNA, checkFuelFirstEntryYes, checkFuelFirstEntryNo);
        }
    }
    public void CheckFuelFirstEntryNoChanged(object sender, EventArgs e)
    {
        if (checkFuelFirstEntryNo.IsChecked)
            DisjunctCheckboxes(checkFuelFirstEntryNo, checkFuelFirstEntryYes, checkFuelFirstEntryNA);
        else
        {
            checkFuelFirstEntryNo.Color = Colors.White;
            if (!checkFuelFirstEntryYes.IsChecked)
                DisjunctCheckboxes(checkFuelFirstEntryNA, checkFuelFirstEntryYes, checkFuelFirstEntryNo);
        }
    }
    public void CheckFuelFirstEntryNAChanged(object sender, EventArgs e)
    {
        if (checkFuelFirstEntryNA.IsChecked || !checkFuelFirstEntryYes.IsChecked && !checkFuelFirstEntryNo.IsChecked)
            DisjunctCheckboxes(checkFuelFirstEntryNA, checkFuelFirstEntryYes, checkFuelFirstEntryNo);
        else
            checkFuelFirstEntryNA.Color = Colors.White;
    }




    public void CheckSystemStopYesChanged(object sender, EventArgs e)
    {
        if (checkSystemStopYes.IsChecked)
            DisjunctCheckboxes(checkSystemStopYes, checkSystemStopNo, checkSystemStopNA);
        else
        {
            checkSystemStopYes.Color = Colors.White;
            if (!checkSystemStopNo.IsChecked)
                DisjunctCheckboxes(checkSystemStopNA, checkSystemStopYes, checkSystemStopNo);
        }
    }
    public void CheckSystemStopNoChanged(object sender, EventArgs e)
    {
        if (checkSystemStopNo.IsChecked)
            DisjunctCheckboxes(checkSystemStopNo, checkSystemStopYes, checkSystemStopNA);
        else
        {
            checkSystemStopNo.Color = Colors.White;
            if (!checkSystemStopYes.IsChecked)
                DisjunctCheckboxes(checkSystemStopNA, checkSystemStopYes, checkSystemStopNo);
        }
    }
    public void CheckSystemStopNAChanged(object sender, EventArgs e)
    {
        if (checkSystemStopNA.IsChecked || !checkSystemStopYes.IsChecked && !checkSystemStopNo.IsChecked)
            DisjunctCheckboxes(checkSystemStopNA, checkSystemStopYes, checkSystemStopNo);
        else
            checkSystemStopNA.Color = Colors.White;
    }




    public void CheckTestAndResetYesChanged(object sender, EventArgs e)
    {
        if (checkTestAndResetYes.IsChecked)
            DisjunctCheckboxes(checkTestAndResetYes, checkTestAndResetNo, checkTestAndResetNA);
        else
        {
            checkTestAndResetYes.Color = Colors.White;
            if (!checkTestAndResetNo.IsChecked)
                DisjunctCheckboxes(checkTestAndResetNA, checkTestAndResetYes, checkTestAndResetNo);
        }
    }
    public void CheckTestAndResetNoChanged(object sender, EventArgs e)
    {
        if (checkTestAndResetNo.IsChecked)
            DisjunctCheckboxes(checkTestAndResetNo, checkTestAndResetYes, checkTestAndResetNA);
        else
        {
            checkTestAndResetNo.Color = Colors.White;
            if (!checkTestAndResetYes.IsChecked)
                DisjunctCheckboxes(checkTestAndResetNA, checkTestAndResetYes, checkTestAndResetNo);
        }
    }
    public void CheckTestAndResetNAChanged(object sender, EventArgs e)
    {
        if (checkTestAndResetNA.IsChecked || !checkTestAndResetYes.IsChecked && !checkTestAndResetNo.IsChecked)
            DisjunctCheckboxes(checkTestAndResetNA, checkTestAndResetYes, checkTestAndResetNo);
        else
            checkTestAndResetNA.Color = Colors.White;
    }



    // ID, AR, NCS


    public void DisjunctABC(CheckBox a, CheckBox b, CheckBox c)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;
    }

    public void A1(object sender, EventArgs e)
    {
        if (checkA_ID1.IsChecked)
            DisjunctABC(checkA_ID1, checkB_AR1, checkC_NCS1);
    }
    public void B1(object sender, EventArgs e)
    {
        if (checkB_AR1.IsChecked)
            DisjunctABC(checkB_AR1, checkA_ID1, checkC_NCS1);
    }
    public void C1(object sender, EventArgs e)
    {
        if (checkC_NCS1.IsChecked)
            DisjunctABC(checkC_NCS1, checkA_ID1, checkB_AR1);
    }


    public void A2(object sender, EventArgs e)
    {
        if (checkA_ID2.IsChecked)
            DisjunctABC(checkA_ID2, checkB_AR2, checkC_NCS2);
    }
    public void B2(object sender, EventArgs e)
    {
        if (checkB_AR2.IsChecked)
            DisjunctABC(checkB_AR2, checkA_ID2, checkC_NCS2);
    }
    public void C2(object sender, EventArgs e)
    {
        if (checkC_NCS2.IsChecked)
            DisjunctABC(checkC_NCS2, checkA_ID2, checkB_AR2);
    }


    public void A3(object sender, EventArgs e)
    {
        if (checkA_ID3.IsChecked)
            DisjunctABC(checkA_ID3, checkB_AR3, checkC_NCS3);
    }
    public void B3(object sender, EventArgs e)
    {
        if (checkB_AR3.IsChecked)
            DisjunctABC(checkB_AR3, checkA_ID3, checkC_NCS3);
    }
    public void C3(object sender, EventArgs e)
    {
        if (checkC_NCS3.IsChecked)
            DisjunctABC(checkC_NCS3, checkA_ID3, checkB_AR3);
    }
}