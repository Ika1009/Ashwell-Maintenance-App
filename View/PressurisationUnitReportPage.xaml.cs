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
			await Navigation.PopModalAsync();
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
        
    }
}