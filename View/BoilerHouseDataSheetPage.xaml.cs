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
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;

        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }
    private async Task UploadReport(Folder folder, Dictionary<string, string> report)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        BoilderHouseDataSheetBackBtt.IsEnabled = false;
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

        if (!string.IsNullOrEmpty(folder.Signature1) && !string.IsNullOrEmpty(folder.Signature2))
        {
            try
            {
                byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");
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
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
        await Navigation.PopModalAsync();
    }

    public async void NewFolder(object sender, EventArgs e)
    {
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (folderName == null) // User clicked Cancel
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