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
        NewFolderPopup popup = new NewFolderPopup();
        popup.PopupClosed += async (s, e) =>
        {
            await LoadFolders();
        };

        this.ShowPopup(popup);
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

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Pressurisation_Unit_Report_{dateTimeString}.pdf";
        reportData = GatherReportData();

        //PdfCreation.PressurisationReport(GatherReportData());
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