namespace Ashwell_Maintenance.View;

public partial class PressurisationUnitReport : ContentPage
{
	public PressurisationUnitReport()
	{
		InitializeComponent();
	}

    private async void PressurisationUnitReportBack(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }
}