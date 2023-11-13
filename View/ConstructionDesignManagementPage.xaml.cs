namespace Ashwell_Maintenance.View;

public partial class ConstructionDesignManagmentPage : ContentPage
{
	public ConstructionDesignManagmentPage()
	{
		InitializeComponent();
	}

    public void CDMBack(object sender, EventArgs e)
    {
        if (CDMSection1.IsVisible)
            Navigation.PopAsync();
        else if (CDMSection2.IsVisible)
        {
            CDMSection2.IsVisible = false;

            CDMSection1.ScrollToAsync(0, 0, false);
            CDMSection1.IsVisible = true;
        }
        else
        {
            CDMSection3.IsVisible = false;

            CDMSection2.ScrollToAsync(0, 0, false);
            CDMSection2.IsVisible = true;
        }
    }

    public void CDMNext1(object sender, EventArgs e)
    {
        CDMSection1.IsVisible = false;
        CDMSection2.IsVisible = true;
    }

    public void CDMNext2(object sender, EventArgs e)
    {
        CDMSection2.IsVisible = false;
        CDMSection3.IsVisible = true;
    }

    public void CDMNext3(object sender, EventArgs e)
    {

    }
}