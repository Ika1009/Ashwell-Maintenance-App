using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Ashwell_Maintenance
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
#pragma warning disable MCT001
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Satoshi-Medium.otf", "SatoshiMedium");
                    fonts.AddFont("Satoshi-Bold.otf", "SatoshiBold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#pragma warning restore MCT001

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}