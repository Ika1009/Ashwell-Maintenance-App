namespace Ashwell_Maintenance.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OpenPage(string page)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            //loadingBG.IsRunning = true; loading.IsRunning = true;

            switch (page)
            {
                case "CDM": await Navigation.PushModalAsync(new ConstructionDesignManagmentPage()); break;
                case "ER": await Navigation.PushModalAsync(new EngineersReportPage()); break;
                case "SR": await Navigation.PushModalAsync(new ServiceRecordPage1()); break;
                case "GRA": await Navigation.PushModalAsync(new GasRiskAssessmentPage()); break;
                case "PUR": await Navigation.PushModalAsync(new PressurisationUnitReportPage()); break;
                case "BHDS": await Navigation.PushModalAsync(new BoilerHouseDataSheetPage()); break;
                case "CC": await Navigation.PushModalAsync(new ConformityCheckPage()); break;
                case "ATT": await Navigation.PushModalAsync(new OneAPage()); break;
                case "BTT": await Navigation.PushModalAsync(new OneBPage()); break;
                case "TT": await Navigation.PushModalAsync(new OnePage()); break;
            }

            //loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        public void CDM_Tapped(object sender, EventArgs e)
        {
            OpenPage("CDM");
        }
        public void ER_Tapped(object sender, EventArgs e)
        {
            OpenPage("ER");
        }
        public void SR_Tapped(object sender, EventArgs e)
        {
            OpenPage("SR");
        }
        public void GRA_Tapped(object sender, EventArgs e)
        {
            OpenPage("GRA");
        }
        public void PUR_Tapped(object sender, EventArgs e)
        {
            OpenPage("PUR");
        }
        public void BHDS_Tapped(object sender, EventArgs e)
        {
            OpenPage("BHDS");
        }
        public void CC_Tapped(object sender, EventArgs e)
        {
            OpenPage("CC");
        }
        private void BTT_Tapped(object sender, EventArgs e)
        {
            OpenPage("BTT");
        }
        private void ATT_Tapped(object sender, EventArgs e)
        {
            OpenPage("ATT");
        }
        public void TT_Tapped(object sender, EventArgs e)
        {
            OpenPage("TT");
        }
    }
}