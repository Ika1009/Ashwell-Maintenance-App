using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class BoilerHouseDataSheetPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public BoilerHouseDataSheetPage()
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
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.BoilerHouseDataSheet, reportName, folder.Id, report);

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
        await LoadFolders();
    }
	public async void BHDSNext4(object sender, EventArgs e)
	{
        BHDSSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Boiler_House_Data_Sheet_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.Boiler(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("uern", uern.Text ?? string.Empty);
        reportData.Add("SheetNo", SheetNo.Text ?? string.Empty);
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("nameOfPremises", nameOfPremises.Text ?? string.Empty);
        reportData.Add("adressOfPremises", adressOfPremises.Text ?? string.Empty);
        reportData.Add("appliancesCoveredByThisCheck", appliancesCoveredByThisCheck.Text ?? string.Empty);
        reportData.Add("meterHouseLocation", meterHouseLocation.Text ?? string.Empty);
        reportData.Add("meterHouseComment", meterHouseComment.Text ?? string.Empty);
        reportData.Add("ventilationLocation", ventilationLocation.Text ?? string.Empty);
        reportData.Add("freeAirExistingHighLevel", freeAirExistingHighLevel.Text ?? string.Empty);
        reportData.Add("freeAirExistingLowLevel", freeAirExistingLowLevel.Text ?? string.Empty);
        reportData.Add("freeAirRequiredHighLevel", freeAirRequiredHighLevel.Text ?? string.Empty);
        reportData.Add("freeAirRequiredLowLevel", freeAirRequiredLowLevel.Text ?? string.Empty);
        reportData.Add("boilerHousePlantRoomComments", boilerHousePlantRoomComments.Text ?? string.Empty);
        reportData.Add("inletWorkingPressureTestFullLoad", inletWorkingPressureTestFullLoad.Text ?? string.Empty);
        reportData.Add("inletWorkingPressureTestPartLoad", inletWorkingPressureTestPartLoad.Text ?? string.Empty);
        reportData.Add("standingPressure", standingPressure.Text ?? string.Empty);
        reportData.Add("plantGasInstallationVolume", plantGasInstallationVolume.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("contractor", contractor.Text ?? string.Empty);
        reportData.Add("companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty);
        reportData.Add("engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);


        reportData.Add("checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString());
        reportData.Add("checkPipeworkToGasMeterYes", checkPipeworkToGasMeterYes.IsChecked.ToString());
        reportData.Add("checkPipeworkToGasMeterNA", checkPipeworkToGasMeterNA.IsChecked.ToString());
        reportData.Add("checkRegulatorAndOrMeterYes", checkRegulatorAndOrMeterYes.IsChecked.ToString());
        reportData.Add("checkRegulatorAndOrMeterNA", checkRegulatorAndOrMeterNA.IsChecked.ToString());
        reportData.Add("checkSafetyNoticesLabelsYes", checkSafetyNoticesLabelsYes.IsChecked.ToString());
        reportData.Add("checkSafetyNoticesLabelsNA", checkSafetyNoticesLabelsNA.IsChecked.ToString());
        reportData.Add("checkLineDiagramYes", checkLineDiagramYes.IsChecked.ToString());
        reportData.Add("checkLineDiagramNA", checkLineDiagramNA.IsChecked.ToString());
        reportData.Add("checkColorCodingIndicationTapeYes", checkColorCodingIndicationTapeYes.IsChecked.ToString());
        reportData.Add("checkColorCodingIndicationTapeNA", checkColorCodingIndicationTapeNA.IsChecked.ToString());
        reportData.Add("checkMeterHouseVentilationYes", checkMeterHouseVentilationYes.IsChecked.ToString());
        reportData.Add("checkMeterHouseVentilationNA", checkMeterHouseVentilationNA.IsChecked.ToString());
        reportData.Add("checkFreeAirExistingCM", checkFreeAirExistingCM.IsChecked.ToString());
        reportData.Add("checkFreeAirExistingMH", checkFreeAirExistingMH.IsChecked.ToString());
        reportData.Add("checkFreeAirRequiredCM", checkFreeAirRequiredCM.IsChecked.ToString());
        reportData.Add("checkcheckFreeAirRequiredMH", checkcheckFreeAirRequiredMH.IsChecked.ToString());
        reportData.Add("checkMainFlueYes", checkMainFlueYes.IsChecked.ToString());
        reportData.Add("checkMainFlueNA", checkMainFlueNA.IsChecked.ToString());
        reportData.Add("checkChimneyFlueTerminalPositionYes", checkChimneyFlueTerminalPositionYes.IsChecked.ToString());
        reportData.Add("checkChimneyFlueTerminalPositionNA", checkChimneyFlueTerminalPositionNA.IsChecked.ToString());
        reportData.Add("checkStubFluersToBoildersYes", checkStubFluersToBoildersYes.IsChecked.ToString());
        reportData.Add("checkStubFluersToBoildersNA", checkStubFluersToBoildersNA.IsChecked.ToString());
        reportData.Add("checkIdFanYes", checkIdFanYes.IsChecked.ToString());
        reportData.Add("checkIdFanNA", checkIdFanNA.IsChecked.ToString());
        reportData.Add("checkFanBoilerSafetyInterlockYes", checkFanBoilerSafetyInterlockYes.IsChecked.ToString());
        reportData.Add("checkFanBoilerSafetyInterlockNA", checkFanBoilerSafetyInterlockNA.IsChecked.ToString());
        reportData.Add("checkGeneralComplianceOfGasPipeYes", checkGeneralComplianceOfGasPipeYes.IsChecked.ToString());
        reportData.Add("checkGeneralComplianceOfGasPipeNA", checkGeneralComplianceOfGasPipeNA.IsChecked.ToString());
        reportData.Add("checkVentilationYes", checkVentilationYes.IsChecked.ToString());
        reportData.Add("checkVentilationNA", checkVentilationNA.IsChecked.ToString());
        reportData.Add("checkAIVYes", checkAIVYes.IsChecked.ToString());
        reportData.Add("checkAIVNo", checkAIVNo.IsChecked.ToString());
        reportData.Add("checkManualYes", checkManualYes.IsChecked.ToString());
        reportData.Add("checkManualNo", checkManualNo.IsChecked.ToString());
        reportData.Add("checkPlantGasTightnessTestYes", checkPlantGasTightnessTestYes.IsChecked.ToString());
     

        return reportData;
    }
}