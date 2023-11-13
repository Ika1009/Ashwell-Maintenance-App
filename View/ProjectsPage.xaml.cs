namespace Ashwell_Maintenance.View;

public partial class ProjectsPage : ContentPage
{
	public ProjectsPage()
	{
		InitializeComponent();
	}

    private async void Finished_Projects_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DisplayedProjectsPage(true));
    }
    private async void Unfinished_Projects_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DisplayedProjectsPage(false));
    }
}