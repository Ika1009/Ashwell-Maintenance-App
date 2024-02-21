namespace Ashwell_Maintenance.View;

using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

public partial class ConstructionDesignManagmentPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public ConstructionDesignManagmentPage()
	{
		InitializeComponent();
	}
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;

        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }

    private async Task UploadReport(Folder folder, Dictionary<string, string> report)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        CDMBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ConstructionDesignManagement, reportName, folder.Id, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully uploaded new sheet.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to upload report.", "OK");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"HTTP request error. Details: {httpEx.Message}", "OK");
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Failed to parse the received data. Details: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unknown error occurred. Details: {ex.Message}", "OK");
        }

        if (!string.IsNullOrEmpty(folder.Signature1) && !string.IsNullOrEmpty(folder.Signature2))
        {
            try
            {
                byte[] signature1 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature2}");

                if (signature1 == null || signature2 == null)
                    throw new Exception("Couldn't retrieve signatures");


                byte[] pdfData = await PdfCreation.ConstructionDesignManagement(reportData, signature1, signature2);

                if (pdfData == null)
                {
                    throw new Exception("PDF didn't have a value");
                }

                HttpResponseMessage signatureResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);

                if (!signatureResponse.IsSuccessStatusCode)
                {
                    await DisplayAlert("Error", $"Failed to upload {reportName} to DropBox with already given signatures.", "OK");
                }
                await Shell.Current.DisplayAlert("Success", $"{reportName} uploaded to Dropbox successfully.", "OK");

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error processing signatures when uploading file to DropBox: {ex.Message}", "OK");
            }
        }
        loadingBG.IsRunning = false;
        loading.IsRunning = false;
        await Navigation.PopModalAsync();
    }

    public async void NewFolder(object sender, EventArgs e)
    {
        this.ShowPopup(new NewFolderPopup());  
        await LoadFolders();
    }
    private async Task LoadFolders()
    {
        try
        {
            HttpResponseMessage response = await ApiService.GetAllFoldersAsync();
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to load folders.", "OK");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();

            JsonDocument jsonDocument = JsonDocument.Parse(json);
            if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataArray))
            {
                // Clear the existing items and add the new ones directly to the ObservableCollection
                Folders.Clear();
                foreach (var element in dataArray.EnumerateArray())
                {
                    Folders.Add(new Folder
                    {
                        Id = element.GetProperty("folder_id").GetString(),
                        Name = element.GetProperty("folder_name").GetString(),
                        Timestamp = element.GetProperty("created_at").GetString(),
                        Signature1 = element.GetProperty("signature1").GetString(),
                        Signature2 = element.GetProperty("signature2").GetString()
                    });
                }

                // Check if the ItemsSource is already set
                FoldersListView.ItemsSource ??= Folders;
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

    public async void CDMBack(object sender, EventArgs e)
    {
        if (CDMSection1.IsVisible)
        {
            CDMBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
        else if (CDMSection2.IsVisible)
        {
            CDMSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CDMSection1.ScrollToAsync(0, 0, false);
            CDMSection1.IsVisible = true;
        }
        else if (CDMSection3.IsVisible)
        {
            CDMSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await CDMSection2.ScrollToAsync(0, 0, false);
            CDMSection2.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            //if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            //    await CDMSection3.ScrollToAsync(0, 0, false);
            CDMSection3.IsVisible = true;
        }
    }

    
    public async void CDMNext1(object sender, EventArgs e)
    {
        CDMSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await CDMSection2.ScrollToAsync(0, 0, false);
        CDMSection2.IsVisible = true;
    }
    
    public async void CDMNext2(object sender, EventArgs e)
    {
        CDMSection2.IsVisible = false;

        //if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        //    await CDMSection3.ScrollToAsync(0, 0, false);
        CDMSection3.IsVisible = true;
        await LoadFolders();
    }
    
    public void CDMNext3(object sender, EventArgs e)
    {
        CDMSection3.IsVisible = false;
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Construction_Design_Management_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
        //await PdfCreation.ConstructionDesignManagement(reportData);
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("siteAdress", siteAdress.Text ?? string.Empty);
        reportData.Add("client", client.Text ?? string.Empty);
        reportData.Add("responsibleSiteEngineer", responsibleSiteEngineer.Text ?? string.Empty);
        reportData.Add("otherEngineers", otherEngineers.Text ?? string.Empty);
        reportData.Add("whatInformationIssued", whatInformationIssued.Text ?? string.Empty);
        reportData.Add("startDate", startDate.Text ?? string.Empty);
        reportData.Add("other", other.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
        reportData.Add("completionDate", date.Text ?? string.Empty);
        //za buduce kometnari
        reportData.Add("ControlActionWorkingAtHeight", ControlActionWorkingAtHeight.Text ?? string.Empty);
        reportData.Add("ControlActionPermitsToWorkRequired", ControlActionPermitsToWorkRequired.Text ?? string.Empty);
        reportData.Add("ControlActionExcavations", ControlActionExcavations.Text ?? string.Empty);
        reportData.Add("DustNoiseCOSHHTheControlActionRequired", DustNoiseCOSHHTheControlActionRequired.Text ?? string.Empty);
        reportData.Add("actionTaken", actionTaken.Text ?? string.Empty);
        reportData.Add("moreDangersTheControlActionRequired", moreDangersTheControlActionRequired.Text ?? string.Empty);
        reportData.Add("ControlActionAnyOtherDanger", ControlActionAnyOtherDanger.Text ?? string.Empty);
        reportData.Add("ControlActionAppointedFirstAider", ControlActionAppointedFirstAider.Text ?? string.Empty);
        reportData.Add("ControlActionAdditionalActions", ControlActionAdditionalActions.Text ?? string.Empty);
        reportData.Add("ControlActionIsItSafe", ControlActionIsItSafe.Text ?? string.Empty);
        
       

        reportData.Add("checkWelfareFacilitiesYes", checkWelfareFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkPortableWelfareFacilitiesYes", checkPortableWelfareFacilitiesYes.IsChecked.ToString());
        reportData.Add("checkWorkingAtHeightYes", checkWorkingAtHeightYes.IsChecked.ToString());
        reportData.Add("checkPermitsToWorkRequiredYes", checkPermitsToWorkRequiredYes.IsChecked.ToString());
        reportData.Add("checkExcavationsYes", checkExcavationsYes.IsChecked.ToString());
        reportData.Add("checkDustYes", checkDustYes.IsChecked.ToString());
        reportData.Add("checkNoiseYes", checkNoiseYes.IsChecked.ToString());
        reportData.Add("checkCOSHHYes", checkCOSHHYes.IsChecked.ToString());
        reportData.Add("checkOtherYes", checkOtherYes.IsChecked.ToString());
        reportData.Add("checkManagementSurveyYes", checkManagementSurveyYes.IsChecked.ToString());
        reportData.Add("checkFiveYearsSurveyYes", checkFiveYearsSurveyYes.IsChecked.ToString());
        reportData.Add("checkElectricalYes", checkElectricalYes.IsChecked.ToString());
        reportData.Add("checkGasYes", checkGasYes.IsChecked.ToString());
        reportData.Add("checkWaterYes", checkWaterYes.IsChecked.ToString());
        reportData.Add("checkOtherServicesYes", checkOtherServicesYes.IsChecked.ToString());
        reportData.Add("checkDangerToOthersYes", checkDangerToOthersYes.IsChecked.ToString());
        reportData.Add("checkDangerToPublicYes", checkDangerToPublicYes.IsChecked.ToString());
        reportData.Add("checkOtherDangersYes", checkOtherDangersYes.IsChecked.ToString());
        reportData.Add("checkAnyOtherDangerYes", checkAnyOtherDangerYes.IsChecked.ToString());
        reportData.Add("checkHotWorksYes", checkHotWorksYes.IsChecked.ToString());
        reportData.Add("checkAppointedFirstAiderYes", checkAppointedFirstAiderYes.IsChecked.ToString());
        reportData.Add("checkAdditionalActionsYes", checkAdditionalActionsYes.IsChecked.ToString());
        reportData.Add("checkIsItSafeYes", checkIsItSafeYes.IsChecked.ToString());
     

        return reportData;
    }
}