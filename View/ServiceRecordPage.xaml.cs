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
        //string site=site.Text;
        //string location=location.Text
        //string assetNumber=assetNo.Text;
        //string applianceNumber= applianceNo.Text;
        //bool testsCompleted= checkTestsCompleted.Text;
        //string remedialWorkRequired= checRemedialWorkRequired.Text;
        //string applianceMake= applianceMake.Text;
        //string applianceModel= applianceModel.Text;
        //string applianceSerialNumber= applianceSerialNo.Text;
        //string gcNumber= GCNo.Text;
        //bool Heating= checkHeating.Text;
        //bool HotWater= checkHotWater.Text;
        //bool Both= checkBoth.Text;
        //string approxAgeOfAppliance= years.Text;
        //string badgedInput= badgedInput.Text;
        //string badgedOutput= budgedOutput.Text;
        //string burnerMake= burnerMake.Text;
        //string burnerModel= burnerModel.Text;
        //string burnerSerialNumber= burnerSerialNo.Text;
        //string Type= type.Text;
        //string Spec= spec.Text;
        //bool OpenFlue= checkOpenFlue.Text;
        //bool Roomseale= checkRoomSealed.Text;
        //bool ForcedDraft= checkForcedDraft.Text;
        //bool Flueless= checkFlueless.Text;
        //string badgedBurnerPressure= badgedBurnerPressure.Text;
        //bool ventilationSatisfactory= checkVentilationSatisfactory.Text;
        //bool flueConditionSatisfactory= checkFlueConditionSatisfactory.Text;
        ////= checkNG.Text;
        ////= checkLPG.Text;
        //bool applianceServiceValveSatisfactory= checkAppServiceValve.Text;
        //bool applianceServiceValveSatisfactoryNA= checkAppServiceValveNA.Text;
        //string applianceServiceValveSatisfactoryComments= applianceServiceValveComment.Text;
        //bool governorsSatisfactory= checkGovernors.Text;
        //bool governorsSatisfactoryNA= checkGovernorsNA.Text;
        //string governorsSatisfactoryComments= governorsComment.Text;
        //bool gasSolenoidValvesSatisfactory= checkGasSolenoidValves.Text;
        //bool gasSolenoidValvesSatisfactoryNA= checkGasSolenoidValvesNA.Text;
        //string gasSolenoidValvesSatisfactoryComments= gasSolenoidValvesComment.Text;
        //bool controlBoxPcbSatisfactory= checkControlBoxPCB.Text;
        //bool controlBoxPcbSatisfactoryNA= checkControlBoxPCBNA.Text;
        //string controlBoxPcbSatisfactoryComments= controlBoxPCBComment.Text;
        //bool gasketSealsSatisfactory= checkGasketSeals.Text;
        //bool gasketSealsSatisfactoryNA= checkGasketSealsNA.Text;
        //string gasketSealsSatisfactoryComments= gasketSealsComment.Text;
        //bool burnerSatisfactory= checkBurner.Text;
        //bool burnerSatisfactoryNA= checkBurnerNA.Text;
        //string burnerSatisfactoryComments = burnerComment.Text;
        //bool burnerJetsSatisfactory= checkBurnerJets.Text;
        //bool burnerJetsSatisfactoryNA= checkBurnerJetsNA.Text;
        //string burnerJetsSatisfactoryComments= burnerJetsComment.Text;
        //bool electrodesTransformerSatisfactory= checkElectrodesTransformer.Text;
        //bool electrodesTransformerSatisfactoryNA= checkElectrodesTransformerNA.Text;
        //string electrodesTransformerSatisfactoryComments= electrodesTransformerComment.Text;
        //bool flameFailureDeviceSatisfactory= checkFlameFailureDevice.Text;
        //bool flameFailureDeviceSatisfactoryNA= checkFlameFailureDeviceNA.Text;
        //string flameFailureDeviceSatisfactoryComments= flameFailureDeviceComment.Text;
        //bool systemBoilerControlsSatisfactory= checkSystemBoilerControls.Text;
        //bool systemBoilerControlsSatisfactoryNA= checkSystemBolierControlsNA.Text;
        //string systemBoilerControlsSatisfactoryComments= systemBoilerControlsComment.Text;
        //bool boilerCasingSatisfactory= checkBoilerCasing.Text;
        //bool boilerCasingSatisfactoryNA= checkBoilerCasingNA.Text;
        //string boilerCasingSatisfactoryComments= boilerCasingComment.Text;
        //bool thermalInsulationSatisfactory= checkThermalInsulation.Text;
        //bool thermalInsulationSatisfactoryNA= checkThermalInsulationNA.Text;
        //string thermalInsulationSatisfactoryComments= thermalInsulationComment.Text;
        //bool combustionFanIdFanSatisfactory= checkCombustionFanIdFan.Text;
        //bool combustionFanIdFanSatisfactoryNA= checkCombustionFanIdFanNA.Text;
        //string combustionFanIdFanSatisfactoryComments= combustionFanIdFanComment.Text;
        //bool airFluePressureSwitchSatisfactory= checkAirFluePressureSwitch.Text;
        //bool airFluePressureSwitchSatisfactoryNA= checkAirFluePressureSwitchNA.Text;
        //string airFluePressureSwitchSatisfactoryComments= airFluePressureSwitchComment.Text;
        //bool controlLimitStatsSatisfactory= checkControlLimitStatus.Text;
        //bool controlLimitStatsSatisfactoryNA= checkControlLimitStatusNA.Text;
        //string controlLimitStatsSatisfactoryComments= controlLimitStatusComment.Text;
        //bool pressureTempGaugesSatisfactory= checkPressureTempGauges.Text;
        //bool pressureTempGaugesSatisfactoryNA= checkPressureTempGaugesNA.Text;
        //string pressureTempGaugesSatisfactoryComments= pressureTempGaugesComment.Text;
        //bool circulationPumpsSatisfactory= checkCirculationPumps.Text;
        //bool circulationPumpsSatisfactoryNA= checkCirculationPumpsNA.Text;
        //string circulationPumpsSatisfactoryComments= circulationPumpsComment.Text;
        //bool condenseTrapSatisfactory= checkCondenseTrap.Text;
        //bool condenseTrapSatisfactoryNA= checkCondenseTrapNA.Text;
        //string condenseTrapSatisfactoryComments= condenseTrapComment.Text;
        //bool heatExhanger= checkHeatExchangerFluewaysClear.Text;
        //bool heatExhangerNA= checkHeatExchangerFluewaysClearNA.Text;
        //string heatExhangerComments= heatExchangerFluewaysClearComment.Text;
        //string workingInletPressure = workingIntelPressure.Text;
        //string recordedBurnerPressure= recordedBurnerPressure.Text;
        //string measuredGasRate= measuredGasRate.Text;
        //bool flueFlowTest= checkFlueFlowTest.Text;
        //bool flueFlowTestNA= checkFlueFlowTestNA.Text;
        //string flueFlowTestComments= flueFlowTestComment.Text;
        //bool spillageTest= checkSpillageTest.Text;
        //bool spillageTestNA= checkSpillageTestNA.Text;
        //string spillageTestComments= spillageTestComment.Text;
        //bool AECVPlantIsolationCorrect= checkAECVPlantIsolationCorrect.Text;
        //bool AECVPlantIsolationCorrectNA= checkAECVPlantIsolationCorrectNA.Text;
        //string AECVPlantIsolationCorrectComments= AECVPlantIsolationCorrectComment.Text;
        //bool safetyShutOffValve= checkSafetyShutOffValve.Text;
        //bool safetyShutOffValveNA= checkSafetyShutOffValveNA.Text;
        //string safetyShutOffValveComments= safetyShutOffValveComment.Text;
        //bool plantroomGasTightnessTest= checkPlantroomGasTightnessTest.Text;
        //bool plantroomGasTightnessTestNA= checkPlantroomGasTightnessTestNA.Text;
        //string plantroomGasTightnessTestComments= plantroomGasTightnessTestComment.Text;
        //string stateApplianceCondition= stateApplianceCondition.Text;
        //string HighFireCO2= highFireCO2.Text;
        //string HighFireCO= highFireCO.Text;
        //string HighFireO2= highFireO2.Text;
        //string HighFireRatio= highFireRatio.Text;
        //string HighFireFlueTemp= highFireFlueTemp.Text;
        //string HighFireEfficiency= highFireEfficiency.Text;
        //string HighFireExcessAir= highFireExcessAir.Text;
        //string HighFireRoomTemp= highFireRoomTemp.Text;
        //string LowFireCO2= lowFireCO2.Text;
        //string LowFireCO= lowFireCO.Text;
        //string LowFireO2= lowFireO2.Text;
        //string LowFireRatio= lowFireRatio.Text;
        //string LowFireFlueTemp= lowFireFlueTemp.Text;
        //string LowFireEfficiency= lowFireEfficiency.Text;
        //string LowFireExcessAir= lowFireExcessAir.Text;
        //string LowFireRoomTemp= lowFireRoomTemp.Text;
        //string engineersName= engineersName.Text;
        //string engineersSignature= engineersSignature.Text;
        //string engineersGasSafeID= engineersGasSafeIDNumber.Text;
        //string clientsName= clientsName.Text;
        //string clientsSignature= clientsSignature.Text;
        //string inspectionDate= inspectionDate.Text;
        //string commetsDefects= additionalCommentsDefects.Text;
        //string warningNoticeIssueNumber= warningNoticeNumber.Text;



        //string siteEntryText = siteEntry.Text;
        //await PdfCreation.CreateServiceRecordPDF();
    }
}