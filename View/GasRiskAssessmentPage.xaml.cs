namespace Ashwell_Maintenance.View;

public partial class GasRiskAssessmentPage : ContentPage
{
	public GasRiskAssessmentPage()
	{
		InitializeComponent();
    }

	public async void GasRiskAssessmentBack(object sender, EventArgs e)
	{
		if (GRASection1.IsVisible)
			await Navigation.PopModalAsync();
		else if (GRASection2.IsVisible)
		{
			GRASection2.IsVisible = false;

			await GRASection1.ScrollToAsync(0, 0, false);
			GRASection1.IsVisible = true;
		}
		else if (GRASection3.IsVisible)
		{
            GRASection3.IsVisible = false;

            await GRASection2.ScrollToAsync(0, 0, false);
            GRASection2.IsVisible = true;
        }
	}

	public async void GasRiskAssessmentNext1(object sender, EventArgs e)
	{
		GRASection1.IsVisible = false;

		await GRASection2.ScrollToAsync(0, 0, false);
		GRASection2.IsVisible = true;
	}

    public async void GasRiskAssessmentNext2(object sender, EventArgs e)
    {
        GRASection2.IsVisible = false;

        await GRASection3.ScrollToAsync(0, 0, false);
        GRASection3.IsVisible = true;
    }

    public void GasRiskAssessmentNext3(object sender, EventArgs e)
    {

    }
}