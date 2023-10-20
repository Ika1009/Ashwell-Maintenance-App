using CommunityToolkit.Maui.Views;

namespace Ashwell_Maintenance.View;

public partial class SignaturePage : ContentPage
{
	public SignaturePage()
	{
		InitializeComponent();
	}

    private void Clear_Button_Clicked(object sender, EventArgs e)
    {
		drawingView.Clear();
    }

    private async void Save_Button_Clicked(object sender, EventArgs e)
    {
        using var stream = await drawingView.GetImageStream(100, 100);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
    }
}