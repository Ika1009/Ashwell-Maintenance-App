using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class GasRiskAssessmentPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public GasRiskAssessmentPage()
	{
		InitializeComponent();

        checkGeneralMeterConditionNA.IsChecked = true;
        checkEarthBondingNA.IsChecked = true;
        checkEmergencyControlsNA.IsChecked = true;
        checkMeterVentilationNA.IsChecked = true;
        checkGasLineDiagramNA.IsChecked = true;
        checkEmergencyContractNumberNA.IsChecked = true;
        checkNoticesAndLabelsNA.IsChecked = true;
        checkGeneralPipeworkConditionNA.IsChecked = true;
        checkPipeworkIdentifiedNA.IsChecked = true;
        checkPipeworkBuriedNA.IsChecked = true;
        checkPipeworkSurfaceNA.IsChecked = true;
        checkPipeworkEarthBondingNA.IsChecked = true;
        checkJointingMethodsNA.IsChecked = true;
        checkPipeworkSupportsNA.IsChecked = true;
        checkFixingsNA.IsChecked = true;
        checkSupportSepparationDistancesNA.IsChecked = true;
        checkPipeworkInVoidsNA.IsChecked = true;
        checkPipeSleevesNA.IsChecked = true;
        checkPipeSleevesSealedNA.IsChecked = true;
        checkServiceValvesNA.IsChecked = true;
        checkAdditionalEmergencyControlValvesNA.IsChecked = true;
        checkIsolationValveNA.IsChecked = true;
        checkTestPointNA.IsChecked = true;
        checkPurgePointsNA.IsChecked = true;
        checkGeneralPipeworkConditionNA.IsChecked = true;
        checkinstallationSafeToOperateNA.IsChecked = true;
        checkWarningNoticeIssuedNA.IsChecked = true;
        checkGasTightnessTestRecommendedNA.IsChecked = true;
        checkGuessTightnessTestCarriedOutNA.IsChecked = true;
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
        GasRiskAssessmentBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.GasRiskAssessment, reportName, folder.Id, report);

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

                byte[] pdfData = await PdfCreation.GasRiskAssessment(reportData, signature1, signature2);

                if (pdfData != null)
                {
                    HttpResponseMessage signatureResponse = await ApiService.UploadPdfToDropboxAsync(pdfData, folder.Name, reportName);

                    if (!signatureResponse.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Error", $"Failed to upload {reportName} to DropBox with already given signatures.", "OK");
                    }
                }
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

    public async void GasRiskAssessmentBack(object sender, EventArgs e)
	{
		if (GRASection1.IsVisible)
        {
            GasRiskAssessmentBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
		else if (GRASection2.IsVisible)
		{
			GRASection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await GRASection1.ScrollToAsync(0, 0, false);
			GRASection1.IsVisible = true;
		}
		else if (GRASection3.IsVisible)
		{
            GRASection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await GRASection2.ScrollToAsync(0, 0, false);
            GRASection2.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await GRASection3.ScrollToAsync(0, 0, false);
            GRASection3.IsVisible = true;

            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;
        }
	}

    
    public async void GasRiskAssessmentNext1(object sender, EventArgs e)
	{
		GRASection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await GRASection2.ScrollToAsync(0, 0, false);
		GRASection2.IsVisible = true;
	}
    
    public async void GasRiskAssessmentNext2(object sender, EventArgs e)
    {
        GRASection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await GRASection3.ScrollToAsync(0, 0, false);
        GRASection3.IsVisible = true;
        await LoadFolders();
    }

    public async void GasRiskAssessmentNext3(object sender, EventArgs e)
    {
        GRASection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Gas_Risk_Assessment_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.GasRisk(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());\
        reportData.Add("nameAndSiteAdress", nameAndSiteAdress.Text ?? string.Empty);
        reportData.Add("client", client.Text ?? string.Empty);
        reportData.Add("meterLocation", meterLocation.Text ?? string.Empty);
        reportData.Add("commentsOnOverallMeter", commentsOnOverallMeter.Text ?? string.Empty);
        reportData.Add("meterComments", meterComments.Text ?? string.Empty);
        reportData.Add("pipeworkLocation", pipeworkLocation.Text ?? string.Empty);
        reportData.Add("commentsOnOverallPipework", commentsOnOverallPipework.Text ?? string.Empty);
        reportData.Add("pipeworkComments", pipeworkComments.Text ?? string.Empty);
        reportData.Add("reasonForWarningNotice", reasonForWarningNotice.Text ?? string.Empty);
        reportData.Add("warningNoticeRefNo", warningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("dateOfLastTightnessTest", dateOfLastTightnessTest.Text ?? string.Empty);
        //reportData.Add("recordTightnessTestResult", recordTightnessTestResult.Text ?? string.Empty);  - promenjeno po zahtevu u check-boxes
        reportData.Add("dropRecorded", dropRecorded.Text ?? string.Empty);
        reportData.Add("engineersName", engineersName.Text ?? string.Empty);
        //reportData.Add("engineersSignature", engineersSignature.Text ?? string.Empty);
        reportData.Add("gasSafeOperativeIdNo", gasSafeOperativeIdNo.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        //reportData.Add("clientsSignature", clientsSignature.Text ?? string.Empty);
        reportData.Add("completionDate", completionDate.Text ?? string.Empty);





        reportData.Add("checkInternalMeter", checkInternalMeter.IsChecked.ToString());
        reportData.Add("checkExternalMeter", checkExternalMeter.IsChecked.ToString());
        reportData.Add("checkGeneralMeterConditionYes", checkGeneralMeterConditionYes.IsChecked.ToString());
        reportData.Add("checkGeneralMeterConditionNo", checkGeneralMeterConditionNo.IsChecked.ToString());
        reportData.Add("checkEarthBondingYes", checkEarthBondingYes.IsChecked.ToString());
        reportData.Add("checkEarthBondingNo", checkEarthBondingNo.IsChecked.ToString());
        reportData.Add("checkEmergencyControlsYes", checkEmergencyControlsYes.IsChecked.ToString());
        reportData.Add("checkEmergencyControlsNo", checkEmergencyControlsNo.IsChecked.ToString());
        reportData.Add("checkMeterVentilationYes", checkMeterVentilationYes.IsChecked.ToString());
        reportData.Add("checkMeterVentilationNo", checkMeterVentilationNo.IsChecked.ToString());
        reportData.Add("checkGasLineDiagramYes", checkGasLineDiagramYes.IsChecked.ToString());
        reportData.Add("checkGasLineDiagramNo", checkGasLineDiagramNo.IsChecked.ToString());
        reportData.Add("checkEmergencyContractNumberYes", checkEmergencyContractNumberYes.IsChecked.ToString());
        reportData.Add("checkEmergencyContractNumberNo", checkEmergencyContractNumberNo.IsChecked.ToString());
        //reportData.Add("checkGeneralPipeworkConditionYes", checkGeneralPipeworkConditionYes.IsChecked.ToString());
        //reportData.Add("checkGeneralPipeworkConditionNo", checkGeneralPipeworkConditionNo.IsChecked.ToString());
        reportData.Add("checkPipeworkIdentifiedYes", checkPipeworkIdentifiedYes.IsChecked.ToString());
        reportData.Add("checkPipeworkIdentifiedNo", checkPipeworkIdentifiedNo.IsChecked.ToString());
        reportData.Add("checkPipeworkBuriedYes", checkPipeworkBuriedYes.IsChecked.ToString());
        reportData.Add("checkPipeworkBuriedNo", checkPipeworkBuriedNo.IsChecked.ToString());
        reportData.Add("checkPipeworkSurfaceYes", checkPipeworkSurfaceYes.IsChecked.ToString());
        reportData.Add("checkPipeworkSurfaceNo", checkPipeworkSurfaceNo.IsChecked.ToString());
        reportData.Add("checkPipeworkEarthBondingYes", checkPipeworkEarthBondingYes.IsChecked.ToString());
        reportData.Add("checkPipeworkEarthBondingNo", checkPipeworkEarthBondingNo.IsChecked.ToString());
        reportData.Add("checkJointingMethodsYes", checkJointingMethodsYes.IsChecked.ToString());
        reportData.Add("checkJointingMethodsNo", checkJointingMethodsNo.IsChecked.ToString());
        reportData.Add("checkPipeworkSupportsYes", checkPipeworkSupportsYes.IsChecked.ToString());
        reportData.Add("checkPipeworkSupportsNo", checkPipeworkSupportsNo.IsChecked.ToString());
        reportData.Add("checkFixingsYes", checkFixingsYes.IsChecked.ToString());
        reportData.Add("checkFixingsNo", checkFixingsNo.IsChecked.ToString());
        reportData.Add("checkSupportSepparationDistancesYes", checkSupportSepparationDistancesYes.IsChecked.ToString());
        reportData.Add("checkSupportSepparationDistancesNo", checkSupportSepparationDistancesNo.IsChecked.ToString());
        reportData.Add("checkPipeworkInVoidsYes", checkPipeworkInVoidsYes.IsChecked.ToString());
        reportData.Add("checkPipeworkInVoidsNo", checkPipeworkInVoidsNo.IsChecked.ToString());
        reportData.Add("checkPipeSleevesYes", checkPipeSleevesYes.IsChecked.ToString());
        reportData.Add("checkPipeSleevesNo", checkPipeSleevesNo.IsChecked.ToString());
        reportData.Add("checkPipeSleevesSealedYes", checkPipeSleevesSealedYes.IsChecked.ToString());
        reportData.Add("checkPipeSleevesSealedNo", checkPipeSleevesSealedNo.IsChecked.ToString());
        reportData.Add("checkServiceValvesYes", checkServiceValvesYes.IsChecked.ToString());
        reportData.Add("checkServiceValvesNo", checkServiceValvesNo.IsChecked.ToString());
        reportData.Add("checkAdditionalEmergencyControlValvesYes", checkAdditionalEmergencyControlValvesYes.IsChecked.ToString());
        reportData.Add("checkAdditionalEmergencyControlValvesNo", checkAdditionalEmergencyControlValvesNo.IsChecked.ToString());
        reportData.Add("checkIsolationValveYes", checkIsolationValveYes.IsChecked.ToString());
        reportData.Add("checkIsolationValveNo", checkIsolationValveNo.IsChecked.ToString());
        reportData.Add("checkTestPointYes", checkTestPointYes.IsChecked.ToString());
        reportData.Add("checkTestPointNo", checkTestPointNo.IsChecked.ToString());
        reportData.Add("checkPurgePointsYes", checkPurgePointsYes.IsChecked.ToString());
        reportData.Add("checkPurgePointsNo", checkPurgePointsNo.IsChecked.ToString());
        reportData.Add("checkGeneralPipeworkConditionYes", checkGeneralPipeworkConditionYes.IsChecked.ToString());
        reportData.Add("checkGeneralPipeworkConditionNo", checkGeneralPipeworkConditionNo.IsChecked.ToString());
        reportData.Add("checkinstallationSafeToOperateYes", checkinstallationSafeToOperateYes.IsChecked.ToString());
        reportData.Add("checkinstallationSafeToOperateNo", checkinstallationSafeToOperateNo.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssuedYes", checkWarningNoticeIssuedYes.IsChecked.ToString());
        reportData.Add("checkWarningNoticeIssuedNo", checkWarningNoticeIssuedNo.IsChecked.ToString());
        reportData.Add("checkGasTightnessTestRecommendedYes", checkGasTightnessTestRecommendedYes.IsChecked.ToString());
        reportData.Add("checkGasTightnessTestRecommendedNo", checkGasTightnessTestRecommendedNo.IsChecked.ToString());
        reportData.Add("checkGuessTightnessTestCarriedOutYes", checkGuessTightnessTestCarriedOutYes.IsChecked.ToString());
        reportData.Add("checkGuessTightnessTestCarriedOutNo", checkGuessTightnessTestCarriedOutNo.IsChecked.ToString());

        reportData.Add("checkNoticesAndLabelsYes", checkNoticesAndLabelsYes.IsChecked.ToString());
        reportData.Add("checkNoticesAndLabelsNo", checkNoticesAndLabelsNo.IsChecked.ToString());



        return reportData;
    }


    // Disjunct Buttons


    public void DisjunctCheckboxes(CheckBox a, CheckBox b, CheckBox c)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;

        a.Color = Colors.Red;
        b.Color = Colors.White;
        c.Color = Colors.White;
    }

    public void CheckGeneralMeterConditionYesChanged(object sender, EventArgs e)
    {
        if (checkGeneralMeterConditionYes.IsChecked)
            DisjunctCheckboxes(checkGeneralMeterConditionYes, checkGeneralMeterConditionNo, checkGeneralMeterConditionNA);
        else
        {
            checkGeneralMeterConditionYes.Color = Colors.White;
            if (!checkGeneralMeterConditionNo.IsChecked)
                DisjunctCheckboxes(checkGeneralMeterConditionNA, checkGeneralMeterConditionYes, checkGeneralMeterConditionNo);
        }
    }
    public void CheckGeneralMeterConditionNoChanged(object sender, EventArgs e)
    {
        if (checkGeneralMeterConditionNo.IsChecked)
        {
            DisjunctCheckboxes(checkGeneralMeterConditionNo, checkGeneralMeterConditionYes, checkGeneralMeterConditionNA);
            generalMeterConditionNo.TextColor = Colors.Red;
        }
        else
        {
            checkGeneralMeterConditionNo.Color = Colors.White;
            if (!checkGeneralMeterConditionYes.IsChecked)
                DisjunctCheckboxes(checkGeneralMeterConditionNA, checkGeneralMeterConditionYes, checkGeneralMeterConditionNo);
        }
    }
    public void CheckGeneralMeterConditionNAChanged(object sender, EventArgs e)
    {
        if (checkGeneralMeterConditionNA.IsChecked || !checkGeneralMeterConditionYes.IsChecked && !checkGeneralMeterConditionNo.IsChecked)
            DisjunctCheckboxes(checkGeneralMeterConditionNA, checkGeneralMeterConditionYes, checkGeneralMeterConditionNo);
        else
            checkGeneralMeterConditionNA.Color = Colors.White;
    }




    public void CheckEarthBondingYesChanged(object sender, EventArgs e)
    {
        if (checkEarthBondingYes.IsChecked)
            DisjunctCheckboxes(checkEarthBondingYes, checkEarthBondingNo, checkEarthBondingNA);
        else
        {
            checkEarthBondingYes.Color = Colors.White;
            if (!checkEarthBondingNo.IsChecked)
                DisjunctCheckboxes(checkEarthBondingNA, checkEarthBondingYes, checkEarthBondingNo);
        }
    }
    public void CheckEarthBondingNoChanged(object sender, EventArgs e)
    {
        if (checkEarthBondingNo.IsChecked)
            DisjunctCheckboxes(checkEarthBondingNo, checkEarthBondingYes, checkEarthBondingNA);
        else
        {
            checkEarthBondingNo.Color = Colors.White;
            if (!checkEarthBondingYes.IsChecked)
                DisjunctCheckboxes(checkEarthBondingNA, checkEarthBondingYes, checkEarthBondingNo);
        }
    }
    public void CheckEarthBondingNAChanged(object sender, EventArgs e)
    {
        if (checkEarthBondingNA.IsChecked || !checkEarthBondingYes.IsChecked && !checkEarthBondingNo.IsChecked)
            DisjunctCheckboxes(checkEarthBondingNA, checkEarthBondingYes, checkEarthBondingNo);
        else
            checkEarthBondingNA.Color = Colors.White;
    }




    public void CheckEmergencyControlsYesChanged(object sender, EventArgs e)
    {
        if (checkEmergencyControlsYes.IsChecked)
            DisjunctCheckboxes(checkEmergencyControlsYes, checkEmergencyControlsNo, checkEmergencyControlsNA);
        else
        {
            checkEmergencyControlsYes.Color = Colors.White;
            if (!checkEmergencyControlsNo.IsChecked)
                DisjunctCheckboxes(checkEmergencyControlsNA, checkEmergencyControlsYes, checkEmergencyControlsNo);
        }
    }
    public void CheckEmergencyControlsNoChanged(object sender, EventArgs e)
    {
        if (checkEmergencyControlsNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyControlsNo, checkEmergencyControlsYes, checkEmergencyControlsNA);
        else
        {
            checkEmergencyControlsNo.Color = Colors.White;
            if (!checkEmergencyControlsYes.IsChecked)
                DisjunctCheckboxes(checkEmergencyControlsNA, checkEmergencyControlsYes, checkEmergencyControlsNo);
        }
    }
    public void CheckEmergencyControlsNAChanged(object sender, EventArgs e)
    {
        if (checkEmergencyControlsNA.IsChecked || !checkEmergencyControlsYes.IsChecked && !checkEmergencyControlsNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyControlsNA, checkEmergencyControlsYes, checkEmergencyControlsNo);
        else
            checkEmergencyControlsNA.Color = Colors.White;
    }



    public void CheckRecordTightnessTestResultYesChanged(object sender, EventArgs e)
    {
        if (checkRecordTightnessTestResultYes.IsChecked)
            DisjunctCheckboxes(checkRecordTightnessTestResultYes, checkRecordTightnessTestResultNo, checkRecordTightnessTestResultNA);
        else
        {
            checkRecordTightnessTestResultYes.Color = Colors.White;
            if (!checkRecordTightnessTestResultNo.IsChecked)
                DisjunctCheckboxes(checkRecordTightnessTestResultNA, checkRecordTightnessTestResultYes, checkRecordTightnessTestResultNo);
        }
    }
    public void CheckRecordTightnessTestResultNoChanged(object sender, EventArgs e)
    {
        if (checkRecordTightnessTestResultNo.IsChecked)
            DisjunctCheckboxes(checkRecordTightnessTestResultNo, checkRecordTightnessTestResultYes, checkRecordTightnessTestResultNA);
        else
        {
            checkRecordTightnessTestResultNo.Color = Colors.White;
            if (!checkRecordTightnessTestResultYes.IsChecked)
                DisjunctCheckboxes(checkRecordTightnessTestResultNA, checkRecordTightnessTestResultYes, checkRecordTightnessTestResultNo);
        }
    }
    public void CheckRecordTightnessTestResultNAChanged(object sender, EventArgs e)
    {
        if (checkRecordTightnessTestResultNA.IsChecked || !checkRecordTightnessTestResultYes.IsChecked && !checkRecordTightnessTestResultNo.IsChecked)
            DisjunctCheckboxes(checkRecordTightnessTestResultNA, checkRecordTightnessTestResultYes, checkRecordTightnessTestResultNo);
        else
            checkRecordTightnessTestResultNA.Color = Colors.White;
    }




    public void CheckMeterVentilationYesChanged(object sender, EventArgs e)
    {
        if (checkMeterVentilationYes.IsChecked)
            DisjunctCheckboxes(checkMeterVentilationYes, checkMeterVentilationNo, checkMeterVentilationNA);
        else
        {
            checkMeterVentilationYes.Color = Colors.White;
            if (!checkMeterVentilationNo.IsChecked)
                DisjunctCheckboxes(checkMeterVentilationNA, checkMeterVentilationYes, checkMeterVentilationNo);
        }
    }
    public void CheckMeterVentilationNoChanged(object sender, EventArgs e)
    {
        if (checkMeterVentilationNo.IsChecked)
            DisjunctCheckboxes(checkMeterVentilationNo, checkMeterVentilationYes, checkMeterVentilationNA);
        else
        {
            checkMeterVentilationNo.Color = Colors.White;
            if (!checkMeterVentilationYes.IsChecked)
                DisjunctCheckboxes(checkMeterVentilationNA, checkMeterVentilationYes, checkMeterVentilationNo);
        }
    }
    public void CheckMeterVentilationNAChanged(object sender, EventArgs e)
    {
        if (checkMeterVentilationNA.IsChecked || !checkMeterVentilationYes.IsChecked && !checkMeterVentilationNo.IsChecked)
            DisjunctCheckboxes(checkMeterVentilationNA, checkMeterVentilationYes, checkMeterVentilationNo);
        else
            checkMeterVentilationNA.Color = Colors.White;
    }




    public void CheckGasLineDiagramYesChanged(object sender, EventArgs e)
    {
        if (checkGasLineDiagramYes.IsChecked)
            DisjunctCheckboxes(checkGasLineDiagramYes, checkGasLineDiagramNo, checkGasLineDiagramNA);
        else
        {
            checkGasLineDiagramYes.Color = Colors.White;
            if (!checkGasLineDiagramNo.IsChecked)
                DisjunctCheckboxes(checkGasLineDiagramNA, checkGasLineDiagramYes, checkGasLineDiagramNo);
        }
    }
    public void CheckGasLineDiagramNoChanged(object sender, EventArgs e)
    {
        if (checkGasLineDiagramNo.IsChecked)
            DisjunctCheckboxes(checkGasLineDiagramNo, checkGasLineDiagramYes, checkGasLineDiagramNA);
        else
        {
            checkGasLineDiagramNo.Color = Colors.White;
            if (!checkGasLineDiagramYes.IsChecked)
                DisjunctCheckboxes(checkGasLineDiagramNA, checkGasLineDiagramYes, checkGasLineDiagramNo);
        }
    }
    public void CheckGasLineDiagramNAChanged(object sender, EventArgs e)
    {
        if (checkGasLineDiagramNA.IsChecked || !checkGasLineDiagramYes.IsChecked && !checkGasLineDiagramNo.IsChecked)
            DisjunctCheckboxes(checkGasLineDiagramNA, checkGasLineDiagramYes, checkGasLineDiagramNo);
        else
            checkGasLineDiagramNA.Color = Colors.White;
    }




    public void CheckEmergencyContractNumberYesChanged(object sender, EventArgs e)
    {
        if (checkEmergencyContractNumberYes.IsChecked)
            DisjunctCheckboxes(checkEmergencyContractNumberYes, checkEmergencyContractNumberNo, checkEmergencyContractNumberNA);
        else
        {
            checkEmergencyContractNumberYes.Color = Colors.White;
            if (!checkEmergencyContractNumberNo.IsChecked)
                DisjunctCheckboxes(checkEmergencyContractNumberNA, checkEmergencyContractNumberYes, checkEmergencyContractNumberNo);
        }
    }
    public void CheckEmergencyContractNumberNoChanged(object sender, EventArgs e)
    {
        if (checkEmergencyContractNumberNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyContractNumberNo, checkEmergencyContractNumberYes, checkEmergencyContractNumberNA);
        else
        {
            checkEmergencyContractNumberNo.Color = Colors.White;
            if (!checkEmergencyContractNumberYes.IsChecked)
                DisjunctCheckboxes(checkEmergencyContractNumberNA, checkEmergencyContractNumberYes, checkEmergencyContractNumberNo);
        }
    }
    public void CheckEmergencyContractNumberNAChanged(object sender, EventArgs e)
    {
        if (checkEmergencyContractNumberNA.IsChecked || !checkEmergencyContractNumberYes.IsChecked && !checkEmergencyContractNumberNo.IsChecked)
            DisjunctCheckboxes(checkEmergencyContractNumberNA, checkEmergencyContractNumberYes, checkEmergencyContractNumberNo);
        else
            checkEmergencyContractNumberNA.Color = Colors.White;
    }




    public void CheckNoticesAndLabelsYesChanged(object sender, EventArgs e)
    {
        if (checkNoticesAndLabelsYes.IsChecked)
            DisjunctCheckboxes(checkNoticesAndLabelsYes, checkNoticesAndLabelsNo, checkNoticesAndLabelsNA);
        else
        {
            checkNoticesAndLabelsYes.Color = Colors.White;
            if (!checkNoticesAndLabelsNo.IsChecked)
                DisjunctCheckboxes(checkNoticesAndLabelsNA, checkNoticesAndLabelsYes, checkNoticesAndLabelsNo);
        }
    }
    public void CheckNoticesAndLabelsNoChanged(object sender, EventArgs e)
    {
        if (checkNoticesAndLabelsNo.IsChecked)
            DisjunctCheckboxes(checkNoticesAndLabelsNo, checkNoticesAndLabelsYes, checkNoticesAndLabelsNA);
        else
        {
            checkNoticesAndLabelsNo.Color = Colors.White;
            if (!checkNoticesAndLabelsYes.IsChecked)
                DisjunctCheckboxes(checkNoticesAndLabelsNA, checkNoticesAndLabelsYes, checkNoticesAndLabelsNo);
        }
    }
    public void CheckNoticesAndLabelsNAChanged(object sender, EventArgs e)
    {
        if (checkNoticesAndLabelsNA.IsChecked || !checkNoticesAndLabelsYes.IsChecked && !checkNoticesAndLabelsNo.IsChecked)
            DisjunctCheckboxes(checkNoticesAndLabelsNA, checkNoticesAndLabelsYes, checkNoticesAndLabelsNo);
        else
            checkNoticesAndLabelsNA.Color = Colors.White;
    }




    public void CheckPipeworkIdentifiedYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkIdentifiedYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkIdentifiedYes, checkPipeworkIdentifiedNo, checkPipeworkIdentifiedNA);
        else
        {
            checkPipeworkIdentifiedYes.Color = Colors.White;
            if (!checkPipeworkIdentifiedNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkIdentifiedNA, checkPipeworkIdentifiedYes, checkPipeworkIdentifiedNo);
        }
    }
    public void CheckPipeworkIdentifiedNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkIdentifiedNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkIdentifiedNo, checkPipeworkIdentifiedYes, checkPipeworkIdentifiedNA);
        else
        {
            checkPipeworkIdentifiedNo.Color = Colors.White;
            if (!checkPipeworkIdentifiedYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkIdentifiedNA, checkPipeworkIdentifiedYes, checkPipeworkIdentifiedNo);
        }
    }
    public void CheckPipeworkIdentifiedNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkIdentifiedNA.IsChecked || !checkPipeworkIdentifiedYes.IsChecked && !checkPipeworkIdentifiedNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkIdentifiedNA, checkPipeworkIdentifiedYes, checkPipeworkIdentifiedNo);
        else
            checkPipeworkIdentifiedNA.Color = Colors.White;
    }




    public void CheckPipeworkBuriedYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkBuriedYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkBuriedYes, checkPipeworkBuriedNo, checkPipeworkBuriedNA);
        else
        {
            checkPipeworkBuriedYes.Color = Colors.White;
            if (!checkPipeworkBuriedNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkBuriedNA, checkPipeworkBuriedYes, checkPipeworkBuriedNo);
        }
    }
    public void CheckPipeworkBuriedNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkBuriedNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkBuriedNo, checkPipeworkBuriedYes, checkPipeworkBuriedNA);
        else
        {
            checkPipeworkBuriedNo.Color = Colors.White;
            if (!checkPipeworkBuriedYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkBuriedNA, checkPipeworkBuriedYes, checkPipeworkBuriedNo);
        }
    }
    public void CheckPipeworkBuriedNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkBuriedNA.IsChecked || !checkPipeworkBuriedYes.IsChecked && !checkPipeworkBuriedNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkBuriedNA, checkPipeworkBuriedYes, checkPipeworkBuriedNo);
        else
            checkPipeworkBuriedNA.Color = Colors.White;
    }




    public void CheckPipeworkSurfaceYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSurfaceYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkSurfaceYes, checkPipeworkSurfaceNo, checkPipeworkSurfaceNA);
        else
        {
            checkPipeworkSurfaceYes.Color = Colors.White;
            if (!checkPipeworkSurfaceNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkSurfaceNA, checkPipeworkSurfaceYes, checkPipeworkSurfaceNo);
        }
    }
    public void CheckPipeworkSurfaceNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSurfaceNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkSurfaceNo, checkPipeworkSurfaceYes, checkPipeworkSurfaceNA);
        else
        {
            checkPipeworkSurfaceNo.Color = Colors.White;
            if (!checkPipeworkSurfaceYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkSurfaceNA, checkPipeworkSurfaceYes, checkPipeworkSurfaceNo);
        }
    }
    public void CheckPipeworkSurfaceNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSurfaceNA.IsChecked || !checkPipeworkSurfaceYes.IsChecked && !checkPipeworkSurfaceNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkSurfaceNA, checkPipeworkSurfaceYes, checkPipeworkSurfaceNo);
        else
            checkPipeworkSurfaceNA.Color = Colors.White;
    }




    public void CheckPipeworkEarthBondingYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkEarthBondingYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkEarthBondingYes, checkPipeworkEarthBondingNo, checkPipeworkEarthBondingNA);
        else
        {
            checkPipeworkEarthBondingYes.Color = Colors.White;
            if (!checkPipeworkEarthBondingNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkEarthBondingNA, checkPipeworkEarthBondingYes, checkPipeworkEarthBondingNo);
        }
    }
    public void CheckPipeworkEarthBondingNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkEarthBondingNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkEarthBondingNo, checkPipeworkEarthBondingYes, checkPipeworkEarthBondingNA);
        else
        {
            checkPipeworkEarthBondingNo.Color = Colors.White;
            if (!checkPipeworkEarthBondingYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkEarthBondingNA, checkPipeworkEarthBondingYes, checkPipeworkEarthBondingNo);
        }
    }
    public void CheckPipeworkEarthBondingNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkEarthBondingNA.IsChecked || !checkPipeworkEarthBondingYes.IsChecked && !checkPipeworkEarthBondingNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkEarthBondingNA, checkPipeworkEarthBondingYes, checkPipeworkEarthBondingNo);
        else
            checkPipeworkEarthBondingNA.Color = Colors.White;
    }




    public void CheckJointingMethodsYesChanged(object sender, EventArgs e)
    {
        if (checkJointingMethodsYes.IsChecked)
            DisjunctCheckboxes(checkJointingMethodsYes, checkJointingMethodsNo, checkJointingMethodsNA);
        else
        {
            checkJointingMethodsYes.Color = Colors.White;
            if (!checkJointingMethodsNo.IsChecked)
                DisjunctCheckboxes(checkJointingMethodsNA, checkJointingMethodsYes, checkJointingMethodsNo);
        }
    }
    public void CheckJointingMethodsNoChanged(object sender, EventArgs e)
    {
        if (checkJointingMethodsNo.IsChecked)
            DisjunctCheckboxes(checkJointingMethodsNo, checkJointingMethodsYes, checkJointingMethodsNA);
        else
        {
            checkJointingMethodsNo.Color = Colors.White;
            if (!checkJointingMethodsYes.IsChecked)
                DisjunctCheckboxes(checkJointingMethodsNA, checkJointingMethodsYes, checkJointingMethodsNo);
        }
    }
    public void CheckJointingMethodsNAChanged(object sender, EventArgs e)
    {
        if (checkJointingMethodsNA.IsChecked || !checkJointingMethodsYes.IsChecked && !checkJointingMethodsNo.IsChecked)
            DisjunctCheckboxes(checkJointingMethodsNA, checkJointingMethodsYes, checkJointingMethodsNo);
        else
            checkJointingMethodsNA.Color = Colors.White;
    }




    public void CheckPipeworkSupportsYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSupportsYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkSupportsYes, checkPipeworkSupportsNo, checkPipeworkSupportsNA);
        else
        {
            checkPipeworkSupportsYes.Color = Colors.White;
            if (!checkPipeworkSupportsNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkSupportsNA, checkPipeworkSupportsYes, checkPipeworkSupportsNo);
        }
    }
    public void CheckPipeworkSupportsNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSupportsNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkSupportsNo, checkPipeworkSupportsYes, checkPipeworkSupportsNA);
        else
        {
            checkPipeworkSupportsNo.Color = Colors.White;
            if (!checkPipeworkSupportsYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkSupportsNA, checkPipeworkSupportsYes, checkPipeworkSupportsNo);
        }
    }
    public void CheckPipeworkSupportsNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkSupportsNA.IsChecked || !checkPipeworkSupportsYes.IsChecked && !checkPipeworkSupportsNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkSupportsNA, checkPipeworkSupportsYes, checkPipeworkSupportsNo);
        else
            checkPipeworkSupportsNA.Color = Colors.White;
    }




    public void CheckFixingsYesChanged(object sender, EventArgs e)
    {
        if (checkFixingsYes.IsChecked)
            DisjunctCheckboxes(checkFixingsYes, checkFixingsNo, checkFixingsNA);
        else
        {
            checkFixingsYes.Color = Colors.White;
            if (!checkFixingsNo.IsChecked)
                DisjunctCheckboxes(checkFixingsNA, checkFixingsYes, checkFixingsNo);
        }
    }
    public void CheckFixingsNoChanged(object sender, EventArgs e)
    {
        if (checkFixingsNo.IsChecked)
            DisjunctCheckboxes(checkFixingsNo, checkFixingsYes, checkFixingsNA);
        else
        {
            checkFixingsNo.Color = Colors.White;
            if (!checkFixingsYes.IsChecked)
                DisjunctCheckboxes(checkFixingsNA, checkFixingsYes, checkFixingsNo);
        }
    }
    public void CheckFixingsNAChanged(object sender, EventArgs e)
    {
        if (checkFixingsNA.IsChecked || !checkFixingsYes.IsChecked && !checkFixingsNo.IsChecked)
            DisjunctCheckboxes(checkFixingsNA, checkFixingsYes, checkFixingsNo);
        else
            checkFixingsNA.Color = Colors.White;
    }




    public void CheckSupportSepparationDistancesYesChanged(object sender, EventArgs e)
    {
        if (checkSupportSepparationDistancesYes.IsChecked)
            DisjunctCheckboxes(checkSupportSepparationDistancesYes, checkSupportSepparationDistancesNo, checkSupportSepparationDistancesNA);
        else
        {
            checkSupportSepparationDistancesYes.Color = Colors.White;
            if (!checkSupportSepparationDistancesNo.IsChecked)
                DisjunctCheckboxes(checkSupportSepparationDistancesNA, checkSupportSepparationDistancesYes, checkSupportSepparationDistancesNo);
        }
    }
    public void CheckSupportSepparationDistancesNoChanged(object sender, EventArgs e)
    {
        if (checkSupportSepparationDistancesNo.IsChecked)
            DisjunctCheckboxes(checkSupportSepparationDistancesNo, checkSupportSepparationDistancesYes, checkSupportSepparationDistancesNA);
        else
        {
            checkSupportSepparationDistancesNo.Color = Colors.White;
            if (!checkSupportSepparationDistancesYes.IsChecked)
                DisjunctCheckboxes(checkSupportSepparationDistancesNA, checkSupportSepparationDistancesYes, checkSupportSepparationDistancesNo);
        }
    }
    public void CheckSupportSepparationDistancesNAChanged(object sender, EventArgs e)
    {
        if (checkSupportSepparationDistancesNA.IsChecked || !checkSupportSepparationDistancesYes.IsChecked && !checkSupportSepparationDistancesNo.IsChecked)
            DisjunctCheckboxes(checkSupportSepparationDistancesNA, checkSupportSepparationDistancesYes, checkSupportSepparationDistancesNo);
        else
            checkSupportSepparationDistancesNA.Color = Colors.White;
    }




    public void CheckPipeworkInVoidsYesChanged(object sender, EventArgs e)
    {
        if (checkPipeworkInVoidsYes.IsChecked)
            DisjunctCheckboxes(checkPipeworkInVoidsYes, checkPipeworkInVoidsNo, checkPipeworkInVoidsNA);
        else
        {
            checkPipeworkInVoidsYes.Color = Colors.White;
            if (!checkPipeworkInVoidsNo.IsChecked)
                DisjunctCheckboxes(checkPipeworkInVoidsNA, checkPipeworkInVoidsYes, checkPipeworkInVoidsNo);
        }
    }
    public void CheckPipeworkInVoidsNoChanged(object sender, EventArgs e)
    {
        if (checkPipeworkInVoidsNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkInVoidsNo, checkPipeworkInVoidsYes, checkPipeworkInVoidsNA);
        else
        {
            checkPipeworkInVoidsNo.Color = Colors.White;
            if (!checkPipeworkInVoidsYes.IsChecked)
                DisjunctCheckboxes(checkPipeworkInVoidsNA, checkPipeworkInVoidsYes, checkPipeworkInVoidsNo);
        }
    }
    public void CheckPipeworkInVoidsNAChanged(object sender, EventArgs e)
    {
        if (checkPipeworkInVoidsNA.IsChecked || !checkPipeworkInVoidsYes.IsChecked && !checkPipeworkInVoidsNo.IsChecked)
            DisjunctCheckboxes(checkPipeworkInVoidsNA, checkPipeworkInVoidsYes, checkPipeworkInVoidsNo);
        else
            checkPipeworkInVoidsNA.Color = Colors.White;
    }




    public void CheckPipeSleevesYesChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesYes.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesYes, checkPipeSleevesNo, checkPipeSleevesNA);
        else
        {
            checkPipeSleevesYes.Color = Colors.White;
            if (!checkPipeSleevesNo.IsChecked)
                DisjunctCheckboxes(checkPipeSleevesNA, checkPipeSleevesYes, checkPipeSleevesNo);
        }
    }
    public void CheckPipeSleevesNoChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesNo.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesNo, checkPipeSleevesYes, checkPipeSleevesNA);
        else
        {
            checkPipeSleevesNo.Color = Colors.White;
            if (!checkPipeSleevesYes.IsChecked)
                DisjunctCheckboxes(checkPipeSleevesNA, checkPipeSleevesYes, checkPipeSleevesNo);
        }
    }
    public void CheckPipeSleevesNAChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesNA.IsChecked || !checkPipeSleevesYes.IsChecked && !checkPipeSleevesNo.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesNA, checkPipeSleevesYes, checkPipeSleevesNo);
        else
            checkPipeSleevesNA.Color = Colors.White;
    }




    public void CheckPipeSleevesSealedYesChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesSealedYes.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesSealedYes, checkPipeSleevesSealedNo, checkPipeSleevesSealedNA);
        else
        {
            checkPipeSleevesSealedYes.Color = Colors.White;
            if (!checkPipeSleevesSealedNo.IsChecked)
                DisjunctCheckboxes(checkPipeSleevesSealedNA, checkPipeSleevesSealedYes, checkPipeSleevesSealedNo);
        }
    }
    public void CheckPipeSleevesSealedNoChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesSealedNo.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesSealedNo, checkPipeSleevesSealedYes, checkPipeSleevesSealedNA);
        else
        {
            checkPipeSleevesSealedNo.Color = Colors.White;
            if (!checkPipeSleevesSealedYes.IsChecked)
                DisjunctCheckboxes(checkPipeSleevesSealedNA, checkPipeSleevesSealedYes, checkPipeSleevesSealedNo);
        }
    }
    public void CheckPipeSleevesSealedNAChanged(object sender, EventArgs e)
    {
        if (checkPipeSleevesSealedNA.IsChecked || !checkPipeSleevesSealedYes.IsChecked && !checkPipeSleevesSealedNo.IsChecked)
            DisjunctCheckboxes(checkPipeSleevesSealedNA, checkPipeSleevesSealedYes, checkPipeSleevesSealedNo);
        else
            checkPipeSleevesSealedNA.Color = Colors.White;
    }




    public void CheckServiceValvesYesChanged(object sender, EventArgs e)
    {
        if (checkServiceValvesYes.IsChecked)
            DisjunctCheckboxes(checkServiceValvesYes, checkServiceValvesNo, checkServiceValvesNA);
        else
        {
            checkServiceValvesYes.Color = Colors.White;
            if (!checkServiceValvesNo.IsChecked)
                DisjunctCheckboxes(checkServiceValvesNA, checkServiceValvesYes, checkServiceValvesNo);
        }
    }
    public void CheckServiceValvesNoChanged(object sender, EventArgs e)
    {
        if (checkServiceValvesNo.IsChecked)
            DisjunctCheckboxes(checkServiceValvesNo, checkServiceValvesYes, checkServiceValvesNA);
        else
        {
            checkServiceValvesNo.Color = Colors.White;
            if (!checkServiceValvesYes.IsChecked)
                DisjunctCheckboxes(checkServiceValvesNA, checkServiceValvesYes, checkServiceValvesNo);
        }
    }
    public void CheckServiceValvesNAChanged(object sender, EventArgs e)
    {
        if (checkServiceValvesNA.IsChecked || !checkServiceValvesYes.IsChecked && !checkServiceValvesNo.IsChecked)
            DisjunctCheckboxes(checkServiceValvesNA, checkServiceValvesYes, checkServiceValvesNo);
        else
            checkServiceValvesNA.Color = Colors.White;
    }




    public void CheckAdditionalEmergencyControlValvesYesChanged(object sender, EventArgs e)
    {
        if (checkAdditionalEmergencyControlValvesYes.IsChecked)
            DisjunctCheckboxes(checkAdditionalEmergencyControlValvesYes, checkAdditionalEmergencyControlValvesNo, checkAdditionalEmergencyControlValvesNA);
        else
        {
            checkAdditionalEmergencyControlValvesYes.Color = Colors.White;
            if (!checkAdditionalEmergencyControlValvesNo.IsChecked)
                DisjunctCheckboxes(checkAdditionalEmergencyControlValvesNA, checkAdditionalEmergencyControlValvesYes, checkAdditionalEmergencyControlValvesNo);
        }
    }
    public void CheckAdditionalEmergencyControlValvesNoChanged(object sender, EventArgs e)
    {
        if (checkAdditionalEmergencyControlValvesNo.IsChecked)
            DisjunctCheckboxes(checkAdditionalEmergencyControlValvesNo, checkAdditionalEmergencyControlValvesYes, checkAdditionalEmergencyControlValvesNA);
        else
        {
            checkAdditionalEmergencyControlValvesNo.Color = Colors.White;
            if (!checkAdditionalEmergencyControlValvesYes.IsChecked)
                DisjunctCheckboxes(checkAdditionalEmergencyControlValvesNA, checkAdditionalEmergencyControlValvesYes, checkAdditionalEmergencyControlValvesNo);
        }
    }
    public void CheckAdditionalEmergencyControlValvesNAChanged(object sender, EventArgs e)
    {
        if (checkAdditionalEmergencyControlValvesNA.IsChecked || !checkAdditionalEmergencyControlValvesYes.IsChecked && !checkAdditionalEmergencyControlValvesNo.IsChecked)
            DisjunctCheckboxes(checkAdditionalEmergencyControlValvesNA, checkAdditionalEmergencyControlValvesYes, checkAdditionalEmergencyControlValvesNo);
        else
            checkAdditionalEmergencyControlValvesNA.Color = Colors.White;
    }




    public void CheckIsolationValveYesChanged(object sender, EventArgs e)
    {
        if (checkIsolationValveYes.IsChecked)
            DisjunctCheckboxes(checkIsolationValveYes, checkIsolationValveNo, checkIsolationValveNA);
        else
        {
            checkIsolationValveYes.Color = Colors.White;
            if (!checkIsolationValveNo.IsChecked)
                DisjunctCheckboxes(checkIsolationValveNA, checkIsolationValveYes, checkIsolationValveNo);
        }
    }
    public void CheckIsolationValveNoChanged(object sender, EventArgs e)
    {
        if (checkIsolationValveNo.IsChecked)
            DisjunctCheckboxes(checkIsolationValveNo, checkIsolationValveYes, checkIsolationValveNA);
        else
        {
            checkIsolationValveNo.Color = Colors.White;
            if (!checkIsolationValveYes.IsChecked)
                DisjunctCheckboxes(checkIsolationValveNA, checkIsolationValveYes, checkIsolationValveNo);
        }
    }
    public void CheckIsolationValveNAChanged(object sender, EventArgs e)
    {
        if (checkIsolationValveNA.IsChecked || !checkIsolationValveYes.IsChecked && !checkIsolationValveNo.IsChecked)
            DisjunctCheckboxes(checkIsolationValveNA, checkIsolationValveYes, checkIsolationValveNo);
        else
            checkIsolationValveNA.Color = Colors.White;
    }




    public void CheckTestPointYesChanged(object sender, EventArgs e)
    {
        if (checkTestPointYes.IsChecked)
            DisjunctCheckboxes(checkTestPointYes, checkTestPointNo, checkTestPointNA);
        else
        {
            checkTestPointYes.Color = Colors.White;
            if (!checkTestPointNo.IsChecked)
                DisjunctCheckboxes(checkTestPointNA, checkTestPointYes, checkTestPointNo);
        }
    }
    public void CheckTestPointNoChanged(object sender, EventArgs e)
    {
        if (checkTestPointNo.IsChecked)
            DisjunctCheckboxes(checkTestPointNo, checkTestPointYes, checkTestPointNA);
        else
        {
            checkTestPointNo.Color = Colors.White;
            if (!checkTestPointYes.IsChecked)
                DisjunctCheckboxes(checkTestPointNA, checkTestPointYes, checkTestPointNo);
        }
    }
    public void CheckTestPointNAChanged(object sender, EventArgs e)
    {
        if (checkTestPointNA.IsChecked || !checkTestPointYes.IsChecked && !checkTestPointNo.IsChecked)
            DisjunctCheckboxes(checkTestPointNA, checkTestPointYes, checkTestPointNo);
        else
            checkTestPointNA.Color = Colors.White;
    }




    public void CheckPurgePointsYesChanged(object sender, EventArgs e)
    {
        if (checkPurgePointsYes.IsChecked)
            DisjunctCheckboxes(checkPurgePointsYes, checkPurgePointsNo, checkPurgePointsNA);
        else
        {
            checkPurgePointsYes.Color = Colors.White;
            if (!checkPurgePointsNo.IsChecked)
                DisjunctCheckboxes(checkPurgePointsNA, checkPurgePointsYes, checkPurgePointsNo);
        }
    }
    public void CheckPurgePointsNoChanged(object sender, EventArgs e)
    {
        if (checkPurgePointsNo.IsChecked)
            DisjunctCheckboxes(checkPurgePointsNo, checkPurgePointsYes, checkPurgePointsNA);
        else
        {
            checkPurgePointsNo.Color = Colors.White;
            if (!checkPurgePointsYes.IsChecked)
                DisjunctCheckboxes(checkPurgePointsNA, checkPurgePointsYes, checkPurgePointsNo);
        }
    }
    public void CheckPurgePointsNAChanged(object sender, EventArgs e)
    {
        if (checkPurgePointsNA.IsChecked || !checkPurgePointsYes.IsChecked && !checkPurgePointsNo.IsChecked)
            DisjunctCheckboxes(checkPurgePointsNA, checkPurgePointsYes, checkPurgePointsNo);
        else
            checkPurgePointsNA.Color = Colors.White;
    }




    public void CheckGeneralPipeworkConditionYesChanged(object sender, EventArgs e)
    {
        if (checkGeneralPipeworkConditionYes.IsChecked)
            DisjunctCheckboxes(checkGeneralPipeworkConditionYes, checkGeneralPipeworkConditionNo, checkGeneralPipeworkConditionNA);
        else
        {
            checkGeneralPipeworkConditionYes.Color = Colors.White;
            if (!checkGeneralPipeworkConditionNo.IsChecked)
                DisjunctCheckboxes(checkGeneralPipeworkConditionNA, checkGeneralPipeworkConditionYes, checkGeneralPipeworkConditionNo);
        }
    }
    public void CheckGeneralPipeworkConditionNoChanged(object sender, EventArgs e)
    {
        if (checkGeneralPipeworkConditionNo.IsChecked)
            DisjunctCheckboxes(checkGeneralPipeworkConditionNo, checkGeneralPipeworkConditionYes, checkGeneralPipeworkConditionNA);
        else
        {
            checkGeneralPipeworkConditionNo.Color = Colors.White;
            if (!checkGeneralPipeworkConditionYes.IsChecked)
                DisjunctCheckboxes(checkGeneralPipeworkConditionNA, checkGeneralPipeworkConditionYes, checkGeneralPipeworkConditionNo);
        }
    }
    public void CheckGeneralPipeworkConditionNAChanged(object sender, EventArgs e)
    {
        if (checkGeneralPipeworkConditionNA.IsChecked || !checkGeneralPipeworkConditionYes.IsChecked && !checkGeneralPipeworkConditionNo.IsChecked)
            DisjunctCheckboxes(checkGeneralPipeworkConditionNA, checkGeneralPipeworkConditionYes, checkGeneralPipeworkConditionNo);
        else
            checkGeneralPipeworkConditionNA.Color = Colors.White;
    }




    public void CheckinstallationSafeToOperateYesChanged(object sender, EventArgs e)
    {
        if (checkinstallationSafeToOperateYes.IsChecked)
            DisjunctCheckboxes(checkinstallationSafeToOperateYes, checkinstallationSafeToOperateNo, checkinstallationSafeToOperateNA);
        else
        {
            checkinstallationSafeToOperateYes.Color = Colors.White;
            if (!checkinstallationSafeToOperateNo.IsChecked)
                DisjunctCheckboxes(checkinstallationSafeToOperateNA, checkinstallationSafeToOperateYes, checkinstallationSafeToOperateNo);
        }
    }
    public void CheckinstallationSafeToOperateNoChanged(object sender, EventArgs e)
    {
        if (checkinstallationSafeToOperateNo.IsChecked)
            DisjunctCheckboxes(checkinstallationSafeToOperateNo, checkinstallationSafeToOperateYes, checkinstallationSafeToOperateNA);
        else
        {
            checkinstallationSafeToOperateNo.Color = Colors.White;
            if (!checkinstallationSafeToOperateYes.IsChecked)
                DisjunctCheckboxes(checkinstallationSafeToOperateNA, checkinstallationSafeToOperateYes, checkinstallationSafeToOperateNo);
        }
    }
    public void CheckinstallationSafeToOperateNAChanged(object sender, EventArgs e)
    {
        if (checkinstallationSafeToOperateNA.IsChecked || !checkinstallationSafeToOperateYes.IsChecked && !checkinstallationSafeToOperateNo.IsChecked)
            DisjunctCheckboxes(checkinstallationSafeToOperateNA, checkinstallationSafeToOperateYes, checkinstallationSafeToOperateNo);
        else
            checkNoticesAndLabelsNA.Color = Colors.White;
    }




    public void CheckWarningNoticeIssuedYesChanged(object sender, EventArgs e)
    {
        if (checkWarningNoticeIssuedYes.IsChecked)
            DisjunctCheckboxes(checkWarningNoticeIssuedYes, checkWarningNoticeIssuedNo, checkWarningNoticeIssuedNA);
        else
        {
            checkWarningNoticeIssuedYes.Color = Colors.White;
            if (!checkWarningNoticeIssuedNo.IsChecked)
                DisjunctCheckboxes(checkWarningNoticeIssuedNA, checkWarningNoticeIssuedYes, checkWarningNoticeIssuedNo);
        }
    }
    public void CheckWarningNoticeIssuedNoChanged(object sender, EventArgs e)
    {
        if (checkWarningNoticeIssuedNo.IsChecked)
            DisjunctCheckboxes(checkWarningNoticeIssuedNo, checkWarningNoticeIssuedYes, checkWarningNoticeIssuedNA);
        else
        {
            checkWarningNoticeIssuedNo.Color = Colors.White;
            if (!checkWarningNoticeIssuedYes.IsChecked)
                DisjunctCheckboxes(checkWarningNoticeIssuedNA, checkWarningNoticeIssuedYes, checkWarningNoticeIssuedNo);
        }
    }
    public void CheckWarningNoticeIssuedNAChanged(object sender, EventArgs e)
    {
        if (checkWarningNoticeIssuedNA.IsChecked || !checkWarningNoticeIssuedYes.IsChecked && !checkWarningNoticeIssuedNo.IsChecked)
            DisjunctCheckboxes(checkWarningNoticeIssuedNA, checkWarningNoticeIssuedYes, checkWarningNoticeIssuedNo);
        else
            checkWarningNoticeIssuedNA.Color = Colors.White;
    }




    public void CheckGasTightnessTestRecommendedYesChanged(object sender, EventArgs e)
    {
        if (checkGasTightnessTestRecommendedYes.IsChecked)
            DisjunctCheckboxes(checkGasTightnessTestRecommendedYes, checkGasTightnessTestRecommendedNo, checkGasTightnessTestRecommendedNA);
        else
        {
            checkGasTightnessTestRecommendedYes.Color = Colors.White;
            if (!checkGasTightnessTestRecommendedNo.IsChecked)
                DisjunctCheckboxes(checkGasTightnessTestRecommendedNA, checkGasTightnessTestRecommendedYes, checkGasTightnessTestRecommendedNo);
        }
    }
    public void CheckGasTightnessTestRecommendedNoChanged(object sender, EventArgs e)
    {
        if (checkGasTightnessTestRecommendedNo.IsChecked)
            DisjunctCheckboxes(checkGasTightnessTestRecommendedNo, checkGasTightnessTestRecommendedYes, checkGasTightnessTestRecommendedNA);
        else
        {
            checkGasTightnessTestRecommendedNo.Color = Colors.White;
            if (!checkGasTightnessTestRecommendedYes.IsChecked)
                DisjunctCheckboxes(checkGasTightnessTestRecommendedNA, checkGasTightnessTestRecommendedYes, checkGasTightnessTestRecommendedNo);
        }
    }
    public void CheckGasTightnessTestRecommendedNAChanged(object sender, EventArgs e)
    {
        if (checkGasTightnessTestRecommendedNA.IsChecked || !checkGasTightnessTestRecommendedYes.IsChecked && !checkGasTightnessTestRecommendedNo.IsChecked)
            DisjunctCheckboxes(checkGasTightnessTestRecommendedNA, checkGasTightnessTestRecommendedYes, checkGasTightnessTestRecommendedNo);
        else
            checkGasTightnessTestRecommendedNA.Color = Colors.White;
    }




    public void CheckGuessTightnessTestCarriedOutYesChanged(object sender, EventArgs e)
    {
        if (checkGuessTightnessTestCarriedOutYes.IsChecked)
            DisjunctCheckboxes(checkGuessTightnessTestCarriedOutYes, checkGuessTightnessTestCarriedOutNo, checkGuessTightnessTestCarriedOutNA);
        else
        {
            checkGuessTightnessTestCarriedOutYes.Color = Colors.White;
            if (!checkGuessTightnessTestCarriedOutNo.IsChecked)
                DisjunctCheckboxes(checkGuessTightnessTestCarriedOutNA, checkGuessTightnessTestCarriedOutYes, checkGuessTightnessTestCarriedOutNo);
        }
    }
    public void CheckGuessTightnessTestCarriedOutNoChanged(object sender, EventArgs e)
    {
        if (checkGuessTightnessTestCarriedOutNo.IsChecked)
            DisjunctCheckboxes(checkGuessTightnessTestCarriedOutNo, checkGuessTightnessTestCarriedOutYes, checkGuessTightnessTestCarriedOutNA);
        else
        {
            checkGuessTightnessTestCarriedOutNo.Color = Colors.White;
            if (!checkGuessTightnessTestCarriedOutYes.IsChecked)
                DisjunctCheckboxes(checkGuessTightnessTestCarriedOutNA, checkGuessTightnessTestCarriedOutYes, checkGuessTightnessTestCarriedOutNo);
        }
    }
    public void CheckGuessTightnessTestCarriedOutNAChanged(object sender, EventArgs e)
    {
        if (checkGuessTightnessTestCarriedOutNA.IsChecked || !checkGuessTightnessTestCarriedOutYes.IsChecked && !checkGuessTightnessTestCarriedOutNo.IsChecked)
            DisjunctCheckboxes(checkGuessTightnessTestCarriedOutNA, checkGuessTightnessTestCarriedOutYes, checkGuessTightnessTestCarriedOutNo);
        else
            checkGuessTightnessTestCarriedOutNA.Color = Colors.White;
    }
}