using Microsoft.Maui;

namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage1 : ContentPage
{
	public ServiceRecordPage1()
	{
		InitializeComponent();
	}

	public void ServiceRecordBack(object sender, EventArgs e)
	{
		if (SRSetion1.IsVisible)
			Navigation.PushAsync(new MainPage());
		else if (SRSetion2.IsVisible == true)
		{
			SRSetion2.IsVisible = false;

			SRSetion1.ScrollToAsync(0, 0, false);
			SRSetion1.IsVisible = true;
		}
		else if (SRSetion3.IsVisible == true)
		{
			SRSetion3.IsVisible = false;

			SRSetion2.ScrollToAsync(0, 0, false);
			SRSetion2.IsVisible = true;
		}
		else
		{
            SRSetion4.IsVisible = false;

            SRSetion3.ScrollToAsync(0, 0, false);
            SRSetion3.IsVisible = true;
        }
    }

	public void ServiceRecordNext1(object sender, EventArgs e)
	{
		SRSetion1.IsVisible = false;

		SRSetion2.ScrollToAsync(0, 0, false);
		SRSetion2.IsVisible = true;
	}

    public void ServiceRecordNext2(object sender, EventArgs e)
    {
		SRSetion2.IsVisible = false;

        SRSetion3.ScrollToAsync(0, 0, false);
        SRSetion3.IsVisible = true;
    }

    public void ServiceRecordNext3(object sender, EventArgs e)
    {
        SRSetion3.IsVisible = false;

        SRSetion4.ScrollToAsync(0, 0, false);
        SRSetion4.IsVisible = true;
    }

    private async void  Button_Clicked(object sender, EventArgs e)
    {
        string site1 = site.Text;
        string location1 = location.Text;
        string assetNumber1 = assetNo.Text;
        string applianceNumber1 = applianceNo.Text;
        bool testsCompleted1 = checkTestsCompleted.Text;
        string remedialWorkRequired1 = checRemedialWorkRequired.Text;
        string applianceMake1 = applianceMake.Text;
        string applianceModel1 = applianceModel.Text;
        string applianceSerialNumber1 = applianceSerialNo.Text;
        string gcNumber1 = GCNo.Text;
        bool Heating1 = checkHeating.Text;
        bool HotWater1 = checkHotWater.Text;
        bool Both1 = checkBoth.Text;
        string approxAgeOfAppliance1 = years.Text;
        string badgedInput1 = badgedInput.Text;
        string badgedOutput1 = budgedOutput.Text;
        string burnerMake1 = burnerMake.Text;
        string burnerModel1 = burnerModel.Text;
        string burnerSerialNumber1 = burnerSerialNo.Text;
        string Type1 = type.Text;
        string Spec1 = spec.Text;
        bool OpenFlue1 = checkOpenFlue.Text;
        bool Roomsealed1 = checkRoomSealed.Text;
        bool ForcedDraft1 = checkForcedDraft.Text;
        bool Flueless1 = checkFlueless.Text;
        string badgedBurnerPressure1 = badgedBurnerPressure.Text;
        bool ventilationSatisfactory1 = checkVentilationSatisfactory.Text;
        bool flueConditionSatisfactory1 = checkFlueConditionSatisfactory.Text;
        //= checkNG.Text;
        //= checkLPG.Text;
        bool applianceServiceValveSatisfactory1 = checkAppServiceValve.Text;
        bool applianceServiceValveSatisfactoryNA1 = checkAppServiceValveNA.Text;
        string applianceServiceValveSatisfactoryComments1 = applianceServiceValveComment.Text;
        bool governorsSatisfactory1 = checkGovernors.Text;
        bool governorsSatisfactoryNA1 = checkGovernorsNA.Text;
        string governorsComments1 = governorsComment.Text;
        bool gasSolenoidValvesSatisfactory1 = checkGasSolenoidValves.Text;
        bool gasSolenoidValvesSatisfactoryNA1 = checkGasSolenoidValvesNA.Text;
        string gasSolenoidValvesComments1 = gasSolenoidValvesComment.Text;
        bool controlBoxPcbSatisfactory1 = checkControlBoxPCB.Text;
        bool controlBoxPcbSatisfactoryNA1 = checkControlBoxPCBNA.Text;
        string controlBoxPcbComments1 = controlBoxPCBComment.Text;
        bool gasketSealsSatisfactory1 = checkGasketSeals.Text;
        bool gasketSealsSatisfactoryNA1 = checkGasketSealsNA.Text;
        string gasketSealsComments1 = gasketSealsComment.Text;
        bool burnerSatisfactory1 = checkBurner.Text;
        bool burnerSatisfactoryNA1 = checkBurnerNA.Text;
        string burnerComments1 = burnerComment.Text;
        bool burnerJetsSatisfactory1 = checkBurnerJets.Text;
        bool burnerJetsSatisfactoryNA1 = checkBurnerJetsNA.Text;
        string burnerJetsComments1 = burnerJetsComment.Text;
        bool electrodesTransformerSatisfactory1 = checkElectrodesTransformer.Text;
        bool electrodesTransformerSatisfactoryNA1 = checkElectrodesTransformerNA.Text;
        string electrodesTransformerComments1 = electrodesTransformerComment.Text;
        bool flameFailureDeviceSatisfactory1 = checkFlameFailureDevice.Text;
        bool flameFailureDeviceSatisfactoryNA1 = checkFlameFailureDeviceNA.Text;
        string flameFailureDeviceComments1 = flameFailureDeviceComment.Text;
        bool systemBoilerControlsSatisfactory1 = checkSystemBoilerControls.Text;
        bool systemBoilerControlsSatisfactoryNA1 = checkSystemBolierControlsNA.Text;
        string systemBoilerControlsComments1 = systemBoilerControlsComment.Text;
        bool boilerCasingSatisfactory1 = checkBoilerCasing.Text;
        bool boilerCasingSatisfactoryNA1 = checkBoilerCasingNA.Text;
        string boilerCasingComments1 = boilerCasingComment.Text;
        bool thermalInsulationSatisfactory1 = checkThermalInsulation.Text;
        bool thermalInsulationSatisfactoryNA1 = checkThermalInsulationNA.Text;
        string thermalInsulationComments1 = thermalInsulationComment.Text;
        bool combustionFanIdFanSatisfactory1 = checkCombustionFanIdFan.Text;
        bool combustionFanIdFanSatisfactoryNA1 = checkCombustionFanIdFanNA.Text;
        string combustionFanIdFanComments1 = combustionFanIdFanComment.Text;
        bool airFluePressureSwitchSatisfactory1 = checkAirFluePressureSwitch.Text;
        bool airFluePressureSwitchSatisfactoryNA1 = checkAirFluePressureSwitchNA.Text;
        string airFluePressureSwitchComments1 = airFluePressureSwitchComment.Text;
        bool controlLimitStatsSatisfactory1 = checkControlLimitStatus.Text;
        bool controlLimitStatsSatisfactoryNA1 = checkControlLimitStatusNA.Text;
        string controlLimitStatsComments1 = controlLimitStatusComment.Text;
        bool pressureTempGaugesSatisfactory1 = checkPressureTempGauges.Text;
        bool pressureTempGaugesSatisfactoryNA1 = checkPressureTempGaugesNA.Text;
        string pressureTempGaugesComments1 = pressureTempGaugesComment.Text;
        bool circulationPumpsSatisfactory1 = checkCirculationPumps.Text;
        bool circulationPumpsSatisfactoryNA1 = checkCirculationPumpsNA.Text;
        string circulationPumpsComments1 = circulationPumpsComment.Text;
        bool condenseTrapSatisfactory1 = checkCondenseTrap.Text;
        bool condenseTrapSatisfactoryNA1 = checkCondenseTrapNA.Text;
        string condenseTrapComments1 = condenseTrapComment.Text;
        bool heatExhanger1 = checkHeatExchangerFluewaysClear.Text;
        bool heatExhangerNA1 = checkHeatExchangerFluewaysClearNA.Text;
        string heatExhangerComments1 = heatExchangerFluewaysClearComment.Text;
        string workingInletPressure1 = workingIntelPressure.Text;
        string recordedBurnerPressure1 = recordedBurnerPressure.Text;
        string measuredGasRate1 = measuredGasRate.Text;
        bool flueFlowTest1 = checkFlueFlowTest.Text;
        bool flueFlowTestNA1 = checkFlueFlowTestNA.Text;
        string flueFlowTestComments1 = flueFlowTestComment.Text;
        bool spillageTest1 = checkSpillageTest.Text;
        bool spillageTestNA1 = checkSpillageTestNA.Text;
        string spillageTestComments1 = spillageTestComment.Text;
        bool AECVPlantIsolationCorrect1 = checkAECVPlantIsolationCorrect.Text;
        bool AECVPlantIsolationCorrectNA1 = checkAECVPlantIsolationCorrectNA.Text;
        string AECVPlantIsolationCorrectComments1 = AECVPlantIsolationCorrectComment.Text;
        bool safetyShutOffValve1 = checkSafetyShutOffValve.Text;
        bool safetyShutOffValveNA1 = checkSafetyShutOffValveNA.Text;
        string safetyShutOffValveComments1 = safetyShutOffValveComment.Text;
        bool plantroomGasTightnessTest1 = checkPlantroomGasTightnessTest.Text;
        bool plantroomGasTightnessTestNA1 = checkPlantroomGasTightnessTestNA.Text;
        string plantroomGasTightnessTestComments1 = plantroomGasTightnessTestComment.Text;
        string stateApplianceCondition1 = stateApplianceCondition.Text;
        string HighFireCO21 = highFireCO2.Text;
        string HighFireCO1 = highFireCO.Text;
        string HighFireO21 = highFireO2.Text;
        string HighFireRatio1 = highFireRatio.Text;
        string HighFireFlueTemp1 = highFireFlueTemp.Text;
        string HighFireEfficiency1 = highFireEfficiency.Text;
        string HighFireExcessAir1 = highFireExcessAir.Text;
        string HighFireRoomTemp1 = highFireRoomTemp.Text;
        string LowFireCO21 = lowFireCO2.Text;
        string LowFireCO1 = lowFireCO.Text;
        string LowFireO21 = lowFireO2.Text;
        string LowFireRatio1 = lowFireRatio.Text;
        string LowFireFlueTemp1 = lowFireFlueTemp.Text;
        string LowFireEfficiency1 = lowFireEfficiency.Text;
        string LowFireExcessAir1 = lowFireExcessAir.Text;
        string LowFireRoomTemp1 = lowFireRoomTemp.Text;
        string engineersName1 = engineersName.Text;
        string engineersSignature1 = engineersSignature.Text;
        string engineersGasSafeID1 = engineersGasSafeIDNumber.Text;
        string clientsName1 = clientsName.Text;
        string clientsSignature1 = clientsSignature.Text;
        string inspectionDate1 = inspectionDate.Text;
        string commetsDefects1 = additionalCommentsDefects.Text;
        string warningNoticeIssueNumber1 = warningNoticeNumber.Text;



        await PdfCreation.CreateServiceRecordPDF(string workingInletPressure1,
            string site1, string location1,
            string applianceNumber1,
string recordedBurnerPressure1,
string assetNumber1,
string measuredGasRate1,
bool heatExhanger1,
bool heatExhangerNA1,
string heatExhangerComments1,
bool flueFlowTest1,
bool flueFlowTestNA1,
string flueFlowTestComments1,
bool spillageTest1,
bool spillageTestNA1,
string spillageTestComments1,
bool safetyShutOffValve1,
bool safetyShutOffValveNA1,
string safetyShutOffValveComments1,
bool plantroomGasTightnessTest1,
bool plantroomGasTightnessTestNA1,
string plantroomGasTightnessTestComments1,
bool AECVPlantIsolationCorrect1,
bool AECVPlantIsolationCorrectNA1,
string AECVPlantIsolationCorrectComments1,
string stateApplianceConditionComments1,
string workingInletPressureComments1,
string recordedBurnerPressureComments1,
string measuredGasRateComments1,
bool testsCompleted1,
bool remedialWorkRequired1,
string applianceMake1,
string applianceModel1,
string applianceSerialNumber1,
string gcNumber1,
string stateApplianceCondition1,
string burnerMake1,
string burnerModel1,
string burnerSerialNumber1,
string Type1,
string Spec1,
bool OpenFlue1,
bool Roomsealed1,
bool ForcedDraft1,
bool Flueless1,
bool Heating1,
bool HotWater1,
bool Both1,
string badgedBurnerPressure1,
bool ventilationSatisfactory1,
string gasType1,
bool flueConditionSatisfactory1,
string approxAgeOfAppliance1,
string badgedInput1,
string badgedOutput1,
bool applianceServiceValveSatisfactory1,
bool governorsSatisfactory1,
bool gasSolenoidValvesSatisfactory1,
bool controlBoxPcbSatisfactory1,
bool gasketSealsSatisfactory1,
bool burnerSatisfactory1,
bool burnerJetsSatisfactory1,
bool electrodesTransformerSatisfactory1,
bool flameFailureDeviceSatisfactory1,
bool systemBoilerControlsSatisfactory1,
bool boilerCasingSatisfactory1,
bool thermalInsulationSatisfactory1,
bool combustionFanIdFanSatisfactory1,
bool airFluePressureSwitchSatisfactory1,
bool controlLimitStatsSatisfactory1,
bool pressureTempGaugesSatisfactory1,
bool circulationPumpsSatisfactory1,
bool condenseTrapSatisfactory1,
bool applianceServiceValveSatisfactoryNA1,
bool governorsSatisfactoryNA1,
bool gasSolenoidValvesSatisfactoryNA1,
bool controlBoxPcbSatisfactoryNA1,
bool gasketSealsSatisfactoryNA1,
bool burnerSatisfactoryNA1,
bool burnerJetsSatisfactoryNA1,
bool electrodesTransformerSatisfactoryNA1,
bool flameFailureDeviceSatisfactoryNA1,
bool systemBoilerControlsSatisfactoryNA1,
bool boilerCasingSatisfactoryNA1,
bool thermalInsulationSatisfactoryNA1,
bool combustionFanIdFanSatisfactoryNA1,
bool airFluePressureSwitchSatisfactoryNA1,
bool controlLimitStatsSatisfactoryNA1,
bool pressureTempGaugesSatisfactoryNA1,
bool circulationPumpsSatisfactoryNA1,
bool condenseTrapSatisfactoryNA1,
string gasSolenoidValvesComments1,
string controlBoxPcbComments1,
string gasketSealsComments1,
string burnerComments1,
string burnerJetsComments1,
string electrodesTransformerComments1,
string flameFailureDeviceComments1,
string systemBoilerControlsComments1,
string boilerCasingComments1,
string thermalInsulationComments1,
string combustionFanIdFanComments1,
string airFluePressureSwitchComments1,
string controlLimitStatsComments1,
string pressureTempGaugesComments1,
string circulationPumpsComments1,
string condenseTrapComments1,
string HighFireCO21,
string HighFireCO1,
string HighFireO21,
string HighFireFlueTemp1,
string HighFireEfficiency1,
string HighFireExcessAir1,
string HighFireRoomTemp1,
string HighFireRatio1,
string LowFireCO21,
string LowFireCO1,
string LowFireO21,
string LowFireFlueTemp1,
string LowFireEfficiency1,
string LowFireExcessAir1,
string LowFireRoomTemp1,
string LowFireRatio1,
string warningNoticeIssueNumber1,
string engineersName1,
string engineersSignature1,
string engineersGasSafeID1,
string clientsName1,
string clientsSignature1,
string inspectionDate1,
        string commetsDefects1
        );
    }
}