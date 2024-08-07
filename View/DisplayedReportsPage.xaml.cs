using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class DisplayedReportsPage : ContentPage
{
    public ObservableCollection<Report> Reports { get; } = new();
    private readonly string folderId;
    private readonly string folderName;
    private readonly bool folderComplete;

    public DisplayedReportsPage(string folderId, string folderName)
    {
	    InitializeComponent();
        this.BindingContext = this; // This line sets the page's context to itself, making the Reports collection bindable in XAML.
        _ = LoadReports(folderId);
        this.folderId = folderId;
        this.folderName = folderName;
    }
    public DisplayedReportsPage(string folderId, string folderName, bool folderComplete)
    {
        InitializeComponent();
        this.BindingContext = this; // This line sets the page's context to itself, making the Reports collection bindable in XAML.
        _ = LoadReports(folderId);
        this.folderId = folderId;
        this.folderName = folderName;
        this.folderComplete = folderComplete;

        if (!folderComplete)
        {
            signTeProjectsButton.IsVisible = true;
            displayedReportsTitle.Text = "Projects To Sign";
        }
    }
    public class Report
    {
        public Enums.ReportType ReportType { get; set; }
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public Dictionary<string, string> ReportData { get; set; }
        public string FolderId { get; set; }
        public string CreatedAt { get; set; }
    }

    public async void ReportsBack(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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

    private async void OnSignaturePageImagesSaved(byte[] customerSignature, byte[] engineerSignature)
    {
        await ApiService.UploadSignaturesAsync(customerSignature, engineerSignature, folderId);
        string failedReports = string.Empty;
        foreach(Report report in Reports)
        {
            try
            {
                byte[] pdfData = null;
                // For each report checks which pdf function to call based on the type
                switch (report.ReportType)
                {
                    case Enums.ReportType.BoilerHouseDataSheet:
                        pdfData = await PdfCreation.BoilerHouseDataSheet(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.ConformityCheck:
                        pdfData = await PdfCreation.ConformityCheck(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.ConstructionDesignManagement:
                        pdfData = await PdfCreation.ConstructionDesignManagement(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.EngineersReport:
                        pdfData = await PdfCreation.EngineersReport(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.GasRiskAssessment:
                        pdfData = await PdfCreation.GasRiskAssessment(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.OneA:
                        pdfData = await PdfCreation.OneA(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.OneB:
                        pdfData = await PdfCreation.OneB(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.One:
                        pdfData = await PdfCreation.One(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.PressurisationUnitReport:
                        pdfData = await PdfCreation.PressurisationReport(report.ReportData, engineerSignature, customerSignature);
                        break;
                    case Enums.ReportType.ServiceRecord:
                        pdfData = await PdfCreation.ServiceRecord(report.ReportData, engineerSignature, customerSignature);
                        break;
                }

                if (pdfData != null)
                {
                    HttpResponseMessage response = await ApiService.UploadPdfToDropboxAsync(pdfData, folderName, report.ReportName);
                    if (!response.IsSuccessStatusCode)
                        failedReports += $"Failed to upload report: {report.ReportName}\n";
                }
            }
            catch (Exception ex)
            {
                failedReports += $"Error uploading report {report.ReportName}: {ex.Message}";
            }
        }

        if(!string.IsNullOrEmpty(failedReports)) await DisplayAlert("Errors when uploading", failedReports, "OK");
        else
        {
            await DisplayAlert("Success", "Successfully uploaded signed sheets to Dropbox!", "OK");
            await Navigation.PopToRootAsync();
        }
    }

    private async Task LoadReports(string folderId)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
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
                    Reports.Add(new Report
                    {
                        ReportId = element.GetProperty("report_id").GetString(),
                        ReportName = element.GetProperty("report_name").GetString(),
                        CreatedAt = element.GetProperty("created_at").GetString(),
                        ReportData = JsonSerializer.Deserialize<Dictionary<string, string>>(element.GetProperty("report_data").GetString()),
                        ReportType = Enum.TryParse(element.GetProperty("report_type").GetString(), out Enums.ReportType parsedReportType)
                                     ? parsedReportType
                                     : Enums.ReportType.ServiceRecord // Default Type if nothing is found
                                  
                    });
                }
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
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
    }

}
