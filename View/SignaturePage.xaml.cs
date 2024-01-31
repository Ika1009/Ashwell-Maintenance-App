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

    public async void SignaturesBack(object sender, EventArgs e)
    {
        if (savedImageData == null)
            await Navigation.PopModalAsync();
        else
        {
            drawingViewEngineer.IsVisible = false;
            drawingViewCustomer.IsVisible = true;

            savedImageData = null;

            signatureTitle.CancelAnimations();
            await Task.WhenAll
            (
                signatureTitle.FadeTo(0, 200),
                signatureTitle.TranslateTo(50, signatureTitle.Y, 300, Easing.SinOut)
            );

            signatureTitle.Text = "Customer's Signature:";
            signatureTitle.TranslationX = -50;

            await Task.WhenAll
            (
                signatureTitle.FadeTo(1, 200),
                signatureTitle.TranslateTo(0, signatureTitle.Y, 300, Easing.SinOut)
            );
        }
    }

    private void Clear_Button_Clicked(object sender, EventArgs e)
    {
        if (drawingViewCustomer.IsVisible)
            drawingViewCustomer.Clear();
        else
            drawingViewEngineer.Clear();
    }

    private async void Save_Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            drawingViewCustomer.Background = Colors.White;
            drawingViewEngineer.Background = Colors.White;

            using var stream = drawingViewCustomer.IsVisible ? await drawingViewCustomer.GetImageStream(100, 100) : await drawingViewEngineer.GetImageStream(100, 100);
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            if (savedImageData == null)
            {
                drawingViewCustomer.IsVisible = false;
                drawingViewEngineer.IsVisible = true;

                savedImageData = memoryStream.ToArray();

                signatureTitle.CancelAnimations();
                await Task.WhenAll
                (
                    signatureTitle.FadeTo(0, 200),
                    signatureTitle.TranslateTo(-50, signatureTitle.Y, 300, Easing.SinOut)
                );

                signatureTitle.Text = "Engineer's Signature:";
                signatureTitle.TranslationX = 50;

                await Task.WhenAll
                (
                    signatureTitle.FadeTo(1, 200),
                    signatureTitle.TranslateTo( 0, signatureTitle.Y, 300, Easing.SinOut)
                );
            }
            else
            {
                OnImagesSaved(savedImageData, memoryStream.ToArray());
                await Navigation.PopModalAsync(); // Close the modal
            }
        }
        catch
        {
            // Kolko sam uzasan u ovome lol, ali radi? - Nixa

            await signatureBorder.FadeTo(1, 100);
            await signatureBorder.FadeTo(1, 250);
            await signatureBorder.FadeTo(0, 1000);
        }
    }
    private void OnImagesSaved(byte[] image1, byte[] image2)
    {
        ImagesSaved?.Invoke(image1, image2);
    }
}
