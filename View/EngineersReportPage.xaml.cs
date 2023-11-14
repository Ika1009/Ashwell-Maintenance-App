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

    }
}