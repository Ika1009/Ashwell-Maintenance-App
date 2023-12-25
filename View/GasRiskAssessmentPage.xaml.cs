namespace Ashwell_Maintenance.View;

public partial class GasRiskAssessmentPage : ContentPage
{
	public GasRiskAssessmentPage()
	{
		InitializeComponent();
    }

    
    public async void GasRiskAssessmentBack(object sender, EventArgs e)
	{
		if (GRASection1.IsVisible)
        {
            GasRiskAssessmentBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (GRASection2.IsVisible)
		{
			GRASection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await GRASection1.ScrollToAsync(0, 0, false);
			GRASection1.IsVisible = true;
		}
		else if (GRASection3.IsVisible)
		{
            GRASection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await GRASection2.ScrollToAsync(0, 0, false);
            GRASection2.IsVisible = true;
        }
	}

    
    public async void GasRiskAssessmentNext1(object sender, EventArgs e)
	{
		GRASection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await GRASection2.ScrollToAsync(0, 0, false);
		GRASection2.IsVisible = true;
	}
    
    public async void GasRiskAssessmentNext2(object sender, EventArgs e)
    {
        GRASection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await GRASection3.ScrollToAsync(0, 0, false);
        GRASection3.IsVisible = true;
    }

    public void GasRiskAssessmentNext3(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string reportName = $"Ashwell_EngineersReport{dateTimeString}.pdf";

        PdfCreation.GasRisk(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());\
        reportData.Add("nameAndSiteAdress", nameAndSiteAdress.Text ?? string.Empty);
        reportData.Add("client", client.Text ?? string.Empty);
        reportData.Add("meterLocation", meterLocation.Text ?? string.Empty);
        reportData.Add("commentsOnOverallMeter", commentsOnOverallMeter.Text ?? string.Empty);
        reportData.Add("meterComments", meterComments.Text ?? string.Empty);
        reportData.Add("pipeworkLocation", pipeworkLocation.Text ?? string.Empty);
        reportData.Add("commentsOnOverallPipework", commentsOnOverallPipework.Text ?? string.Empty);
        reportData.Add("pipeworkComments", pipeworkComments.Text ?? string.Empty);
        reportData.Add("reasonForWarningNotice", reasonForWarningNotice.Text ?? string.Empty);
        reportData.Add("warningNoticeRefNo", warningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("dateOfLastTightnessTest", dateOfLastTightnessTest.Text ?? string.Empty);
        reportData.Add("recordTightnessTestResult", recordTightnessTestResult.Text ?? string.Empty);
        reportData.Add("dropRecorded", dropRecorded.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        //reportData.Add("engineersSignature", engineersSignature.Text ?? string.Empty);
        reportData.Add("gasSafeOperativeIdNo", gasSafeOperativeIdNo.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        //reportData.Add("clientsSignature", clientsSignature.Text ?? string.Empty);
        reportData.Add("completionDate", completionDate.Text ?? string.Empty);





        reportData.Add("checkInternalMeter", checkInternalMeter.IsChecked.ToString());
        reportData.Add("checkExternalMeter", checkExternalMeter.IsChecked.ToString());
        reportData.Add("checkGeneralMeterConditionYes", checkGeneralMeterConditionYes.IsChecked.ToString());
        reportData.Add("checkGeneralMeterConditionNo", checkGeneralMeterConditionNo.IsChecked.ToString());
        reportData.Add("checkEarthBondingYes", checkEarthBondingYes.IsChecked.ToString());
        reportData.Add("checkEarthBondingNo", checkEarthBondingNo.IsChecked.ToString());
        reportData.Add("checkEmergencyControlsYes", checkEmergencyControlsYes.IsChecked.ToString());
        reportData.Add("checkEmergencyControlsNo", checkEmergencyControlsNo.IsChecked.ToString());
        reportData.Add("checkMeterVentilationYes", checkMeterVentilationYes.IsChecked.ToString());
        reportData.Add("checkMeterVentilationNo", checkMeterVentilationNo.IsChecked.ToString());
        reportData.Add("checkGasLineDiagramYes", checkGasLineDiagramYes.IsChecked.ToString());
        reportData.Add("checkGasLineDiagramNo", checkGasLineDiagramNo.IsChecked.ToString());
        reportData.Add("checkEmergencyContractNumberYes", checkEmergencyContractNumberYes.IsChecked.ToString());
        reportData.Add("checkEmergencyContractNumberNo", checkEmergencyContractNumberNo.IsChecked.ToString());
        reportData.Add("checkNoticesAndLabelsYes", checkNoticesAndLabelsYes.IsChecked.ToString());
        reportData.Add("checkNoticesAndLabelsNo", checkNoticesAndLabelsNo.IsChecked.ToString());
        reportData.Add("checkPipeworkIdentifiedYes", checkPipeworkIdentifiedYes.IsChecked.ToString());
        reportData.Add("checkPipeworkIdentifiedNo", checkPipeworkIdentifiedNo.IsChecked.ToString());
        reportData.Add("checkPipeworkBuriedYes", checkPipeworkBuriedYes.IsChecked.ToString());
        reportData.Add("checkPipeworkBuriedNo", checkPipeworkBuriedNo.IsChecked.ToString());
        reportData.Add("checkPipeworkSurfaceYes", checkPipeworkSurfaceYes.IsChecked.ToString());
        reportData.Add("checkPipeworkSurfaceNo", checkPipeworkSurfaceNo.IsChecked.ToString());
        reportData.Add("checkPipeworkEarthBondingYes", checkPipeworkEarthBondingYes.IsChecked.ToString());
        reportData.Add("checkPipeworkEarthBondingNo", checkPipeworkEarthBondingNo.IsChecked.ToString());
        reportData.Add("checkJointingMethodsYes", checkJointingMethodsYes.IsChecked.ToString());
        reportData.Add("checkJointingMethodsNo", checkJointingMethodsNo.IsChecked.ToString());
        reportData.Add("checkPipeworkSupportsYes", checkPipeworkSupportsYes.IsChecked.ToString());
        reportData.Add("checkPipeworkSupportsNo", checkPipeworkSupportsNo.IsChecked.ToString());
        reportData.Add("checkFixingsYes", checkFixingsYes.IsChecked.ToString());
        reportData.Add("checkFixingsNo", checkFixingsNo.IsChecked.ToString());
        reportData.Add("checkSupportSepparationDistancesYes", checkSupportSepparationDistancesYes.IsChecked.ToString());
        reportData.Add("checkSupportSepparationDistancesNo", checkSupportSepparationDistancesNo.IsChecked.ToString());
        reportData.Add("checkPipeworkInVoidsYes", checkPipeworkInVoidsYes.IsChecked.ToString());
        reportData.Add("checkPipeworkInVoidsNo", checkPipeworkInVoidsNo.IsChecked.ToString());
        reportData.Add("checkPipeSleevesYes", checkPipeSleevesYes.IsChecked.ToString());
        reportData.Add("checkPipeSleevesNo", checkPipeSleevesNo.IsChecked.ToString());
        reportData.Add("checkPipeSleevesSealedYes", checkPipeSleevesSealedYes.IsChecked.ToString());
        reportData.Add("checkPipeSleevesSealedNo", checkPipeSleevesSealedNo.IsChecked.ToString());
        reportData.Add("checkServiceValvesYes", checkServiceValvesYes.IsChecked.ToString());
        reportData.Add("checkServiceValvesNo", checkServiceValvesNo.IsChecked.ToString());
        reportData.Add("checkAdditionalEmergencyControlValvesYes", checkAdditionalEmergencyControlValvesYes.IsChecked.ToString());
        reportData.Add("checkAdditionalEmergencyControlValvesNo", checkAdditionalEmergencyControlValvesNo.IsChecked.ToString());
        reportData.Add("checkIsolationValveYes", checkIsolationValveYes.IsChecked.ToString());
        reportData.Add("checkIsolationValveNo", checkIsolationValveNo.IsChecked.ToString());
        reportData.Add("checkTestPointYes", checkTestPointYes.IsChecked.ToString());
        reportData.Add("checkTestPointNo", checkTestPointNo.IsChecked.ToString());
        reportData.Add("checkPurgePointsYes", checkPurgePointsYes.IsChecked.ToString());
        reportData.Add("checkPurgePointsNo", checkPurgePointsNo.IsChecked.ToString());
        reportData.Add("checkGeneralPipeworkConditionYes", checkGeneralPipeworkConditionYes.IsChecked.ToString());
        reportData.Add("checkGeneralPipeworkConditionNo", checkGeneralPipeworkConditionNo.IsChecked.ToString());
        reportData.Add("checkinstallationSafeToOperateYes", checkinstallationSafeToOperateYes.IsChecked.ToString());
        reportData.Add("checkinstallationSafeToOperateNo", checkinstallationSafeToOperateNo.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssuedYes", checkWarningNoticeIssuedYes.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssuedNo", checkWarningNoticeIssuedNo.IsChecked.ToString());
        reportData.Add("checkGasTightnessTestRecommendedYes", checkGasTightnessTestRecommendedYes.IsChecked.ToString());
        reportData.Add("checkGasTightnessTestRecommendedNo", checkGasTightnessTestRecommendedNo.IsChecked.ToString());
        reportData.Add("checkGuessTightnessTestCarriedOutYes", checkGuessTightnessTestCarriedOutYes.IsChecked.ToString());
        reportData.Add("checkGuessTightnessTestCarriedOutNo", checkGuessTightnessTestCarriedOutNo.IsChecked.ToString());




        return reportData;
    }
}