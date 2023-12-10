namespace Ashwell_Maintenance.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void CDM_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false; 
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new ConstructionDesignManagmentPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void ER_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new EngineersReportPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void SR_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new ServiceRecordPage1());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void GRA_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new GasRiskAssessmentPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void PUR_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new PressurisationUnitReportPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void BHDS_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new BoilerHouseDataSheetPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private async void CC_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new ConformityCheckPage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
        private void BTT_Tapped(object sender, EventArgs e)
        {

        }
        private void ATT_Tapped(object sender, EventArgs e)
        {

        }
        private async void TT_Tapped(object sender, EventArgs e)
        {
            CDM.IsEnabled = false; ER.IsEnabled = false; SR.IsEnabled = false; GRA.IsEnabled = false; PUR.IsEnabled = false;
            BHDS.IsEnabled = false; CC.IsEnabled = false; TT.IsEnabled = false;

            loadingBG.IsRunning = true; loading.IsRunning = true;

            await Navigation.PushModalAsync(new OnePage());

            loading.IsRunning = false; loadingBG.IsRunning = false;

            CDM.IsEnabled = true; ER.IsEnabled = true; SR.IsEnabled = true; GRA.IsEnabled = true; PUR.IsEnabled = true;
            BHDS.IsEnabled = true; CC.IsEnabled = true; TT.IsEnabled = true;
        }
    }
}