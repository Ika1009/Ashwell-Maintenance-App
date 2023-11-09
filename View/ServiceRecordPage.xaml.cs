using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using static Ashwell_Maintenance.View.ServiceRecordPage1;

namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage1 : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new ObservableCollection<Folder>();

    public ServiceRecordPage1()
    {
        InitializeComponent();
    }
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;
        
        // Call the UploadReport function and ignore the result
        _ = UploadReport(folderId, GatherReportData());
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
            if (response.IsSuccessStatusCode)
            {
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
                    if (FoldersListView.ItemsSource == null)
                        FoldersListView.ItemsSource = Folders;
                }
                else
                {
                    await DisplayAlert("Information", "No folders found.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load folders.", "OK");
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

        SRSection3.ScrollToAsync(0, 0, false);
        SRSection3.IsVisible = true;
        await LoadFolders();
    }

    public async void ServiceRecordNext3(object sender, EventArgs e)
    {
        Button_Clicked(sender, e);
        await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
        SRSection3.IsVisible = false;

        SRSection4.ScrollToAsync(0, 0, false);
        SRSection4.IsVisible = true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string reportName = $"Ashwell_Service_Report_{dateTimeString}.pdf";

        string site1 = site.Text ?? string.Empty;
        string location1 = location.Text ?? string.Empty;
        string assetNumber1 = assetNo.Text ?? string.Empty;
        string applianceNumber1 = applianceNo.Text ?? string.Empty;
        bool testsCompleted1 = checkTestsCompleted.IsChecked;
        bool remedialWorkRequired1 = checRemedialWorkRequired.IsChecked;
        string applianceMake1 = applianceMake.Text ?? string.Empty;
        string applianceModel1 = applianceModel.Text ?? string.Empty;
        string applianceSerialNumber1 = applianceSerialNo.Text ?? string.Empty;
        string gcNumber1 = GCNo.Text ?? string.Empty;
        bool Heating1 = checkHeating.IsChecked;
        bool HotWater1 = checkHotWater.IsChecked;
        bool Both1 = checkBoth.IsChecked;
        string approxAgeOfAppliance1 = years.Text ?? string.Empty;
        string badgedInput1 = badgedInput.Text ?? string.Empty;
        string badgedOutput1 = budgedOutput.Text ?? string.Empty;
        string burnerMake1 = burnerMake.Text ?? string.Empty;
        string burnerModel1 = burnerModel.Text ?? string.Empty;
        string burnerSerialNumber1 = burnerSerialNo.Text ?? string.Empty;
        string Type1 = type.Text ?? string.Empty;
        string Spec1 = spec.Text ?? string.Empty;
        bool OpenFlue1 = checkOpenFlue.IsChecked;
        bool Roomsealed1 = checkRoomSealed.IsChecked;
        bool ForcedDraft1 = checkForcedDraft.IsChecked;
        bool Flueless1 = checkFlueless.IsChecked;
        string badgedBurnerPressure1 = badgedBurnerPressure.Text ?? string.Empty;
        bool ventilationSatisfactory1 = checkVentilationSatisfactory.IsChecked;
        bool flueConditionSatisfactory1 = checkFlueConditionSatisfactory.IsChecked;
        bool tempNG = checkNG.IsChecked;
        bool tempLPG = checkLPG.IsChecked;
        bool applianceServiceValveSatisfactory1 = checkAppServiceValve.IsChecked;
        bool applianceServiceValveSatisfactoryNA1 = checkAppServiceValveNA.IsChecked;
        string applianceServiceValveSatisfactoryComments1 = applianceServiceValveComment.Text ?? string.Empty;
        bool governorsSatisfactory1 = checkGovernors.IsChecked;
        bool governorsSatisfactoryNA1 = checkGovernorsNA.IsChecked;
        string governorsComments1 = governorsComment.Text ?? string.Empty;
        bool gasSolenoidValvesSatisfactory1 = checkGasSolenoidValves.IsChecked;
        bool gasSolenoidValvesSatisfactoryNA1 = checkGasSolenoidValvesNA.IsChecked;
        string gasSolenoidValvesComments1 = gasSolenoidValvesComment.Text ?? string.Empty;
        bool controlBoxPcbSatisfactory1 = checkControlBoxPCB.IsChecked;
        bool controlBoxPcbSatisfactoryNA1 = checkControlBoxPCBNA.IsChecked;
        string controlBoxPcbComments1 = controlBoxPCBComment.Text ?? string.Empty;
        bool gasketSealsSatisfactory1 = checkGasketSeals.IsChecked;
        bool gasketSealsSatisfactoryNA1 = checkGasketSealsNA.IsChecked;
        string gasketSealsComments1 = gasketSealsComment.Text ?? string.Empty;
        bool burnerSatisfactory1 = checkBurner.IsChecked;
        bool burnerSatisfactoryNA1 = checkBurnerNA.IsChecked;
        string burnerComments1 = burnerComment.Text ?? string.Empty;
        bool burnerJetsSatisfactory1 = checkBurnerJets.IsChecked;
        bool burnerJetsSatisfactoryNA1 = checkBurnerJetsNA.IsChecked;
        string burnerJetsComments1 = burnerJetsComment.Text ?? string.Empty;
        bool electrodesTransformerSatisfactory1 = checkElectrodesTransformer.IsChecked;
        bool electrodesTransformerSatisfactoryNA1 = checkElectrodesTransformerNA.IsChecked;
        string electrodesTransformerComments1 = electrodesTransformerComment.Text ?? string.Empty;
        bool flameFailureDeviceSatisfactory1 = checkFlameFailureDevice.IsChecked;
        bool flameFailureDeviceSatisfactoryNA1 = checkFlameFailureDeviceNA.IsChecked;
        string flameFailureDeviceComments1 = flameFailureDeviceComment.Text ?? string.Empty;
        bool systemBoilerControlsSatisfactory1 = checkSystemBoilerControls.IsChecked;
        bool systemBoilerControlsSatisfactoryNA1 = checkSystemBolierControlsNA.IsChecked;
        string systemBoilerControlsComments1 = systemBoilerControlsComment.Text ?? string.Empty;
        bool boilerCasingSatisfactory1 = checkBoilerCasing.IsChecked;
        bool boilerCasingSatisfactoryNA1 = checkBoilerCasingNA.IsChecked;
        string boilerCasingComments1 = boilerCasingComment.Text ?? string.Empty;
        bool thermalInsulationSatisfactory1 = checkThermalInsulation.IsChecked;
        bool thermalInsulationSatisfactoryNA1 = checkThermalInsulationNA.IsChecked;
        string thermalInsulationComments1 = thermalInsulationComment.Text ?? string.Empty;
        bool combustionFanIdFanSatisfactory1 = checkCombustionFanIdFan.IsChecked;
        bool combustionFanIdFanSatisfactoryNA1 = checkCombustionFanIdFanNA.IsChecked;
        string combustionFanIdFanComments1 = combustionFanIdFanComment.Text ?? string.Empty;
        bool airFluePressureSwitchSatisfactory1 = checkAirFluePressureSwitch.IsChecked;
        bool airFluePressureSwitchSatisfactoryNA1 = checkAirFluePressureSwitchNA.IsChecked;
        string airFluePressureSwitchComments1 = airFluePressureSwitchComment.Text ?? string.Empty;
        bool controlLimitStatsSatisfactory1 = checkControlLimitStatus.IsChecked;
        bool controlLimitStatsSatisfactoryNA1 = checkControlLimitStatusNA.IsChecked;
        string controlLimitStatsComments1 = controlLimitStatusComment.Text ?? string.Empty;
        bool pressureTempGaugesSatisfactory1 = checkPressureTempGauges.IsChecked;
        bool pressureTempGaugesSatisfactoryNA1 = checkPressureTempGaugesNA.IsChecked;
        string pressureTempGaugesComments1 = pressureTempGaugesComment.Text ?? string.Empty;
        bool circulationPumpsSatisfactory1 = checkCirculationPumps.IsChecked;
        bool circulationPumpsSatisfactoryNA1 = checkCirculationPumpsNA.IsChecked;
        string circulationPumpsComments1 = circulationPumpsComment.Text ?? string.Empty;
        bool condenseTrapSatisfactory1 = checkCondenseTrap.IsChecked;
        bool condenseTrapSatisfactoryNA1 = checkCondenseTrapNA.IsChecked;
        string condenseTrapComments1 = condenseTrapComment.Text ?? string.Empty;
        bool heatExhanger1 = checkHeatExchangerFluewaysClear.IsChecked;
        bool heatExhangerNA1 = checkHeatExchangerFluewaysClearNA.IsChecked;
        string heatExhangerComments1 = heatExchangerFluewaysClearComment.Text ?? string.Empty;
        string workingInletPressure1 = workingIntelPressure.Text ?? string.Empty;
        string recordedBurnerPressure1 = recordedBurnerPressure.Text ?? string.Empty;
        string measuredGasRate1 = measuredGasRate.Text ?? string.Empty;
        bool flueFlowTest1 = checkFlueFlowTest.IsChecked;
        bool flueFlowTestNA1 = checkFlueFlowTestNA.IsChecked;
        string flueFlowTestComments1 = flueFlowTestComment.Text ?? string.Empty;
        bool spillageTest1 = checkSpillageTest.IsChecked;
        bool spillageTestNA1 = checkSpillageTestNA.IsChecked;
        string spillageTestComments1 = spillageTestComment.Text ?? string.Empty;
        bool AECVPlantIsolationCorrect1 = checkAECVPlantIsolationCorrect.IsChecked;
        bool AECVPlantIsolationCorrectNA1 = checkAECVPlantIsolationCorrectNA.IsChecked;
        string AECVPlantIsolationCorrectComments1 = AECVPlantIsolationCorrectComment.Text ?? string.Empty;
        bool safetyShutOffValve1 = checkSafetyShutOffValve.IsChecked;
        bool safetyShutOffValveNA1 = checkSafetyShutOffValveNA.IsChecked;
        string safetyShutOffValveComments1 = safetyShutOffValveComment.Text ?? string.Empty;
        bool plantroomGasTightnessTest1 = checkPlantroomGasTightnessTest.IsChecked;
        bool plantroomGasTightnessTestNA1 = checkPlantroomGasTightnessTestNA.IsChecked;
        string plantroomGasTightnessTestComments1 = plantroomGasTightnessTestComment.Text ?? string.Empty;
        string stateApplianceCondition1 = stateApplianceCondition.Text ?? string.Empty;
        string HighFireCO21 = highFireCO2.Text ?? string.Empty;
        string HighFireCO1 = highFireCO.Text ?? string.Empty;
        string HighFireO21 = highFireO2.Text ?? string.Empty;
        string HighFireRatio1 = highFireRatio.Text ?? string.Empty;
        string HighFireFlueTemp1 = highFireFlueTemp.Text ?? string.Empty;
        string HighFireEfficiency1 = highFireEfficiency.Text ?? string.Empty;
        string HighFireExcessAir1 = highFireExcessAir.Text ?? string.Empty;
        string HighFireRoomTemp1 = highFireRoomTemp.Text ?? string.Empty;
        string LowFireCO21 = lowFireCO2.Text ?? string.Empty;
        string LowFireCO1 = lowFireCO.Text ?? string.Empty;
        string LowFireO21 = lowFireO2.Text ?? string.Empty;
        string LowFireRatio1 = lowFireRatio.Text ?? string.Empty;
        string LowFireFlueTemp1 = lowFireFlueTemp.Text ?? string.Empty;
        string LowFireEfficiency1 = lowFireEfficiency.Text ?? string.Empty;
        string LowFireExcessAir1 = lowFireExcessAir.Text ?? string.Empty;
        string LowFireRoomTemp1 = lowFireRoomTemp.Text ?? string.Empty;
        string engineersName1 = engineersName.Text ?? string.Empty;
        string engineersSignature1 = engineersSignature.Text ?? string.Empty;
        string engineersGasSafeID1 = engineersGasSafeIDNumber.Text ?? string.Empty;
        string clientsName1 = clientsName.Text ?? string.Empty;
        string clientsSignature1 = clientsSignature.Text ?? string.Empty;
        string inspectionDate1 = inspectionDate.Text ?? string.Empty;
        string commetsDefects1 = additionalCommentsDefects.Text ?? string.Empty;
        string warningNoticeIssueNumber1 = warningNoticeNumber.Text ?? string.Empty;
        string gasType1 = "";
        if (tempNG)
        {
            gasType1 = "NG";
        }
        else if (tempLPG)
        {
            gasType1 = "LPG";
        }


        //await PdfCreation.CreateServiceRecordPDF(GatherReportData()); ;
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

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
        return reportData;
    }

}