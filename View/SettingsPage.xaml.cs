namespace Ashwell_Maintenance.View;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

	public async void LogOut(object sender, EventArgs e)
	{
		CurrentUser.Clear();
		await Navigation.PushModalAsync(new LoginPage(), false);
	}
}