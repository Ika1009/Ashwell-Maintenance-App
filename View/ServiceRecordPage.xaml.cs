using CommunityToolkit.Maui.Views;
using Org.Apache.Http.Client.Params;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class ServiceRecordPage : ContentPage
{
    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public ServiceRecordPage()
    {
        InitializeComponent();
        checkHeating.IsChecked = true;
        checkOpenFlue.IsChecked = true;
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
        ServiceRecordBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ServiceRecord, reportName, folder.Id, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully created new sheet.", "OK");
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

                byte[] pdfData = await PdfCreation.ServiceRecord(reportData, signature1, signature2);

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
            if (!response.IsSuccessStatusCode) {
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
    public void ServiceRecordBack(object sender, EventArgs e)
    {
        if (SRSection1.IsVisible)
        {
            ServiceRecordBackBtt.IsEnabled = false;
            Navigation.PopModalAsync();
        }
        else if (SRSection2.IsVisible)
        {
            SRSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection1.ScrollToAsync(0, 0, false);
            SRSection1.IsVisible = true;
        }
        else if (SRSection3.IsVisible)
        {
            SRSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection2.ScrollToAsync(0, 0, false);
            SRSection2.IsVisible = true;
        }
        else if (SRSection4.IsVisible) {
            SRSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection3.ScrollToAsync(0, 0, false);
            SRSection3.IsVisible = true;
        }
        else if (SRSection5.IsVisible)
        {
            SRSection5.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection4.ScrollToAsync(0, 0, false);
            SRSection4.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                SRSection5.ScrollToAsync(0, 0, false);
            SRSection5.IsVisible = true;

            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;
        }
    }

    
    public void ServiceRecordNext1(object sender, EventArgs e)
    {
        SRSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            SRSection2.ScrollToAsync(0, 0, false);
        SRSection2.IsVisible = true;
    }

    
    public async void ServiceRecordNext2(object sender, EventArgs e)
    {
        SRSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await SRSection3.ScrollToAsync(0, 0, false);
        SRSection3.IsVisible = true;
    }

    
    public async void ServiceRecordNext3(object sender, EventArgs e)
    {
        SRSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await SRSection4.ScrollToAsync(0, 0, false);
        SRSection4.IsVisible = true;
    }

    public async void ServiceRecordNext4(object sender, EventArgs e)
    {
        SRSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await SRSection5.ScrollToAsync(0, 0, false);
        SRSection5.IsVisible = true;
    }

    public async void ServiceRecordNext5(object sender, EventArgs e)
    {
        SRSection5.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Ashwell_Service_Report_{dateTimeString}.pdf";
        GatherReportData();
        //await PdfCreation.ServiceRecord(reportData, Array.Empty<byte>(), Array.Empty<byte>());
        //await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
        await LoadFolders();
    }
    public void previewServiceRecordPage(Dictionary<string, string> reportData)
    {
        if (reportData.ContainsKey("site"))
            site.Text = reportData["site"];

        if (reportData.ContainsKey("location"))
            location.Text = reportData["location"];

        if (reportData.ContainsKey("assetNumber"))
            assetNo.Text = reportData["assetNumber"];

        if (reportData.ContainsKey("applianceNumber"))
            applianceNo.Text = reportData["applianceNumber"];

        if (reportData.ContainsKey("testsCompleted"))
            checkTestsCompleted.IsChecked = reportData["testsCompleted"] == "True";

        if (reportData.ContainsKey("remedialWorkRequired"))
            checRemedialWorkRequired.IsChecked = reportData["remedialWorkRequired"] == "True";

        if (reportData.ContainsKey("applianceMake"))
            applianceMake.Text = reportData["applianceMake"];

        if (reportData.ContainsKey("applianceModel"))
            applianceModel.Text = reportData["applianceModel"];

        if (reportData.ContainsKey("applianceSerialNumber"))
            applianceSerialNo.Text = reportData["applianceSerialNumber"];

        if (reportData.ContainsKey("gcNumber"))
            GCNo.Text = reportData["gcNumber"];

        if (reportData.ContainsKey("Heating"))
            checkHeating.IsChecked = reportData["Heating"] == "True";

        if (reportData.ContainsKey("HotWater"))
            checkHotWater.IsChecked = reportData["HotWater"] == "True";

        if (reportData.ContainsKey("Both"))
            checkBoth.IsChecked = reportData["Both"] == "True";

        if (reportData.ContainsKey("approxAgeOfAppliance"))
            years.Text = reportData["approxAgeOfAppliance"];

        if (reportData.ContainsKey("badgedInput"))
            badgedInput.Text = reportData["badgedInput"];

        if (reportData.ContainsKey("badgedOutput"))
            budgedOutput.Text = reportData["badgedOutput"];

        if (reportData.ContainsKey("burnerMake"))
            burnerMake.Text = reportData["burnerMake"];

        if (reportData.ContainsKey("burnerModel"))
            burnerModel.Text = reportData["burnerModel"];

        if (reportData.ContainsKey("burnerSerialNumber"))
            burnerSerialNo.Text = reportData["burnerSerialNumber"];

        if (reportData.ContainsKey("Type"))
            type.Text = reportData["Type"];

        if (reportData.ContainsKey("secs"))
            flameFailureDeviceSecs.Text = reportData["secs"];

        if (reportData.ContainsKey("Spec"))
            spec.Text = reportData["Spec"];

        if (reportData.ContainsKey("OpenFlue"))
            checkOpenFlue.IsChecked = reportData["OpenFlue"] == "True";

        if (reportData.ContainsKey("Roomsealed"))
            checkRoomSealed.IsChecked = reportData["Roomsealed"] == "True";

        if (reportData.ContainsKey("ForcedDraft"))
            checkForcedDraft.IsChecked = reportData["ForcedDraft"] == "True";

        if (reportData.ContainsKey("Flueless"))
            checkFlueless.IsChecked = reportData["Flueless"] == "True";

        if (reportData.ContainsKey("badgedBurnerPressure"))
            badgedBurnerPressure.Text = reportData["badgedBurnerPressure"];

        if (reportData.ContainsKey("ventilationSatisfactory"))
            checkVentilationSatisfactory.IsChecked = reportData["ventilationSatisfactory"] == "True";

        if (reportData.ContainsKey("flueConditionSatisfactory"))
            checkFlueConditionSatisfactory.IsChecked = reportData["flueConditionSatisfactory"] == "True";

        if (reportData.ContainsKey("tempNG"))
            checkNG.IsChecked = reportData["tempNG"] == "True";

        if (reportData.ContainsKey("tempLPG"))
            checkLPG.IsChecked = reportData["tempLPG"] == "True";

        if (reportData.ContainsKey("applianceServiceValveSatisfactory"))
            checkAppServiceValve.IsChecked = reportData["applianceServiceValveSatisfactory"] == "True";

        if (reportData.ContainsKey("applianceServiceValveSatisfactoryNA"))
            checkAppServiceValveNA.IsChecked = reportData["applianceServiceValveSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("applianceServiceValveSatisfactoryComments"))
            applianceServiceValveComment.Text = reportData["applianceServiceValveSatisfactoryComments"];

        if (reportData.ContainsKey("governorsSatisfactory"))
            checkGovernors.IsChecked = reportData["governorsSatisfactory"] == "True";

        if (reportData.ContainsKey("governorsSatisfactoryNA"))
            checkGovernorsNA.IsChecked = reportData["governorsSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("governorsComments"))
            governorsComment.Text = reportData["governorsComments"];

        if (reportData.ContainsKey("gasSolenoidValvesSatisfactory"))
            checkGasSolenoidValves.IsChecked = reportData["gasSolenoidValvesSatisfactory"] == "True";

        if (reportData.ContainsKey("gasSolenoidValvesSatisfactoryNA"))
            checkGasSolenoidValvesNA.IsChecked = reportData["gasSolenoidValvesSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("gasSolenoidValvesComments"))
            gasSolenoidValvesComment.Text = reportData["gasSolenoidValvesComments"];

        if (reportData.ContainsKey("controlBoxPcbSatisfactory"))
            checkControlBoxPCB.IsChecked = reportData["controlBoxPcbSatisfactory"] == "True";

        if (reportData.ContainsKey("controlBoxPcbSatisfactoryNA"))
            checkControlBoxPCBNA.IsChecked = reportData["controlBoxPcbSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("controlBoxPcbComments"))
            controlBoxPCBComment.Text = reportData["controlBoxPcbComments"];

        if (reportData.ContainsKey("gasketSealsSatisfactory"))
            checkGasketSeals.IsChecked = reportData["gasketSealsSatisfactory"] == "True";

        if (reportData.ContainsKey("gasketSealsSatisfactoryNA"))
            checkGasketSealsNA.IsChecked = reportData["gasketSealsSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("gasketSealsComments"))
            gasketSealsComment.Text = reportData["gasketSealsComments"];

        if (reportData.ContainsKey("burnerSatisfactory"))
            checkBurner.IsChecked = reportData["burnerSatisfactory"] == "True";

        if (reportData.ContainsKey("burnerSatisfactoryNA"))
            checkBurnerNA.IsChecked = reportData["burnerSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("burnerComments"))
            burnerComment.Text = reportData["burnerComments"];

        if (reportData.ContainsKey("burnerJetsSatisfactory"))
            checkBurnerJets.IsChecked = reportData["burnerJetsSatisfactory"] == "True";

        if (reportData.ContainsKey("burnerJetsSatisfactoryNA"))
            checkBurnerJetsNA.IsChecked = reportData["burnerJetsSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("burnerJetsComments"))
            burnerJetsComment.Text = reportData["burnerJetsComments"];

        if (reportData.ContainsKey("electrodesTransformerSatisfactory"))
            checkElectrodesTransformer.IsChecked = reportData["electrodesTransformerSatisfactory"] == "True";

        if (reportData.ContainsKey("electrodesTransformerSatisfactoryNA"))
            checkElectrodesTransformerNA.IsChecked = reportData["electrodesTransformerSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("electrodesTransformerComments"))
            electrodesTransformerComment.Text = reportData["electrodesTransformerComments"];

        if (reportData.ContainsKey("flameFailureDeviceSatisfactory"))
            checkFlameFailureDevice.IsChecked = reportData["flameFailureDeviceSatisfactory"] == "True";

        if (reportData.ContainsKey("flameFailureDeviceSatisfactoryNA"))
            checkFlameFailureDeviceNA.IsChecked = reportData["flameFailureDeviceSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("flameFailureDeviceComments"))
            flameFailureDeviceComment.Text = reportData["flameFailureDeviceComments"];

        if (reportData.ContainsKey("systemBoilerControlsSatisfactory"))
            checkSystemBoilerControls.IsChecked = reportData["systemBoilerControlsSatisfactory"] == "True";

        if (reportData.ContainsKey("systemBoilerControlsSatisfactoryNA"))
            checkSystemBoilerControlsNA.IsChecked = reportData["systemBoilerControlsSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("systemBoilerControlsComments"))
            systemBoilerControlsComment.Text = reportData["systemBoilerControlsComments"];

        if (reportData.ContainsKey("boilerCasingSatisfactory"))
            checkBoilerCasing.IsChecked = reportData["boilerCasingSatisfactory"] == "True";

        if (reportData.ContainsKey("boilerCasingSatisfactoryNA"))
            checkBoilerCasingNA.IsChecked = reportData["boilerCasingSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("boilerCasingComments"))
            boilerCasingComment.Text = reportData["boilerCasingComments"];

        if (reportData.ContainsKey("thermalInsulationSatisfactory"))
            checkThermalInsulation.IsChecked = reportData["thermalInsulationSatisfactory"] == "True";

        if (reportData.ContainsKey("thermalInsulationSatisfactoryNA"))
            checkThermalInsulationNA.IsChecked = reportData["thermalInsulationSatisfactoryNA"] == "True";

        if (reportData.ContainsKey("thermalInsulationComments"))
            thermalInsulationComment.Text = reportData["thermalInsulationComments"];

        //drugi deo
        // Populate the form fields using the data from the dictionary
        checkCombustionFanIdFan.IsChecked = bool.Parse(reportData["combustionFanIdFanSatisfactory"]);
        checkCombustionFanIdFanNA.IsChecked = bool.Parse(reportData["combustionFanIdFanSatisfactoryNA"]);
        combustionFanIdFanComment.Text = reportData["combustionFanIdFanComments"];

        checkAirFluePressureSwitch.IsChecked = bool.Parse(reportData["airFluePressureSwitchSatisfactory"]);
        checkAirFluePressureSwitchNA.IsChecked = bool.Parse(reportData["airFluePressureSwitchSatisfactoryNA"]);
        airFluePressureSwitchComment.Text = reportData["airFluePressureSwitchComments"];

        checkControlLimitStatus.IsChecked = bool.Parse(reportData["controlLimitStatsSatisfactory"]);
        checkControlLimitStatusNA.IsChecked = bool.Parse(reportData["controlLimitStatsSatisfactoryNA"]);
        controlLimitStatusComment.Text = reportData["controlLimitStatsComments"];

        checkPressureTempGauges.IsChecked = bool.Parse(reportData["pressureTempGaugesSatisfactory"]);
        checkPressureTempGaugesNA.IsChecked = bool.Parse(reportData["pressureTempGaugesSatisfactoryNA"]);
        pressureTempGaugesComment.Text = reportData["pressureTempGaugesComments"];

        checkCirculationPumps.IsChecked = bool.Parse(reportData["circulationPumpsSatisfactory"]);
        checkCirculationPumpsNA.IsChecked = bool.Parse(reportData["circulationPumpsSatisfactoryNA"]);
        circulationPumpsComment.Text = reportData["circulationPumpsComments"];

        checkCondenseTrap.IsChecked = bool.Parse(reportData["condenseTrapSatisfactory"]);
        checkCondenseTrapNA.IsChecked = bool.Parse(reportData["condenseTrapSatisfactoryNA"]);
        condenseTrapComment.Text = reportData["condenseTrapComments"];

        checkHeatExchangerFluewaysClear.IsChecked = bool.Parse(reportData["heatExhanger"]);
        checkHeatExchangerFluewaysClearNA.IsChecked = bool.Parse(reportData["heatExhangerNA"]);
        heatExchangerFluewaysClearComment.Text = reportData["heatExhangerComments"];

        workingIntelPressure.Text = reportData["workingInletPressure"];
        workingIntelPressureComment.Text = reportData["workingInletPressureComments"];

        recordedBurnerPressure.Text = reportData["recordedBurnerPressure"];
        recordedBurnerPressureComment.Text = reportData["recordedBurnerPressureComments"];

        measuredGasRate.Text = reportData["measuredGasRate"];
        mesuredGasRateComment.Text = reportData["measuredGasRateComments"];

        checkFlueFlowTest.IsChecked = bool.Parse(reportData["flueFlowTest"]);
        checkFlueFlowTestNA.IsChecked = bool.Parse(reportData["flueFlowTestNA"]);
        flueFlowTestComment.Text = reportData["flueFlowTestComments"];

        checkSpillageTest.IsChecked = bool.Parse(reportData["spillageTest"]);
        checkBoth.IsChecked = bool.Parse(reportData["spillageTestNA"]);
        spillageTestComment.Text = reportData["spillageTestComments"];

        checkAECVPlantIsolationCorrect.IsChecked = bool.Parse(reportData["AECVPlantIsolationCorrect"]);
        checkAECVPlantIsolationCorrectNA.IsChecked = bool.Parse(reportData["AECVPlantIsolationCorrectNA"]);
        AECVPlantIsolationCorrectComment.Text = reportData["AECVPlantIsolationCorrectComments"];

        checkSafetyShutOffValve.IsChecked = bool.Parse(reportData["safetyShutOffValve"]);
        checkSafetyShutOffValveNA.IsChecked = bool.Parse(reportData["safetyShutOffValveNA"]);
        safetyShutOffValveComment.Text = reportData["safetyShutOffValveComments"];

        checkPlantroomGasTightnessTest.IsChecked = bool.Parse(reportData["plantroomGasTightnessTest"]);
        checkPlantroomGasTightnessTestNA.IsChecked = bool.Parse(reportData["plantroomGasTightnessTestNA"]);
        plantroomGasTightnessTestComment.Text = reportData["plantroomGasTightnessTestComments"];

        stateApplianceCondition.Text = reportData["stateApplianceCondition"];
        stateApplianceConditionComment.Text = reportData["stateApplianceConditionComments"];

        highFireCO2.Text = reportData["HighFireCO2"];
        highFireCO.Text = reportData["HighFireCO"];
        highFireO2.Text = reportData["HighFireO2"];
        highFireRatio.Text = reportData["HighFireRatio"];
        highFireFlueTemp.Text = reportData["HighFireFlueTemp"];
        highFireEfficiency.Text = reportData["HighFireEfficiency"];
        highFireExcessAir.Text = reportData["HighFireExcessAir"];
        highFireRoomTemp.Text = reportData["HighFireRoomTemp"];

        lowFireCO2.Text = reportData["LowFireCO2"];
        lowFireCO.Text = reportData["LowFireCO"];
        lowFireO2.Text = reportData["LowFireO2"];
        lowFireRatio.Text = reportData["LowFireRatio"];
        lowFireFlueTemp.Text = reportData["LowFireFlueTemp"];
        lowFireEfficiency.Text = reportData["LowFireEfficiency"];
        lowFireExcessAir.Text = reportData["LowFireExcessAir"];
        lowFireRoomTemp.Text = reportData["LowFireRoomTemp"];

        engineersName.Text = reportData["engineersName"];
        // engineersSignature.Text = reportData["engineersSignature"]; // Uncomment if using signatures
        engineersGasSafeIDNumber.Text = reportData["engineersGasSafeID"];

        clientsName.Text = reportData["clientsName"];
        // clientsSignature.Text = reportData["clientsSignature"]; // Uncomment if using signatures

        inspectionDate.Text = reportData["inspectionDate"];
        additionalCommentsDefects.Text = reportData["commetsDefects"];
        warningNoticeNumber.Text = reportData["warningNoticeIssueNumber"];

        if (reportData["gasType"] == "NG")
        {
            checkNG.IsChecked = true;
        }
        else checkLPG.IsChecked = true;
    }
    private void GatherReportData()
    {

        reportData = new Dictionary<string, string>
        {
            { "site", site.Text ?? string.Empty },
            { "location", location.Text ?? string.Empty },
            { "assetNumber", assetNo.Text ?? string.Empty },
            { "applianceNumber", applianceNo.Text ?? string.Empty },
            { "testsCompleted", checkTestsCompleted.IsChecked.ToString() },
            { "remedialWorkRequired", checRemedialWorkRequired.IsChecked.ToString() },
            { "applianceMake", applianceMake.Text ?? string.Empty },
            { "applianceModel", applianceModel.Text ?? string.Empty },
            { "applianceSerialNumber", applianceSerialNo.Text ?? string.Empty },
            { "gcNumber", GCNo.Text ?? string.Empty },
            { "Heating", checkHeating.IsChecked.ToString() },
            { "HotWater", checkHotWater.IsChecked.ToString() },
            { "Both", checkBoth.IsChecked.ToString() },
            { "approxAgeOfAppliance", years.Text ?? string.Empty },
            { "badgedInput", badgedInput.Text ?? string.Empty },
            { "badgedOutput", budgedOutput.Text ?? string.Empty },
            { "burnerMake", burnerMake.Text ?? string.Empty },
            { "burnerModel", burnerModel.Text ?? string.Empty },
            { "burnerSerialNumber", burnerSerialNo.Text ?? string.Empty },
            { "Type", type.Text ?? string.Empty },
            { "secs", flameFailureDeviceSecs.Text ?? string.Empty },
            { "Spec", spec.Text ?? string.Empty },
            { "OpenFlue", checkOpenFlue.IsChecked.ToString() },
            { "Roomsealed", checkRoomSealed.IsChecked.ToString() },
            { "ForcedDraft", checkForcedDraft.IsChecked.ToString() },
            { "Flueless", checkFlueless.IsChecked.ToString() },
            { "badgedBurnerPressure", badgedBurnerPressure.Text ?? string.Empty },
            { "ventilationSatisfactory", checkVentilationSatisfactory.IsChecked.ToString() },
            { "flueConditionSatisfactory", checkFlueConditionSatisfactory.IsChecked.ToString() },
            { "tempNG", checkNG.IsChecked.ToString() },
            { "tempLPG", checkLPG.IsChecked.ToString() },
            { "applianceServiceValveSatisfactory", checkAppServiceValve.IsChecked.ToString() },
            { "applianceServiceValveSatisfactoryNA", checkAppServiceValveNA.IsChecked.ToString() },
            { "applianceServiceValveSatisfactoryComments", applianceServiceValveComment.Text ?? string.Empty },
            { "governorsSatisfactory", checkGovernors.IsChecked.ToString() },
            { "governorsSatisfactoryNA", checkGovernorsNA.IsChecked.ToString() },
            { "governorsComments", governorsComment.Text ?? string.Empty },
            { "gasSolenoidValvesSatisfactory", checkGasSolenoidValves.IsChecked.ToString() },
            { "gasSolenoidValvesSatisfactoryNA", checkGasSolenoidValvesNA.IsChecked.ToString() },
            { "gasSolenoidValvesComments", gasSolenoidValvesComment.Text ?? string.Empty },
            { "controlBoxPcbSatisfactory", checkControlBoxPCB.IsChecked.ToString() },
            { "controlBoxPcbSatisfactoryNA", checkControlBoxPCBNA.IsChecked.ToString() },
            { "controlBoxPcbComments", controlBoxPCBComment.Text ?? string.Empty },
            { "gasketSealsSatisfactory", checkGasketSeals.IsChecked.ToString() },
            { "gasketSealsSatisfactoryNA", checkGasketSealsNA.IsChecked.ToString() },
            { "gasketSealsComments", gasketSealsComment.Text ?? string.Empty },
            { "burnerSatisfactory", checkBurner.IsChecked.ToString() },
            { "burnerSatisfactoryNA", checkBurnerNA.IsChecked.ToString() },
            { "burnerComments", burnerComment.Text ?? string.Empty },
            { "burnerJetsSatisfactory", checkBurnerJets.IsChecked.ToString() },
            { "burnerJetsSatisfactoryNA", checkBurnerJetsNA.IsChecked.ToString() },
            { "burnerJetsComments", burnerJetsComment.Text ?? string.Empty },
            { "electrodesTransformerSatisfactory", checkElectrodesTransformer.IsChecked.ToString() },
            { "electrodesTransformerSatisfactoryNA", checkElectrodesTransformerNA.IsChecked.ToString() },
            { "electrodesTransformerComments", electrodesTransformerComment.Text ?? string.Empty },
            { "flameFailureDeviceSatisfactory", checkFlameFailureDevice.IsChecked.ToString() },
            { "flameFailureDeviceSatisfactoryNA", checkFlameFailureDeviceNA.IsChecked.ToString() },
            { "flameFailureDeviceComments", flameFailureDeviceComment.Text ?? string.Empty },
            { "systemBoilerControlsSatisfactory", checkSystemBoilerControls.IsChecked.ToString() },
            { "systemBoilerControlsSatisfactoryNA", checkSystemBoilerControlsNA.IsChecked.ToString() },
            { "systemBoilerControlsComments", systemBoilerControlsComment.Text ?? string.Empty },
            { "boilerCasingSatisfactory", checkBoilerCasing.IsChecked.ToString() },
            { "boilerCasingSatisfactoryNA", checkBoilerCasingNA.IsChecked.ToString() },
            { "boilerCasingComments", boilerCasingComment.Text ?? string.Empty },
            { "thermalInsulationSatisfactory", checkThermalInsulation.IsChecked.ToString() },
            { "thermalInsulationSatisfactoryNA", checkThermalInsulationNA.IsChecked.ToString() },
            { "thermalInsulationComments", thermalInsulationComment.Text ?? string.Empty },//do ovde
            { "combustionFanIdFanSatisfactory", checkCombustionFanIdFan.IsChecked.ToString() },
            { "combustionFanIdFanSatisfactoryNA", checkCombustionFanIdFanNA.IsChecked.ToString() },
            { "combustionFanIdFanComments", combustionFanIdFanComment.Text ?? string.Empty },
            { "airFluePressureSwitchSatisfactory", checkAirFluePressureSwitch.IsChecked.ToString() },
            { "airFluePressureSwitchSatisfactoryNA", checkAirFluePressureSwitchNA.IsChecked.ToString() },
            { "airFluePressureSwitchComments", airFluePressureSwitchComment.Text ?? string.Empty },
            { "controlLimitStatsSatisfactory", checkControlLimitStatus.IsChecked.ToString() },
            { "controlLimitStatsSatisfactoryNA", checkControlLimitStatusNA.IsChecked.ToString() },
            { "controlLimitStatsComments", controlLimitStatusComment.Text ?? string.Empty },
            { "pressureTempGaugesSatisfactory", checkPressureTempGauges.IsChecked.ToString() },
            { "pressureTempGaugesSatisfactoryNA", checkPressureTempGaugesNA.IsChecked.ToString() },
            { "pressureTempGaugesComments", pressureTempGaugesComment.Text ?? string.Empty },
            { "circulationPumpsSatisfactory", checkCirculationPumps.IsChecked.ToString() },
            { "circulationPumpsSatisfactoryNA", checkCirculationPumpsNA.IsChecked.ToString() },
            { "circulationPumpsComments", circulationPumpsComment.Text ?? string.Empty },
            { "condenseTrapSatisfactory", checkCondenseTrap.IsChecked.ToString() },
            { "condenseTrapSatisfactoryNA", checkCondenseTrapNA.IsChecked.ToString() },
            { "condenseTrapComments", condenseTrapComment.Text ?? string.Empty },
            { "heatExhanger", checkHeatExchangerFluewaysClear.IsChecked.ToString() },
            { "heatExhangerNA", checkHeatExchangerFluewaysClearNA.IsChecked.ToString() },
            { "heatExhangerComments", heatExchangerFluewaysClearComment.Text ?? string.Empty },
            { "workingInletPressure", workingIntelPressure.Text ?? string.Empty },
            { "workingInletPressureComments", workingIntelPressureComment.Text ?? string.Empty },
            { "recordedBurnerPressure", recordedBurnerPressure.Text ?? string.Empty },
            { "recordedBurnerPressureComments", recordedBurnerPressureComment.Text ?? string.Empty },
            { "measuredGasRate", measuredGasRate.Text ?? string.Empty },
            { "measuredGasRateComments", mesuredGasRateComment.Text ?? string.Empty },
            { "flueFlowTest", checkFlueFlowTest.IsChecked.ToString() },
            { "flueFlowTestNA", checkFlueFlowTestNA.IsChecked.ToString() },
            { "flueFlowTestComments", flueFlowTestComment.Text ?? string.Empty },
            { "spillageTest", checkSpillageTest.IsChecked.ToString() },
            { "spillageTestNA", checkBoth.IsChecked.ToString() },
            { "spillageTestComments", spillageTestComment.Text ?? string.Empty },
            { "AECVPlantIsolationCorrect", checkAECVPlantIsolationCorrect.IsChecked.ToString() },
            { "AECVPlantIsolationCorrectNA", checkAECVPlantIsolationCorrectNA.IsChecked.ToString() },
            { "AECVPlantIsolationCorrectComments", AECVPlantIsolationCorrectComment.Text ?? string.Empty },
            { "safetyShutOffValve", checkSafetyShutOffValve.IsChecked.ToString() },
            { "safetyShutOffValveNA", checkSafetyShutOffValveNA.IsChecked.ToString() },
            { "safetyShutOffValveComments", safetyShutOffValveComment.Text ?? string.Empty },
            { "plantroomGasTightnessTest", checkPlantroomGasTightnessTest.IsChecked.ToString() },
            { "plantroomGasTightnessTestNA", checkPlantroomGasTightnessTestNA.IsChecked.ToString() },
            { "plantroomGasTightnessTestComments", plantroomGasTightnessTestComment.Text ?? string.Empty },
            { "stateApplianceCondition", stateApplianceCondition.Text ?? string.Empty },
            { "stateApplianceConditionComments", stateApplianceConditionComment.Text ?? string.Empty },
            { "HighFireCO2", highFireCO2.Text ?? string.Empty },
            { "HighFireCO", highFireCO.Text ?? string.Empty },
            { "HighFireO2", highFireO2.Text ?? string.Empty },
            { "HighFireRatio", highFireRatio.Text ?? string.Empty },
            { "HighFireFlueTemp", highFireFlueTemp.Text ?? string.Empty },
            { "HighFireEfficiency", highFireEfficiency.Text ?? string.Empty },
            { "HighFireExcessAir", highFireExcessAir.Text ?? string.Empty },
            { "HighFireRoomTemp", highFireRoomTemp.Text ?? string.Empty },
            { "LowFireCO2", lowFireCO2.Text ?? string.Empty },
            { "LowFireCO", lowFireCO.Text ?? string.Empty },
            { "LowFireO2", lowFireO2.Text ?? string.Empty },
            { "LowFireRatio", lowFireRatio.Text ?? string.Empty },
            { "LowFireFlueTemp", lowFireFlueTemp.Text ?? string.Empty },
            { "LowFireEfficiency", lowFireEfficiency.Text ?? string.Empty },
            { "LowFireExcessAir", lowFireExcessAir.Text ?? string.Empty },
            { "LowFireRoomTemp", lowFireRoomTemp.Text ?? string.Empty },
            { "engineersName", engineersName.Text ?? string.Empty },
            //{ "engineersSignature", engineersSignature.Text ?? string.Empty },
            { "engineersGasSafeID", engineersGasSafeIDNumber.Text ?? string.Empty },
            { "clientsName", clientsName.Text ?? string.Empty },
            //{ "clientsSignature", clientsSignature.Text ?? string.Empty },
            { "inspectionDate", inspectionDate.Text ?? string.Empty },
            { "commetsDefects", additionalCommentsDefects.Text ?? string.Empty },
            { "warningNoticeIssueNumber", warningNoticeNumber.Text ?? string.Empty }
        };

        bool tempNG = checkNG.IsChecked;
        bool tempLPG = checkLPG.IsChecked;
        reportData.Add("gasType", tempNG ? "NG" : (tempLPG ? "LPG" : ""));
        reportData.Add("reportName", reportName);
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
    public void DisjunctCheckboxes(CheckBox a, CheckBox b, CheckBox c, CheckBox d)
    {
        a.IsChecked = true;
        b.IsChecked = false;
        c.IsChecked = false;
        d.IsChecked = false;

        a.Color = Colors.Red;
        b.Color = Colors.White;
        c.Color = Colors.White;
        d.Color = Colors.White;
    }


    public void CheckHotWater(object sender, EventArgs e)
    {
        if (checkHotWater.IsChecked)
            DisjunctCheckboxes(checkHotWater, checkBoth, checkHeating);
        else
        {
            checkHotWater.Color = Colors.White;
            if (!checkBoth.IsChecked)
                DisjunctCheckboxes(checkHeating, checkHotWater, checkBoth);
        }
    }
    public void CheckBoth(object sender, EventArgs e)
    {
        if (checkBoth.IsChecked)
            DisjunctCheckboxes(checkBoth, checkHotWater, checkHeating);
        else
        {
            checkBoth.Color = Colors.White;
            if (!checkHotWater.IsChecked)
                DisjunctCheckboxes(checkHeating, checkHotWater, checkBoth);
        }
    }
    public void CheckHeating(object sender, EventArgs e)
    {
        if (checkHeating.IsChecked || !checkHotWater.IsChecked && !checkBoth.IsChecked)
            DisjunctCheckboxes(checkHeating, checkHotWater, checkBoth);
        else
            checkHeating.Color = Colors.White;
    }


    public void CheckOpenFlue(object sender, EventArgs e)
    {
        if (checkOpenFlue.IsChecked || !checkRoomSealed.IsChecked && !checkForcedDraft.IsChecked && !checkFlueless.IsChecked)
            DisjunctCheckboxes(checkOpenFlue, checkRoomSealed, checkForcedDraft, checkFlueless);
        else
            checkOpenFlue.Color = Colors.White;
    }
    public void CheckRoomSealed(object sender, EventArgs e)
    {
        if (checkRoomSealed.IsChecked)
            DisjunctCheckboxes(checkRoomSealed, checkOpenFlue, checkForcedDraft, checkFlueless);
        else
        {
            checkRoomSealed.Color = Colors.White;
            if (!checkForcedDraft.IsChecked && !checkFlueless.IsChecked)
                DisjunctCheckboxes(checkOpenFlue, checkRoomSealed, checkForcedDraft, checkFlueless);
        }
    }
    public void CheckForcedDraft(object sender, EventArgs e)
    {
        if (checkForcedDraft.IsChecked)
            DisjunctCheckboxes(checkForcedDraft, checkOpenFlue, checkRoomSealed, checkFlueless);
        else
        {
            checkForcedDraft.Color = Colors.White;
            if (!checkFlueless.IsChecked && !checkRoomSealed.IsChecked)
                DisjunctCheckboxes(checkOpenFlue, checkRoomSealed, checkForcedDraft, checkFlueless);
        }
    }
    public void CheckFlueless(object sender, EventArgs e)
    {
        if (checkFlueless.IsChecked)
            DisjunctCheckboxes(checkFlueless, checkForcedDraft, checkOpenFlue, checkRoomSealed);
        else
        {
            checkFlueless.Color = Colors.White;
            if (!checkForcedDraft.IsChecked && !checkRoomSealed.IsChecked)
                DisjunctCheckboxes(checkOpenFlue, checkRoomSealed, checkForcedDraft, checkFlueless);
        }
    }
}