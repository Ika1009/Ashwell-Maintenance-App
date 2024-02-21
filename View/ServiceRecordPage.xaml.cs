using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public ServiceRecordPage()
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
        ServiceRecordBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ServiceRecord, reportName, folder.Id, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully created new sheet.", "OK");
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

                byte[] pdfData = await PdfCreation.ServiceRecord(reportData, signature1, signature2);

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
        this.ShowPopup(new NewFolderPopup());        
        await LoadFolders();
    }
    private async Task LoadFolders()
    {
        try
        {
            HttpResponseMessage response = await ApiService.GetAllFoldersAsync();
            if (!response.IsSuccessStatusCode) {
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
    public void ServiceRecordBack(object sender, EventArgs e)
    {
        if (SRSection1.IsVisible)
        {
            ServiceRecordBackBtt.IsEnabled = false;
            Navigation.PopModalAsync();
        }
        else if (SRSection2.IsVisible == true)
        {
            SRSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection1.ScrollToAsync(0, 0, false);
            SRSection1.IsVisible = true;
        }
        else if (SRSection3.IsVisible == true)
        {
            SRSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection2.ScrollToAsync(0, 0, false);
            SRSection2.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection3.ScrollToAsync(0, 0, false);
            SRSection3.IsVisible = true;
        }
    }

    
    public void ServiceRecordNext1(object sender, EventArgs e)
    {
        SRSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            SRSection2.ScrollToAsync(0, 0, false);
        SRSection2.IsVisible = true;
    }

    
    public async void ServiceRecordNext2(object sender, EventArgs e)
    {
        SRSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await SRSection3.ScrollToAsync(0, 0, false);
        SRSection3.IsVisible = true;
        await LoadFolders();
    }

    
    public async void ServiceRecordNext3(object sender, EventArgs e)
    {
        SRSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Ashwell_Service_Report_{dateTimeString}.pdf";
        GatherReportData();
        //await PdfCreation.ServiceRecord(reportData, Array.Empty<byte>(), Array.Empty<byte>());
        //await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
    }
    private void GatherReportData()
    {

        reportData = new Dictionary<string, string>
        {
            { "site", site.Text ?? string.Empty },
            { "location", location.Text ?? string.Empty },
            { "assetNumber", assetNo.Text ?? string.Empty },
            { "applianceNumber", applianceNo.Text ?? string.Empty },
            { "testsCompleted", checkTestsCompleted.IsChecked.ToString() },
            { "remedialWorkRequired", checRemedialWorkRequired.IsChecked.ToString() },
            { "applianceMake", applianceMake.Text ?? string.Empty },
            { "applianceModel", applianceModel.Text ?? string.Empty },
            { "applianceSerialNumber", applianceSerialNo.Text ?? string.Empty },
            { "gcNumber", GCNo.Text ?? string.Empty },
            { "Heating", checkHeating.IsChecked.ToString() },
            { "HotWater", checkHotWater.IsChecked.ToString() },
            { "Both", checkBoth.IsChecked.ToString() },
            { "approxAgeOfAppliance", years.Text ?? string.Empty },
            { "badgedInput", badgedInput.Text ?? string.Empty },
            { "badgedOutput", budgedOutput.Text ?? string.Empty },
            { "burnerMake", burnerMake.Text ?? string.Empty },
            { "burnerModel", burnerModel.Text ?? string.Empty },
            { "burnerSerialNumber", burnerSerialNo.Text ?? string.Empty },
            { "Type", type.Text ?? string.Empty },
            { "Spec", spec.Text ?? string.Empty },
            { "OpenFlue", checkOpenFlue.IsChecked.ToString() },
            { "Roomsealed", checkRoomSealed.IsChecked.ToString() },
            { "ForcedDraft", checkForcedDraft.IsChecked.ToString() },
            { "Flueless", checkFlueless.IsChecked.ToString() },
            { "badgedBurnerPressure", badgedBurnerPressure.Text ?? string.Empty },
            { "ventilationSatisfactory", checkVentilationSatisfactory.IsChecked.ToString() },
            { "flueConditionSatisfactory", checkFlueConditionSatisfactory.IsChecked.ToString() },
            { "tempNG", checkNG.IsChecked.ToString() },
            { "tempLPG", checkLPG.IsChecked.ToString() },
            { "applianceServiceValveSatisfactory", checkAppServiceValve.IsChecked.ToString() },
            { "applianceServiceValveSatisfactoryNA", checkAppServiceValveNA.IsChecked.ToString() },
            { "applianceServiceValveSatisfactoryComments", applianceServiceValveComment.Text ?? string.Empty },
            { "governorsSatisfactory", checkGovernors.IsChecked.ToString() },
            { "governorsSatisfactoryNA", checkGovernorsNA.IsChecked.ToString() },
            { "governorsComments", governorsComment.Text ?? string.Empty },
            { "gasSolenoidValvesSatisfactory", checkGasSolenoidValves.IsChecked.ToString() },
            { "gasSolenoidValvesSatisfactoryNA", checkGasSolenoidValvesNA.IsChecked.ToString() },
            { "gasSolenoidValvesComments", gasSolenoidValvesComment.Text ?? string.Empty },
            { "controlBoxPcbSatisfactory", checkControlBoxPCB.IsChecked.ToString() },
            { "controlBoxPcbSatisfactoryNA", checkControlBoxPCBNA.IsChecked.ToString() },
            { "controlBoxPcbComments", controlBoxPCBComment.Text ?? string.Empty },
            { "gasketSealsSatisfactory", checkGasketSeals.IsChecked.ToString() },
            { "gasketSealsSatisfactoryNA", checkGasketSealsNA.IsChecked.ToString() },
            { "gasketSealsComments", gasketSealsComment.Text ?? string.Empty },
            { "burnerSatisfactory", checkBurner.IsChecked.ToString() },
            { "burnerSatisfactoryNA", checkBurnerNA.IsChecked.ToString() },
            { "burnerComments", burnerComment.Text ?? string.Empty },
            { "burnerJetsSatisfactory", checkBurnerJets.IsChecked.ToString() },
            { "burnerJetsSatisfactoryNA", checkBurnerJetsNA.IsChecked.ToString() },
            { "burnerJetsComments", burnerJetsComment.Text ?? string.Empty },
            { "electrodesTransformerSatisfactory", checkElectrodesTransformer.IsChecked.ToString() },
            { "electrodesTransformerSatisfactoryNA", checkElectrodesTransformerNA.IsChecked.ToString() },
            { "electrodesTransformerComments", electrodesTransformerComment.Text ?? string.Empty },
            { "flameFailureDeviceSatisfactory", checkFlameFailureDevice.IsChecked.ToString() },
            { "flameFailureDeviceSatisfactoryNA", checkFlameFailureDeviceNA.IsChecked.ToString() },
            { "flameFailureDeviceComments", flameFailureDeviceComment.Text ?? string.Empty },
            { "systemBoilerControlsSatisfactory", checkSystemBoilerControls.IsChecked.ToString() },
            { "systemBoilerControlsSatisfactoryNA", checkSystemBolierControlsNA.IsChecked.ToString() },
            { "systemBoilerControlsComments", systemBoilerControlsComment.Text ?? string.Empty },
            { "boilerCasingSatisfactory", checkBoilerCasing.IsChecked.ToString() },
            { "boilerCasingSatisfactoryNA", checkBoilerCasingNA.IsChecked.ToString() },
            { "boilerCasingComments", boilerCasingComment.Text ?? string.Empty },
            { "thermalInsulationSatisfactory", checkThermalInsulation.IsChecked.ToString() },
            { "thermalInsulationSatisfactoryNA", checkThermalInsulationNA.IsChecked.ToString() },
            { "thermalInsulationComments", thermalInsulationComment.Text ?? string.Empty },
            { "combustionFanIdFanSatisfactory", checkCombustionFanIdFan.IsChecked.ToString() },
            { "combustionFanIdFanSatisfactoryNA", checkCombustionFanIdFanNA.IsChecked.ToString() },
            { "combustionFanIdFanComments", combustionFanIdFanComment.Text ?? string.Empty },
            { "airFluePressureSwitchSatisfactory", checkAirFluePressureSwitch.IsChecked.ToString() },
            { "airFluePressureSwitchSatisfactoryNA", checkAirFluePressureSwitchNA.IsChecked.ToString() },
            { "airFluePressureSwitchComments", airFluePressureSwitchComment.Text ?? string.Empty },
            { "controlLimitStatsSatisfactory", checkControlLimitStatus.IsChecked.ToString() },
            { "controlLimitStatsSatisfactoryNA", checkControlLimitStatusNA.IsChecked.ToString() },
            { "controlLimitStatsComments", controlLimitStatusComment.Text ?? string.Empty },
            { "pressureTempGaugesSatisfactory", checkPressureTempGauges.IsChecked.ToString() },
            { "pressureTempGaugesSatisfactoryNA", checkPressureTempGaugesNA.IsChecked.ToString() },
            { "pressureTempGaugesComments", pressureTempGaugesComment.Text ?? string.Empty },
            { "circulationPumpsSatisfactory", checkCirculationPumps.IsChecked.ToString() },
            { "circulationPumpsSatisfactoryNA", checkCirculationPumpsNA.IsChecked.ToString() },
            { "circulationPumpsComments", circulationPumpsComment.Text ?? string.Empty },
            { "condenseTrapSatisfactory", checkCondenseTrap.IsChecked.ToString() },
            { "condenseTrapSatisfactoryNA", checkCondenseTrapNA.IsChecked.ToString() },
            { "condenseTrapComments", condenseTrapComment.Text ?? string.Empty },
            { "heatExhanger", checkHeatExchangerFluewaysClear.IsChecked.ToString() },
            { "heatExhangerNA", checkHeatExchangerFluewaysClearNA.IsChecked.ToString() },
            { "heatExhangerComments", heatExchangerFluewaysClearComment.Text ?? string.Empty },
            { "workingInletPressure", workingIntelPressure.Text ?? string.Empty },
            { "recordedBurnerPressure", recordedBurnerPressure.Text ?? string.Empty },
            { "measuredGasRate", measuredGasRate.Text ?? string.Empty },
            { "flueFlowTest", checkFlueFlowTest.IsChecked.ToString() },
            { "flueFlowTestNA", checkFlueFlowTestNA.IsChecked.ToString() },
            { "flueFlowTestComments", flueFlowTestComment.Text ?? string.Empty },
            { "spillageTest", checkSpillageTest.IsChecked.ToString() },
            { "spillageTestNA", checkSpillageTestNA.IsChecked.ToString() },
            { "spillageTestComments", spillageTestComment.Text ?? string.Empty },
            { "AECVPlantIsolationCorrect", checkAECVPlantIsolationCorrect.IsChecked.ToString() },
            { "AECVPlantIsolationCorrectNA", checkAECVPlantIsolationCorrectNA.IsChecked.ToString() },
            { "AECVPlantIsolationCorrectComments", AECVPlantIsolationCorrectComment.Text ?? string.Empty },
            { "safetyShutOffValve", checkSafetyShutOffValve.IsChecked.ToString() },
            { "safetyShutOffValveNA", checkSafetyShutOffValveNA.IsChecked.ToString() },
            { "safetyShutOffValveComments", safetyShutOffValveComment.Text ?? string.Empty },
            { "plantroomGasTightnessTest", checkPlantroomGasTightnessTest.IsChecked.ToString() },
            { "plantroomGasTightnessTestNA", checkPlantroomGasTightnessTestNA.IsChecked.ToString() },
            { "plantroomGasTightnessTestComments", plantroomGasTightnessTestComment.Text ?? string.Empty },
            { "stateApplianceCondition", stateApplianceCondition.Text ?? string.Empty },
            { "HighFireCO2", highFireCO2.Text ?? string.Empty },
            { "HighFireCO", highFireCO.Text ?? string.Empty },
            { "HighFireO2", highFireO2.Text ?? string.Empty },
            { "HighFireRatio", highFireRatio.Text ?? string.Empty },
            { "HighFireFlueTemp", highFireFlueTemp.Text ?? string.Empty },
            { "HighFireEfficiency", highFireEfficiency.Text ?? string.Empty },
            { "HighFireExcessAir", highFireExcessAir.Text ?? string.Empty },
            { "HighFireRoomTemp", highFireRoomTemp.Text ?? string.Empty },
            { "LowFireCO2", lowFireCO2.Text ?? string.Empty },
            { "LowFireCO", lowFireCO.Text ?? string.Empty },
            { "LowFireO2", lowFireO2.Text ?? string.Empty },
            { "LowFireRatio", lowFireRatio.Text ?? string.Empty },
            { "LowFireFlueTemp", lowFireFlueTemp.Text ?? string.Empty },
            { "LowFireEfficiency", lowFireEfficiency.Text ?? string.Empty },
            { "LowFireExcessAir", lowFireExcessAir.Text ?? string.Empty },
            { "LowFireRoomTemp", lowFireRoomTemp.Text ?? string.Empty },
            { "engineersName", engineersName.Text ?? string.Empty },
            //{ "engineersSignature", engineersSignature.Text ?? string.Empty },
            { "engineersGasSafeID", engineersGasSafeIDNumber.Text ?? string.Empty },
            { "clientsName", clientsName.Text ?? string.Empty },
            //{ "clientsSignature", clientsSignature.Text ?? string.Empty },
            { "inspectionDate", inspectionDate.Text ?? string.Empty },
            { "commetsDefects", additionalCommentsDefects.Text ?? string.Empty },
            { "warningNoticeIssueNumber", warningNoticeNumber.Text ?? string.Empty }
        };

        bool tempNG = checkNG.IsChecked;
        bool tempLPG = checkLPG.IsChecked;
        reportData.Add("gasType", tempNG ? "NG" : (tempLPG ? "LPG" : ""));
        reportData.Add("reportName", reportName);
    }
}