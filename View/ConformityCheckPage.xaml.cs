namespace Ashwell_Maintenance.View;

public partial class ConformityCheckPage : ContentPage
{
	public ConformityCheckPage()
	{
		InitializeComponent();
	}

    [Obsolete]
    public async void ConformityCheckBack(object sender, EventArgs e)
	{
		if (CCSection1.IsVisible)
			await Navigation.PopModalAsync();
		else if (CCSection2.IsVisible)
		{
			CCSection2.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await CCSection1.ScrollToAsync(0, 0, false);
			CCSection1.IsVisible = true;
		}
		else if (CCSection3.IsVisible)
		{
            CCSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await CCSection2.ScrollToAsync(0, 0, false);
            CCSection2.IsVisible = true;
        }
		else if (CCSection4.IsVisible)
		{
            CCSection4.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await CCSection3.ScrollToAsync(0, 0, false);
            CCSection3.IsVisible = true;
        }
        else
        {
            CCSection5.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await CCSection4.ScrollToAsync(0, 0, false);
            CCSection4.IsVisible = true;
        }
	}

    [Obsolete]
    public async void CCNext1(object sender, EventArgs e)
	{
		CCSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CCSection2.ScrollToAsync(0, 0, false);
		CCSection2.IsVisible = true;
	}
    [Obsolete]
    public async void CCNext2(object sender, EventArgs e)
    {
        CCSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CCSection3.ScrollToAsync(0, 0, false);
        CCSection3.IsVisible = true;
    }
    [Obsolete]
    public async void CCNext3(object sender, EventArgs e)
    {
        CCSection3.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CCSection4.ScrollToAsync(0, 0, false);
        CCSection4.IsVisible = true;
    }
    [Obsolete]
    public async void CCNext4(object sender, EventArgs e)
    {
        CCSection4.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CCSection5.ScrollToAsync(0, 0, false);
        CCSection5.IsVisible = true;
    }
    public void CCNext5(object sender, EventArgs e)
    {

    }
}