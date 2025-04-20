using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class PressurisationUnitReportPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    private bool previewOnly = false;
    private readonly Enums.ReportType reportType = Enums.ReportType.PressurisationUnitReport;

    public PressurisationUnitReportPage()
	{
		InitializeComponent();
	}
    public PressurisationUnitReportPage(Report report)
    {
        InitializeComponent();
        previewOnly = true;
        PreviewPressurisationUnitReportPage(report.ReportData);
    }
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;

        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }
    private async Task UploadReport(Folder folder, Dictionary<string, string> reportData)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        PressurisationUnitReportBackBtt.IsEnabled = false;

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
            PressurisationUnitReportBackBtt.IsEnabled = true;
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

        // Always refresh displayed list
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
        // Do not Show Folders if in preview of PDF page
        if (!previewOnly)
            await LoadFolders();
    }
    public async void PressurisationUnitReportNext3(object sender, EventArgs e)
    {
        // Do not Show Folders if in preview of PDF page
        if (previewOnly)
            await Navigation.PopModalAsync();

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
    public void PreviewPressurisationUnitReportPage(Dictionary<string,string> reportData) 
    {
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
            this.date.Date = DateTime.ParseExact(date, "d/M/yyyy", null);

        //fale bool
        if (reportData.ContainsKey("checkMainWaterSupply"))
        {
            checkMainsWatersSupplyYes.IsChecked = Convert.ToBoolean(reportData["checkMainWaterSupply"]);
        }
        if (reportData.ContainsKey("checkColdFillPressureSet"))
        {
            checkColdFillPressureSetCorrectlyYes.IsChecked = Convert.ToBoolean(reportData["checkColdFillPressureSet"]);
        }
        if (reportData.ContainsKey("checkElectricalSupplyWorking"))
        {
            checkElectricalSupplyWorkingYes.IsChecked = Convert.ToBoolean(reportData["checkElectricalSupplyWorking"]);
        }
        if (reportData.ContainsKey("checkFillingLoopDisconnected"))
        {
            checkFillingLoopDisconnectedYes.IsChecked = Convert.ToBoolean(reportData["checkFillingLoopDisconnected"]);
        }
        if (reportData.ContainsKey("checkUnitLeftOperational"))
        {
            checkUnitLeftOperationalYes.IsChecked = Convert.ToBoolean(reportData["checkUnitLeftOperational"]);
        }


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
        reportData.Add("date", date.Date.ToString("d/M/yyyy") ?? string.Empty);


        reportData.Add("checkMainWaterSupply", checkMainsWatersSupplyYes.IsChecked.ToString());
        reportData.Add("checkColdFillPressureSet", checkColdFillPressureSetCorrectlyYes.IsChecked.ToString());
        reportData.Add("checkElectricalSupplyWorking", checkElectricalSupplyWorkingYes.IsChecked.ToString());
        reportData.Add("checkFillingLoopDisconnected", checkFillingLoopDisconnectedYes.IsChecked.ToString());
        reportData.Add("checkUnitLeftOperational", checkUnitLeftOperationalYes.IsChecked.ToString());





        return reportData;
    }
}