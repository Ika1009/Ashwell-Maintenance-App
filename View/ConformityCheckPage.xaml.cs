using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class ConformityCheckPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public ConformityCheckPage()
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

        if (folder.Signature1 != null && folder.Signature2 != null)
        {
            try
            {
                byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
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
        await Navigation.PopModalAsync();
    }

    public void NewFolder(object sender, EventArgs e)
    {
        this.ShowPopup(new NewFolderPopup(LoadFolders));
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
        await LoadFolders();
    }
    public async void CCNext5(object sender, EventArgs e)
    {
        CCSection5.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Conformity_Check_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.CheckPage(GatherReportData());
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
        reportData.Add("existingHighLevel", existingHighLevel.Text ?? string.Empty);
        reportData.Add("existingLowLevel", existingLowLevel.Text ?? string.Empty);
        reportData.Add("requiredHighLevel", requiredHighLevel.Text ?? string.Empty);
        reportData.Add("requiredLowLevel", requiredLowLevel.Text ?? string.Empty);
        reportData.Add("ventilationChecksComments", ventilationChecksComments.Text ?? string.Empty);
        reportData.Add("flueChecksComments", flueChecksComments.Text ?? string.Empty);
        reportData.Add("emergencyStopButtonComment", emergencyStopButtonComment.Text ?? string.Empty);
        reportData.Add("safetyInterlocksComments", safetyInterlocksComments.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("contractor", contractor.Text ?? string.Empty);
        reportData.Add("companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Text ?? string.Empty);
        reportData.Add("engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        


        reportData.Add("checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString());
        reportData.Add("checkExistingHighLevelCM", checkExistingHighLevelCM.IsChecked.ToString());
        reportData.Add("checkExistingLowLevelCM", checkExistingLowLevelCM.IsChecked.ToString());
        reportData.Add("checkRequiredHighLevelCM", checkRequiredHighLevelCM.IsChecked.ToString());
        reportData.Add("checkRequiredLowLevelCM", checkRequiredLowLevelCM.IsChecked.ToString());
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
}