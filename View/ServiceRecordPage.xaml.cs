using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage1 : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public ServiceRecordPage1()
    {
        InitializeComponent();
    }
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;
        
        // Call the UploadReport function and ignore the result
        _ = UploadReport(folderId, reportData);
    }

    private async Task UploadReport(string folderId, Dictionary<string, string> report)
    {
        try
        {
            // Assuming ApiService.UploadReportAsync takes folderId and a Report object
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ServiceRecord, reportName, folderId, report);

            if (response.IsSuccessStatusCode)
            {
                await Navigation.PopAsync();
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
                        Timestamp = element.GetProperty("created_at").GetString()
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

    public class Folder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Timestamp { get; set; }
    }

    public void ServiceRecordBack(object sender, EventArgs e)
    {
        if (SRSection1.IsVisible)
            Navigation.PopAsync();
        else if (SRSection2.IsVisible == true)
        {
            SRSection2.IsVisible = false;

            SRSection1.ScrollToAsync(0, 0, false);
            SRSection1.IsVisible = true;
        }
        else if (SRSection3.IsVisible == true)
        {
            SRSection3.IsVisible = false;

            SRSection2.ScrollToAsync(0, 0, false);
            SRSection2.IsVisible = true;
        }
        else
        {
            SRSection4.IsVisible = false;

            SRSection3.ScrollToAsync(0, 0, false);
            SRSection3.IsVisible = true;
        }
    }

    public void ServiceRecordNext1(object sender, EventArgs e)
    {
        SRSection1.IsVisible = false;

        SRSection2.ScrollToAsync(0, 0, false);
        SRSection2.IsVisible = true;
    }

    public async void ServiceRecordNext2(object sender, EventArgs e)
    {
        SRSection2.IsVisible = false;

        await SRSection3.ScrollToAsync(0, 0, false);
        SRSection3.IsVisible = true;
        await LoadFolders();
    }

    public async void ServiceRecordNext3(object sender, EventArgs e)
    {
        Button_Clicked(sender, e);
        await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
        SRSection3.IsVisible = false;

        await SRSection4.ScrollToAsync(0, 0, false);
        SRSection4.IsVisible = true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Ashwell_Service_Report_{dateTimeString}.pdf";
        GatherReportData();
        await PdfCreation.CreateServiceRecordPDF(reportData);
    }
    private void GatherReportData()
    {

        reportData = new Dictionary<string, string>();

        reportData.Add("site", site.Text ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        reportData.Add("assetNumber", assetNo.Text ?? string.Empty);
        reportData.Add("applianceNumber", applianceNo.Text ?? string.Empty);
        reportData.Add("testsCompleted", checkTestsCompleted.IsChecked.ToString());
        reportData.Add("remedialWorkRequired", checRemedialWorkRequired.IsChecked.ToString());
        reportData.Add("applianceMake", applianceMake.Text ?? string.Empty);
        reportData.Add("applianceModel", applianceModel.Text ?? string.Empty);
        reportData.Add("applianceSerialNumber", applianceSerialNo.Text ?? string.Empty);
        reportData.Add("gcNumber", GCNo.Text ?? string.Empty);
        reportData.Add("Heating", checkHeating.IsChecked.ToString());
        reportData.Add("HotWater", checkHotWater.IsChecked.ToString());
        reportData.Add("Both", checkBoth.IsChecked.ToString());
        reportData.Add("approxAgeOfAppliance", years.Text ?? string.Empty);
        reportData.Add("badgedInput", badgedInput.Text ?? string.Empty);
        reportData.Add("badgedOutput", budgedOutput.Text ?? string.Empty);
        reportData.Add("burnerMake", burnerMake.Text ?? string.Empty);
        reportData.Add("burnerModel", burnerModel.Text ?? string.Empty);
        reportData.Add("burnerSerialNumber", burnerSerialNo.Text ?? string.Empty);
        reportData.Add("Type", type.Text ?? string.Empty);
        reportData.Add("Spec", spec.Text ?? string.Empty);
        reportData.Add("OpenFlue", checkOpenFlue.IsChecked.ToString());
        reportData.Add("Roomsealed", checkRoomSealed.IsChecked.ToString());
        reportData.Add("ForcedDraft", checkForcedDraft.IsChecked.ToString());
        reportData.Add("Flueless", checkFlueless.IsChecked.ToString());
        reportData.Add("badgedBurnerPressure", badgedBurnerPressure.Text ?? string.Empty);
        reportData.Add("ventilationSatisfactory", checkVentilationSatisfactory.IsChecked.ToString());
        reportData.Add("flueConditionSatisfactory", checkFlueConditionSatisfactory.IsChecked.ToString());
        reportData.Add("tempNG", checkNG.IsChecked.ToString());
        reportData.Add("tempLPG", checkLPG.IsChecked.ToString());
        reportData.Add("applianceServiceValveSatisfactory", checkAppServiceValve.IsChecked.ToString());
        reportData.Add("applianceServiceValveSatisfactoryNA", checkAppServiceValveNA.IsChecked.ToString());
        reportData.Add("applianceServiceValveSatisfactoryComments", applianceServiceValveComment.Text ?? string.Empty);
        reportData.Add("governorsSatisfactory", checkGovernors.IsChecked.ToString());
        reportData.Add("governorsSatisfactoryNA", checkGovernorsNA.IsChecked.ToString());
        reportData.Add("governorsComments", governorsComment.Text ?? string.Empty);
        reportData.Add("gasSolenoidValvesSatisfactory", checkGasSolenoidValves.IsChecked.ToString());
        reportData.Add("gasSolenoidValvesSatisfactoryNA", checkGasSolenoidValvesNA.IsChecked.ToString());
        reportData.Add("gasSolenoidValvesComments", gasSolenoidValvesComment.Text ?? string.Empty);
        reportData.Add("controlBoxPcbSatisfactory", checkControlBoxPCB.IsChecked.ToString());
        reportData.Add("controlBoxPcbSatisfactoryNA", checkControlBoxPCBNA.IsChecked.ToString());
        reportData.Add("controlBoxPcbComments", controlBoxPCBComment.Text ?? string.Empty);
        reportData.Add("gasketSealsSatisfactory", checkGasketSeals.IsChecked.ToString());
        reportData.Add("gasketSealsSatisfactoryNA", checkGasketSealsNA.IsChecked.ToString());
        reportData.Add("gasketSealsComments", gasketSealsComment.Text ?? string.Empty);
        reportData.Add("burnerSatisfactory", checkBurner.IsChecked.ToString());
        reportData.Add("burnerSatisfactoryNA", checkBurnerNA.IsChecked.ToString());
        reportData.Add("burnerComments", burnerComment.Text ?? string.Empty);
        reportData.Add("burnerJetsSatisfactory", checkBurnerJets.IsChecked.ToString());
        reportData.Add("burnerJetsSatisfactoryNA", checkBurnerJetsNA.IsChecked.ToString());
        reportData.Add("burnerJetsComments", burnerJetsComment.Text ?? string.Empty);
        reportData.Add("electrodesTransformerSatisfactory", checkElectrodesTransformer.IsChecked.ToString());
        reportData.Add("electrodesTransformerSatisfactoryNA", checkElectrodesTransformerNA.IsChecked.ToString());
        reportData.Add("electrodesTransformerComments", electrodesTransformerComment.Text ?? string.Empty);
        reportData.Add("flameFailureDeviceSatisfactory", checkFlameFailureDevice.IsChecked.ToString());
        reportData.Add("flameFailureDeviceSatisfactoryNA", checkFlameFailureDeviceNA.IsChecked.ToString());
        reportData.Add("flameFailureDeviceComments", flameFailureDeviceComment.Text ?? string.Empty);
        reportData.Add("systemBoilerControlsSatisfactory", checkSystemBoilerControls.IsChecked.ToString());
        reportData.Add("systemBoilerControlsSatisfactoryNA", checkSystemBolierControlsNA.IsChecked.ToString());
        reportData.Add("systemBoilerControlsComments", systemBoilerControlsComment.Text ?? string.Empty);
        reportData.Add("boilerCasingSatisfactory", checkBoilerCasing.IsChecked.ToString());
        reportData.Add("boilerCasingSatisfactoryNA", checkBoilerCasingNA.IsChecked.ToString());
        reportData.Add("boilerCasingComments", boilerCasingComment.Text ?? string.Empty);
        reportData.Add("thermalInsulationSatisfactory", checkThermalInsulation.IsChecked.ToString());
        reportData.Add("thermalInsulationSatisfactoryNA", checkThermalInsulationNA.IsChecked.ToString());
        reportData.Add("thermalInsulationComments", thermalInsulationComment.Text ?? string.Empty);
        reportData.Add("combustionFanIdFanSatisfactory", checkCombustionFanIdFan.IsChecked.ToString());
        reportData.Add("combustionFanIdFanSatisfactoryNA", checkCombustionFanIdFanNA.IsChecked.ToString());
        reportData.Add("combustionFanIdFanComments", combustionFanIdFanComment.Text ?? string.Empty);
        reportData.Add("airFluePressureSwitchSatisfactory", checkAirFluePressureSwitch.IsChecked.ToString());
        reportData.Add("airFluePressureSwitchSatisfactoryNA", checkAirFluePressureSwitchNA.IsChecked.ToString());
        reportData.Add("airFluePressureSwitchComments", airFluePressureSwitchComment.Text ?? string.Empty);
        reportData.Add("controlLimitStatsSatisfactory", checkControlLimitStatus.IsChecked.ToString());
        reportData.Add("controlLimitStatsSatisfactoryNA", checkControlLimitStatusNA.IsChecked.ToString());
        reportData.Add("controlLimitStatsComments", controlLimitStatusComment.Text ?? string.Empty);
        reportData.Add("pressureTempGaugesSatisfactory", checkPressureTempGauges.IsChecked.ToString());
        reportData.Add("pressureTempGaugesSatisfactoryNA", checkPressureTempGaugesNA.IsChecked.ToString());
        reportData.Add("pressureTempGaugesComments", pressureTempGaugesComment.Text ?? string.Empty);
        reportData.Add("circulationPumpsSatisfactory", checkCirculationPumps.IsChecked.ToString());
        reportData.Add("circulationPumpsSatisfactoryNA", checkCirculationPumpsNA.IsChecked.ToString());
        reportData.Add("circulationPumpsComments", circulationPumpsComment.Text ?? string.Empty);
        reportData.Add("condenseTrapSatisfactory", checkCondenseTrap.IsChecked.ToString());
        reportData.Add("condenseTrapSatisfactoryNA", checkCondenseTrapNA.IsChecked.ToString());
        reportData.Add("condenseTrapComments", condenseTrapComment.Text ?? string.Empty);
        reportData.Add("heatExhanger", checkHeatExchangerFluewaysClear.IsChecked.ToString());
        reportData.Add("heatExhangerNA", checkHeatExchangerFluewaysClearNA.IsChecked.ToString());
        reportData.Add("heatExhangerComments", heatExchangerFluewaysClearComment.Text ?? string.Empty);
        reportData.Add("workingInletPressure", workingIntelPressure.Text ?? string.Empty);
        reportData.Add("recordedBurnerPressure", recordedBurnerPressure.Text ?? string.Empty);
        reportData.Add("measuredGasRate", measuredGasRate.Text ?? string.Empty);
        reportData.Add("flueFlowTest", checkFlueFlowTest.IsChecked.ToString());
        reportData.Add("flueFlowTestNA", checkFlueFlowTestNA.IsChecked.ToString());
        reportData.Add("flueFlowTestComments", flueFlowTestComment.Text ?? string.Empty);
        reportData.Add("spillageTest", checkSpillageTest.IsChecked.ToString());
        reportData.Add("spillageTestNA", checkSpillageTestNA.IsChecked.ToString());
        reportData.Add("spillageTestComments", spillageTestComment.Text ?? string.Empty);
        reportData.Add("AECVPlantIsolationCorrect", checkAECVPlantIsolationCorrect.IsChecked.ToString());
        reportData.Add("AECVPlantIsolationCorrectNA", checkAECVPlantIsolationCorrectNA.IsChecked.ToString());
        reportData.Add("AECVPlantIsolationCorrectComments", AECVPlantIsolationCorrectComment.Text ?? string.Empty);
        reportData.Add("safetyShutOffValve", checkSafetyShutOffValve.IsChecked.ToString());
        reportData.Add("safetyShutOffValveNA", checkSafetyShutOffValveNA.IsChecked.ToString());
        reportData.Add("safetyShutOffValveComments", safetyShutOffValveComment.Text ?? string.Empty);
        reportData.Add("plantroomGasTightnessTest", checkPlantroomGasTightnessTest.IsChecked.ToString());
        reportData.Add("plantroomGasTightnessTestNA", checkPlantroomGasTightnessTestNA.IsChecked.ToString());
        reportData.Add("plantroomGasTightnessTestComments", plantroomGasTightnessTestComment.Text ?? string.Empty);
        reportData.Add("stateApplianceCondition", stateApplianceCondition.Text ?? string.Empty);
        reportData.Add("HighFireCO2", highFireCO2.Text ?? string.Empty);
        reportData.Add("HighFireCO", highFireCO.Text ?? string.Empty);
        reportData.Add("HighFireO2", highFireO2.Text ?? string.Empty);
        reportData.Add("HighFireRatio", highFireRatio.Text ?? string.Empty);
        reportData.Add("HighFireFlueTemp", highFireFlueTemp.Text ?? string.Empty);
        reportData.Add("HighFireEfficiency", highFireEfficiency.Text ?? string.Empty);
        reportData.Add("HighFireExcessAir", highFireExcessAir.Text ?? string.Empty);
        reportData.Add("HighFireRoomTemp", highFireRoomTemp.Text ?? string.Empty);
        reportData.Add("LowFireCO2", lowFireCO2.Text ?? string.Empty);
        reportData.Add("LowFireCO", lowFireCO.Text ?? string.Empty);
        reportData.Add("LowFireO2", lowFireO2.Text ?? string.Empty);
        reportData.Add("LowFireRatio", lowFireRatio.Text ?? string.Empty);
        reportData.Add("LowFireFlueTemp", lowFireFlueTemp.Text ?? string.Empty);
        reportData.Add("LowFireEfficiency", lowFireEfficiency.Text ?? string.Empty);
        reportData.Add("LowFireExcessAir", lowFireExcessAir.Text ?? string.Empty);
        reportData.Add("LowFireRoomTemp", lowFireRoomTemp.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("engineersSignature", engineersSignature.Text ?? string.Empty);
        reportData.Add("engineersGasSafeID", engineersGasSafeIDNumber.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("clientsSignature", clientsSignature.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Text ?? string.Empty);
        reportData.Add("commetsDefects", additionalCommentsDefects.Text ?? string.Empty);
        reportData.Add("warningNoticeIssueNumber", warningNoticeNumber.Text ?? string.Empty);

        bool tempNG = checkNG.IsChecked;
        bool tempLPG = checkLPG.IsChecked;
        reportData.Add("gasType", tempNG ? "NG" : (tempLPG ? "LPG" : ""));
        reportData.Add("reportName", reportName);
    }
}