namespace Ashwell_Maintenance.View;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        loadingBG.IsRunning = true; loading.IsRunning = true;
        loginButton.IsEnabled = false;
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both username and password.", "OK");
            return;
        }

        var (loginSuccess, errorMessage) = await ApiService.LoginAsync(username, password);

        if (loginSuccess)
        {
            await DisplayAlert("Success", "Login successful!", "OK");
            await Navigation.PopModalAsync();
        }
        else
        {
            await DisplayAlert("Login Failed", errorMessage, "OK");
        }
        loginButton.IsEnabled = true;
        loadingBG.IsRunning = false; loading.IsRunning = false;

    }
}