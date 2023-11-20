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
            await Navigation.PushAsync(new ConstructionDesignManagmentPage());
        }
        private async void ER_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EngineersReportPage());
        }
        private async void SR_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServiceRecordPage1());
        }
        private async void GRA_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GasRiskAssessmentPage());
        }
        private void BTT_Tapped(object sender, EventArgs e)
        {

        }
        private void ATT_Tapped(object sender, EventArgs e)
        {

        }
        private void TT_Tapped(object sender, EventArgs e)
        {

        }
    }
}