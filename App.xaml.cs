using Ashwell_Maintenance.Customs;
using Ashwell_Maintenance.View;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Ashwell_Maintenance
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new SignaturePage();



            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(LinelessEntry), (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetHighlightColor(Android.Graphics.Color.Red);
#elif __IOS__
                handler.PlatformView.TintColor = UIKit.UIColor.Red;
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("CursorColor", (handler, view) =>
            {
#if __ANDROID__
                //handler.PlatformView.TextCursorDrawable.SetTint(Colors.Red.ToAndroid());
#endif
            });

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(LinelessEditor), (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetHighlightColor(Android.Graphics.Color.Red);
#elif __IOS__
                handler.PlatformView.TintColor = UIKit.UIColor.Red;
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("CursorColor", (handler, view) =>
            {
#if __ANDROID__
                //handler.PlatformView.TextCursorDrawable.SetTint(Colors.Red.ToAndroid());
#endif
            });
        }
    }
}