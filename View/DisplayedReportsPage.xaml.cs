using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class DisplayedReportsPage : ContentPage
{
    public ObservableCollection<Report> Reports { get; } = new();
    public ObservableCollection<ReportImage> Images { get; } = new();

    private readonly string folderId;
    private readonly string folderName;
    private readonly bool folderComplete;

    public DisplayedReportsPage(string folderId, string folderName)
    {
	    InitializeComponent();
        this.BindingContext = this; // This line sets the page's context to itself, making the Reports collection bindable in XAML.
        _ = LoadReports(folderId);
        _ = LoadImages(folderId); // Load images on page open
        this.folderId = folderId;
        this.folderName = folderName;
    }
    public DisplayedReportsPage(string folderId, string folderName, bool folderComplete)
    {
        InitializeComponent();
        this.BindingContext = this; // This line sets the page's context to itself, making the Reports collection bindable in XAML.
        _ = LoadReports(folderId);
        _ = LoadImages(folderId); // Load images on page open
        this.folderId = folderId;
        this.folderName = folderName;
        this.folderComplete = folderComplete;

        if (!folderComplete)
        {
            signTeProjectsButton.IsVisible = true;
            displayedReportsTitle.Text = "Projects To Sign";
        }
    }

    public async void ReportsBack(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    public async void ReportChosen(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Retrieve the CommandParameter, which should be the ReportId
            if (button.CommandParameter is string reportId)
            {
                Report report = await ApiService.GetReportByIdAsync(reportId);

                if (report != null)
                {
                    // Successfully retrieved the report, handle the data as needed
                    switch (report.ReportType)
                    {
                        case Enums.ReportType.BoilerHouseDataSheet:
                            await Navigation.PushModalAsync(new BoilerHouseDataSheetPage(report));
                            break;
                        case Enums.ReportType.ConformityCheck:
                            await Navigation.PushModalAsync(new ConformityCheckPage(report));
                            break;
                        case Enums.ReportType.ConstructionDesignManagement:
                            await Navigation.PushModalAsync(new ConstructionDesignManagmentPage(report));
                            break;
                        case Enums.ReportType.EngineersReport:
                            await Navigation.PushModalAsync(new EngineersReportPage(report));
                            break;
                        case Enums.ReportType.GasRiskAssessment:
                            await Navigation.PushModalAsync(new GasRiskAssessmentPage(report));
                            break;
                        case Enums.ReportType.OneA:
                            await Navigation.PushModalAsync(new OneAPage(report));
                            break;
                        case Enums.ReportType.OneB:
                            await Navigation.PushModalAsync(new OneBPage(report));
                            break;
                        case Enums.ReportType.One:
                            await Navigation.PushModalAsync(new OnePage(report));
                            break;
                        case Enums.ReportType.PressurisationUnitReport:
                            await Navigation.PushModalAsync(new PressurisationUnitReportPage(report));
                            break;
                        case Enums.ReportType.ServiceRecord:
                            await Navigation.PushModalAsync(new ServiceRecordPage(report));
                            break;
                    }
                }
                else
                {
                    // Handle the case where the report was not found or an error occurred
                    Console.WriteLine("Failed to retrieve the report.");
                }
            }
        }
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
    private async Task LoadImages(string folderId)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        try
        {
            // Call the backend method to get image paths
            List<string> imagePaths = await ApiService.GetImagePathsAsync(folderId);

            if (imagePaths == null || imagePaths.Count == 0)
            {
                return;
            }

            // Clear the current image list and load new images
            Images.Clear();
            foreach (var imagePath in imagePaths)
            {
                // Ensure the image path is a full URL and has an extension
                string fileName = Path.GetFileName(imagePath);
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    // Default to .jpg if no extension
                    fileName += ".jpg";
                }
                string fullPath = imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? imagePath
                    : $"https://ashwellmaintenance.host/images/{fileName}";

                Images.Add(new ReportImage
                {
                    ImagePath = fullPath,
                    ImageName = fileName
                });
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"Failed to load images. Details: {httpEx.Message}", "OK");
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Failed to parse the received data. Details: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unknown error occurred. Details: {ex.Message}", "OK");
        }
        finally
        {
            loadingBG.IsRunning = false;
            loading.IsRunning = false;
        }
    }

    private async void OnUploadButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // Open the file picker for images
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select an Image"
            });

            if (result != null)
            {
                // Ensure the file name has an extension
                string fileName = result.FileName;
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    // Try to infer extension from content type
                    ext = ".jpg"; // Default
                    if (!string.IsNullOrWhiteSpace(result.ContentType))
                    {
                        if (result.ContentType.Contains("png")) ext = ".png";
                        else if (result.ContentType.Contains("jpeg")) ext = ".jpg";
                        else if (result.ContentType.Contains("gif")) ext = ".gif";
                        else if (result.ContentType.Contains("bmp")) ext = ".bmp";
                        else if (result.ContentType.Contains("webp")) ext = ".webp";
                    }
                    fileName = "image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
                }

                // Read the image data as byte array
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageData = memoryStream.ToArray();

                // Now upload the image data
                await ApiService.UploadImageAsync(imageData, fileName, folderId);
                await LoadImages(folderId); // Refresh images after upload
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
