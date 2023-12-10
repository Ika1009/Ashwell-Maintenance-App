namespace Ashwell_Maintenance.View;

public partial class BoilerHouseDataSheetPage : ContentPage
{
	public BoilerHouseDataSheetPage()
	{
		InitializeComponent();
	}

    [Obsolete]
    public async void BoilerHouseDataSheetBack(object sender, EventArgs e)
	{
		if (BHDSSection1.IsVisible)
			await Navigation.PopModalAsync();
		else if (BHDSSection2.IsVisible)
		{
			BHDSSection2.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await BHDSSection1.ScrollToAsync(0, 0, false);
			BHDSSection1.IsVisible = true;
		}
		else if (BHDSSection3.IsVisible)
		{
			BHDSSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await BHDSSection2.ScrollToAsync(0, 0, false);
			BHDSSection2.IsVisible = true;
		}
		else 
		{
            BHDSSection4.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await BHDSSection3.ScrollToAsync(0, 0, false);
            BHDSSection3.IsVisible = true;
        }
	}

    [Obsolete]
    public async void BHDSNext1(object sender, EventArgs e)
	{
        BHDSSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await BHDSSection2.ScrollToAsync(0, 0, false);
        BHDSSection2.IsVisible = true;
    }
    [Obsolete]
    public async void BHDSNext2(object sender, EventArgs e)
    {
        BHDSSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await BHDSSection3.ScrollToAsync(0, 0, false);
        BHDSSection3.IsVisible = true;
    }
    [Obsolete]
    public async void BHDSNext3(object sender, EventArgs e)
    {
        BHDSSection3.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await BHDSSection4.ScrollToAsync(0, 0, false);
        BHDSSection4.IsVisible = true;
    }
	public void BHDSNext4(object sender, EventArgs e)
	{

	}
}