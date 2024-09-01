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
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (folderName == null || folderName == "") // User clicked Cancel
            return;

        try
        {
            var response = await ApiService.UploadFolderAsync(folderName);

            if (response.IsSuccessStatusCode)
            {
                // Load folders after successful upload
                await LoadFolders();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                string errorMessage;
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                        errorMessage = errorObj.ContainsKey("error") ? errorObj["error"] : "An unknown error occurred.";
                    }
                    catch
                    {
                        errorMessage = "An unknown error occurred.";
                    }
                }
                else
                    errorMessage = "Internal server error.";

                await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            // Handle other potential exceptions like network errors, timeouts, etc.
            await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
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
    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                string searchText = e.NewTextValue;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // If search text is empty, load all folders
                    if (FoldersListView != null && Folders != null)
                    {
                        FoldersListView.ItemsSource = null;
                        FoldersListView.ItemsSource = Folders;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Folders or FoldersListView is null.", "OK");
                    }
                }
                else
                {
                    // Filter folders based on search text
                    if (Folders != null)
                    {
                        List<Folder> filteredFolders = new List<Folder>(Folders.Where(folder => folder.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
                        // Update the ItemsSource with filtered folders
                        if (FoldersListView != null)
                        {
                            FoldersListView.ItemsSource = null;
                            FoldersListView.ItemsSource = filteredFolders;
                        }
                        else
                        {
                            await DisplayAlert("Error", "FoldersListView is null.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Folders is null.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");

            }
        });


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
    public void previewConstructionDesignManagmentPage(Dictionary<string,string> reportData)
    {
        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("siteAdress"))
            siteAdress.Text = reportData["siteAdress"];
        if (reportData.ContainsKey("client"))
            client.Text = reportData["client"];
        if (reportData.ContainsKey("responsibleSiteEngineer"))
            responsibleSiteEngineer.Text = reportData["responsibleSiteEngineer"];
        if (reportData.ContainsKey("otherEngineers"))
            otherEngineers.Text = reportData["otherEngineers"];
        if (reportData.ContainsKey("whatInformationIssued"))
            whatInformationIssued.Text = reportData["whatInformationIssued"];
        if (reportData.ContainsKey("startDate"))
            startDate.Date = DateTime.Parse(reportData["startDate"]);
        if (reportData.ContainsKey("other"))
            other.Text = reportData["other"];
        if (reportData.ContainsKey("date"))
            date.Date = DateTime.Parse(reportData["date"]);
        if (reportData.ContainsKey("completionDate"))
            completionDate.Date = DateTime.Parse(reportData["completionDate"]);


        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("ControlActionWorkingAtHeight"))
            ControlActionWorkingAtHeight.Text = reportData["ControlActionWorkingAtHeight"];
        if (reportData.ContainsKey("ControlActionPermitsToWorkRequired"))
            ControlActionPermitsToWorkRequired.Text = reportData["ControlActionPermitsToWorkRequired"];
        if (reportData.ContainsKey("ControlActionExcavations"))
            ControlActionExcavations.Text = reportData["ControlActionExcavations"];
        if (reportData.ContainsKey("DustNoiseCOSHHTheControlActionRequired"))
            DustNoiseCOSHHTheControlActionRequired.Text = reportData["DustNoiseCOSHHTheControlActionRequired"];
        if (reportData.ContainsKey("actionTaken"))
            actionTaken.Text = reportData["actionTaken"];
        if (reportData.ContainsKey("moreDangersTheControlActionRequired"))
            moreDangersTheControlActionRequired.Text = reportData["moreDangersTheControlActionRequired"];
        if (reportData.ContainsKey("ControlActionAnyOtherDanger"))
            ControlActionAnyOtherDanger.Text = reportData["ControlActionAnyOtherDanger"];
        if (reportData.ContainsKey("ControlActionAppointedFirstAider"))
            ControlActionAppointedFirstAider.Text = reportData["ControlActionAppointedFirstAider"];
        if (reportData.ContainsKey("ControlActionAdditionalActions"))
            ControlActionAdditionalActions.Text = reportData["ControlActionAdditionalActions"];
        if (reportData.ContainsKey("ControlActionIsItSafe"))
            ControlActionIsItSafe.Text = reportData["ControlActionIsItSafe"];
        // Assume 'reportData' is the dictionary containing the data
        if (reportData.ContainsKey("checkWelfareFacilitiesYes"))
            checkWelfareFacilitiesYes.IsChecked = bool.Parse(reportData["checkWelfareFacilitiesYes"]);
        if (reportData.ContainsKey("checkPortableWelfareFacilitiesYes"))
            checkPortableWelfareFacilitiesYes.IsChecked = bool.Parse(reportData["checkPortableWelfareFacilitiesYes"]);
        if (reportData.ContainsKey("checkWorkingAtHeightYes"))
            checkWorkingAtHeightYes.IsChecked = bool.Parse(reportData["checkWorkingAtHeightYes"]);
        if (reportData.ContainsKey("checkPermitsToWorkRequiredYes"))
            checkPermitsToWorkRequiredYes.IsChecked = bool.Parse(reportData["checkPermitsToWorkRequiredYes"]);
        if (reportData.ContainsKey("checkExcavationsYes"))
            checkExcavationsYes.IsChecked = bool.Parse(reportData["checkExcavationsYes"]);
        if (reportData.ContainsKey("checkDustYes"))
            checkDustYes.IsChecked = bool.Parse(reportData["checkDustYes"]);
        if (reportData.ContainsKey("checkNoiseYes"))
            checkNoiseYes.IsChecked = bool.Parse(reportData["checkNoiseYes"]);
        if (reportData.ContainsKey("checkCOSHHYes"))
            checkCOSHHYes.IsChecked = bool.Parse(reportData["checkCOSHHYes"]);
        if (reportData.ContainsKey("checkOtherYes"))
            checkOtherYes.IsChecked = bool.Parse(reportData["checkOtherYes"]);
        if (reportData.ContainsKey("checkManagementSurveyYes"))
            checkManagementSurveyYes.IsChecked = bool.Parse(reportData["checkManagementSurveyYes"]);
        if (reportData.ContainsKey("checkFiveYearsSurveyYes"))
            checkFiveYearsSurveyYes.IsChecked = bool.Parse(reportData["checkFiveYearsSurveyYes"]);
        if (reportData.ContainsKey("checkElectricalYes"))
            checkElectricalYes.IsChecked = bool.Parse(reportData["checkElectricalYes"]);
        if (reportData.ContainsKey("checkGasYes"))
            checkGasYes.IsChecked = bool.Parse(reportData["checkGasYes"]);
        if (reportData.ContainsKey("checkWaterYes"))
            checkWaterYes.IsChecked = bool.Parse(reportData["checkWaterYes"]);
        if (reportData.ContainsKey("checkOtherServicesYes"))
            checkOtherServicesYes.IsChecked = bool.Parse(reportData["checkOtherServicesYes"]);
        if (reportData.ContainsKey("checkDangerToOthersYes"))
            checkDangerToOthersYes.IsChecked = bool.Parse(reportData["checkDangerToOthersYes"]);
        if (reportData.ContainsKey("checkDangerToPublicYes"))
            checkDangerToPublicYes.IsChecked = bool.Parse(reportData["checkDangerToPublicYes"]);
        if (reportData.ContainsKey("checkOtherDangersYes"))
            checkOtherDangersYes.IsChecked = bool.Parse(reportData["checkOtherDangersYes"]);
        if (reportData.ContainsKey("checkAnyOtherDangerYes"))
            checkAnyOtherDangerYes.IsChecked = bool.Parse(reportData["checkAnyOtherDangerYes"]);
        if (reportData.ContainsKey("checkHotWorksYes"))
            checkHotWorksYes.IsChecked = bool.Parse(reportData["checkHotWorksYes"]);
        if (reportData.ContainsKey("checkAppointedFirstAiderYes"))
            checkAppointedFirstAiderYes.IsChecked = bool.Parse(reportData["checkAppointedFirstAiderYes"]);
        if (reportData.ContainsKey("checkAdditionalActionsYes"))
            checkAdditionalActionsYes.IsChecked = bool.Parse(reportData["checkAdditionalActionsYes"]);
        if (reportData.ContainsKey("checkIsItSafeYes"))
            checkIsItSafeYes.IsChecked = bool.Parse(reportData["checkIsItSafeYes"]);

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
        reportData.Add("startDate", startDate.Date.ToString("d/M/yyyy") ?? string.Empty);
        reportData.Add("other", other.Text ?? string.Empty);
        reportData.Add("date", date.Date.ToString("d/M/yyyy") ?? string.Empty);
        reportData.Add("completionDate", completionDate.Date.ToString("d/M/yyyy") ?? string.Empty);//mozda treba date umesto completion date
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