namespace Ashwell_Maintenance.View;

public partial class ConstructionDesignManagmentPage : ContentPage
{
	public ConstructionDesignManagmentPage()
	{
		InitializeComponent();
	}

    [Obsolete]
    public void CDMBack(object sender, EventArgs e)
    {
        if (CDMSection1.IsVisible)
        {
            CDMBackBtt.IsEnabled = false;
            Navigation.PopModalAsync();
        }
        else if (CDMSection2.IsVisible)
        {
            CDMSection2.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                CDMSection1.ScrollToAsync(0, 0, false);
            CDMSection1.IsVisible = true;
        }
        else
        {
            CDMSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                CDMSection2.ScrollToAsync(0, 0, false);
            CDMSection2.IsVisible = true;
        }
    }

    [Obsolete]
    public async void CDMNext1(object sender, EventArgs e)
    {
        CDMSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CDMSection2.ScrollToAsync(0, 0, false);
        CDMSection2.IsVisible = true;
    }
    [Obsolete]
    public void CDMNext2(object sender, EventArgs e)
    {
        CDMSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            CDMSection3.ScrollToAsync(0, 0, false);
        CDMSection3.IsVisible = true;
    }
    [Obsolete]
    public async void CDMNext3(object sender, EventArgs e)
    {
        await PdfCreation.CDM(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("siteAdress", siteAdress.Text ?? string.Empty);
        reportData.Add("clinet", clinet.Text ?? string.Empty);
        reportData.Add("responsibleSiteEngineer", responsibleSiteEngineer.Text ?? string.Empty);
        reportData.Add("otherEngineers", otherEngineers.Text ?? string.Empty);
        reportData.Add("whatInformationIssued", whatInformationIssued.Text ?? string.Empty);
        reportData.Add("startDate", startDate.Text ?? string.Empty);
        reportData.Add("other", other.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        reportData.Add("completionDate", date.Text ?? string.Empty);
        //za buduce kometnari
        reportData.Add("ControlActionWorkingAtHeight", ControlActionWorkingAtHeight.Text ?? string.Empty);
        reportData.Add("ControlActionPermitsToWorkRequired", ControlActionPermitsToWorkRequired.Text ?? string.Empty);
        reportData.Add("ControlActionExcavations", ControlActionExcavations.Text ?? string.Empty);
        reportData.Add("DustNoiseCOSHHTheControlActionRequired", DustNoiseCOSHHTheControlActionRequired.Text ?? string.Empty);
        reportData.Add("actionTaken", actionTaken.Text ?? string.Empty);
        reportData.Add("moreDangersTheControlActionRequired", moreDangersTheControlActionRequired.Text ?? string.Empty);
        reportData.Add("ControlActionAnyOtherDanger", ControlActionAnyOtherDanger.Text ?? string.Empty);
        reportData.Add("ControlActionAppointedFirstAider", ControlActionAppointedFirstAider.Text ?? string.Empty);
        reportData.Add("ControlActionAdditionalActions", ControlActionAdditionalActions.Text ?? string.Empty);
        reportData.Add("ControlActionIsItSafe", ControlActionIsItSafe.Text ?? string.Empty);
        
       

        reportData.Add("checkWelfareFacilitiesYes", checkWelfareFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkPortableWelfareFacilitiesYes", checkPortableWelfareFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkWorkingAtHeightYes", checkWorkingAtHeightYes.IsChecked.ToString());
        reportData.Add("checkPermitsToWorkRequiredYes", checkPermitsToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkExcavationsYes", checkExcavationsYes.IsChecked.ToString());
        reportData.Add("checkDustYes", checkDustYes.IsChecked.ToString());
        reportData.Add("checkNoiseYes", checkNoiseYes.IsChecked.ToString());
        reportData.Add("checkCOSHHYes", checkCOSHHYes.IsChecked.ToString());
        reportData.Add("checkOtherYes", checkOtherYes.IsChecked.ToString());
        reportData.Add("checkManagementSurveyYes", checkManagementSurveyYes.IsChecked.ToString());
        reportData.Add("checkFiveYearsSurveyYes", checkFiveYearsSurveyYes.IsChecked.ToString());
        reportData.Add("checkElectricalYes", checkElectricalYes.IsChecked.ToString());
        reportData.Add("checkGasYes", checkGasYes.IsChecked.ToString());
        reportData.Add("checkWaterYes", checkWaterYes.IsChecked.ToString());
        reportData.Add("checkOtherServicesYes", checkOtherServicesYes.IsChecked.ToString());
        reportData.Add("checkDangerToOthersYes", checkDangerToOthersYes.IsChecked.ToString());
        reportData.Add("checkDangerToPublicYes", checkDangerToPublicYes.IsChecked.ToString());
        reportData.Add("checkOtherDangersYes", checkOtherDangersYes.IsChecked.ToString());
        reportData.Add("checkAnyOtherDangerYes", checkAnyOtherDangerYes.IsChecked.ToString());
        reportData.Add("checkHotWorksYes", checkHotWorksYes.IsChecked.ToString());
        reportData.Add("checkAppointedFirstAiderYes", checkAppointedFirstAiderYes.IsChecked.ToString());
        reportData.Add("checkAdditionalActionsYes", checkAdditionalActionsYes.IsChecked.ToString());
        reportData.Add("checkIsItSafeYes", checkIsItSafeYes.IsChecked.ToString());
     

        return reportData;
    }
}