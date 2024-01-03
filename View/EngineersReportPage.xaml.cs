using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class EngineersReportPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public EngineersReportPage()
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
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.EngineersReport, reportName, folder.Id, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully uploaded new sheet.", "OK");
                await Navigation.PopModalAsync();
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
    
    public async void EngineersReportBack(object sender, EventArgs e)
	{
		if (ERSection1.IsVisible)
        {
            EngineersReportBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (ERSection2.IsVisible)
		{
			ERSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await ERSection1.ScrollToAsync(0, 0, false);
			ERSection1.IsVisible = true;

		}
		else if(ERSection3.IsVisible)
		{
            ERSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await ERSection2.ScrollToAsync(0, 0, false);
            ERSection2.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await ERSection3.ScrollToAsync(0, 0, false);
            ERSection3.IsVisible = true;
        }
    }

    
    public async void ERNext1(object sender, EventArgs e)
	{
		ERSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await ERSection2.ScrollToAsync(0, 0, false);
		ERSection2.IsVisible = true;
	}
    
    public async void ERNext2(object sender, EventArgs e)
    {
        ERSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await ERSection3.ScrollToAsync(0, 0, false);
        ERSection3.IsVisible = true;
        await LoadFolders();
    }
    
    public async void ERNext3(object sender, EventArgs e)
    {
        ERSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Engineers_Report_{dateTimeString}.pdf";
        reportData = GatherReportData();
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

      //  reportData.Add("", .Text ?? string.Empty);
      //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("address", clientsAdress.Text ?? string.Empty);
        reportData.Add("applianceMake", applianceMake.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        reportData.Add("engineer", engineer.Text ?? string.Empty);
        reportData.Add("taskTNo", taskTNo.Text ?? string.Empty);
        reportData.Add("serialNumber", serialNumber.Text ?? string.Empty);
        reportData.Add("description", descriptionOfWork.Text ?? string.Empty);
        reportData.Add("gasOperatinPressure", gasOperatingPressure.Text ?? string.Empty);
        reportData.Add("inletPressure", intletPressure.Text ?? string.Empty);
        reportData.Add("warningNoticeNumber", warningNoticeNumber.Text ?? string.Empty);
        reportData.Add("totalHoursIncludingTravel", totalHours.Text ?? string.Empty);
      //  reportData.Add("", operativesSignature.Text ?? string.Empty);
      //  reportData.Add("", clientsSignature.Text ?? string.Empty);
        
        reportData.Add("checkTaskComplete", checkTaskCompleteYes.IsChecked.ToString());
        reportData.Add("checkSpillageTestPerformed", checkSpillageTestPass.IsChecked.ToString());
        reportData.Add("checkSpillageTestPerformedNA", checkSpillageTestNA.IsChecked.ToString());
        reportData.Add("checkRiskAssesmentCompleted", checkRiskAssessmentYes.IsChecked.ToString());
        reportData.Add("checkFlueFlowTest", checkFlueFlowTestPass.IsChecked.ToString());
        reportData.Add("checkFlueFlowTestNA", checkFlueFlowTestNA.IsChecked.ToString());
        reportData.Add("checkThightnessTestCarriedOut", checkTightnessTestPass.IsChecked.ToString());
        reportData.Add("checkThightnessTestCarriedOutNA", checkTightnessTestNA.IsChecked.ToString());
        reportData.Add("checkApplianceSafeToUse", checkApplianceSafeToUseYes.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssued", checkWarningNoticeYes.IsChecked.ToString());

        return reportData;
    }
}