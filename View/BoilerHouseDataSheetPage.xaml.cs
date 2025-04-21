using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class BoilerHouseDataSheetPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    bool previewOnly = false;
    private readonly Enums.ReportType reportType = Enums.ReportType.BoilerHouseDataSheet;
    public BoilerHouseDataSheetPage()
	{
		InitializeComponent();

        checkPipeworkToGasMeterNA.IsChecked = true;
        checkRegulatorAndOrMeterNA.IsChecked = true;
        checkSafetyNoticesLabelsNA.IsChecked = true;
        checkLineDiagramNA.IsChecked = true;
        checkColorCodingIndicationTapeNA.IsChecked = true;
        checkMeterHouseVentilationNA.IsChecked = true;
        checkMainFlueNA.IsChecked = true;
        checkChimneyFlueTerminalPositionNA.IsChecked = true;
        checkStubFluersToBoildersNA.IsChecked = true;
        checkIdFanNA.IsChecked = true;
        checkFanBoilerSafetyInterlockNA.IsChecked = true;
        checkGeneralComplianceOfGasPipeNA.IsChecked = true;
        checkVentilationNA.IsChecked = true;
        checkAIVNA.IsChecked = true;
        checkManualNA.IsChecked = true;
	}
    public BoilerHouseDataSheetPage(Report report)
    {
        InitializeComponent();
        previewOnly = true;
        PreviewBoilerHouseDataSheetPage(report.ReportData);
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
        BoilderHouseDataSheetBackBtt.IsEnabled = false;

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
            BoilderHouseDataSheetBackBtt.IsEnabled = true;
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

    public async void BoilerHouseDataSheetBack(object sender, EventArgs e)
	{
		if (BHDSSection1.IsVisible)
        {
            BoilderHouseDataSheetBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (BHDSSection2.IsVisible)
		{
			BHDSSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await BHDSSection1.ScrollToAsync(0, 0, false);
			BHDSSection1.IsVisible = true;
		}
		else if (BHDSSection3.IsVisible)
		{
			BHDSSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await BHDSSection2.ScrollToAsync(0, 0, false);
			BHDSSection2.IsVisible = true;
		}
		else if(BHDSSection4.IsVisible)
		{
            BHDSSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await BHDSSection3.ScrollToAsync(0, 0, false);
            BHDSSection3.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await BHDSSection4.ScrollToAsync(0, 0, false);
            BHDSSection4.IsVisible = true;
        }
	}

    
    public async void BHDSNext1(object sender, EventArgs e)
	{
        BHDSSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await BHDSSection2.ScrollToAsync(0, 0, false);
        BHDSSection2.IsVisible = true;
    }
    
    public async void BHDSNext2(object sender, EventArgs e)
    {
        BHDSSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await BHDSSection3.ScrollToAsync(0, 0, false);
        BHDSSection3.IsVisible = true;
    }
    
    public async void BHDSNext3(object sender, EventArgs e)
    {
        BHDSSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await BHDSSection4.ScrollToAsync(0, 0, false);
        BHDSSection4.IsVisible = true;

        // Do not Show Folders if in preview of PDF page
        if(!previewOnly)
            await LoadFolders();
    }
	public async void BHDSNext4(object sender, EventArgs e)
	{
        // Do not Show Folders if in preview of PDF page
        if (previewOnly)
            await Navigation.PopModalAsync();

        BHDSSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Boiler_House_Data_Sheet_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.BoilerHouseDataSheet(GatherReportData());
    }
    public void PreviewBoilerHouseDataSheetPage(Dictionary<string, string> reportData)
    {
        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("uern"))
            uern.Text = reportData["uern"];
        if (reportData.ContainsKey("SheetNo"))
            SheetNo.Text = reportData["SheetNo"];
        if (reportData.ContainsKey("WarningNoticeRefNo"))
            WarningNoticeRefNo.Text = reportData["WarningNoticeRefNo"];
        if (reportData.ContainsKey("nameOfPremises"))
            nameOfPremises.Text = reportData["nameOfPremises"];
        if (reportData.ContainsKey("adressOfPremises"))
            adressOfPremises.Text = reportData["adressOfPremises"];
        if (reportData.ContainsKey("appliancesCoveredByThisCheck"))
            appliancesCoveredByThisCheck.Text = reportData["appliancesCoveredByThisCheck"];
        if (reportData.ContainsKey("meterHouseLocation"))
            meterHouseLocation.Text = reportData["meterHouseLocation"];
        if (reportData.ContainsKey("meterHouseComment"))
            meterHouseComment.Text = reportData["meterHouseComment"];
        if (reportData.ContainsKey("ventilationLocation"))
            ventilationLocation.Text = reportData["ventilationLocation"];
        if (reportData.ContainsKey("freeAirExistingHighLevel"))
            freeAirExistingHighLevel.Text = reportData["freeAirExistingHighLevel"];
        if (reportData.ContainsKey("freeAirExistingLowLevel"))
            freeAirExistingLowLevel.Text = reportData["freeAirExistingLowLevel"];
        if (reportData.ContainsKey("freeAirRequiredHighLevel"))
            freeAirRequiredHighLevel.Text = reportData["freeAirRequiredHighLevel"];
        if (reportData.ContainsKey("freeAirRequiredLowLevel"))
            freeAirRequiredLowLevel.Text = reportData["freeAirRequiredLowLevel"];
        if (reportData.ContainsKey("boilerHousePlantRoomComments"))
            boilerHousePlantRoomComments.Text = reportData["boilerHousePlantRoomComments"];
        if (reportData.ContainsKey("inletWorkingPressureTestFullLoad"))
            inletWorkingPressureTestFullLoad.Text = reportData["inletWorkingPressureTestFullLoad"];
        if (reportData.ContainsKey("inletWorkingPressureTestPartLoad"))
            inletWorkingPressureTestPartLoad.Text = reportData["inletWorkingPressureTestPartLoad"];
        if (reportData.ContainsKey("standingPressure"))
            standingPressure.Text = reportData["standingPressure"];
        if (reportData.ContainsKey("plantGasInstallationVolume"))
            plantGasInstallationVolume.Text = reportData["plantGasInstallationVolume"];
        if (reportData.ContainsKey("engineersName"))
            engineersName.Text = reportData["engineersName"];
        if (reportData.ContainsKey("contractor"))
            contractor.Text = reportData["contractor"];
        if (reportData.ContainsKey("companyGasSafeRegistrationNo"))
            companyGasSafeRegistrationNo.Text = reportData["companyGasSafeRegistrationNo"];
        if (reportData.ContainsKey("engineersGasSafeIDNo"))
            engineersGasSafeIDNo.Text = reportData["engineersGasSafeIDNo"];
        if (reportData.ContainsKey("inspectionDate"))
            inspectionDate.Date = DateTime.ParseExact(reportData["inspectionDate"], "d/M/yyyy", null);
        if (reportData.ContainsKey("clientsName"))
            clientsName.Text = reportData["clientsName"];
        if (reportData.ContainsKey("date"))
            date.Date = DateTime.ParseExact(reportData["date"], "d/M/yyyy", null);

        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("checkRemedialToWorkRequiredYes"))
            checkRemedialToWorkRequiredYes.IsChecked = bool.Parse(reportData["checkRemedialToWorkRequiredYes"]);
        if (reportData.ContainsKey("checkTestsCompletedSatisfactoryYes"))
            checkTestsCompletedSatisfactoryYes.IsChecked = bool.Parse(reportData["checkTestsCompletedSatisfactoryYes"]);

        if (reportData.ContainsKey("checkPipeworkToGasMeterYes"))
            checkPipeworkToGasMeterYes.IsChecked = bool.Parse(reportData["checkPipeworkToGasMeterYes"]);
        if (reportData.ContainsKey("checkPipeworkToGasMeterNA"))
            checkPipeworkToGasMeterNA.IsChecked = bool.Parse(reportData["checkPipeworkToGasMeterNA"]);
        if (checkPipeworkToGasMeterYes.IsChecked == false && checkPipeworkToGasMeterNA.IsChecked == false)
            checkPipeworkToGasMeterNo.IsChecked = true;

        if (reportData.ContainsKey("checkRegulatorAndOrMeterYes"))
            checkRegulatorAndOrMeterYes.IsChecked = bool.Parse(reportData["checkRegulatorAndOrMeterYes"]);
        if (reportData.ContainsKey("checkRegulatorAndOrMeterNA"))
            checkRegulatorAndOrMeterNA.IsChecked = bool.Parse(reportData["checkRegulatorAndOrMeterNA"]);
        if (checkRegulatorAndOrMeterYes.IsChecked == false && checkRegulatorAndOrMeterNA.IsChecked == false)
            checkRegulatorAndOrMeterNo.IsChecked = true;

        if (reportData.ContainsKey("checkSafetyNoticesLabelsYes"))
            checkSafetyNoticesLabelsYes.IsChecked = bool.Parse(reportData["checkSafetyNoticesLabelsYes"]);
        if (reportData.ContainsKey("checkSafetyNoticesLabelsNA"))
            checkSafetyNoticesLabelsNA.IsChecked = bool.Parse(reportData["checkSafetyNoticesLabelsNA"]);
        if (checkSafetyNoticesLabelsYes.IsChecked == false && checkSafetyNoticesLabelsNA.IsChecked == false)
            checkSafetyNoticesLabelsNo.IsChecked = true;
        if (reportData.ContainsKey("checkLineDiagramYes"))
            checkLineDiagramYes.IsChecked = bool.Parse(reportData["checkLineDiagramYes"]);
        if (reportData.ContainsKey("checkLineDiagramNA"))
            checkLineDiagramNA.IsChecked = bool.Parse(reportData["checkLineDiagramNA"]);
        if (checkLineDiagramYes.IsChecked == false && checkLineDiagramNA.IsChecked == false)
            checkLineDiagramNo.IsChecked = true;


        if (reportData.ContainsKey("checkColorCodingIndicationTapeYes"))
            checkColorCodingIndicationTapeYes.IsChecked = bool.Parse(reportData["checkColorCodingIndicationTapeYes"]);
        if (reportData.ContainsKey("checkColorCodingIndicationTapeNA"))
            checkColorCodingIndicationTapeNA.IsChecked = bool.Parse(reportData["checkColorCodingIndicationTapeNA"]);
        if (checkColorCodingIndicationTapeYes.IsChecked == false && checkColorCodingIndicationTapeNA.IsChecked == false)
            checkColorCodingIndicationTapeNo.IsChecked = true;

        if (reportData.ContainsKey("checkMeterHouseVentilationYes"))
            checkMeterHouseVentilationYes.IsChecked = bool.Parse(reportData["checkMeterHouseVentilationYes"]);
        if (reportData.ContainsKey("checkMeterHouseVentilationNA"))
            checkMeterHouseVentilationNA.IsChecked = bool.Parse(reportData["checkMeterHouseVentilationNA"]);
        if (checkMeterHouseVentilationYes.IsChecked == false && checkMeterHouseVentilationNA.IsChecked == false)
            checkMeterHouseVentilationNo.IsChecked = true;

        if (reportData.ContainsKey("checkFreeAirExistingCM"))
            checkFreeAirExistingCM.IsChecked = bool.Parse(reportData["checkFreeAirExistingCM"]);
        if (reportData.ContainsKey("checkFreeAirExistingMH"))
            checkFreeAirExistingMH.IsChecked = bool.Parse(reportData["checkFreeAirExistingMH"]);
        if (reportData.ContainsKey("checkFreeAirRequiredCM"))
            checkFreeAirRequiredCM.IsChecked = bool.Parse(reportData["checkFreeAirRequiredCM"]);
        if (reportData.ContainsKey("checkFreeAirRequiredMH"))
            checkcheckFreeAirRequiredMH.IsChecked = bool.Parse(reportData["checkFreeAirRequiredMH"]);

        if (reportData.ContainsKey("checkMainFlueYes"))
            checkMainFlueYes.IsChecked = bool.Parse(reportData["checkMainFlueYes"]);
        if (reportData.ContainsKey("checkMainFlueNA"))
            checkMainFlueNA.IsChecked = bool.Parse(reportData["checkMainFlueNA"]);
        if (checkMainFlueYes.IsChecked == false && checkMainFlueNA.IsChecked == false)
            checkMainFlueNo.IsChecked = true;

        if (reportData.ContainsKey("checkChimneyFlueTerminalPositionYes"))
            checkChimneyFlueTerminalPositionYes.IsChecked = bool.Parse(reportData["checkChimneyFlueTerminalPositionYes"]);
        if (reportData.ContainsKey("checkChimneyFlueTerminalPositionNA"))
            checkChimneyFlueTerminalPositionNA.IsChecked = bool.Parse(reportData["checkChimneyFlueTerminalPositionNA"]);
        if (checkChimneyFlueTerminalPositionYes.IsChecked == false && checkChimneyFlueTerminalPositionNA.IsChecked == false)
            checkChimneyFlueTerminalPositionNo.IsChecked = true;

        if (reportData.ContainsKey("checkStubFluersToBoildersYes"))
            checkStubFluersToBoildersYes.IsChecked = bool.Parse(reportData["checkStubFluersToBoildersYes"]);
        if (reportData.ContainsKey("checkStubFluersToBoildersNA"))
            checkStubFluersToBoildersNA.IsChecked = bool.Parse(reportData["checkStubFluersToBoildersNA"]);
        if (checkStubFluersToBoildersYes.IsChecked == false && checkStubFluersToBoildersNA.IsChecked == false)
            checkStubFluersToBoildersNo.IsChecked = true;

        if (reportData.ContainsKey("checkIdFanYes"))
            checkIdFanYes.IsChecked = bool.Parse(reportData["checkIdFanYes"]);
        if (reportData.ContainsKey("checkIdFanNA"))
            checkIdFanNA.IsChecked = bool.Parse(reportData["checkIdFanNA"]);
        if (checkIdFanYes.IsChecked == false && checkIdFanNA.IsChecked == false)
            checkIdFanNo.IsChecked = true;

        if (reportData.ContainsKey("checkFanBoilerSafetyInterlockYes"))
            checkFanBoilerSafetyInterlockYes.IsChecked = bool.Parse(reportData["checkFanBoilerSafetyInterlockYes"]);
        if (reportData.ContainsKey("checkFanBoilerSafetyInterlockNA"))
            checkFanBoilerSafetyInterlockNA.IsChecked = bool.Parse(reportData["checkFanBoilerSafetyInterlockNA"]);
        if (checkFanBoilerSafetyInterlockYes.IsChecked == false && checkFanBoilerSafetyInterlockNA.IsChecked == false)
            checkFanBoilerSafetyInterlockNo.IsChecked = true;

        if (reportData.ContainsKey("checkGeneralComplianceOfGasPipeYes"))
            checkGeneralComplianceOfGasPipeYes.IsChecked = bool.Parse(reportData["checkGeneralComplianceOfGasPipeYes"]);
        if (reportData.ContainsKey("checkGeneralComplianceOfGasPipeNA"))
            checkGeneralComplianceOfGasPipeNA.IsChecked = bool.Parse(reportData["checkGeneralComplianceOfGasPipeNA"]);
        if (checkGeneralComplianceOfGasPipeYes.IsChecked == false && checkGeneralComplianceOfGasPipeNA.IsChecked == false)
            checkGeneralComplianceOfGasPipeNo.IsChecked = true;


        if (reportData.ContainsKey("checkVentilationYes"))
            checkVentilationYes.IsChecked = bool.Parse(reportData["checkVentilationYes"]);
        if (reportData.ContainsKey("checkVentilationNA"))
            checkVentilationNA.IsChecked = bool.Parse(reportData["checkVentilationNA"]);
        if (checkVentilationYes.IsChecked == false && checkVentilationNA.IsChecked == false)
            checkVentilationNo.IsChecked = true;

        if (reportData.ContainsKey("checkAIVYes"))
            checkAIVYes.IsChecked = bool.Parse(reportData["checkAIVYes"]);
        if (reportData.ContainsKey("checkAIVNo"))
            checkAIVNo.IsChecked = bool.Parse(reportData["checkAIVNo"]);
        if (checkAIVYes.IsChecked == false && checkAIVNo.IsChecked == false)
            checkAIVNA.IsChecked = true;

        if (reportData.ContainsKey("checkManualYes"))
            checkManualYes.IsChecked = bool.Parse(reportData["checkManualYes"]);
        if (reportData.ContainsKey("checkManualNo"))
            checkManualNo.IsChecked = bool.Parse(reportData["checkManualNo"]);
        if (checkManualYes.IsChecked == false && checkManualNo.IsChecked == false)
            checkManualNA.IsChecked = true;
        if (reportData.ContainsKey("checkPlantGasTightnessTestYes"))
            checkPlantGasTightnessTestYes.IsChecked = bool.Parse(reportData["checkPlantGasTightnessTestYes"]);
        if (reportData.ContainsKey("checkPlantGasTightnessTestNo"))
            checkPlantGasTightnessTestNo.IsChecked = bool.Parse(reportData["checkPlantGasTightnessTestNo"]);


    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new()
        {
            //  reportData.Add("", .Text ?? string.Empty);
            //  reportData.Add("", .IsChecked.ToString());
            { "uern", uern.Text ?? string.Empty },
            { "SheetNo", SheetNo.Text ?? string.Empty },
            { "WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty },
            { "nameOfPremises", nameOfPremises.Text ?? string.Empty },
            { "adressOfPremises", adressOfPremises.Text ?? string.Empty },
            { "appliancesCoveredByThisCheck", appliancesCoveredByThisCheck.Text ?? string.Empty },
            { "meterHouseLocation", meterHouseLocation.Text ?? string.Empty },
            { "meterHouseComment", meterHouseComment.Text ?? string.Empty },
            { "ventilationLocation", ventilationLocation.Text ?? string.Empty },
            { "freeAirExistingHighLevel", freeAirExistingHighLevel.Text ?? string.Empty },
            { "freeAirExistingLowLevel", freeAirExistingLowLevel.Text ?? string.Empty },
            { "freeAirRequiredHighLevel", freeAirRequiredHighLevel.Text ?? string.Empty },
            { "freeAirRequiredLowLevel", freeAirRequiredLowLevel.Text ?? string.Empty },
            { "boilerHousePlantRoomComments", boilerHousePlantRoomComments.Text ?? string.Empty },
            { "inletWorkingPressureTestFullLoad", inletWorkingPressureTestFullLoad.Text ?? string.Empty },
            { "inletWorkingPressureTestPartLoad", inletWorkingPressureTestPartLoad.Text ?? string.Empty },
            { "standingPressure", standingPressure.Text ?? string.Empty },
            { "plantGasInstallationVolume", plantGasInstallationVolume.Text ?? string.Empty },
            { "engineersName", engineersName.Text ?? string.Empty },
            { "contractor", contractor.Text ?? string.Empty },
            { "companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty },
            { "engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty },
            { "inspectionDate", inspectionDate.Date.ToString("d/M/yyyy") ?? string.Empty },
            { "clientsName", clientsName.Text ?? string.Empty },
            { "date", date.Date.ToString("d/M/yyyy") ?? string.Empty },

            { "checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString() },
            { "checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString() },
            { "checkPipeworkToGasMeterYes", checkPipeworkToGasMeterYes.IsChecked.ToString() },
            { "checkPipeworkToGasMeterNA", checkPipeworkToGasMeterNA.IsChecked.ToString() },
            { "checkRegulatorAndOrMeterYes", checkRegulatorAndOrMeterYes.IsChecked.ToString() },
            { "checkRegulatorAndOrMeterNA", checkRegulatorAndOrMeterNA.IsChecked.ToString() },
            { "checkSafetyNoticesLabelsYes", checkSafetyNoticesLabelsYes.IsChecked.ToString() },
            { "checkSafetyNoticesLabelsNA", checkSafetyNoticesLabelsNA.IsChecked.ToString() },
            { "checkLineDiagramYes", checkLineDiagramYes.IsChecked.ToString() },
            { "checkLineDiagramNA", checkLineDiagramNA.IsChecked.ToString() },
            { "checkColorCodingIndicationTapeYes", checkColorCodingIndicationTapeYes.IsChecked.ToString() },
            { "checkColorCodingIndicationTapeNA", checkColorCodingIndicationTapeNA.IsChecked.ToString() },
            { "checkMeterHouseVentilationYes", checkMeterHouseVentilationYes.IsChecked.ToString() },
            { "checkMeterHouseVentilationNA", checkMeterHouseVentilationNA.IsChecked.ToString() },
            { "checkFreeAirExistingCM", checkFreeAirExistingCM.IsChecked.ToString() },
            { "checkFreeAirExistingMH", checkFreeAirExistingMH.IsChecked.ToString() },
            { "checkFreeAirRequiredCM", checkFreeAirRequiredCM.IsChecked.ToString() },
            { "checkFreeAirRequiredMH", checkcheckFreeAirRequiredMH.IsChecked.ToString() },
            { "checkMainFlueYes", checkMainFlueYes.IsChecked.ToString() },
            { "checkMainFlueNA", checkMainFlueNA.IsChecked.ToString() },
            { "checkChimneyFlueTerminalPositionYes", checkChimneyFlueTerminalPositionYes.IsChecked.ToString() },
            { "checkChimneyFlueTerminalPositionNA", checkChimneyFlueTerminalPositionNA.IsChecked.ToString() },
            { "checkStubFluersToBoildersYes", checkStubFluersToBoildersYes.IsChecked.ToString() },
            { "checkStubFluersToBoildersNA", checkStubFluersToBoildersNA.IsChecked.ToString() },
            { "checkIdFanYes", checkIdFanYes.IsChecked.ToString() },
            { "checkIdFanNA", checkIdFanNA.IsChecked.ToString() },
            { "checkFanBoilerSafetyInterlockYes", checkFanBoilerSafetyInterlockYes.IsChecked.ToString() },
            { "checkFanBoilerSafetyInterlockNA", checkFanBoilerSafetyInterlockNA.IsChecked.ToString() },
            { "checkGeneralComplianceOfGasPipeYes", checkGeneralComplianceOfGasPipeYes.IsChecked.ToString() },
            { "checkGeneralComplianceOfGasPipeNA", checkGeneralComplianceOfGasPipeNA.IsChecked.ToString() },
            { "checkVentilationYes", checkVentilationYes.IsChecked.ToString() },
            { "checkVentilationNA", checkVentilationNA.IsChecked.ToString() },
            { "checkAIVYes", checkAIVYes.IsChecked.ToString() },
            { "checkAIVNo", checkAIVNo.IsChecked.ToString() },
            { "checkManualYes", checkManualYes.IsChecked.ToString() },
            { "checkManualNo", checkManualNo.IsChecked.ToString() },
            { "checkPlantGasTightnessTestYes", checkPlantGasTightnessTestYes.IsChecked.ToString() },
             { "checkPlantGasTightnessTestNo", checkPlantGasTightnessTestNo.IsChecked.ToString() }
        };


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


    public void CheckPipeworkToGasMeterYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkToGasMeterYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkToGasMeterYes, checkPipeworkToGasMeterNo, checkPipeworkToGasMeterNA);
        else
        {
            checkPipeworkToGasMeterYes.Color = Colors.White;
            if (!checkPipeworkToGasMeterNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkToGasMeterNA, checkPipeworkToGasMeterYes, checkPipeworkToGasMeterNo);
        }
    }
    public void CheckPipeworkToGasMeterNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkToGasMeterNo.IsChecked)
        {
            DisjunctCheckboxes(checkPipeworkToGasMeterNo, checkPipeworkToGasMeterYes, checkPipeworkToGasMeterNA);
        }
        else
        {
            checkPipeworkToGasMeterNo.Color = Colors.White;
            if (!checkPipeworkToGasMeterYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkToGasMeterNA, checkPipeworkToGasMeterYes, checkPipeworkToGasMeterNo);
        }
    }
    public void CheckPipeworkToGasMeterNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkToGasMeterNA.IsChecked || !checkPipeworkToGasMeterYes.IsChecked && !checkPipeworkToGasMeterNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkToGasMeterNA, checkPipeworkToGasMeterYes, checkPipeworkToGasMeterNo);
        else
            checkPipeworkToGasMeterNA.Color = Colors.White;
    }




    public void CheckRegulatorAndOrMeterYesChanged(object sender, EventArgs e)
    {
        if (checkRegulatorAndOrMeterYes.IsChecked)
            DisjunctCheckboxes(checkRegulatorAndOrMeterYes, checkRegulatorAndOrMeterNo, checkRegulatorAndOrMeterNA);
        else
        {
            checkRegulatorAndOrMeterYes.Color = Colors.White;
            if (!checkRegulatorAndOrMeterNo.IsChecked)
                DisjunctCheckboxes(checkRegulatorAndOrMeterNA, checkRegulatorAndOrMeterYes, checkRegulatorAndOrMeterNo);
        }
    }
    public void CheckRegulatorAndOrMeterNoChanged(object sender, EventArgs e)
    {
        if (checkRegulatorAndOrMeterNo.IsChecked)
            DisjunctCheckboxes(checkRegulatorAndOrMeterNo, checkRegulatorAndOrMeterYes, checkRegulatorAndOrMeterNA);
        else
        {
            checkRegulatorAndOrMeterNo.Color = Colors.White;
            if (!checkRegulatorAndOrMeterYes.IsChecked)
                DisjunctCheckboxes(checkRegulatorAndOrMeterNA, checkRegulatorAndOrMeterYes, checkRegulatorAndOrMeterNo);
        }
    }
    public void CheckRegulatorAndOrMeterNAChanged(object sender, EventArgs e)
    {
        if (checkRegulatorAndOrMeterNA.IsChecked || !checkRegulatorAndOrMeterYes.IsChecked && !checkRegulatorAndOrMeterNo.IsChecked)
            DisjunctCheckboxes(checkRegulatorAndOrMeterNA, checkRegulatorAndOrMeterYes, checkRegulatorAndOrMeterNo);
        else
            checkRegulatorAndOrMeterNA.Color = Colors.White;
    }




    public void CheckSafetyNoticesLabelsYesChanged(object sender, EventArgs e)
    {
        if (checkSafetyNoticesLabelsYes.IsChecked)
            DisjunctCheckboxes(checkSafetyNoticesLabelsYes, checkSafetyNoticesLabelsNo, checkSafetyNoticesLabelsNA);
        else
        {
            checkSafetyNoticesLabelsYes.Color = Colors.White;
            if (!checkSafetyNoticesLabelsNo.IsChecked)
                DisjunctCheckboxes(checkSafetyNoticesLabelsNA, checkSafetyNoticesLabelsYes, checkSafetyNoticesLabelsNo);
        }
    }
    public void CheckSafetyNoticesLabelsNoChanged(object sender, EventArgs e)
    {
        if (checkSafetyNoticesLabelsNo.IsChecked)
            DisjunctCheckboxes(checkSafetyNoticesLabelsNo, checkSafetyNoticesLabelsYes, checkSafetyNoticesLabelsNA);
        else
        {
            checkSafetyNoticesLabelsNo.Color = Colors.White;
            if (!checkSafetyNoticesLabelsYes.IsChecked)
                DisjunctCheckboxes(checkSafetyNoticesLabelsNA, checkSafetyNoticesLabelsYes, checkSafetyNoticesLabelsNo);
        }
    }
    public void CheckSafetyNoticesLabelsNAChanged(object sender, EventArgs e)
    {
        if (checkSafetyNoticesLabelsNA.IsChecked || !checkSafetyNoticesLabelsYes.IsChecked && !checkSafetyNoticesLabelsNo.IsChecked)
            DisjunctCheckboxes(checkSafetyNoticesLabelsNA, checkSafetyNoticesLabelsYes, checkSafetyNoticesLabelsNo);
        else
            checkSafetyNoticesLabelsNA.Color = Colors.White;
    }




    public void CheckLineDiagramYesChanged(object sender, EventArgs e)
    {
        if (checkLineDiagramYes.IsChecked)
            DisjunctCheckboxes(checkLineDiagramYes, checkLineDiagramNo, checkLineDiagramNA);
        else
        {
            checkLineDiagramYes.Color = Colors.White;
            if (!checkLineDiagramNo.IsChecked)
                DisjunctCheckboxes(checkLineDiagramNA, checkLineDiagramYes, checkLineDiagramNo);
        }
    }
    public void CheckLineDiagramNoChanged(object sender, EventArgs e)
    {
        if (checkLineDiagramNo.IsChecked)
            DisjunctCheckboxes(checkLineDiagramNo, checkLineDiagramYes, checkLineDiagramNA);
        else
        {
            checkLineDiagramNo.Color = Colors.White;
            if (!checkLineDiagramYes.IsChecked)
                DisjunctCheckboxes(checkLineDiagramNA, checkLineDiagramYes, checkLineDiagramNo);
        }
    }
    public void CheckLineDiagramNAChanged(object sender, EventArgs e)
    {
        if (checkLineDiagramNA.IsChecked || !checkLineDiagramYes.IsChecked && !checkLineDiagramNo.IsChecked)
            DisjunctCheckboxes(checkLineDiagramNA, checkLineDiagramYes, checkLineDiagramNo);
        else
            checkLineDiagramNA.Color = Colors.White;
    }




    public void CheckColorCodingIndicationTapeYesChanged(object sender, EventArgs e)
    {
        if (checkColorCodingIndicationTapeYes.IsChecked)
            DisjunctCheckboxes(checkColorCodingIndicationTapeYes, checkColorCodingIndicationTapeNo, checkColorCodingIndicationTapeNA);
        else
        {
            checkColorCodingIndicationTapeYes.Color = Colors.White;
            if (!checkColorCodingIndicationTapeNo.IsChecked)
                DisjunctCheckboxes(checkColorCodingIndicationTapeNA, checkColorCodingIndicationTapeYes, checkColorCodingIndicationTapeNo);
        }
    }
    public void CheckColorCodingIndicationTapeNoChanged(object sender, EventArgs e)
    {
        if (checkColorCodingIndicationTapeNo.IsChecked)
            DisjunctCheckboxes(checkColorCodingIndicationTapeNo, checkColorCodingIndicationTapeYes, checkColorCodingIndicationTapeNA);
        else
        {
            checkColorCodingIndicationTapeNo.Color = Colors.White;
            if (!checkColorCodingIndicationTapeYes.IsChecked)
                DisjunctCheckboxes(checkColorCodingIndicationTapeNA, checkColorCodingIndicationTapeYes, checkColorCodingIndicationTapeNo);
        }
    }
    public void CheckColorCodingIndicationTapeNAChanged(object sender, EventArgs e)
    {
        if (checkColorCodingIndicationTapeNA.IsChecked || !checkColorCodingIndicationTapeYes.IsChecked && !checkColorCodingIndicationTapeNo.IsChecked)
            DisjunctCheckboxes(checkColorCodingIndicationTapeNA, checkColorCodingIndicationTapeYes, checkColorCodingIndicationTapeNo);
        else
            checkColorCodingIndicationTapeNA.Color = Colors.White;
    }




    public void CheckMeterHouseVentilationYesChanged(object sender, EventArgs e)
    {
        if (checkMeterHouseVentilationYes.IsChecked)
            DisjunctCheckboxes(checkMeterHouseVentilationYes, checkMeterHouseVentilationNo, checkMeterHouseVentilationNA);
        else
        {
            checkMeterHouseVentilationYes.Color = Colors.White;
            if (!checkMeterHouseVentilationNo.IsChecked)
                DisjunctCheckboxes(checkMeterHouseVentilationNA, checkMeterHouseVentilationYes, checkMeterHouseVentilationNo);
        }
    }
    public void CheckMeterHouseVentilationNoChanged(object sender, EventArgs e)
    {
        if (checkMeterHouseVentilationNo.IsChecked)
            DisjunctCheckboxes(checkMeterHouseVentilationNo, checkMeterHouseVentilationYes, checkMeterHouseVentilationNA);
        else
        {
            checkMeterHouseVentilationNo.Color = Colors.White;
            if (!checkMeterHouseVentilationYes.IsChecked)
                DisjunctCheckboxes(checkMeterHouseVentilationNA, checkMeterHouseVentilationYes, checkMeterHouseVentilationNo);
        }
    }
    public void CheckMeterHouseVentilationNAChanged(object sender, EventArgs e)
    {
        if (checkMeterHouseVentilationNA.IsChecked || !checkMeterHouseVentilationYes.IsChecked && !checkMeterHouseVentilationNo.IsChecked)
            DisjunctCheckboxes(checkMeterHouseVentilationNA, checkMeterHouseVentilationYes, checkMeterHouseVentilationNo);
        else
            checkMeterHouseVentilationNA.Color = Colors.White;
    }




    public void CheckMainFlueYesChanged(object sender, EventArgs e)
    {
        if (checkMainFlueYes.IsChecked)
            DisjunctCheckboxes(checkMainFlueYes, checkMainFlueNo, checkMainFlueNA);
        else
        {
            checkMainFlueYes.Color = Colors.White;
            if (!checkMainFlueNo.IsChecked)
                DisjunctCheckboxes(checkMainFlueNA, checkMainFlueYes, checkMainFlueNo);
        }
    }
    public void CheckMainFlueNoChanged(object sender, EventArgs e)
    {
        if (checkMainFlueNo.IsChecked)
            DisjunctCheckboxes(checkMainFlueNo, checkMainFlueYes, checkMainFlueNA);
        else
        {
            checkMainFlueNo.Color = Colors.White;
            if (!checkMainFlueYes.IsChecked)
                DisjunctCheckboxes(checkMainFlueNA, checkMainFlueYes, checkMainFlueNo);
        }
    }
    public void CheckMainFlueNAChanged(object sender, EventArgs e)
    {
        if (checkMainFlueNA.IsChecked || !checkMainFlueYes.IsChecked && !checkMainFlueNo.IsChecked)
            DisjunctCheckboxes(checkMainFlueNA, checkMainFlueYes, checkMainFlueNo);
        else
            checkMainFlueNA.Color = Colors.White;
    }




    public void CheckChimneyFlueTerminalPositionYesChanged(object sender, EventArgs e)
    {
        if (checkChimneyFlueTerminalPositionYes.IsChecked)
            DisjunctCheckboxes(checkChimneyFlueTerminalPositionYes, checkChimneyFlueTerminalPositionNo, checkChimneyFlueTerminalPositionNA);
        else
        {
            checkChimneyFlueTerminalPositionYes.Color = Colors.White;
            if (!checkChimneyFlueTerminalPositionNo.IsChecked)
                DisjunctCheckboxes(checkChimneyFlueTerminalPositionNA, checkChimneyFlueTerminalPositionYes, checkChimneyFlueTerminalPositionNo);
        }
    }
    public void CheckChimneyFlueTerminalPositionNoChanged(object sender, EventArgs e)
    {
        if (checkChimneyFlueTerminalPositionNo.IsChecked)
            DisjunctCheckboxes(checkChimneyFlueTerminalPositionNo, checkChimneyFlueTerminalPositionYes, checkChimneyFlueTerminalPositionNA);
        else
        {
            checkChimneyFlueTerminalPositionNo.Color = Colors.White;
            if (!checkChimneyFlueTerminalPositionYes.IsChecked)
                DisjunctCheckboxes(checkChimneyFlueTerminalPositionNA, checkChimneyFlueTerminalPositionYes, checkChimneyFlueTerminalPositionNo);
        }
    }
    public void CheckChimneyFlueTerminalPositionNAChanged(object sender, EventArgs e)
    {
        if (checkChimneyFlueTerminalPositionNA.IsChecked || !checkChimneyFlueTerminalPositionYes.IsChecked && !checkChimneyFlueTerminalPositionNo.IsChecked)
            DisjunctCheckboxes(checkChimneyFlueTerminalPositionNA, checkChimneyFlueTerminalPositionYes, checkChimneyFlueTerminalPositionNo);
        else
            checkChimneyFlueTerminalPositionNA.Color = Colors.White;
    }





    public void CheckStubFluersToBoildersYesChanged(object sender, EventArgs e)
    {
        if (checkStubFluersToBoildersYes.IsChecked)
            DisjunctCheckboxes(checkStubFluersToBoildersYes, checkStubFluersToBoildersNo, checkStubFluersToBoildersNA);
        else
        {
            checkStubFluersToBoildersYes.Color = Colors.White;
            if (!checkStubFluersToBoildersNo.IsChecked)
                DisjunctCheckboxes(checkStubFluersToBoildersNA, checkStubFluersToBoildersYes, checkStubFluersToBoildersNo);
        }
    }
    public void CheckStubFluersToBoildersNoChanged(object sender, EventArgs e)
    {
        if (checkStubFluersToBoildersNo.IsChecked)
            DisjunctCheckboxes(checkStubFluersToBoildersNo, checkStubFluersToBoildersYes, checkStubFluersToBoildersNA);
        else
        {
            checkStubFluersToBoildersNo.Color = Colors.White;
            if (!checkStubFluersToBoildersYes.IsChecked)
                DisjunctCheckboxes(checkStubFluersToBoildersNA, checkStubFluersToBoildersYes, checkStubFluersToBoildersNo);
        }
    }
    public void CheckStubFluersToBoildersNAChanged(object sender, EventArgs e)
    {
        if (checkStubFluersToBoildersNA.IsChecked || !checkStubFluersToBoildersYes.IsChecked && !checkStubFluersToBoildersNo.IsChecked)
            DisjunctCheckboxes(checkStubFluersToBoildersNA, checkStubFluersToBoildersYes, checkStubFluersToBoildersNo);
        else
            checkStubFluersToBoildersNA.Color = Colors.White;
    }




    public void CheckIdFanYesChanged(object sender, EventArgs e)
    {
        if (checkIdFanYes.IsChecked)
            DisjunctCheckboxes(checkIdFanYes, checkIdFanNo, checkIdFanNA);
        else
        {
            checkIdFanYes.Color = Colors.White;
            if (!checkIdFanNo.IsChecked)
                DisjunctCheckboxes(checkIdFanNA, checkIdFanYes, checkIdFanNo);
        }
    }
    public void CheckIdFanNoChanged(object sender, EventArgs e)
    {
        if (checkIdFanNo.IsChecked)
            DisjunctCheckboxes(checkIdFanNo, checkIdFanYes, checkIdFanNA);
        else
        {
            checkIdFanNo.Color = Colors.White;
            if (!checkIdFanYes.IsChecked)
                DisjunctCheckboxes(checkIdFanNA, checkIdFanYes, checkIdFanNo);
        }
    }
    public void CheckIdFanNAChanged(object sender, EventArgs e)
    {
        if (checkIdFanNA.IsChecked || !checkIdFanYes.IsChecked && !checkIdFanNo.IsChecked)
            DisjunctCheckboxes(checkIdFanNA, checkIdFanYes, checkIdFanNo);
        else
            checkIdFanNA.Color = Colors.White;
    }




    public void CheckFanBoilerSafetyInterlockYesChanged(object sender, EventArgs e)
    {
        if (checkFanBoilerSafetyInterlockYes.IsChecked)
            DisjunctCheckboxes(checkFanBoilerSafetyInterlockYes, checkFanBoilerSafetyInterlockNo, checkFanBoilerSafetyInterlockNA);
        else
        {
            checkFanBoilerSafetyInterlockYes.Color = Colors.White;
            if (!checkFanBoilerSafetyInterlockNo.IsChecked)
                DisjunctCheckboxes(checkFanBoilerSafetyInterlockNA, checkFanBoilerSafetyInterlockYes, checkFanBoilerSafetyInterlockNo);
        }
    }
    public void CheckFanBoilerSafetyInterlockNoChanged(object sender, EventArgs e)
    {
        if (checkFanBoilerSafetyInterlockNo.IsChecked)
            DisjunctCheckboxes(checkFanBoilerSafetyInterlockNo, checkFanBoilerSafetyInterlockYes, checkFanBoilerSafetyInterlockNA);
        else
        {
            checkFanBoilerSafetyInterlockNo.Color = Colors.White;
            if (!checkFanBoilerSafetyInterlockYes.IsChecked)
                DisjunctCheckboxes(checkFanBoilerSafetyInterlockNA, checkFanBoilerSafetyInterlockYes, checkFanBoilerSafetyInterlockNo);
        }
    }
    public void CheckFanBoilerSafetyInterlockNAChanged(object sender, EventArgs e)
    {
        if (checkFanBoilerSafetyInterlockNA.IsChecked || !checkFanBoilerSafetyInterlockYes.IsChecked && !checkFanBoilerSafetyInterlockNo.IsChecked)
            DisjunctCheckboxes(checkFanBoilerSafetyInterlockNA, checkFanBoilerSafetyInterlockYes, checkFanBoilerSafetyInterlockNo);
        else
            checkFanBoilerSafetyInterlockNA.Color = Colors.White;
    }




    public void CheckGeneralComplianceOfGasPipeYesChanged(object sender, EventArgs e)
    {
        if (checkGeneralComplianceOfGasPipeYes.IsChecked)
            DisjunctCheckboxes(checkGeneralComplianceOfGasPipeYes, checkGeneralComplianceOfGasPipeNo, checkGeneralComplianceOfGasPipeNA);
        else
        {
            checkGeneralComplianceOfGasPipeYes.Color = Colors.White;
            if (!checkGeneralComplianceOfGasPipeNo.IsChecked)
                DisjunctCheckboxes(checkGeneralComplianceOfGasPipeNA, checkGeneralComplianceOfGasPipeYes, checkGeneralComplianceOfGasPipeNo);
        }
    }
    public void CheckGeneralComplianceOfGasPipeNoChanged(object sender, EventArgs e)
    {
        if (checkGeneralComplianceOfGasPipeNo.IsChecked)
            DisjunctCheckboxes(checkGeneralComplianceOfGasPipeNo, checkGeneralComplianceOfGasPipeYes, checkGeneralComplianceOfGasPipeNA);
        else
        {
            checkGeneralComplianceOfGasPipeNo.Color = Colors.White;
            if (!checkGeneralComplianceOfGasPipeYes.IsChecked)
                DisjunctCheckboxes(checkGeneralComplianceOfGasPipeNA, checkGeneralComplianceOfGasPipeYes, checkGeneralComplianceOfGasPipeNo);
        }
    }
    public void CheckGeneralComplianceOfGasPipeNAChanged(object sender, EventArgs e)
    {
        if (checkGeneralComplianceOfGasPipeNA.IsChecked || !checkGeneralComplianceOfGasPipeYes.IsChecked && !checkGeneralComplianceOfGasPipeNo.IsChecked)
            DisjunctCheckboxes(checkGeneralComplianceOfGasPipeNA, checkGeneralComplianceOfGasPipeYes, checkGeneralComplianceOfGasPipeNo);
        else
            checkGeneralComplianceOfGasPipeNA.Color = Colors.White;
    }




    public void CheckVentilationYesChanged(object sender, EventArgs e)
    {
        if (checkVentilationYes.IsChecked)
            DisjunctCheckboxes(checkVentilationYes, checkVentilationNo, checkVentilationNA);
        else
        {
            checkVentilationYes.Color = Colors.White;
            if (!checkVentilationNo.IsChecked)
                DisjunctCheckboxes(checkVentilationNA, checkVentilationYes, checkVentilationNo);
        }
    }
    public void CheckVentilationNoChanged(object sender, EventArgs e)
    {
        if (checkVentilationNo.IsChecked)
            DisjunctCheckboxes(checkVentilationNo, checkVentilationYes, checkVentilationNA);
        else
        {
            checkVentilationNo.Color = Colors.White;
            if (!checkVentilationYes.IsChecked)
                DisjunctCheckboxes(checkVentilationNA, checkVentilationYes, checkVentilationNo);
        }
    }
    public void CheckVentilationNAChanged(object sender, EventArgs e)
    {
        if (checkVentilationNA.IsChecked || !checkVentilationYes.IsChecked && !checkVentilationNo.IsChecked)
            DisjunctCheckboxes(checkVentilationNA, checkVentilationYes, checkVentilationNo);
        else
            checkVentilationNA.Color = Colors.White;
    }




    public void CheckAIVYesChanged(object sender, EventArgs e)
    {
        if (checkAIVYes.IsChecked)
            DisjunctCheckboxes(checkAIVYes, checkAIVNo, checkAIVNA);
        else
        {
            checkAIVYes.Color = Colors.White;
            if (!checkAIVNo.IsChecked)
                DisjunctCheckboxes(checkAIVNA, checkAIVYes, checkAIVNo);
        }
    }
    public void CheckAIVNoChanged(object sender, EventArgs e)
    {
        if (checkAIVNo.IsChecked)
            DisjunctCheckboxes(checkAIVNo, checkAIVYes, checkAIVNA);
        else
        {
            checkAIVNo.Color = Colors.White;
            if (!checkAIVYes.IsChecked)
                DisjunctCheckboxes(checkAIVNA, checkAIVYes, checkAIVNo);
        }
    }
    public void CheckAIVNAChanged(object sender, EventArgs e)
    {
        if (checkAIVNA.IsChecked || !checkAIVYes.IsChecked && !checkAIVNo.IsChecked)
            DisjunctCheckboxes(checkAIVNA, checkAIVYes, checkAIVNo);
        else
            checkAIVNA.Color = Colors.White;
    }




    public void CheckManualYesChanged(object sender, EventArgs e)
    {
        if (checkManualYes.IsChecked)
            DisjunctCheckboxes(checkManualYes, checkManualNo, checkManualNA);
        else
        {
            checkManualYes.Color = Colors.White;
            if (!checkManualNo.IsChecked)
                DisjunctCheckboxes(checkManualNA, checkManualYes, checkManualNo);
        }
    }
    public void CheckManualNoChanged(object sender, EventArgs e)
    {
        if (checkManualNo.IsChecked)
            DisjunctCheckboxes(checkManualNo, checkManualYes, checkManualNA);
        else
        {
            checkManualNo.Color = Colors.White;
            if (!checkManualYes.IsChecked)
                DisjunctCheckboxes(checkManualNA, checkManualYes, checkManualNo);
        }
    }
    public void CheckManualNAChanged(object sender, EventArgs e)
    {
        if (checkManualNA.IsChecked || !checkManualYes.IsChecked && !checkManualNo.IsChecked)
            DisjunctCheckboxes(checkManualNA, checkManualYes, checkManualNo);
        else
            checkManualNA.Color = Colors.White;
    }
}