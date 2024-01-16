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

                byte[] pdfData = await PdfCreation.BoilerHouseDataSheet(reportData, signature1, signature2);

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
        //PdfCreation.BoilerHouseDataSheet(GatherReportData());
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
            { "inspectionDate", inspectionDate.Text ?? string.Empty },
            { "clientsName", clientsName.Text ?? string.Empty },
            { "date", date.Text ?? string.Empty },

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
            { "checkcheckFreeAirRequiredMH", checkcheckFreeAirRequiredMH.IsChecked.ToString() },
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
            { "checkPlantGasTightnessTestYes", checkPlantGasTightnessTestYes.IsChecked.ToString() }
        };


        return reportData;
    }
}