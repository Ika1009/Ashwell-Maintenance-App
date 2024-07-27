using Ashwell_Maintenance.View;

namespace Ashwell_Maintenance
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            CheckForLogin();
        }
        private async void CheckForLogin()
        {
            // Navigate to the Login Page if UserId is not available
            if (string.IsNullOrEmpty(CurrentUser.UserId))
            {
                await Task.Delay(100); // Ensures the navigation stack is ready
                await Navigation.PushModalAsync(new LoginPage());
            }
        }
    }
}