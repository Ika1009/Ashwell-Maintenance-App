namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage1 : ContentPage
{
	public ServiceRecordPage1()
	{
		InitializeComponent();
	}

	public void ServiceRecordBack(object sender, EventArgs e)
	{
		if (SRSetion1.IsVisible)
			Navigation.PushAsync(new MainPage());
		else if (SRSetion2.IsVisible == true)
		{
			SRSetion2.IsVisible = false;

			SRSetion1.ScrollToAsync(0, 0, false);
			SRSetion1.IsVisible = true;
		}
		else if (SRSetion3.IsVisible == true)
		{
			SRSetion3.IsVisible = false;

			SRSetion2.ScrollToAsync(0, 0, false);
			SRSetion2.IsVisible = true;
		}
		else
		{
            SRSetion4.IsVisible = false;

            SRSetion3.ScrollToAsync(0, 0, false);
            SRSetion3.IsVisible = true;
        }
    }

	public void ServiceRecordNext1(object sender, EventArgs e)
	{
		SRSetion1.IsVisible = false;

		SRSetion2.ScrollToAsync(0, 0, false);
		SRSetion2.IsVisible = true;
	}

    public void ServiceRecordNext2(object sender, EventArgs e)
    {
		SRSetion2.IsVisible = false;

        SRSetion3.ScrollToAsync(0, 0, false);
        SRSetion3.IsVisible = true;
    }

    public void ServiceRecordNext3(object sender, EventArgs e)
    {
        SRSetion3.IsVisible = false;

        SRSetion4.ScrollToAsync(0, 0, false);
        SRSetion4.IsVisible = true;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
		//string siteEntryText = siteEntry.Text;
		//PdfCreation.CreateServiceRecordPDF(siteEntryText);
    }
}