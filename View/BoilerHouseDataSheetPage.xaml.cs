namespace Ashwell_Maintenance.View;

public partial class BoilerHouseDataSheetPage : ContentPage
{
	public BoilerHouseDataSheetPage()
	{
		InitializeComponent();
	}

	public async void BoilerHouseDataSheetBack(object sender, EventArgs e)
	{
		if (BHDSSection1.IsVisible)
			await Navigation.PopModalAsync();
		else if (BHDSSection2.IsVisible)
		{
			BHDSSection2.IsVisible = false;

			await BHDSSection1.ScrollToAsync(0, 0, false);
			BHDSSection1.IsVisible = true;
		}
		else if (BHDSSection3.IsVisible)
		{
			BHDSSection3.IsVisible = false;

			await BHDSSection2.ScrollToAsync(0, 0, false);
			BHDSSection2.IsVisible = true;
		}
		else 
		{
            BHDSSection4.IsVisible = false;

            await BHDSSection3.ScrollToAsync(0, 0, false);
            BHDSSection3.IsVisible = true;
        }
	}

	public async void BHDSNext1(object sender, EventArgs e)
	{
        BHDSSection1.IsVisible = false;

        await BHDSSection2.ScrollToAsync(0, 0, false);
        BHDSSection2.IsVisible = true;
    }
    public async void BHDSNext2(object sender, EventArgs e)
    {
        BHDSSection2.IsVisible = false;

        await BHDSSection3.ScrollToAsync(0, 0, false);
        BHDSSection3.IsVisible = true;
    }
    public async void BHDSNext3(object sender, EventArgs e)
    {
        BHDSSection3.IsVisible = false;

        await BHDSSection4.ScrollToAsync(0, 0, false);
        BHDSSection4.IsVisible = true;
    }
	public void BHDSNext4(object sender, EventArgs e)
	{
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string reportName = $"Ashwell_Engineers_Report_{dateTimeString}.pdf";

        PdfCreation.Boiler(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("uern", uern.Text ?? string.Empty);
        reportData.Add("SheetNo", SheetNo.Text ?? string.Empty);
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("nameOfPremises", nameOfPremises.Text ?? string.Empty);
        reportData.Add("adressOfPremises", adressOfPremises.Text ?? string.Empty);
        reportData.Add("appliancesCoveredByThisCheck", appliancesCoveredByThisCheck.Text ?? string.Empty);
        reportData.Add("meterHouseLocation", meterHouseLocation.Text ?? string.Empty);
        reportData.Add("meterHouseComment", meterHouseComment.Text ?? string.Empty);
        reportData.Add("ventilationLocation", ventilationLocation.Text ?? string.Empty);
        reportData.Add("freeAirExistingHighLevel", freeAirExistingHighLevel.Text ?? string.Empty);
        reportData.Add("freeAirExistingLowLevel", freeAirExistingLowLevel.Text ?? string.Empty);
        reportData.Add("freeAirRequiredHighLevel", freeAirRequiredHighLevel.Text ?? string.Empty);
        reportData.Add("freeAirRequiredLowLevel", freeAirRequiredLowLevel.Text ?? string.Empty);
        reportData.Add("boilerHousePlantRoomComments", boilerHousePlantRoomComments.Text ?? string.Empty);
        reportData.Add("inletWorkingPressureTestFullLoad", inletWorkingPressureTestFullLoad.Text ?? string.Empty);
        reportData.Add("inletWorkingPressureTestPartLoad", inletWorkingPressureTestPartLoad.Text ?? string.Empty);
        reportData.Add("standingPressure", standingPressure.Text ?? string.Empty);
        reportData.Add("plantGasInstallationVolume", plantGasInstallationVolume.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        reportData.Add("contractor", contractor.Text ?? string.Empty);
        reportData.Add("companyGasSafeRegistrationNo", companyGasSafeRegistrationNo.Text ?? string.Empty);
        reportData.Add("engineersGasSafeIDNo", engineersGasSafeIDNo.Text ?? string.Empty);
        reportData.Add("inspectionDate", inspectionDate.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);


        reportData.Add("checkRemedialToWorkRequiredYes", checkRemedialToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkTestsCompletedSatisfactoryYes", checkTestsCompletedSatisfactoryYes.IsChecked.ToString());
        reportData.Add("checkPipeworkToGasMeterYes", checkPipeworkToGasMeterYes.IsChecked.ToString());
        reportData.Add("checkPipeworkToGasMeterNA", checkPipeworkToGasMeterNA.IsChecked.ToString());
        reportData.Add("checkRegulatorAndOrMeterYes", checkRegulatorAndOrMeterYes.IsChecked.ToString());
        reportData.Add("checkRegulatorAndOrMeterNA", checkRegulatorAndOrMeterNA.IsChecked.ToString());
        reportData.Add("checkSafetyNoticesLabelsYes", checkSafetyNoticesLabelsYes.IsChecked.ToString());
        reportData.Add("checkSafetyNoticesLabelsNA", checkSafetyNoticesLabelsNA.IsChecked.ToString());
        reportData.Add("checkLineDiagramYes", checkLineDiagramYes.IsChecked.ToString());
        reportData.Add("checkLineDiagramNA", checkLineDiagramNA.IsChecked.ToString());
        reportData.Add("checkColorCodingIndicationTapeYes", checkColorCodingIndicationTapeYes.IsChecked.ToString());
        reportData.Add("checkColorCodingIndicationTapeNA", checkColorCodingIndicationTapeNA.IsChecked.ToString());
        reportData.Add("checkMeterHouseVentilationYes", checkMeterHouseVentilationYes.IsChecked.ToString());
        reportData.Add("checkMeterHouseVentilationNA", checkMeterHouseVentilationNA.IsChecked.ToString());
        reportData.Add("checkFreeAirExistingCM", checkFreeAirExistingCM.IsChecked.ToString());
        reportData.Add("checkFreeAirExistingMH", checkFreeAirExistingMH.IsChecked.ToString());
        reportData.Add("checkFreeAirRequiredCM", checkFreeAirRequiredCM.IsChecked.ToString());
        reportData.Add("checkcheckFreeAirRequiredMH", checkcheckFreeAirRequiredMH.IsChecked.ToString());
        reportData.Add("checkMainFlueYes", checkMainFlueYes.IsChecked.ToString());
        reportData.Add("checkMainFlueNA", checkMainFlueNA.IsChecked.ToString());
        reportData.Add("checkChimneyFlueTerminalPositionYes", checkChimneyFlueTerminalPositionYes.IsChecked.ToString());
        reportData.Add("checkChimneyFlueTerminalPositionNA", checkChimneyFlueTerminalPositionNA.IsChecked.ToString());
        reportData.Add("checkStubFluersToBoildersYes", checkStubFluersToBoildersYes.IsChecked.ToString());
        reportData.Add("checkStubFluersToBoildersNA", checkStubFluersToBoildersNA.IsChecked.ToString());
        reportData.Add("checkIdFanYes", checkIdFanYes.IsChecked.ToString());
        reportData.Add("checkIdFanNA", checkIdFanNA.IsChecked.ToString());
        reportData.Add("checkFanBoilerSafetyInterlockYes", checkFanBoilerSafetyInterlockYes.IsChecked.ToString());
        reportData.Add("checkFanBoilerSafetyInterlockNA", checkFanBoilerSafetyInterlockNA.IsChecked.ToString());
        reportData.Add("checkGeneralComplianceOfGasPipeYes", checkGeneralComplianceOfGasPipeYes.IsChecked.ToString());
        reportData.Add("checkGeneralComplianceOfGasPipeNA", checkGeneralComplianceOfGasPipeNA.IsChecked.ToString());
        reportData.Add("checkVentilationYes", checkVentilationYes.IsChecked.ToString());
        reportData.Add("checkVentilationNA", checkVentilationNA.IsChecked.ToString());
        reportData.Add("checkAIVYes", checkAIVYes.IsChecked.ToString());
        reportData.Add("checkAIVNo", checkAIVNo.IsChecked.ToString());
        reportData.Add("checkManualYes", checkManualYes.IsChecked.ToString());
        reportData.Add("checkManualNo", checkManualNo.IsChecked.ToString());
        reportData.Add("checkPlantGasTightnessTestYes", checkPlantGasTightnessTestYes.IsChecked.ToString());
     

        return reportData;
    }
}