using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class EngineersReportPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    bool previewOnly = false;
    private readonly Enums.ReportType reportType = Enums.ReportType.EngineersReport;

    public EngineersReportPage()
	{
		InitializeComponent();
	}
    public EngineersReportPage(Report report)
    {
        InitializeComponent();
        previewOnly = true;
        PreviewEngineersReportPage(report.ReportData);
    }
    public void FolderChosen(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        EngineersReportBackBtt.IsEnabled = false;

        string folderId = button?.CommandParameter as string;
        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }
    private async Task UploadReport(Folder folder, Dictionary<string, string> reportData)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        EngineersReportBackBtt.IsEnabled = false;

        try
        {
            await ReportManager.UploadReportAsync(reportType, reportName, folder, reportData);

            // If we reach here, both data and PDF (if any) uploaded successfully
            await DisplayAlert("Success", "Report successfully uploaded.", "OK");
        }
        catch (HttpRequestException)
        {
            // Network/server issue: saved locally by retry logic
            await DisplayAlert("Offline", "No internet or server error. Report saved locally.", "OK");

            await ReportManager.SaveReportLocallyAsync(reportType, reportName, folder, reportData);
        }
        catch (Exception ex)
        {
            // Any other error (PDF generation/upload, etc.)
            await DisplayAlert("Error", $"Upload failed: {ex.Message}. Report saved locally.", "OK");

            await ReportManager.SaveReportLocallyAsync(reportType, reportName, folder, reportData);
        }
        finally
        {
            loadingBG.IsRunning = false;
            loading.IsRunning = false;
            EngineersReportBackBtt.IsEnabled = true;
            await Navigation.PopModalAsync();
        }
    }

    public async void NewFolder(object sender, EventArgs e)
    {
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (string.IsNullOrWhiteSpace(folderName)) return;

        loadingBG.IsRunning = true;
        loading.IsRunning = true;

        var newFolder = await FolderManager.CreateFolderAsync(folderName);
        Folders.Add(newFolder);

        // Always refresh displayed list
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            await FolderManager.LoadFoldersAsync(Folders, FoldersListView);

        loadingBG.IsRunning = false;
        loading.IsRunning = false;
    }
    private async Task LoadFolders()
    {
        await FolderManager.LoadFoldersAsync(Folders, FoldersListView);
    }

    public async void FolderEdit(object sender, EventArgs e)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;

        var folderId = (sender as ImageButton)?.CommandParameter as string;
        var folder = Folders.FirstOrDefault(f => f.Id == folderId);

        if (folder != null)
            await FolderManager.EditFolderAsync(folder, Folders);

        loadingBG.IsRunning = false;
        loading.IsRunning = false;
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

            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;
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

        // Do not Show Folders if in preview of PDF page
        if (!previewOnly)
            await LoadFolders();
    }
    
    public async void ERNext3(object sender, EventArgs e)
    {
        // Do not Show Folders if in preview of PDF page
        if (previewOnly)
            await Navigation.PopModalAsync();

        ERSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Engineers_Report_{dateTimeString}.pdf";
        reportData = GatherReportData();
    }
    public void PreviewEngineersReportPage(Dictionary<string, string> reportData)
    {
        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("clientsName")) clientsName.Text = reportData["clientsName"];
        if (reportData.ContainsKey("address")) clientsAdress.Text = reportData["address"];
        if (reportData.ContainsKey("applianceMake")) applianceMake.Text = reportData["applianceMake"];
        if (reportData.ContainsKey("date")) date.Date = DateTime.ParseExact(reportData["date"], "d/M/yyyy", null);
        if (reportData.ContainsKey("engineer")) engineer.Text = reportData["engineer"];
        if (reportData.ContainsKey("taskTNo")) taskTNo.Text = reportData["taskTNo"];
        if (reportData.ContainsKey("serialNumber")) serialNumber.Text = reportData["serialNumber"];
        if (reportData.ContainsKey("description")) descriptionOfWork.Text = reportData["description"];
        if (reportData.ContainsKey("gasOperatinPressure")) gasOperatingPressure.Text = reportData["gasOperatinPressure"];
        if (reportData.ContainsKey("inletPressure")) intletPressure.Text = reportData["inletPressure"];
        if (reportData.ContainsKey("warningNoticeNumber")) warningNoticeNumber.Text = reportData["warningNoticeNumber"];
        if (reportData.ContainsKey("totalHoursIncludingTravel")) totalHours.Text = reportData["totalHoursIncludingTravel"];

        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("checkTaskComplete"))
            checkTaskCompleteYes.IsChecked = bool.Parse(reportData["checkTaskComplete"]);
        if (reportData.ContainsKey("checkSpillageTestPerformed"))
            checkSpillageTestPass.IsChecked = bool.Parse(reportData["checkSpillageTestPerformed"]);
        if (reportData.ContainsKey("checkSpillageTestPerformedNA"))
            checkSpillageTestNA.IsChecked = bool.Parse(reportData["checkSpillageTestPerformedNA"]);
        if (checkSpillageTestPass.IsChecked == false && checkSpillageTestNA.IsChecked == false)
            checkSpillageTestNo.IsChecked = true;

        if (reportData.ContainsKey("checkRiskAssesmentCompleted"))
            checkRiskAssessmentYes.IsChecked = bool.Parse(reportData["checkRiskAssesmentCompleted"]);


        if (reportData.ContainsKey("checkFlueFlowTest"))
            checkFlueFlowTestPass.IsChecked = bool.Parse(reportData["checkFlueFlowTest"]);
        if (reportData.ContainsKey("checkFlueFlowTestNA"))
            checkFlueFlowTestNA.IsChecked = bool.Parse(reportData["checkFlueFlowTestNA"]);
        if (checkFlueFlowTestPass.IsChecked == false && checkFlueFlowTestNA.IsChecked == false)
            checkFlueFlowTestFail.IsChecked = true;


        if (reportData.ContainsKey("checkThightnessTestCarriedOut"))
            checkTightnessTestPass.IsChecked = bool.Parse(reportData["checkThightnessTestCarriedOut"]);
        if (reportData.ContainsKey("checkThightnessTestCarriedOutNA"))
            checkTightnessTestNA.IsChecked = bool.Parse(reportData["checkThightnessTestCarriedOutNA"]);
        if (checkTightnessTestPass.IsChecked == false && checkTightnessTestNA.IsChecked == false)
            checkTightnessTestFail.IsChecked = true;

        if (reportData.ContainsKey("checkApplianceSafeToUse"))
            checkApplianceSafeToUseYes.IsChecked = bool.Parse(reportData["checkApplianceSafeToUse"]);
        if (reportData.ContainsKey("checkWarningNoticeIssued"))
            checkWarningNoticeYes.IsChecked = bool.Parse(reportData["checkWarningNoticeIssued"]);


    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

      //  reportData.Add("", .Text ?? string.Empty);
      //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("address", clientsAdress.Text ?? string.Empty);
        reportData.Add("applianceMake", applianceMake.Text ?? string.Empty);
        reportData.Add("date", date.Date.ToString("d/M/yyyy") ?? string.Empty);
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

    public void CheckSpillageTestPassChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestPass.IsChecked)
            DisjunctCheckboxes(checkSpillageTestPass, checkSpillageTestNo, checkSpillageTestNA);
        else
        {
            checkSpillageTestPass.Color = Colors.White;
            if (!checkSpillageTestNo.IsChecked)
                DisjunctCheckboxes(checkSpillageTestNA, checkSpillageTestPass, checkSpillageTestNo);
        }
    }
    public void CheckSpillageTestNoChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNo.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNo, checkSpillageTestPass, checkSpillageTestNA);
        else
        {
            checkSpillageTestNo.Color = Colors.White;
            if (!checkSpillageTestPass.IsChecked)
                DisjunctCheckboxes(checkSpillageTestNA, checkSpillageTestPass, checkSpillageTestNo);
        }
    }
    public void CheckSpillageTestNAChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNA.IsChecked || !checkSpillageTestPass.IsChecked && !checkSpillageTestNo.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNA, checkSpillageTestPass, checkSpillageTestNo);
        else
            checkSpillageTestNA.Color = Colors.White;
    }

    public void CheckFlueFlowTestPassChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestPass.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestPass, checkFlueFlowTestFail, checkFlueFlowTestNA);
        else
        {
            checkFlueFlowTestPass.Color = Colors.White;
            if (!checkFlueFlowTestFail.IsChecked)
                DisjunctCheckboxes(checkFlueFlowTestNA, checkFlueFlowTestPass, checkFlueFlowTestFail);
        }
    }
    public void CheckFlueFlowTestFailChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestFail.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestFail, checkFlueFlowTestPass, checkFlueFlowTestNA);
        else
        {
            checkFlueFlowTestFail.Color = Colors.White;
            if (!checkFlueFlowTestPass.IsChecked)
                DisjunctCheckboxes(checkFlueFlowTestNA, checkFlueFlowTestPass, checkFlueFlowTestFail);
        }
    }
    public void CheckFlueFlowTestNAChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestNA.IsChecked || !checkFlueFlowTestPass.IsChecked && !checkFlueFlowTestFail.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestNA, checkFlueFlowTestPass, checkFlueFlowTestFail);
        else
            checkFlueFlowTestNA.Color = Colors.White;
    }

    public void CheckTightnessTestPassChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestPass.IsChecked)
            DisjunctCheckboxes(checkTightnessTestPass, checkTightnessTestFail, checkTightnessTestNA);
        else
        {
            checkTightnessTestPass.Color = Colors.White;
            if (!checkTightnessTestFail.IsChecked)
                DisjunctCheckboxes(checkTightnessTestNA, checkTightnessTestPass, checkTightnessTestFail);
        }
    }
    public void CheckTightnessTestFailChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestFail.IsChecked)
            DisjunctCheckboxes(checkTightnessTestFail, checkTightnessTestPass, checkTightnessTestNA);
        else
        {
            checkTightnessTestFail.Color = Colors.White;
            if (!checkTightnessTestPass.IsChecked)
                DisjunctCheckboxes(checkTightnessTestNA, checkTightnessTestPass, checkTightnessTestFail);
        }
    }
    public void CheckTightnessTestNAChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestNA.IsChecked || !checkTightnessTestPass.IsChecked && !checkTightnessTestFail.IsChecked)
            DisjunctCheckboxes(checkTightnessTestNA, checkTightnessTestPass, checkTightnessTestFail);
        else
            checkTightnessTestNA.Color = Colors.White;
    }
}