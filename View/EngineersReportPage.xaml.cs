namespace Ashwell_Maintenance.View;

public partial class EngineersReportPage : ContentPage
{
	public EngineersReportPage()
	{
		InitializeComponent();
	}

	public void EngineersReportBack(object sender, EventArgs e)
	{
		if (ERSection1.IsVisible)
			Navigation.PopAsync();
		else if (ERSection2.IsVisible)
		{
			ERSection2.IsVisible = false;

			ERSection1.ScrollToAsync(0, 0, false);
			ERSection1.IsVisible = true;

		}
		else
		{
            ERSection3.IsVisible = false;

            ERSection2.ScrollToAsync(0, 0, false);
            ERSection2.IsVisible = true;
        }
    }

	public void ERNext1(object sender, EventArgs e)
	{
		ERSection1.IsVisible = false;
		ERSection2.IsVisible = true;
	}

    public void ERNext2(object sender, EventArgs e)
    {
        ERSection2.IsVisible = false;
        ERSection3.IsVisible = true;
    }

    public void ERNext3(object sender, EventArgs e)
    {
        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        string  reportName = $"Ashwell_Engineers_Report_{dateTimeString}.pdf";

        PdfCreation.CreateEngineersReport(GatherReportData());
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
}