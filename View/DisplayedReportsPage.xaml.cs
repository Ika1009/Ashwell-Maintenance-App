using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class DisplayedReportsPage : ContentPage
{
    public ObservableCollection<Report> Reports = new();
    public DisplayedReportsPage(string folderId)
    {
	    InitializeComponent();
        _ = LoadReports(folderId);
    }
    public class Report
    {
        public string ReportType { get; set; }
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public string ReportData { get; set; }
        public string FolderId { get; set; }
        public string CreatedAt { get; set; }
    }

    public void ReportChosen(object sender, EventArgs e)
    {

    }

    public async void SignatureButton_Clicked(object sender, EventArgs e)
    {
        var signaturePage = new SignaturePage();
        signaturePage.ImagesSaved += OnSignaturePageImagesSaved;
        await Navigation.PushModalAsync(signaturePage);
    }

    private void OnSignaturePageImagesSaved(byte[] customerSignature, byte[] engineerSignature)
    {
        // Handle the images here
    }

    private async Task LoadReports(string folderId)
    {
        try
        {
            HttpResponseMessage response = await ApiService.GetReportsForFolderAsync(folderId);
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to load reports.", "OK");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();

            JsonDocument jsonDocument = JsonDocument.Parse(json);
            if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataArray))
            {
                Reports.Clear();
                foreach (var element in dataArray.EnumerateArray())
                {
                    // Customize this based on the actual structure of your reports
                    Reports.Add(new Report
                    {
                        ReportId = element.GetProperty("report_id").GetString(),
                        ReportName = element.GetProperty("report_name").GetString(),
                        // Add other properties as needed
                    });
                }

                // Assuming you have a ReportsListView for displaying reports
                ReportsListView.ItemsSource ??= Reports;
            }
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Failed to parse the received data. Details: {jsonEx.Message}", "OK");
        }
        catch (FormatException formatEx)
        {
            await DisplayAlert("Error", $"Failed to format the date. Details: {formatEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unknown error occurred. Details: {ex.Message}", "OK");
        }
    }

}
