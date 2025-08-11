using Microsoft.Maui.Controls;

namespace Ashwell_Maintenance.View;

public partial class ImagePreviewPage : ContentPage
{
    public ImagePreviewPage(string imagePath)
    {
        InitializeComponent();
        loading.IsRunning = true;
        loading.IsVisible = true;
        PreviewImage.IsVisible = false;

        PreviewImage.Source = imagePath;

        PreviewImage.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PopModalAsync())
        });

        PreviewImage.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PreviewImage.IsLoading))
            {
                if (!PreviewImage.IsLoading)
                {
                    loading.IsRunning = false;
                    loading.IsVisible = false;
                    PreviewImage.IsVisible = true;
                }
                else
                {
                    loading.IsRunning = true;
                    loading.IsVisible = true;
                    PreviewImage.IsVisible = false;
                }
            }
        };
    }
}