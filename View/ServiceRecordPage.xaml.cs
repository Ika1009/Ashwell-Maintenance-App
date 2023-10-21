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
			Navigation.PopAsync();
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
        bool testsCompleted1 = checkTestsCompleted.IsChecked;
        bool remedialWorkRequired1 = checRemedialWorkRequired.IsChecked;
        string applianceMake1 = applianceMake.Text;
        string applianceModel1 = applianceModel.Text;
        string applianceSerialNumber1 = applianceSerialNo.Text;
        string gcNumber1 = GCNo.Text;
        bool Heating1 = checkHeating.IsChecked;
        bool HotWater1 = checkHotWater.IsChecked;
        bool Both1 = checkBoth.IsChecked;
        string approxAgeOfAppliance1 = years.Text;
        string badgedInput1 = badgedInput.Text;
        string badgedOutput1 = budgedOutput.Text;
        string burnerMake1 = burnerMake.Text;
        string burnerModel1 = burnerModel.Text;
        string burnerSerialNumber1 = burnerSerialNo.Text;
        string Type1 = type.Text;
        string Spec1 = spec.Text;
        bool OpenFlue1 = checkOpenFlue.IsChecked;
        bool Roomsealed1 = checkRoomSealed.IsChecked;
        bool ForcedDraft1 = checkForcedDraft.IsChecked;
        bool Flueless1 = checkFlueless.IsChecked;
        string badgedBurnerPressure1 = badgedBurnerPressure.Text;
        bool ventilationSatisfactory1 = checkVentilationSatisfactory.IsChecked;
        bool flueConditionSatisfactory1 = checkFlueConditionSatisfactory.IsChecked;
        bool tempNG= checkNG.IsChecked;
        bool tempLPG= checkLPG.IsChecked;
        bool applianceServiceValveSatisfactory1 = checkAppServiceValve.IsChecked;
        bool applianceServiceValveSatisfactoryNA1 = checkAppServiceValveNA.IsChecked;
        string applianceServiceValveSatisfactoryComments1 = applianceServiceValveComment.Text;
        bool governorsSatisfactory1 = checkGovernors.IsChecked;
        bool governorsSatisfactoryNA1 = checkGovernorsNA.IsChecked;
        string governorsComments1 = governorsComment.Text;
        bool gasSolenoidValvesSatisfactory1 = checkGasSolenoidValves.IsChecked;
        bool gasSolenoidValvesSatisfactoryNA1 = checkGasSolenoidValvesNA.IsChecked;
        string gasSolenoidValvesComments1 = gasSolenoidValvesComment.Text;
        bool controlBoxPcbSatisfactory1 = checkControlBoxPCB.IsChecked;
        bool controlBoxPcbSatisfactoryNA1 = checkControlBoxPCBNA.IsChecked;
        string controlBoxPcbComments1 = controlBoxPCBComment.Text;
        bool gasketSealsSatisfactory1 = checkGasketSeals.IsChecked;
        bool gasketSealsSatisfactoryNA1 = checkGasketSealsNA.IsChecked;
        string gasketSealsComments1 = gasketSealsComment.Text;
        bool burnerSatisfactory1 = checkBurner.IsChecked;
        bool burnerSatisfactoryNA1 = checkBurnerNA.IsChecked;
        string burnerComments1 = burnerComment.Text;
        bool burnerJetsSatisfactory1 = checkBurnerJets.IsChecked;
        bool burnerJetsSatisfactoryNA1 = checkBurnerJetsNA.IsChecked;
        string burnerJetsComments1 = burnerJetsComment.Text;
        bool electrodesTransformerSatisfactory1 = checkElectrodesTransformer.IsChecked;
        bool electrodesTransformerSatisfactoryNA1 = checkElectrodesTransformerNA.IsChecked;
        string electrodesTransformerComments1 = electrodesTransformerComment.Text;
        bool flameFailureDeviceSatisfactory1 = checkFlameFailureDevice.IsChecked;
        bool flameFailureDeviceSatisfactoryNA1 = checkFlameFailureDeviceNA.IsChecked;
        string flameFailureDeviceComments1 = flameFailureDeviceComment.Text;
        bool systemBoilerControlsSatisfactory1 = checkSystemBoilerControls.IsChecked;
        bool systemBoilerControlsSatisfactoryNA1 = checkSystemBolierControlsNA.IsChecked;
        string systemBoilerControlsComments1 = systemBoilerControlsComment.Text;
        bool boilerCasingSatisfactory1 = checkBoilerCasing.IsChecked;
        bool boilerCasingSatisfactoryNA1 = checkBoilerCasingNA.IsChecked;
        string boilerCasingComments1 = boilerCasingComment.Text;
        bool thermalInsulationSatisfactory1 = checkThermalInsulation.IsChecked;
        bool thermalInsulationSatisfactoryNA1 = checkThermalInsulationNA.IsChecked;
        string thermalInsulationComments1 = thermalInsulationComment.Text;
        bool combustionFanIdFanSatisfactory1 = checkCombustionFanIdFan.IsChecked;
        bool combustionFanIdFanSatisfactoryNA1 = checkCombustionFanIdFanNA.IsChecked;
        string combustionFanIdFanComments1 = combustionFanIdFanComment.Text;
        bool airFluePressureSwitchSatisfactory1 = checkAirFluePressureSwitch.IsChecked;
        bool airFluePressureSwitchSatisfactoryNA1 = checkAirFluePressureSwitchNA.IsChecked;
        string airFluePressureSwitchComments1 = airFluePressureSwitchComment.Text;
        bool controlLimitStatsSatisfactory1 = checkControlLimitStatus.IsChecked;
        bool controlLimitStatsSatisfactoryNA1 = checkControlLimitStatusNA.IsChecked;
        string controlLimitStatsComments1 = controlLimitStatusComment.Text;
        bool pressureTempGaugesSatisfactory1 = checkPressureTempGauges.IsChecked;
        bool pressureTempGaugesSatisfactoryNA1 = checkPressureTempGaugesNA.IsChecked;
        string pressureTempGaugesComments1 = pressureTempGaugesComment.Text;
        bool circulationPumpsSatisfactory1 = checkCirculationPumps.IsChecked;
        bool circulationPumpsSatisfactoryNA1 = checkCirculationPumpsNA.IsChecked;
        string circulationPumpsComments1 = circulationPumpsComment.Text;
        bool condenseTrapSatisfactory1 = checkCondenseTrap.IsChecked;
        bool condenseTrapSatisfactoryNA1 = checkCondenseTrapNA.IsChecked;
        string condenseTrapComments1 = condenseTrapComment.Text;
        bool heatExhanger1 = checkHeatExchangerFluewaysClear.IsChecked;
        bool heatExhangerNA1 = checkHeatExchangerFluewaysClearNA.IsChecked;
        string heatExhangerComments1 = heatExchangerFluewaysClearComment.Text;
        string workingInletPressure1 = workingIntelPressure.Text;
        string recordedBurnerPressure1 = recordedBurnerPressure.Text;
        string measuredGasRate1 = measuredGasRate.Text;
        bool flueFlowTest1 = checkFlueFlowTest.IsChecked;
        bool flueFlowTestNA1 = checkFlueFlowTestNA.IsChecked;
        string flueFlowTestComments1 = flueFlowTestComment.Text;
        bool spillageTest1 = checkSpillageTest.IsChecked;
        bool spillageTestNA1 = checkSpillageTestNA.IsChecked;
        string spillageTestComments1 = spillageTestComment.Text;
        bool AECVPlantIsolationCorrect1 = checkAECVPlantIsolationCorrect.IsChecked;
        bool AECVPlantIsolationCorrectNA1 = checkAECVPlantIsolationCorrectNA.IsChecked;
        string AECVPlantIsolationCorrectComments1 = AECVPlantIsolationCorrectComment.Text;
        bool safetyShutOffValve1 = checkSafetyShutOffValve.IsChecked;
        bool safetyShutOffValveNA1 = checkSafetyShutOffValveNA.IsChecked;
        string safetyShutOffValveComments1 = safetyShutOffValveComment.Text;
        bool plantroomGasTightnessTest1 = checkPlantroomGasTightnessTest.IsChecked;
        bool plantroomGasTightnessTestNA1 = checkPlantroomGasTightnessTestNA.IsChecked;
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
        string gasType1 = "";
        if (tempNG)
        {
            gasType1 = "NG";
        }
        else if (tempLPG)
        {
            gasType1 = "LPG";
        }


        await PdfCreation.CreateServiceRecordPDF(workingInletPressure1,
             site1,location1,
            applianceNumber1,
 recordedBurnerPressure1,
 assetNumber1,
 measuredGasRate1,
 heatExhanger1,
 heatExhangerNA1,
 heatExhangerComments1,
 flueFlowTest1,
 flueFlowTestNA1,
 flueFlowTestComments1,
 spillageTest1,
 spillageTestNA1,
 spillageTestComments1,
 safetyShutOffValve1,
 safetyShutOffValveNA1,
 safetyShutOffValveComments1,
 plantroomGasTightnessTest1,
 plantroomGasTightnessTestNA1,
 plantroomGasTightnessTestComments1,
 AECVPlantIsolationCorrect1,
 AECVPlantIsolationCorrectNA1,
 AECVPlantIsolationCorrectComments1,
"",// stateApplianceConditionComments1,
"",// workingInletPressureComments1,
"",// recordedBurnerPressureComments1,
"",// measuredGasRateComments1,
 testsCompleted1,
 remedialWorkRequired1,
 applianceMake1,
 applianceModel1,
 applianceSerialNumber1,
 gcNumber1,
 stateApplianceCondition1,
 burnerMake1,
 burnerModel1,
 burnerSerialNumber1,
 Type1,
 Spec1,
 OpenFlue1,
 Roomsealed1,
 ForcedDraft1,
 Flueless1,
 Heating1,
 HotWater1,
 Both1,
 badgedBurnerPressure1,
 ventilationSatisfactory1,
 gasType1,
 flueConditionSatisfactory1,
 approxAgeOfAppliance1,
 badgedInput1,
 badgedOutput1,
 applianceServiceValveSatisfactory1,
 governorsSatisfactory1,
 gasSolenoidValvesSatisfactory1,
 controlBoxPcbSatisfactory1,
 gasketSealsSatisfactory1,
 burnerSatisfactory1,
 burnerJetsSatisfactory1,
 electrodesTransformerSatisfactory1,
 flameFailureDeviceSatisfactory1,
 systemBoilerControlsSatisfactory1,
 boilerCasingSatisfactory1,
 thermalInsulationSatisfactory1,
 combustionFanIdFanSatisfactory1,
 airFluePressureSwitchSatisfactory1,
 controlLimitStatsSatisfactory1,
 pressureTempGaugesSatisfactory1,
 circulationPumpsSatisfactory1,
 condenseTrapSatisfactory1,
 applianceServiceValveSatisfactoryNA1,
 governorsSatisfactoryNA1,
 gasSolenoidValvesSatisfactoryNA1,
 controlBoxPcbSatisfactoryNA1,
 gasketSealsSatisfactoryNA1,
 burnerSatisfactoryNA1,
 burnerJetsSatisfactoryNA1,
 electrodesTransformerSatisfactoryNA1,
 flameFailureDeviceSatisfactoryNA1,
 systemBoilerControlsSatisfactoryNA1,
 boilerCasingSatisfactoryNA1,
 thermalInsulationSatisfactoryNA1,
 combustionFanIdFanSatisfactoryNA1,
 airFluePressureSwitchSatisfactoryNA1,
 controlLimitStatsSatisfactoryNA1,
 pressureTempGaugesSatisfactoryNA1,
 circulationPumpsSatisfactoryNA1,
 condenseTrapSatisfactoryNA1,
 gasSolenoidValvesComments1,
 controlBoxPcbComments1,
 gasketSealsComments1,
 burnerComments1,
 burnerJetsComments1,
 electrodesTransformerComments1,
 flameFailureDeviceComments1,
 systemBoilerControlsComments1,
 boilerCasingComments1,
 thermalInsulationComments1,
 combustionFanIdFanComments1,
 airFluePressureSwitchComments1,
 controlLimitStatsComments1,
 pressureTempGaugesComments1,
 circulationPumpsComments1,
 condenseTrapComments1,
 HighFireCO21,
 HighFireCO1,
 HighFireO21,
 HighFireFlueTemp1,
 HighFireEfficiency1,
 HighFireExcessAir1,
 HighFireRoomTemp1,
 HighFireRatio1,
 LowFireCO21,
 LowFireCO1,
 LowFireO21,
 LowFireFlueTemp1,
 LowFireEfficiency1,
 LowFireExcessAir1,
 LowFireRoomTemp1,
 LowFireRatio1,
 warningNoticeIssueNumber1,
 engineersName1,
 engineersSignature1,
 engineersGasSafeID1,
 clientsName1,
 clientsSignature1,
 inspectionDate1,
     commetsDefects1,
     applianceServiceValveSatisfactoryComments1,
     governorsComments1
    );
    }
}