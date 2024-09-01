using Ashwell_Maintenance.Customs;
using Ashwell_Maintenance.View;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using PdfSharp.Fonts;

namespace Ashwell_Maintenance
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Assign the custom font resolver
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            MainPage = new AppShell();


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
                handler.PlatformView.TextCursorDrawable.SetTint(Android.Graphics.Color.Red);
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
                handler.PlatformView.TextCursorDrawable.SetTint(Android.Graphics.Color.Red);
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(LinelessPicker), (handler, view) =>
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
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("CursorColor", (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.TextCursorDrawable.SetTint(Android.Graphics.Color.Red);
#endif
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(LinelessDatePicker), (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.SetHighlightColor(Android.Graphics.Color.Red);
#elif __IOS__
                handler.PlatformView.TintColor = UIKit.UIColor.Red;
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("CursorColor", (handler, view) =>
            {
#if __ANDROID__
                handler.PlatformView.TextCursorDrawable.SetTint(Android.Graphics.Color.Red);
#endif
            });
        }
    }
}