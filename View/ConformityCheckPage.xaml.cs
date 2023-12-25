namespace Ashwell_Maintenance.View;

public partial class ConformityCheckPage : ContentPage
{
	public ConformityCheckPage()
	{
		InitializeComponent();
	}

    
    public async void ConformityCheckBack(object sender, EventArgs e)
	{
		if (CCSection1.IsVisible)
        {
            ConformityCheckBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (CCSection2.IsVisible)
		{
			CCSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection1.ScrollToAsync(0, 0, false);
			CCSection1.IsVisible = true;
		}
		else if (CCSection3.IsVisible)
		{
            CCSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection2.ScrollToAsync(0, 0, false);
            CCSection2.IsVisible = true;
        }
		else if (CCSection4.IsVisible)
		{
            CCSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection3.ScrollToAsync(0, 0, false);
            CCSection3.IsVisible = true;
        }
        else
        {
            CCSection5.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CCSection4.ScrollToAsync(0, 0, false);
            CCSection4.IsVisible = true;
        }
	}

    
    public async void CCNext1(object sender, EventArgs e)
	{
		CCSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection2.ScrollToAsync(0, 0, false);
		CCSection2.IsVisible = true;
	}
    
    public async void CCNext2(object sender, EventArgs e)
    {
        CCSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection3.ScrollToAsync(0, 0, false);
        CCSection3.IsVisible = true;
    }
    
    public async void CCNext3(object sender, EventArgs e)
    {
        CCSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection4.ScrollToAsync(0, 0, false);
        CCSection4.IsVisible = true;
    }
    
    public async void CCNext4(object sender, EventArgs e)
    {
        CCSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CCSection5.ScrollToAsync(0, 0, false);
        CCSection5.IsVisible = true;
    }
    public void CCNext5(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string reportName = $"Ashwell_Engineers_Report_{dateTimeString}.pdf";

        PdfCreation.CheckPage(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("uern", uern.Text ?? string.Empty);
        reportData.Add("SheetNo", SheetNo.Text ?? string.Empty);
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("nameAndAddressOfPremises", nameAndAddressOfPremises.Text ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        reportData.Add("ventilationCalculations", ventilationCalculations.Text ?? string.Empty);
        reportData.Add("existingHighLevel", existingHighLevel.Text ?? string.Empty);
        reportData.Add("existingLowLevel", existingLowLevel.Text ?? string.Empty);
        reportData.Add("requiredHighLevel", requiredHighLevel.Text ?? string.Empty);
        reportData.Add("requiredLowLevel", requiredLowLevel.Text ?? string.Empty);
        reportData.Add("ventilationChecksComments", ventilationChecksComments.Text ?? string.Empty);
        reportData.Add("flueChecksComments", flueChecksComments.Text ?? string.Empty);
        reportData.Add("emergencyStopButtonComment", emergencyStopButtonComment.Text ?? string.Empty);
        reportData.Add("safetyInterlocksComments", safetyInterlocksComments.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("contractor", contractor.Text ?? string.Empty);
        reportData.Add("companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Text ?? string.Empty);
        reportData.Add("engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        


        reportData.Add("checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString());
        reportData.Add("checkExistingHighLevelCM", checkExistingHighLevelCM.IsChecked.ToString());
        reportData.Add("checkExistingLowLevelCM", checkExistingLowLevelCM.IsChecked.ToString());
        reportData.Add("checkRequiredHighLevelCM", checkRequiredHighLevelCM.IsChecked.ToString());
        reportData.Add("checkRequiredLowLevelCM", checkRequiredLowLevelCM.IsChecked.ToString());
        reportData.Add("checkVentilationCorrectlySizedYes", checkVentilationCorrectlySizedYes.IsChecked.ToString());
        reportData.Add("checkVentilationAtTheCorrectHeightYes", checkVentilationAtTheCorrectHeightYes.IsChecked.ToString());
        reportData.Add("checkVentilationArrangementsYes", checkVentilationArrangementsYes.IsChecked.ToString());
        reportData.Add("checkA_ID1", checkA_ID1.IsChecked.ToString());
        reportData.Add("checkB_AR1", checkB_AR1.IsChecked.ToString());
        reportData.Add("checkC_NCS1", checkC_NCS1.IsChecked.ToString());
        reportData.Add("checkFluesFittedYes", checkFluesFittedYes.IsChecked.ToString());
        reportData.Add("checkFluesFittedNo", checkFluesFittedNo.IsChecked.ToString());
        reportData.Add("checkFluesSupportedYes", checkFluesSupportedYes.IsChecked.ToString());
        reportData.Add("checkFluesSupportedNo", checkFluesSupportedNo.IsChecked.ToString());
        reportData.Add("checkFluesInLineYes", checkFluesInLineYes.IsChecked.ToString());
        reportData.Add("checkFluesInLineNo", checkFluesInLineNo.IsChecked.ToString());
        reportData.Add("checkFacilitiesYes", checkFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkFacilitiesNo", checkFacilitiesNo.IsChecked.ToString());
        reportData.Add("checkFlueGradientsYes", checkFlueGradientsYes.IsChecked.ToString());
        reportData.Add("checkFlueGradientsNo", checkFlueGradientsNo.IsChecked.ToString());
        reportData.Add("checkFluesInspectionYes", checkFluesInspectionYes.IsChecked.ToString());
        reportData.Add("checkFluesInspectionNo", checkFluesInspectionNo.IsChecked.ToString());
        reportData.Add("checkFlueJointsYes", checkFlueJointsYes.IsChecked.ToString());
        reportData.Add("checkFlueJointsNo", checkFlueJointsNo.IsChecked.ToString());
        reportData.Add("checkA_ID2", checkA_ID2.IsChecked.ToString());
        reportData.Add("checkB_AR2", checkB_AR2.IsChecked.ToString());
        reportData.Add("checkC_NCS2", checkC_NCS2.IsChecked.ToString());
        reportData.Add("checkInterlocksProvidedYes", checkInterlocksProvidedYes.IsChecked.ToString());
        reportData.Add("checkInterlocksProvidedNo", checkInterlocksProvidedNo.IsChecked.ToString());
        reportData.Add("checkEmergencyShutOffButtonYes", checkEmergencyShutOffButtonYes.IsChecked.ToString());
        reportData.Add("checkEmergencyShutOffButtonNo", checkEmergencyShutOffButtonNo.IsChecked.ToString());
        reportData.Add("checkPlantInterlinkYes", checkPlantInterlinkYes.IsChecked.ToString());
        reportData.Add("checkPlantInterlinkNo", checkPlantInterlinkNo.IsChecked.ToString());
        reportData.Add("checkFuelShutOffYes", checkFuelShutOffYes.IsChecked.ToString());
        reportData.Add("checkFuelShutOffNo", checkFuelShutOffNo.IsChecked.ToString());
        reportData.Add("checkFuelFirstEntryYes", checkFuelFirstEntryYes.IsChecked.ToString());
        reportData.Add("checkFuelFirstEntryNo", checkFuelFirstEntryNo.IsChecked.ToString());
        reportData.Add("checkSystemStopYes", checkSystemStopYes.IsChecked.ToString());
        reportData.Add("checkSystemStopNo", checkSystemStopNo.IsChecked.ToString());
        reportData.Add("checkTestAndResetYes", checkTestAndResetYes.IsChecked.ToString());
        reportData.Add("checkTestAndResetNo", checkTestAndResetNo.IsChecked.ToString());
        reportData.Add("checkA_ID3", checkA_ID3.IsChecked.ToString());
        reportData.Add("checkB_AR3", checkB_AR3.IsChecked.ToString());
        reportData.Add("checkC_NCS3", checkC_NCS3.IsChecked.ToString());
        reportData.Add("checkSystemDosingFacilitiesYes", checkSystemDosingFacilitiesYes.IsChecked.ToString());
     





        return reportData;
    }
}