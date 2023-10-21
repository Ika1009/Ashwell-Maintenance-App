namespace Ashwell_Maintenance.View
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void CDM_Tapped(object sender, EventArgs e)
        {

        }
        private void ER_Tapped(object sender, EventArgs e)
        {

        }
        private async void SR_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServiceRecordPage1());
        }
        private void GRA_Tapped(object sender, EventArgs e)
        {

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