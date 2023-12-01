namespace Ashwell_Maintenance.View;

public partial class PressurisationUnitReportPage : ContentPage
{
	public PressurisationUnitReportPage()
	{
		InitializeComponent();
	}

    public async void PressurisationUnitReportBack(object sender, EventArgs e)
    {
		if (PURSection1.IsVisible)
			await Navigation.PopAsync();
		else if (PURSection2.IsVisible)
		{
			PURSection2.IsVisible = false;

			await PURSection1.ScrollToAsync(0, 0, false);
			PURSection1.IsVisible = true;
		}
		else
		{
            PURSection3.IsVisible = false;

            await PURSection2.ScrollToAsync(0, 0, false);
            PURSection2.IsVisible = true;
        }
    }

	public async void PressurisationUnitReportNext1(object sender, EventArgs e)
	{
		PURSection1.IsVisible = false;

		await PURSection2.ScrollToAsync(0, 0, false);
		PURSection2.IsVisible = true;
	}

    public async void PressurisationUnitReportNext2(object sender, EventArgs e)
    {
        PURSection2.IsVisible = false;

        await PURSection3.ScrollToAsync(0, 0, false);
        PURSection3.IsVisible = true;
    }

    public void PressurisationUnitReportNext3(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string reportName = $"Ashwell_Pressurisation_Report_{dateTimeString}.pdf";

        PdfCreation.CreateEngineersReport(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());
        string SNameAdress = siteAddress.Text + siteName.Text;
        reportData.Add("siteNameAndAddress", SNameAdress?? string.Empty);
        reportData.Add("totalHeatingSystemRating", totalHeatingSystemRating.Text ?? string.Empty);
        reportData.Add("numberOfBoilers", numberOfBoilers.Text ?? string.Empty);
        reportData.Add("flowTemperature", flowTemperature.Text ?? string.Empty);
        reportData.Add("returnTemperature", returnTemperature.Text ?? string.Empty);
        reportData.Add("currentWorkingPressure", currentWorkingPressure.Text ?? string.Empty);
        reportData.Add("safetyValveSetting", safetyValveSetting.Text ?? string.Empty);
        reportData.Add("unitModel", unitModel.Text ?? string.Empty);
        reportData.Add("serialNo", serialNo.Text ?? string.Empty);
        reportData.Add("expansionVesselSize", expansionVesselSize.Text ?? string.Empty);
        reportData.Add("numberOfPressureVessels", numberOfPressureVessels.Text ?? string.Empty);
        reportData.Add("setFillPressure", setFillPressure.Text ?? string.Empty);
        reportData.Add("ratedExpansionVesselCharge", ratedExpensionVesselCharge.Text ?? string.Empty);
        reportData.Add("highPressureSwitchSetting", highPressureSwitchSetting.Text ?? string.Empty);
        reportData.Add("lowPressureSwitchSetting", lowPressureSwitchSetting.Text ?? string.Empty);
        reportData.Add("finalSystemPressure", finalSystemPressure.Text ?? string.Empty);
        reportData.Add("notes", notes.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);


        reportData.Add("checkMainWaterSupply", checkMainsWatersSupplyYes.IsChecked.ToString());
        reportData.Add("checkColdFillPressureSet", checkColdFillPressureSetCorrectlyYes.IsChecked.ToString());
        reportData.Add("checkElectricalSupplyWorking", checkElectricalSupplyWorkingYes.IsChecked.ToString());
        reportData.Add("checkFillingLoopDisconnected", checkFillingLoopDisconnectedYes.IsChecked.ToString());
        reportData.Add("checkUnitLeftOperational", checkUnitLeftOperationalYes.IsChecked.ToString());





        return reportData;
    }
}