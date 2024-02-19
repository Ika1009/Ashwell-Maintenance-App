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

        _ = UploadReport(Folders.First(folder => folder.Id == folderId));
    }

    /// <summary>
    /// Uploads the report to the server and handles the response.
    /// If the upload is successful, it navigates back.
    /// If the folder contains signatures, it retrieves them and creates a PDF report.
    /// The PDF report is then uploaded to Dropbox.
    /// </summary>
    /// <param name="folder">The folder containing the report data.</param>
    private async Task UploadReport(Folder folder)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        EngineersReportBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.EngineersReport, reportName, folder.Id, reportData);

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

                byte[] pdfData = await PdfCreation.EngineersReport(reportData, signature1, signature2); ;
                
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



    public void DisjunctCheckboxes(CheckBox a, CheckBox b, CheckBox c)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;

        a.Color = Colors.Red;
        b.Color = Colors.White;
        c.Color = Colors.White;
    }

    public void CheckSpillageTestPassChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestPass.IsChecked)
            DisjunctCheckboxes(checkSpillageTestPass, checkSpillageTestNo, checkSpillageTestNA);
        else
            checkSpillageTestPass.Color = Colors.White;
    }
    public void CheckSpillageTestNoChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNo.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNo, checkSpillageTestPass, checkSpillageTestNA);
        else
            checkSpillageTestNo.Color = Colors.White;
    }
    public void CheckSpillageTestNAChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNA.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNA, checkSpillageTestPass, checkSpillageTestNo);
        else
            checkSpillageTestNA.Color = Colors.White;
    }

    public void CheckFlueFlowTestPassChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestPass.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestPass, checkFlueFlowTestFail, checkFlueFlowTestNA);
        else
            checkFlueFlowTestPass.Color = Colors.White;
    }
    public void CheckFlueFlowTestFailChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestFail.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestFail, checkFlueFlowTestPass, checkFlueFlowTestNA);
        else
            checkFlueFlowTestFail.Color = Colors.White;
    }
    public void CheckFlueFlowTestNAChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestNA.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestNA, checkFlueFlowTestPass, checkFlueFlowTestFail);
        else
            checkFlueFlowTestNA.Color = Colors.White;
    }

    public void CheckTightnessTestPassChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestPass.IsChecked)
            DisjunctCheckboxes(checkTightnessTestPass, checkTightnessTestFail, checkTightnessTestNA);
        else
            checkTightnessTestPass.Color = Colors.White;
    }
    public void CheckTightnessTestFailChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestFail.IsChecked)
            DisjunctCheckboxes(checkTightnessTestFail, checkTightnessTestPass, checkTightnessTestNA);
        else
            checkTightnessTestFail.Color = Colors.White;
    }
    public void CheckTightnessTestNAChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestNA.IsChecked)
            DisjunctCheckboxes(checkTightnessTestNA, checkTightnessTestPass, checkTightnessTestFail);
        else
            checkTightnessTestNA.Color = Colors.White;
    }
}