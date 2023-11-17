using CommunityToolkit.Maui.Views;
using System.IO;

namespace Ashwell_Maintenance.View;

public partial class SignaturePage : ContentPage
{
    private byte[] savedImageData;

    // Define a delegate and an event for when images are saved
    public delegate void ImagesSavedHandler(byte[] image1, byte[] image2);
    public event ImagesSavedHandler ImagesSaved;

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

        if (savedImageData == null)
        {
            savedImageData = memoryStream.ToArray();
            drawingView.Clear();
            signatureTitle.Text = "Engineer's Signature:";
        }
        else
        {
            OnImagesSaved(savedImageData, memoryStream.ToArray());
            await Navigation.PopModalAsync(); // Close the modal
        }
    }
    private void OnImagesSaved(byte[] image1, byte[] image2)
    {
        ImagesSaved?.Invoke(image1, image2);
    }
}
