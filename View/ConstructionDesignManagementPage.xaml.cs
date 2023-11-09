namespace Ashwell_Maintenance.View;

public partial class ConstructionDesignManagmentPage : ContentPage
{
	public ConstructionDesignManagmentPage()
	{
		InitializeComponent();
	}

    public void CDMback(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}