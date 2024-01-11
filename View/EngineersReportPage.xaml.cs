namespace Ashwell_Maintenance.View;

public partial class EngineersReportPage : ContentPage
{
	public EngineersReportPage()
	{
		InitializeComponent();
	}

    [Obsolete]
    public void EngineersReportBack(object sender, EventArgs e)
	{
		if (ERSection1.IsVisible)
        {
            EngineersReportBackBtt.IsEnabled = false;
            Navigation.PopModalAsync();
        }
		else if (ERSection2.IsVisible)
		{
			ERSection2.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                ERSection1.ScrollToAsync(0, 0, false);
			ERSection1.IsVisible = true;

		}
		else
		{
            ERSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                ERSection2.ScrollToAsync(0, 0, false);
            ERSection2.IsVisible = true;
        }
    }

    [Obsolete]
    public async void ERNext1(object sender, EventArgs e)
	{
		ERSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await ERSection2.ScrollToAsync(0, 0, false);
		ERSection2.IsVisible = true;
	}
    [Obsolete]
    public async void ERNext2(object sender, EventArgs e)
    {
        ERSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await ERSection3.ScrollToAsync(0, 0, false);
        ERSection3.IsVisible = true;
    }
    [Obsolete]
    public async void ERNext3(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string  reportName = $"Ashwell_Engineers_Report_{dateTimeString}.pdf";

        await PdfCreation.CreateEngineersReport(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

      //  reportData.Add("", .Text ?? string.Empty);
      //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("address", clientsAdress.Text ?? string.Empty);
        reportData.Add("applianceMake", applianceMake.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        reportData.Add("engineer", engineer.Text ?? string.Empty);
        reportData.Add("taskTNo", taskTNo.Text ?? string.Empty);
        reportData.Add("serialNumber", serialNumber.Text ?? string.Empty);
        reportData.Add("description", descriptionOfWork.Text ?? string.Empty);
        reportData.Add("gasOperatinPressure", gasOperatingPressure.Text ?? string.Empty);
        reportData.Add("inletPressure", intletPressure.Text ?? string.Empty);
        reportData.Add("warningNoticeNumber", warningNoticeNumber.Text ?? string.Empty);
        reportData.Add("totalHoursIncludingTravel", totalHours.Text ?? string.Empty);
      //  reportData.Add("", operativesSignature.Text ?? string.Empty);
      //  reportData.Add("", clientsSignature.Text ?? string.Empty);
        
        reportData.Add("checkTaskComplete", checkTaskCompleteYes.IsChecked.ToString());
        reportData.Add("checkSpillageTestPerformed", checkSpillageTestPass.IsChecked.ToString());
        reportData.Add("checkSpillageTestPerformedNA", checkSpillageTestNA.IsChecked.ToString());
        reportData.Add("checkRiskAssesmentCompleted", checkRiskAssessmentYes.IsChecked.ToString());
        reportData.Add("checkFlueFlowTest", checkFlueFlowTestPass.IsChecked.ToString());
        reportData.Add("checkFlueFlowTestNA", checkFlueFlowTestNA.IsChecked.ToString());
        reportData.Add("checkThightnessTestCarriedOut", checkTightnessTestPass.IsChecked.ToString());
        reportData.Add("checkThightnessTestCarriedOutNA", checkTightnessTestNA.IsChecked.ToString());
        reportData.Add("checkApplianceSafeToUse", checkApplianceSafeToUseYes.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssued", checkWarningNoticeYes.IsChecked.ToString());

        return reportData;
    }



    public void DisjunctCheckboxes(CheckBox a, CheckBox b, CheckBox c)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;

        a.Color = Colors.Red;
        b.Color = Colors.White;
        c.Color = Colors.White;
    }

    public void CheckSpillageTestPassChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestPass.IsChecked)
            DisjunctCheckboxes(checkSpillageTestPass, checkSpillageTestNo, checkSpillageTestNA);
        else
            checkSpillageTestPass.Color = Colors.White;
    }
    public void CheckSpillageTestNoChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNo.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNo, checkSpillageTestPass, checkSpillageTestNA);
        else
            checkSpillageTestNo.Color = Colors.White;
    }
    public void CheckSpillageTestNAChanged(object sender, EventArgs e)
    {
        if (checkSpillageTestNA.IsChecked)
            DisjunctCheckboxes(checkSpillageTestNA, checkSpillageTestPass, checkSpillageTestNo);
        else
            checkSpillageTestNA.Color = Colors.White;
    }

    public void CheckFlueFlowTestPassChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestPass.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestPass, checkFlueFlowTestFail, checkFlueFlowTestNA);
        else
            checkFlueFlowTestPass.Color = Colors.White;
    }
    public void CheckFlueFlowTestFailChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestFail.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestFail, checkFlueFlowTestPass, checkFlueFlowTestNA);
        else
            checkFlueFlowTestFail.Color = Colors.White;
    }
    public void CheckFlueFlowTestNAChanged(object sender, EventArgs e)
    {
        if (checkFlueFlowTestNA.IsChecked)
            DisjunctCheckboxes(checkFlueFlowTestNA, checkFlueFlowTestPass, checkFlueFlowTestFail);
        else
            checkFlueFlowTestNA.Color = Colors.White;
    }

    public void CheckTightnessTestPassChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestPass.IsChecked)
            DisjunctCheckboxes(checkTightnessTestPass, checkTightnessTestFail, checkTightnessTestNA);
        else
            checkTightnessTestPass.Color = Colors.White;
    }
    public void CheckTightnessTestFailChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestFail.IsChecked)
            DisjunctCheckboxes(checkTightnessTestFail, checkTightnessTestPass, checkTightnessTestNA);
        else
            checkTightnessTestFail.Color = Colors.White;
    }
    public void CheckTightnessTestNAChanged(object sender, EventArgs e)
    {
        if (checkTightnessTestNA.IsChecked)
            DisjunctCheckboxes(checkTightnessTestNA, checkTightnessTestPass, checkTightnessTestFail);
        else
            checkTightnessTestNA.Color = Colors.White;
    }
}