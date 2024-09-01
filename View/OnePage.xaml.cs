using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class OnePage : ContentPage
{
    double pipeworkVolumeNumber;
    double totalPipeworkVolumeNumber = 0;

    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public OnePage()
	{
		InitializeComponent();

        checkAreasWithInadequateVentilationNA.IsChecked = true;
        checkAreaA.IsChecked = true;

        List<Int64> numbers = new List<Int64>();
        for (Int64 i = 1; i <= 1000; i++)
            numbers.Add(i);

        steel1.ItemsSource = numbers;
        steel2.ItemsSource = numbers;
        steel3.ItemsSource = numbers;
        steel4.ItemsSource = numbers;
        steel5.ItemsSource = numbers;
        steel6.ItemsSource = numbers;
        steel7.ItemsSource = numbers;
        steel8.ItemsSource = numbers;
        steel9.ItemsSource = numbers;
        steel10.ItemsSource = numbers;
        steel11.ItemsSource = numbers;
        steel12.ItemsSource = numbers;
        steel13.ItemsSource = numbers;

        copper1.ItemsSource = numbers;
        copper2.ItemsSource = numbers;
        copper3.ItemsSource = numbers;
        copper4.ItemsSource = numbers;
        copper5.ItemsSource = numbers;
        copper6.ItemsSource = numbers;
        copper7.ItemsSource = numbers;

        pesdr1.ItemsSource = numbers;
        pesdr2.ItemsSource = numbers;
        pesdr3.ItemsSource = numbers;
        pesdr4.ItemsSource = numbers;
        pesdr5.ItemsSource = numbers;
        pesdr6.ItemsSource = numbers;
        pesdr7.ItemsSource = numbers;
        pesdr8.ItemsSource = numbers;
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
        OneBackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.EngineersReport, reportName, folder.Id, report);

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

                byte[] pdfData = await PdfCreation.One(reportData, signature1, signature2);

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
    public async void OneBack(object sender, EventArgs e)
	{
        if (OSection1.IsVisible)
        {
            OneBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
        else if (OSection2.IsVisible)
        {
            OSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection1.ScrollToAsync(0, 0, false);
            OSection1.IsVisible = true;
        }
        else if (OSection3.IsVisible)
        {
            OSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection2.ScrollToAsync(0, 0, false);
            OSection2.IsVisible = true;
        }
        else if (OSection4.IsVisible)
        {
            checkAreaA.IsChecked = false;
            checkAreaB.IsChecked = false;
            checkAreaCD.IsChecked = false;

            OSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection3.ScrollToAsync(0, 0, false);
            OSection3.IsVisible = true;
        }
        else if (OSection5.IsVisible)
        {
            OSection5.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection4.ScrollToAsync(0, 0, false);
            OSection4.IsVisible = true;
        }
        else if(OSection6.IsVisible)
        {
            OSection6.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection5.ScrollToAsync(0, 0, false);
            OSection5.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OSection6.ScrollToAsync(0, 0, false);
            OSection6.IsVisible = true;
        }
    }

    
    public async void ONext1(object sender, EventArgs e)
    {
        OSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection2.ScrollToAsync(0, 0, false);
        OSection2.IsVisible = true;
    }
    
    public async void ONext2(object sender, EventArgs e)
    {
        OSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection3.ScrollToAsync(0, 0, false);
        OSection3.IsVisible = true;
    }
    public int oskip22 = 0; public int oskip33 = 0;
    public async void OSkip2(object sender, EventArgs e)
    {
        OSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection3.ScrollToAsync(0, 0, false);
        OSection3.IsVisible = true;
        oskip22 = 1;
        // ...
    }

    private async void ONext3()
    {
        OSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection4.ScrollToAsync(0, 0, false);
        OSection4.IsVisible = true;

        if (!new[] { null, "", ".", "-", "+"}.Contains(gaugeReadableMovement.Text) && totalVolumeForTesting.Text != null && testMediumFactor.Text != null)
        {
            AreaA_Value.Text = Math.Round(double.Parse(gaugeReadableMovement.Text) * double.Parse(totalVolumeForTesting.Text) * double.Parse(testMediumFactor.Text), 3).ToString();
            if (!new[] { null, "", ".", "-", "+" }.Contains(roomVolume.Text))
                AreaB_Value.Text = Math.Round(2.8 * double.Parse(AreaA_Value.Text) / double.Parse(roomVolume.Text), 3).ToString();
            AreaCD_Value.Text = Math.Round(0.047 * double.Parse(AreaA_Value.Text), 3).ToString();
        }
        else
        {
            AreaA_Value.Text = null;
            AreaB_Value.Text = null;
            AreaCD_Value.Text = null;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }

    public void ONext3(object sender, EventArgs e)
    {
        ONext3();
    }

    public void OSkip3(object sender, EventArgs e)
    {
        ONext3();
    oskip33 = 1;
        // ...
    }
    
    public async void ONext4(object sender, EventArgs e)
    {
        OSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection5.ScrollToAsync(0, 0, false);
        OSection5.IsVisible = true;
    }
    
    public async void ONext5(object sender, EventArgs e)
    {
        OSection5.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OSection6.ScrollToAsync(0, 0, false);
        OSection6.IsVisible = true;
        await LoadFolders();

    }
    public async void ONextFinish(object sender, EventArgs e)
    {
        OSection6.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"1_Tightness_Testing__{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.One(GatherReportData());
    }
    public void PreviewOnePage(Dictionary<string, string> reportData)
    {
        // Populate site and location
        if (reportData.ContainsKey("site"))
            site.Text = reportData["site"];

        if (reportData.ContainsKey("location"))
            location.Text = reportData["location"];

        // Populate steel items
        if (reportData.ContainsKey("steel1"))
            steel1.SelectedItem = reportData["steel1"];

        if (reportData.ContainsKey("steel1Total"))
            steel1Total.Text = reportData["steel1Total"];

        if (reportData.ContainsKey("steel2"))
            steel2.SelectedItem = reportData["steel2"];

        if (reportData.ContainsKey("steel2Total"))
            steel2Total.Text = reportData["steel2Total"];

        if (reportData.ContainsKey("steel3"))
            steel3.SelectedItem = reportData["steel3"];

        if (reportData.ContainsKey("steel3Total"))
            steel3Total.Text = reportData["steel3Total"];

        if (reportData.ContainsKey("steel4"))
            steel4.SelectedItem = reportData["steel4"];

        if (reportData.ContainsKey("steel4Total"))
            steel4Total.Text = reportData["steel4Total"];

        if (reportData.ContainsKey("steel5"))
            steel5.SelectedItem = reportData["steel5"];

        if (reportData.ContainsKey("steel5Total"))
            steel5Total.Text = reportData["steel5Total"];

        if (reportData.ContainsKey("steel6"))
            steel6.SelectedItem = reportData["steel6"];

        if (reportData.ContainsKey("steel6Total"))
            steel6Total.Text = reportData["steel6Total"];

        if (reportData.ContainsKey("steel7"))
            steel7.SelectedItem = reportData["steel7"];

        if (reportData.ContainsKey("steel7Total"))
            steel7Total.Text = reportData["steel7Total"];

        if (reportData.ContainsKey("steel8"))
            steel8.SelectedItem = reportData["steel8"];

        if (reportData.ContainsKey("steel8Total"))
            steel8Total.Text = reportData["steel8Total"];

        if (reportData.ContainsKey("steel9"))
            steel9.SelectedItem = reportData["steel9"];

        if (reportData.ContainsKey("steel9Total"))
            steel9Total.Text = reportData["steel9Total"];

        if (reportData.ContainsKey("steel10"))
            steel10.SelectedItem = reportData["steel10"];

        if (reportData.ContainsKey("steel10Total"))
            steel10Total.Text = reportData["steel10Total"];

        if (reportData.ContainsKey("steel11"))
            steel11.SelectedItem = reportData["steel11"];

        if (reportData.ContainsKey("steel11Total"))
            steel11Total.Text = reportData["steel11Total"];

        if (reportData.ContainsKey("steel12"))
            steel12.SelectedItem = reportData["steel12"];

        if (reportData.ContainsKey("steel12Total"))
            steel12Total.Text = reportData["steel12Total"];

        if (reportData.ContainsKey("steel13"))
            steel13.SelectedItem = reportData["steel13"];

        if (reportData.ContainsKey("steel13Total"))
            steel13Total.Text = reportData["steel13Total"];

        // Populate copper items
        if (reportData.ContainsKey("copper1"))
            copper1.SelectedItem = reportData["copper1"];

        if (reportData.ContainsKey("copper1Total"))
            copper1Total.Text = reportData["copper1Total"];

        if (reportData.ContainsKey("copper2"))
            copper2.SelectedItem = reportData["copper2"];

        if (reportData.ContainsKey("copper2Total"))
            copper2Total.Text = reportData["copper2Total"];

        if (reportData.ContainsKey("copper3"))
            copper3.SelectedItem = reportData["copper3"];

        if (reportData.ContainsKey("copper3Total"))
            copper3Total.Text = reportData["copper3Total"];

        if (reportData.ContainsKey("copper4"))
            copper4.SelectedItem = reportData["copper4"];

        if (reportData.ContainsKey("copper4Total"))
            copper4Total.Text = reportData["copper4Total"];

        if (reportData.ContainsKey("copper5"))
            copper5.SelectedItem = reportData["copper5"];

        if (reportData.ContainsKey("copper5Total"))
            copper5Total.Text = reportData["copper5Total"];

        if (reportData.ContainsKey("copper6"))
            copper6.SelectedItem = reportData["copper6"];

        if (reportData.ContainsKey("copper6Total"))
            copper6Total.Text = reportData["copper6Total"];

        if (reportData.ContainsKey("copper7"))
            copper7.SelectedItem = reportData["copper7"];

        if (reportData.ContainsKey("copper7Total"))
            copper7Total.Text = reportData["copper7Total"];

        // Populate PESDR items
        if (reportData.ContainsKey("pesdr1"))
            pesdr1.SelectedItem = reportData["pesdr1"];

        if (reportData.ContainsKey("pesdr1Total"))
            pesdr1Total.Text = reportData["pesdr1Total"];

        if (reportData.ContainsKey("pesdr2"))
            pesdr2.SelectedItem = reportData["pesdr2"];

        if (reportData.ContainsKey("pesdr2Total"))
            pesdr2Total.Text = reportData["pesdr2Total"];

        if (reportData.ContainsKey("pesdr3"))
            pesdr3.SelectedItem = reportData["pesdr3"];

        if (reportData.ContainsKey("pesdr3Total"))
            pesdr3Total.Text = reportData["pesdr3Total"];

        if (reportData.ContainsKey("pesdr4"))
            pesdr4.SelectedItem = reportData["pesdr4"];

        if (reportData.ContainsKey("pesdr4Total"))
            pesdr4Total.Text = reportData["pesdr4Total"];

        if (reportData.ContainsKey("pesdr5"))
            pesdr5.SelectedItem = reportData["pesdr5"];

        if (reportData.ContainsKey("pesdr5Total"))
            pesdr5Total.Text = reportData["pesdr5Total"];

        if (reportData.ContainsKey("pesdr6"))
            pesdr6.SelectedItem = reportData["pesdr6"];

        if (reportData.ContainsKey("pesdr6Total"))
            pesdr6Total.Text = reportData["pesdr6Total"];

        if (reportData.ContainsKey("pesdr7"))
            pesdr7.SelectedItem = reportData["pesdr7"];

        if (reportData.ContainsKey("pesdr7Total"))
            pesdr7Total.Text = reportData["pesdr7Total"];

        if (reportData.ContainsKey("pesdr8"))
            pesdr8.SelectedItem = reportData["pesdr8"];

        if (reportData.ContainsKey("pesdr8Total"))
            pesdr8Total.Text = reportData["pesdr8Total"];

        // Populate other fields
        if (reportData.ContainsKey("meterVolumePicker"))
            meterVolumePicker.SelectedItem = reportData["meterVolumePicker"];

        if (reportData.ContainsKey("testMediumPicker"))
            testMediumPicker.SelectedItem = reportData["testMediumPicker"];

        if (reportData.ContainsKey("installationPicker"))
            installationPicker.SelectedItem = reportData["installationPicker"];

        if (reportData.ContainsKey("totalPipeworkVolume"))
            totalPipeworkVolume.Text = reportData["totalPipeworkVolume"];

        if (reportData.ContainsKey("pipeworkFittingsIV"))
            pipeworkFittingsIV.Text = reportData["pipeworkFittingsIV"];

        if (reportData.ContainsKey("meterVolume"))
            meterVolume.Text = reportData["meterVolume"];

        if (reportData.ContainsKey("totalVolumeForTesting"))
            totalVolumeForTesting.Text = reportData["totalVolumeForTesting"];

        if (reportData.ContainsKey("checkIsWeatherTemperatureStableYes"))
        {
            bool isWeatherStableYes = reportData["checkIsWeatherTemperatureStableYes"] == "True";
            checkIsWeatherTemperatureStableYes.IsChecked = isWeatherStableYes;
            checkIsWeatherTemperatureStableNo.IsChecked = !isWeatherStableYes;
        }

        if (reportData.ContainsKey("checkMeterBypassYes"))
        {
            bool isMeterBypassYes = reportData["checkMeterBypassYes"] == "True";
            checkMeterBypassYes.IsChecked = isMeterBypassYes;
            checkMeterBypassNo.IsChecked = !isMeterBypassYes;
        }

        //
        if (reportData.ContainsKey("checkBarometricPressureCorrectionYes"))
        {
            bool isWeatherStableYes1 = reportData["checkBarometricPressureCorrectionYes"] == "True";
            checkBarometricPressureCorrectionYes.IsChecked = isWeatherStableYes1;
            checkBarometricPressureCorrectionNo.IsChecked = !isWeatherStableYes1;
        }

        if (reportData.ContainsKey("checkComponentsRemovedBypassedYes"))
        {
            bool isMeterBypassYes1 = reportData["checkComponentsRemovedBypassedYes"] == "True";
            checkComponentsRemovedBypassedYes.IsChecked = isMeterBypassYes1;
            checkComponentsRemovedBypassedNo.IsChecked = !isMeterBypassYes1;
        }
        // Populate text fields
        if (reportData.ContainsKey("testMediumFactor"))
            testMediumFactor.Text = reportData["testMediumFactor"];

        if (reportData.ContainsKey("testGaugeUsed"))
            testGuageUsed.Text = reportData["testGaugeUsed"];

        if (reportData.ContainsKey("tightnessTestPressure"))
            tightnessTestPressure.Text = reportData["tightnessTestPressure"];

        if (reportData.ContainsKey("gaugeReadableMovement"))
            gaugeReadableMovement.Text = reportData["gaugeReadableMovement"];

        if (reportData.ContainsKey("strengthTestPressure"))
            strengthTestPressure.Text = reportData["strengthTestPressure"];

        if (reportData.ContainsKey("stabilisationPeriod"))
            stabilisationPeriod.Text = reportData["stabilisationPeriod"];

        if (reportData.ContainsKey("strenghtTestDuration"))
            strenghtTestDuration.Text = reportData["strenghtTestDuration"];

        if (reportData.ContainsKey("permittedPressureDrop"))
            permittedPressureDrop.Text = reportData["permittedPressureDrop"];

        if (reportData.ContainsKey("actualPressureDrop"))
            actualPressureDrop.Text = reportData["actualPressureDrop"];

        if (reportData.ContainsKey("letByDuration"))
            letByDuration.Text = reportData["letByDuration"];

        if (reportData.ContainsKey("stabilisationDuration"))
            stabilisationDuration.Text = reportData["stabilisationDuration"];

        if (reportData.ContainsKey("testDuration"))
            testDuration.Text = reportData["testDuration"];

        if (reportData.ContainsKey("actualPressureDropResult"))
            actualPressureDropResult.Text = reportData["actualPressureDropResult"];

        if (reportData.ContainsKey("date"))
            date.Text = reportData["date"];

        if (reportData.ContainsKey("engineer"))
            engineer.Text = reportData["engineer"];

        if (reportData.ContainsKey("cardNumber"))
            cardNumber.Text = reportData["cardNumber"];

        if (reportData.ContainsKey("clientsName"))
            clientsName.Text = reportData["clientsName"];

        if (reportData.ContainsKey("WarningNoticeNo"))
            WarningNoticeRefNo.Text = reportData["WarningNoticeNo"];

        if (reportData.ContainsKey("AreaA_Value"))
            AreaA_Value.Text = reportData["AreaA_Value"];

        if (reportData.ContainsKey("AreaB_Value"))
            AreaB_Value.Text = reportData["AreaB_Value"];

        if (reportData.ContainsKey("roomVolume"))
            roomVolume.Text = reportData["roomVolume"];

        if (reportData.ContainsKey("AreaCD_Value"))
            AreaCD_Value.Text = reportData["AreaCD_Value"];

        // Populate combo boxes
        if (reportData.ContainsKey("maximumPermittedLeakRate"))
            maximumPermittedLeakRate.SelectedItem = reportData["maximumPermittedLeakRate"];

        if (reportData.ContainsKey("testPassedOrFailed"))
            testPassedOrFailed.SelectedItem = reportData["testPassedOrFailed"];

        // Populate checkboxes
        if (reportData.ContainsKey("checkAreaA"))
            checkAreaA.IsChecked = bool.Parse(reportData["checkAreaA"]);

        if (reportData.ContainsKey("checkAreaB"))
            checkAreaB.IsChecked = bool.Parse(reportData["checkAreaB"]);
        if (reportData.ContainsKey("checkTestPassedOrFailedPass"))
        {
            checkTestPassedOrFailedPass.IsChecked = reportData["checkTestPassedOrFailedPass"] == "Passed";
        }
        if (reportData.ContainsKey("checkAreasWithInadequateVentilationYes"))
        {
            string value = reportData["checkAreasWithInadequateVentilationYes"];
            checkAreasWithInadequateVentilationYes.IsChecked = (value == "yes");
            checkAreasWithInadequateVentilationNA.IsChecked = (value == "N/A");
            checkAreasWithInadequateVentilationNo.IsChecked = (value == "no");
        }
        if (reportData.ContainsKey("actualLeakRateResult"))
            actualLeakRateResult.Text = reportData["actualLeakRateResult"];

    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());


        reportData.Add("site", site.Text ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        if (steel1.SelectedIndex != -1)
            reportData.Add("steel1", (steel1.SelectedItem).ToString());
        else
            reportData.Add("steel1", string.Empty);
        reportData.Add("steel1Total", steel1Total.Text ?? string.Empty);

        if (steel2.SelectedIndex != -1)
            reportData.Add("steel2", (steel2.SelectedItem).ToString());
        else
            reportData.Add("steel2", string.Empty);
        reportData.Add("steel2Total", steel2Total.Text ?? string.Empty);


        if (steel3.SelectedIndex != -1)
            reportData.Add("steel3", (steel3.SelectedItem).ToString());
        else
            reportData.Add("steel3", string.Empty);
        reportData.Add("steel3Total", steel3Total.Text ?? string.Empty);


        if (steel4.SelectedIndex != -1)
            reportData.Add("steel4", (steel4.SelectedItem).ToString());
        else
            reportData.Add("steel4", string.Empty);
        reportData.Add("steel4Total", steel4Total.Text ?? string.Empty);


        if (steel5.SelectedIndex != -1)
            reportData.Add("steel5", (steel5.SelectedItem).ToString());
        else
            reportData.Add("steel5", string.Empty);
        reportData.Add("steel5Total", steel5Total.Text ?? string.Empty);


        if (steel6.SelectedIndex != -1)
            reportData.Add("steel6", (steel6.SelectedItem).ToString());
        else
            reportData.Add("steel6", string.Empty);
        reportData.Add("steel6Total", steel6Total.Text ?? string.Empty);

        if (steel7.SelectedIndex != -1)
            reportData.Add("steel7", (steel7.SelectedItem).ToString());
        else
            reportData.Add("steel7", string.Empty);
        reportData.Add("steel7Total", steel7Total.Text ?? string.Empty);

        if (steel8.SelectedIndex != -1)
            reportData.Add("steel8", (steel8.SelectedItem).ToString());
        else
            reportData.Add("steel8", string.Empty);
        reportData.Add("steel8Total", steel8Total.Text ?? string.Empty);

        if (steel9.SelectedIndex != -1)
            reportData.Add("steel9", (steel9.SelectedItem).ToString());
        else
            reportData.Add("steel9", string.Empty);
        reportData.Add("steel9Total", steel9Total.Text ?? string.Empty);

        if (steel10.SelectedIndex != -1)
            reportData.Add("steel10", (steel10.SelectedItem).ToString());
        else
            reportData.Add("steel10", string.Empty);
        reportData.Add("steel10Total", steel10Total.Text ?? string.Empty);

        if (steel11.SelectedIndex != -1)
            reportData.Add("steel11", (steel11.SelectedItem).ToString());
        else
            reportData.Add("steel11", string.Empty);
        reportData.Add("steel11Total", steel11Total.Text ?? string.Empty);

        if (steel12.SelectedIndex != -1)
            reportData.Add("steel12", (steel12.SelectedItem).ToString());
        else
            reportData.Add("steel12", string.Empty);
        reportData.Add("steel12Total", steel12Total.Text ?? string.Empty);

        if (steel13.SelectedIndex != -1)
            reportData.Add("steel13", (steel13.SelectedItem).ToString());
        else
            reportData.Add("steel13", string.Empty);
        reportData.Add("steel13Total", steel13Total.Text ?? string.Empty);
        //copper
        if (copper1.SelectedIndex != -1)
            reportData.Add("copper1", (copper1.SelectedItem).ToString());
        else
            reportData.Add("copper1", string.Empty);
        reportData.Add("copper1Total", copper1Total.Text ?? string.Empty);

        if (copper2.SelectedIndex != -1)
            reportData.Add("copper2", (copper2.SelectedItem).ToString());
        else
            reportData.Add("copper2", string.Empty);
        reportData.Add("copper2Total", copper2Total.Text ?? string.Empty);

        if (copper3.SelectedIndex != -1)
            reportData.Add("copper3", (copper3.SelectedItem).ToString());
        else
            reportData.Add("copper3", string.Empty);
        reportData.Add("copper3Total", copper3Total.Text ?? string.Empty);

        if (copper4.SelectedIndex != -1)
            reportData.Add("copper4", (copper4.SelectedItem).ToString());
        else
            reportData.Add("copper4", string.Empty);
        reportData.Add("copper4Total", copper4Total.Text ?? string.Empty);

        if (copper5.SelectedIndex != -1)
            reportData.Add("copper5", (copper5.SelectedItem).ToString());
        else
            reportData.Add("copper5", string.Empty);
        reportData.Add("copper5Total", copper5Total.Text ?? string.Empty);
        //if (copper5.SelectedIndex != -1)
        //    reportData.Add("copper5", (copper5.SelectedItem).ToString());
        //else
        //    reportData.Add("copper5", string.Empty);
        //reportData.Add("copper5Total", copper5Total.Text ?? string.Empty);

        if (copper6.SelectedIndex != -1)
            reportData.Add("copper6", (copper6.SelectedItem).ToString());
        else
            reportData.Add("copper6", string.Empty);
        reportData.Add("copper6Total", copper6Total.Text ?? string.Empty);

        if (copper7.SelectedIndex != -1)
            reportData.Add("copper7", (copper7.SelectedItem).ToString());
        else
            reportData.Add("copper7", string.Empty);
        reportData.Add("copper7Total", copper7Total.Text ?? string.Empty);


        if (pesdr1.SelectedIndex != -1)
            reportData.Add("pesdr1", (pesdr1.SelectedItem).ToString());
        else
            reportData.Add("pesdr1", string.Empty);
        reportData.Add("pesdr1Total", pesdr1Total.Text ?? string.Empty);

        if (pesdr2.SelectedIndex != -1)
            reportData.Add("pesdr2", (pesdr2.SelectedItem).ToString());
        else
            reportData.Add("pesdr2", string.Empty);
        reportData.Add("pesdr2Total", pesdr2Total.Text ?? string.Empty);


        if (pesdr3.SelectedIndex != -1)
            reportData.Add("pesdr3", (pesdr1.SelectedItem).ToString());
        else
            reportData.Add("pesdr3", string.Empty);
        reportData.Add("pesdr3Total", pesdr3Total.Text ?? string.Empty);

        if (pesdr4.SelectedIndex != -1)
            reportData.Add("pesdr4", (pesdr4.SelectedItem).ToString());
        else
            reportData.Add("pesdr4", string.Empty);
        reportData.Add("pesdr4Total", pesdr4Total.Text ?? string.Empty);

        if (pesdr5.SelectedIndex != -1)
            reportData.Add("pesdr5", (pesdr5.SelectedItem).ToString());
        else
            reportData.Add("pesdr5", string.Empty);
        reportData.Add("pesdr5Total", pesdr5Total.Text ?? string.Empty);

        if (pesdr6.SelectedIndex != -1)
            reportData.Add("pesdr6", (pesdr6.SelectedItem).ToString());
        else
            reportData.Add("pesdr6", string.Empty);
        reportData.Add("pesdr6Total", pesdr6Total.Text ?? string.Empty);

        if (pesdr7.SelectedIndex != -1)
            reportData.Add("pesdr7", (pesdr7.SelectedItem).ToString());
        else
            reportData.Add("pesdr7", string.Empty);
        reportData.Add("pesdr7Total", pesdr7Total.Text ?? string.Empty);

        if (pesdr8.SelectedIndex != -1)
            reportData.Add("pesdr8", (pesdr1.SelectedItem).ToString());
        else
            reportData.Add("pesdr8", string.Empty);
        reportData.Add("pesdr8Total", pesdr8Total.Text ?? string.Empty);

        if (meterVolumePicker.SelectedIndex != -1)
            reportData.Add("meterVolumePicker", (meterVolumePicker.SelectedItem).ToString());
        else
            reportData.Add("meterVolumePicker", string.Empty);

        if (testMediumPicker.SelectedIndex != -1)
            reportData.Add("testMediumPicker", (testMediumPicker.SelectedItem).ToString());
        else
            reportData.Add("testMediumPicker", string.Empty);

        if (installationPicker.SelectedIndex != -1)
            reportData.Add("installationPicker", (installationPicker.SelectedItem).ToString());
        else
            reportData.Add("installationPicker", string.Empty);

        reportData.Add("totalPipeworkVolume", totalPipeworkVolume.Text ?? string.Empty);
        reportData.Add("pipeworkFittingsIV", pipeworkFittingsIV.Text ?? string.Empty);
        reportData.Add("meterVolume", meterVolume.Text ?? string.Empty);
        reportData.Add("totalVolumeForTesting", totalVolumeForTesting.Text ?? string.Empty);
        //till here
        if (checkIsWeatherTemperatureStableYes.IsChecked)
            reportData.Add("checkIsWeatherTemperatureStableYes", "yes");
        else reportData.Add("checkIsWeatherTemperatureStableYes", "no");

        //checkMeterBypassYes,checkInadequateVentilationYes
        if (checkMeterBypassYes.IsChecked)
            reportData.Add("checkMeterBypassYes", "yes");
        else reportData.Add("checkMeterBypassYes", "no");
        reportData.Add("testMediumFactor", testMediumFactor.Text ?? string.Empty);
        reportData.Add("testGaugeUsed", testGuageUsed.Text ?? string.Empty);

        //if (testGaugeUsed.SelectedIndex != -1)
        //    reportData.Add("testGaugeUsed", (testGaugeUsed.SelectedItem).ToString());
        //else
        //    reportData.Add("testGaugeUsed", string.Empty);

        reportData.Add("tightnessTestPressure", tightnessTestPressure.Text ?? string.Empty);
        reportData.Add("gaugeReadableMovement", gaugeReadableMovement.Text ?? string.Empty);
        if (maximumPermittedLeakRate.SelectedIndex != -1)
            reportData.Add("maximumPermittedLeakRate", (maximumPermittedLeakRate.SelectedItem).ToString());
        else
            reportData.Add("maximumPermittedLeakRate", string.Empty);
        //checkBarometricPressureCorrectionYes

        if (checkBarometricPressureCorrectionYes.IsChecked)
            reportData.Add("checkBarometricPressureCorrectionYes", "yes");
        else reportData.Add("checkBarometricPressureCorrectionYes", "no");

        reportData.Add("strengthTestPressure", strengthTestPressure.Text ?? string.Empty);
        ////checkComponentsRemovedBypassedYes
        if (checkComponentsRemovedBypassedYes.IsChecked)
            reportData.Add("checkComponentsRemovedBypassedYes", "yes");
        else reportData.Add("checkComponentsRemovedBypassedYes", "no");
        reportData.Add("stabilisationPeriod", stabilisationPeriod.Text ?? string.Empty);
        reportData.Add("strenghtTestDuration", strenghtTestDuration.Text ?? string.Empty);
        reportData.Add("permittedPressureDrop", permittedPressureDrop.Text ?? string.Empty);
        reportData.Add("actualPressureDrop", actualPressureDrop.Text ?? string.Empty);

        //checkTestPassedOrFailedPass,testPassedOrFailed
        if (checkTestPassedOrFailedPass.IsChecked)
            reportData.Add("checkTestPassedOrFailedPass", "Passed");
        else reportData.Add("checkTestPassedOrFailedPass", "Failed");
        reportData.Add("letByDuration", letByDuration.Text ?? string.Empty);
        reportData.Add("stabilisationDuration", stabilisationDuration.Text ?? string.Empty);
        reportData.Add("testDuration", testDuration.Text ?? string.Empty);
        reportData.Add("actualPressureDropResult", actualPressureDropResult.Text ?? string.Empty);

        if (testPassedOrFailed.SelectedIndex != -1)
            reportData.Add("testPassedOrFailed", (testPassedOrFailed.SelectedItem).ToString());
        else
            reportData.Add("testPassedOrFailed", string.Empty);

        reportData.Add("date", date.Text ?? string.Empty);
        reportData.Add("engineer", engineer.Text ?? string.Empty);
        reportData.Add("cardNumber", cardNumber.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("WarningNoticeNo", WarningNoticeRefNo.Text ?? string.Empty);

        reportData.Add("checkAreaA", checkAreaA.IsChecked.ToString());
        reportData.Add("checkAreaB", checkAreaA.IsChecked.ToString());
        //AreaA_Value
        reportData.Add("AreaA_Value", AreaA_Value.Text ?? string.Empty);
        reportData.Add("AreaB_Value", AreaA_Value.Text ?? string.Empty);
        reportData.Add("roomVolume", roomVolume.Text ?? string.Empty);
        reportData.Add("AreaCD_Value", AreaA_Value.Text ?? string.Empty);

      // reportData.Add("letByDuration", letByDuration.Text ?? string.Empty);
      //  reportData.Add("stabilisationDuration", stabilisationDuration.Text ?? string.Empty);
      //  reportData.Add("testDuration", testDuration.Text ?? string.Empty);
      //  reportData.Add("actualPressureDropResult", actualPressureDropResult.Text ?? string.Empty);
        reportData.Add("actualLeakRateResult", actualLeakRateResult.Text ?? string.Empty);
        if (checkAreasWithInadequateVentilationYes.IsChecked)
            reportData.Add("checkAreasWithInadequateVentilationYes", "yes");
        else if(checkAreasWithInadequateVentilationNA.IsChecked) reportData.Add("checkAreasWithInadequateVentilationYes", "N/A");
        else reportData.Add("checkAreasWithInadequateVentilationYes", "no");
        //testPassedOrFailed
        //if (testPassedOrFailed.SelectedIndex != -1)
        //    reportData.Add("testPassedOrFailed", (testPassedOrFailed.SelectedItem).ToString());
        //else
        //    reportData.Add("testPassedOrFailed", string.Empty);

        if (oskip22 == 1)
        {
            reportData["testMediumPicker"] = "N/A";
            reportData["installationPicker"] = "N/A";
            reportData["testMediumFactor"] = "N/A";
            reportData["checkMeterBypassYes"] = "N/A";
            reportData["testGaugeUsed"] = "N/A";
            reportData["checkIsWeatherTemperatureStableYes"] = "N/A";
            reportData["tightnessTestPressure"] = "N/A";
            reportData["gaugeReadableMovement"] = "N/A";
            reportData["maximumPermittedLeakRate"] = "N/A";
            reportData["checkBarometricPressureCorrectionYes"] = "N/A";
         
        }
        if (oskip33 == 1)
        {
            reportData["strengthTestPressure"] = "N/A";
            reportData["checkComponentsRemovedBypassedYes"] = "N/A";
            reportData["stabilisationPeriod"] = "N/A";
            reportData["strenghtTestDuration"] = "N/A";
            reportData["permittedPressureDrop"] = "N/A";
            reportData["actualPressureDrop"] = "N/A";
            reportData["testPassedOrFailed"] = "N/A";
       
        }

        return reportData;
    }

    private void AreaA_Control()
    {
        checkAreaA.IsChecked = true;
        checkAreaA.Color = Colors.Red;
        checkAreaB.IsChecked = false;
        checkAreaCD.IsChecked = false;

        AreaB.IsVisible = false;
        AreaCD.IsVisible = false;
        AreaA.IsVisible = true;

        if (AreaA_Value.Text != null)
        {
            letByDuration.Text = "2";
            stabilisationDuration.Text = "15";
            testDuration.Text = "2";

            double min = Math.Ceiling(double.Parse(AreaA_Value.Text));
            string min_string = min.ToString();

            if (min > 15)
            {
                letByDuration.Text = min_string;
                stabilisationDuration.Text = min_string;
                testDuration.Text = min_string;
            }
            else if (min > 2)
            {
                letByDuration.Text = min_string;
                testDuration.Text = min_string;
            }
        }
        else
        {
            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }
    public void checkAreaA_Tap(object sender, EventArgs e)
    {
        if (checkAreaA.IsChecked || (!checkAreaB.IsChecked && !checkAreaCD.IsChecked))
            AreaA_Control();
        else
        {
            checkAreaA.Color = Colors.White;
            AreaA.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }
    public void checkAreaB_Tap(object sender, EventArgs e)
    {
        if (checkAreaB.IsChecked)
        {
            checkAreaB.Color = Colors.Red;
            checkAreaA.IsChecked = false;
            checkAreaCD.IsChecked = false;

            AreaA.IsVisible = false;
            AreaCD.IsVisible = false;
            AreaB.IsVisible = true;

            if (AreaB_Value.Text != null)
            {
                letByDuration.Text = "2";
                stabilisationDuration.Text = "15";
                testDuration.Text = "2";

                double min = Math.Ceiling(double.Parse(AreaB_Value.Text));
                string min_string = min.ToString();

                if (min > 15)
                {
                    letByDuration.Text = min_string;
                    stabilisationDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
                else if (min > 2)
                {
                    letByDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
            }
            else
            {
                letByDuration.Text = null;
                stabilisationDuration.Text = null;
                testDuration.Text = null;
            }
        }
        else
        {
            checkAreaB.Color = Colors.White;
            AreaB.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;

            if (!checkAreaCD.IsChecked)
                AreaA_Control();
        }
    }
    public void checkAreaCD_Tap(object sender, EventArgs e)
    {
        if (checkAreaCD.IsChecked)
        {
            checkAreaCD.Color = Colors.Red;
            checkAreaB.IsChecked = false;
            checkAreaA.IsChecked = false;

            AreaA.IsVisible = false;
            AreaB.IsVisible = false;
            AreaCD.IsVisible = true;

            if (AreaCD_Value.Text != null)
            {
                letByDuration.Text = "2";
                stabilisationDuration.Text = "15";
                testDuration.Text = "2";

                double min = Math.Ceiling(double.Parse(AreaCD_Value.Text));
                string min_string = min.ToString();

                if (min > 15)
                {
                    letByDuration.Text = min_string;
                    stabilisationDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
                else if (min > 2)
                {
                    letByDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
            }
            else
            {
                letByDuration.Text = null;
                stabilisationDuration.Text = null;
                testDuration.Text = null;
            }
        }
        else
        {
            checkAreaCD.Color = Colors.White;
            AreaCD.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;

            if (!checkAreaB.IsChecked)
                AreaA_Control();
        }
    }


    //public void letByDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(letByDuration);
    //}
    //public void stabilisationDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(stabilisationDuration);
    //}
    //public void testDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(testDuration);
    //}


    private void UpdateTotalPipeworkVolume(Label total, Picker quantificator, double k)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(quantificator.SelectedItem.ToString()) * k, 5);
        total.Text = pipeworkVolumeNumber.ToString();

        totalPipeworkVolumeNumber += pipeworkVolumeNumber;
        totalPipeworkVolume.Text = Math.Round(totalPipeworkVolumeNumber, 5).ToString();
        pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

        if (meterVolume.Text != null && meterVolume.Text != "" && meterVolume.Text != "." && meterVolume.Text != "-")
            totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 5).ToString();
    }
    private void SubtractTotalPipeworkVolume()
    {
        double subtract = Math.Round(totalPipeworkVolumeNumber - pipeworkVolumeNumber, 5);
        if (subtract == 0)
        {
            totalPipeworkVolumeNumber = 0;
            totalPipeworkVolume.Text = null;
            pipeworkFittingsIV.Text = null;
            totalVolumeForTesting.Text = null;
        }
        else
        {
            totalPipeworkVolumeNumber = subtract;
            totalPipeworkVolume.Text = subtract.ToString();
            pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

            if (meterVolume.Text != null && meterVolume.Text != "" && meterVolume.Text != "." && meterVolume.Text != "-")
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 5).ToString();
        }
    }

    //private void EntryChanged(Entry entry)
    //{
    //    double min;
    //    double setMin = entry == stabilisationDuration ? 15 : 2;
    //    string value = entry.Text;

    //    if (!checkAreaA.IsChecked && !checkAreaB.IsChecked && !checkAreaCD.IsChecked)
    //        entry.Text = null;
    //    else if (checkAreaA.IsChecked)
    //    {
    //        if (AreaA_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaA_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //    else if (checkAreaB.IsChecked)
    //    {
    //        if (AreaB_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaB_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //    else
    //    {
    //        if (AreaCD_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaCD_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //}


    public async void meterVolumePicker_IndexChanged(object sender, EventArgs e)
    {
        if (meterVolumePicker.SelectedIndex != -1)
        {
            meterVolumePicker_delete.IsVisible = true;

            switch (meterVolumePicker.SelectedItem)
            {
                case "G4 / U6": meterVolume.Text = "0.008"; break;
                case "U16": meterVolume.Text = "0.025"; break;
                case "U25": meterVolume.Text = "0.037"; break;
                case "U40": meterVolume.Text = "0.067"; break;
                case "U65": meterVolume.Text = "0.1"; break;
                case "U100": meterVolume.Text = "0.182"; break;
                case "U160": meterVolume.Text = "0.304"; break;
                //case "RD or Turbine": meterVolume.Text = "0.079d2L*"; break;
                case "Ultrasonic": meterVolume.Text = "0.0024"; break;
                case "Custom": meterVolume.Text = await DisplayPromptAsync("Meter Volume", "Enter the custom value:", keyboard: Keyboard.Numeric); break;
            }

            if (meterVolume.Text != null && meterVolume.Text != "" && meterVolume.Text != "-" && meterVolume.Text != "." && double.Parse(meterVolume.Text) > 0)
            {
                if (pipeworkFittingsIV.Text != null)
                    totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
            }
            else
            {
                meterVolume.Text = null;
                meterVolumePicker_Delete();
            }
        }
        else
        {
            meterVolumePicker_delete.IsVisible = false;
        }
    }
    public void meterVolumePicker_Delete(object sender, EventArgs e)
    {
        meterVolume.Text = null;
        totalVolumeForTesting.Text = null;
        meterVolumePicker.SelectedIndex = -1;
    }
    private void meterVolumePicker_Delete()
    {
        meterVolume.Text = null;
        totalVolumeForTesting.Text = null;
        meterVolumePicker.SelectedIndex = -1;
    }


    public void roomVolume_TextChanged(object sender, EventArgs e)
    {
        if (!new[] { null, "", ".", "-", "+" }.Contains(roomVolume.Text) && !new[] { null, " ", ".", "-", "+" }.Contains(gaugeReadableMovement.Text) && totalVolumeForTesting.Text != null && testMediumFactor.Text != null)
        {
            AreaB_Value.Text = Math.Round(2.8 * double.Parse(AreaA_Value.Text) / double.Parse(roomVolume.Text), 3).ToString();

            letByDuration.Text = "2";
            stabilisationDuration.Text = "15";
            testDuration.Text = "2";

            double min = Math.Ceiling(double.Parse(AreaB_Value.Text));
            string min_string = min.ToString();

            if (min > 15)
            {
                letByDuration.Text = min_string;
                stabilisationDuration.Text = min_string;
                testDuration.Text = min_string;
            }
            else if (min > 2)
            {
                letByDuration.Text = min_string;
                testDuration.Text = min_string;
            }
        }
        else
        {
            AreaB_Value.Text = null;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }

    public void testMediumPicker_IndexChanged(object sender, EventArgs e)
    {
        if (testMediumPicker.SelectedIndex != -1)
        {
            testMediumPicker_x.IsVisible = true;
            testMediumPicker_delete.IsVisible = true;


            switch (testMediumPicker.SelectedItem.ToString())
            {
                case "Natural Gas": testMediumFactor.Text = "42"; break;
                case "Air / Nitrogen": testMediumFactor.Text = "67"; break;
                case "P - Fuel Gas": testMediumFactor.Text = "102"; break;
                case "P - Air": testMediumFactor.Text = "221"; break;
                case "B - Fuel Gas": testMediumFactor.Text = "128"; break;
                case "B - Air": testMediumFactor.Text = "305"; break;
            }
        }
        else
        {
            testMediumPicker_x.IsVisible = false;
            testMediumPicker_delete.IsVisible = false;
        }
    }
    public void testMediumPicker_Delete(object sender, EventArgs e)
    {
        testMediumFactor.Text = null;
        testMediumPicker.SelectedIndex = -1;
    }


    public void installationPicker_IndexChanged(object sender, EventArgs e)
    {
        if (installationPicker.SelectedIndex != -1)
        {
            installationPicker_x.IsVisible = true;
            installationPicker_delete.IsVisible = true;
        }
        else
        {
            installationPicker_x.IsVisible = false;
            installationPicker_delete.IsVisible = false;
        }
    }
    public void installationPicker_Delete(object sender, EventArgs e)
    {
        installationPicker.SelectedIndex = -1;
    }


    public void maximumPermittedLeakRate_IndexChanged(object sender, EventArgs e)
    {
        if (maximumPermittedLeakRate.SelectedIndex != -1)
        {
            maximumPermittedLeakRate_x.IsVisible = true;
            maximumPermittedLeakRate_delete.IsVisible = true;
        }
        else
        {
            maximumPermittedLeakRate_x.IsVisible = false;
            maximumPermittedLeakRate_delete.IsVisible = false;
        }
    }
    public void maximumPermittedLeakRate_Delete(object sender, EventArgs e)
    {
        maximumPermittedLeakRate.SelectedIndex = -1;
    }

    // steel ================================================================================================================================================================================================================

    public void steel1_IndexChanged(object sender, EventArgs e)
    {
        if (steel1.SelectedIndex != -1)
        {
            steel1_x.IsVisible = true;
            steel1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel1Total, steel1, 0.00024);
        }
        else
        {
            steel1_x.IsVisible = false;
            steel1_delete.IsVisible = false;
            steel1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel1.SelectedItem.ToString()) * 0.00024, 5);
        steel1.SelectedIndex = -1;
    }


    public void steel2_IndexChanged(object sender, EventArgs e)
    {
        if (steel2.SelectedIndex != -1)
        {
            steel2_x.IsVisible = true;
            steel2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel2Total, steel2, 0.00046);
        }
        else
        {
            steel2_x.IsVisible = false;
            steel2_delete.IsVisible = false;
            steel2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel2.SelectedItem.ToString()) * 0.00046, 5);
        steel2.SelectedIndex = -1;
    }


    public void steel3_IndexChanged(object sender, EventArgs e)
    {
        if (steel3.SelectedIndex != -1)
        {
            steel3_x.IsVisible = true;
            steel3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel3Total, steel3, 0.00064);
        }
        else
        {
            steel3_x.IsVisible = false;
            steel3_delete.IsVisible = false;
            steel3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel3.SelectedItem.ToString()) * 0.00064, 5);
        steel3.SelectedIndex = -1;
    }


    public void steel4_IndexChanged(object sender, EventArgs e)
    {
        if (steel4.SelectedIndex != -1)
        {
            steel4_x.IsVisible = true;
            steel4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel4Total, steel4, 0.0011);
        }
        else
        {
            steel4_x.IsVisible = false;
            steel4_delete.IsVisible = false;
            steel4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel4.SelectedItem.ToString()) * 0.0011, 5);
        steel4.SelectedIndex = -1;
    }


    public void steel5_IndexChanged(object sender, EventArgs e)
    {
        if (steel5.SelectedIndex != -1)
        {
            steel5_x.IsVisible = true;
            steel5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel5Total, steel5, 0.0015);
        }
        else
        {
            steel5_x.IsVisible = false;
            steel5_delete.IsVisible = false;
            steel5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel5.SelectedItem.ToString()) * 0.0015, 5);
        steel5.SelectedIndex = -1;
    }


    public void steel6_IndexChanged(object sender, EventArgs e)
    {
        if (steel6.SelectedIndex != -1)
        {
            steel6_x.IsVisible = true;
            steel6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel6Total, steel6, 0.0024);
        }
        else
        {
            steel6_x.IsVisible = false;
            steel6_delete.IsVisible = false;
            steel6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel6.SelectedItem.ToString()) * 0.0024, 5);
        steel6.SelectedIndex = -1;
    }


    public void steel7_IndexChanged(object sender, EventArgs e)
    {
        if (steel7.SelectedIndex != -1)
        {
            steel7_x.IsVisible = true;
            steel7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel7Total, steel7, 0.0038);
        }
        else
        {
            steel7_x.IsVisible = false;
            steel7_delete.IsVisible = false;
            steel7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel7.SelectedItem.ToString()) * 0.0038, 5);
        steel7.SelectedIndex = -1;
    }


    public void steel8_IndexChanged(object sender, EventArgs e)
    {
        if (steel8.SelectedIndex != -1)
        {
            steel8_x.IsVisible = true;
            steel8_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel8Total, steel8, 0.0054);
        }
        else
        {
            steel8_x.IsVisible = false;
            steel8_delete.IsVisible = false;
            steel8Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel8_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel8.SelectedItem.ToString()) * 0.0054, 5);
        steel8.SelectedIndex = -1;
    }


    public void steel9_IndexChanged(object sender, EventArgs e)
    {
        if (steel9.SelectedIndex != -1)
        {
            steel9_x.IsVisible = true;
            steel9_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel9Total, steel9, 0.009);
        }
        else
        {
            steel9_x.IsVisible = false;
            steel9_delete.IsVisible = false;
            steel9Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel9_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel9.SelectedItem.ToString()) * 0.009, 5);
        steel9.SelectedIndex = -1;
    }


    public void steel10_IndexChanged(object sender, EventArgs e)
    {
        if (steel10.SelectedIndex != -1)
        {
            steel10_x.IsVisible = true;
            steel10_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel10Total, steel10, 0.014);
        }
        else
        {
            steel10_x.IsVisible = false;
            steel10_delete.IsVisible = false;
            steel10Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel10_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel10.SelectedItem.ToString()) * 0.014, 5);
        steel10.SelectedIndex = -1;
    }


    public void steel11_IndexChanged(object sender, EventArgs e)
    {
        if (steel11.SelectedIndex != -1)
        {
            steel11_x.IsVisible = true;
            steel11_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel11Total, steel11, 0.02);
        }
        else
        {
            steel11_x.IsVisible = false;
            steel11_delete.IsVisible = false;
            steel11Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel11_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel11.SelectedItem.ToString()) * 0.02, 5);
        steel11.SelectedIndex = -1;
    }


    public void steel12_IndexChanged(object sender, EventArgs e)
    {
        if (steel12.SelectedIndex != -1)
        {
            steel12_x.IsVisible = true;
            steel12_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel12Total, steel12, 0.035);
        }
        else
        {
            steel12_x.IsVisible = false;
            steel12_delete.IsVisible = false;
            steel12Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel12_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel12.SelectedItem.ToString()) * 0.035, 5);
        steel12.SelectedIndex = -1;
    }


    public void steel13_IndexChanged(object sender, EventArgs e)
    {
        if (steel13.SelectedIndex != -1)
        {
            steel13_x.IsVisible = true;
            steel13_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel13Total, steel13, 0.053);
        }
        else
        {
            steel13_x.IsVisible = false;
            steel13_delete.IsVisible = false;
            steel13Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel13_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel13.SelectedItem.ToString()) * 0.053, 5);
        steel13.SelectedIndex = -1;
    }


    // copper ================================================================================================================================================================================================================

    public void copper1_IndexChanged(object sender, EventArgs e)
    {
        if (copper1.SelectedIndex != -1)
        {
            copper1_x.IsVisible = true;
            copper1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper1Total, copper1, 0.00014);
        }
        else
        {
            copper1_x.IsVisible = false;
            copper1_delete.IsVisible = false;
            copper1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper1.SelectedItem.ToString()) * 0.00014, 5);
        copper1.SelectedIndex = -1;
    }


    public void copper2_IndexChanged(object sender, EventArgs e)
    {
        if (copper2.SelectedIndex != -1)
        {
            copper2_x.IsVisible = true;
            copper2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper2Total, copper2, 0.00032);
        }
        else
        {
            copper2_x.IsVisible = false;
            copper2_delete.IsVisible = false;
            copper2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper2.SelectedItem.ToString()) * 0.00032, 5);
        copper2.SelectedIndex = -1;
    }


    public void copper3_IndexChanged(object sender, EventArgs e)
    {
        if (copper3.SelectedIndex != -1)
        {
            copper3_x.IsVisible = true;
            copper3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper3Total, copper3, 0.00054);
        }
        else
        {
            copper3_x.IsVisible = false;
            copper3_delete.IsVisible = false;
            copper3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper3.SelectedItem.ToString()) * 0.00054, 5);
        copper3.SelectedIndex = -1;
    }


    public void copper4_IndexChanged(object sender, EventArgs e)
    {
        if (copper4.SelectedIndex != -1)
        {
            copper4_x.IsVisible = true;
            copper4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper4Total, copper4, 0.00084);
        }
        else
        {
            copper4_x.IsVisible = false;
            copper4_delete.IsVisible = false;
            copper4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper4.SelectedItem.ToString()) * 0.00084, 5);
        copper4.SelectedIndex = -1;
    }


    public void copper5_IndexChanged(object sender, EventArgs e)
    {
        if (copper5.SelectedIndex != -1)
        {
            copper5_x.IsVisible = true;
            copper5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper5Total, copper5, 0.0012);
        }
        else
        {
            copper5_x.IsVisible = false;
            copper5_delete.IsVisible = false;
            copper5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper5.SelectedItem.ToString()) * 0.0012, 5);
        copper5.SelectedIndex = -1;
    }


    public void copper6_IndexChanged(object sender, EventArgs e)
    {
        if (copper6.SelectedIndex != -1)
        {
            copper6_x.IsVisible = true;
            copper6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper6Total, copper6, 0.0021);
        }
        else
        {
            copper6_x.IsVisible = false;
            copper6_delete.IsVisible = false;
            copper6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper6.SelectedItem.ToString()) * 0.0021, 5);
        copper6.SelectedIndex = -1;
    }


    public void copper7_IndexChanged(object sender, EventArgs e)
    {
        if (copper7.SelectedIndex != -1)
        {
            copper7_x.IsVisible = true;
            copper7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper7Total, copper7, 0.0033);
        }
        else
        {
            copper7_x.IsVisible = false;
            copper7_delete.IsVisible = false;
            copper7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper7.SelectedItem.ToString()) * 0.0033, 5);
        copper7.SelectedIndex = -1;
    }


    // PE SDR 11 ================================================================================================================================================================================================================

    public void pesdr1_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr1.SelectedIndex != -1)
        {
            pesdr1_x.IsVisible = true;
            pesdr1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr1Total, pesdr1, 0.00019);
        }
        else
        {
            pesdr1_x.IsVisible = false;
            pesdr1_delete.IsVisible = false;
            pesdr1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr1.SelectedItem.ToString()) * 0.00019, 5);
        pesdr1.SelectedIndex = -1;
    }


    public void pesdr2_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr2.SelectedIndex != -1)
        {
            pesdr2_x.IsVisible = true;
            pesdr2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr2Total, pesdr2, 0.00033);
        }
        else
        {
            pesdr2_x.IsVisible = false;
            pesdr2_delete.IsVisible = false;
            pesdr2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr2.SelectedItem.ToString()) * 0.00033, 5);
        pesdr2.SelectedIndex = -1;
    }


    public void pesdr3_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr3.SelectedIndex != -1)
        {
            pesdr3_x.IsVisible = true;
            pesdr3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr3Total, pesdr3, 0.00053);
        }
        else
        {
            pesdr3_x.IsVisible = false;
            pesdr3_delete.IsVisible = false;
            pesdr3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr3.SelectedItem.ToString()) * 0.00053, 5);
        pesdr3.SelectedIndex = -1;
    }


    public void pesdr4_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr4.SelectedIndex != -1)
        {
            pesdr4_x.IsVisible = true;
            pesdr4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr4Total, pesdr4, 0.0016);
        }
        else
        {
            pesdr4_x.IsVisible = false;
            pesdr4_delete.IsVisible = false;
            pesdr4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr4.SelectedItem.ToString()) * 0.0016, 5);
        pesdr4.SelectedIndex = -1;
    }


    public void pesdr5_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr5.SelectedIndex != -1)
        {
            pesdr5_x.IsVisible = true;
            pesdr5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr5Total, pesdr5, 0.0021);
        }
        else
        {
            pesdr5_x.IsVisible = false;
            pesdr5_delete.IsVisible = false;
            pesdr5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr5.SelectedItem.ToString()) * 0.0021, 5);
        pesdr5.SelectedIndex = -1;
    }


    public void pesdr6_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr6.SelectedIndex != -1)
        {
            pesdr6_x.IsVisible = true;
            pesdr6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr6Total, pesdr6, 0.0029);
        }
        else
        {
            pesdr6_x.IsVisible = false;
            pesdr6_delete.IsVisible = false;
            pesdr6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr6.SelectedItem.ToString()) * 0.0029, 5);
        pesdr6.SelectedIndex = -1;
    }


    public void pesdr7_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr7.SelectedIndex != -1)
        {
            pesdr7_x.IsVisible = true;
            pesdr7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr7Total, pesdr7, 0.004);
        }
        else
        {
            pesdr7_x.IsVisible = false;
            pesdr7_delete.IsVisible = false;
            pesdr7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr7.SelectedItem.ToString()) * 0.004, 5);
        pesdr7.SelectedIndex = -1;
    }


    public void pesdr8_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr8.SelectedIndex != -1)
        {
            pesdr8_x.IsVisible = true;
            pesdr8_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr8Total, pesdr8, 0.008);
        }
        else
        {
            pesdr8_x.IsVisible = false;
            pesdr8_delete.IsVisible = false;
            pesdr8Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr8_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr8.SelectedItem.ToString()) * 0.008, 5);
        pesdr8.SelectedIndex = -1;
    }

    // =======================================================================================================================================================================================

    private async Task stampAnimation(Image image)
    {
        var rotate = image.RotateTo(30, 350, Easing.Default);
        var scale = image.ScaleTo(0.85, 1000, Easing.BounceOut);
        var opacity = image.FadeTo(0.5, 1000, Easing.BounceOut);

        await Task.WhenAll(opacity, rotate, scale);

        await image.FadeTo(0.5, 2000);
        await image.FadeTo(0, 200);
    }
    private async Task stampAnimationEnd(Image image)
    {
        await image.FadeTo(0, 0);
        await image.RotateTo(0, 0);
        await image.ScaleTo(1, 0);
    }
    public async void testPassedOrFailed_IndexChanged(object sender, EventArgs e)
    {
        if (testPassedOrFailed.SelectedIndex != -1)
        {
            testPassedOrFailed_x.IsVisible = true;
            testPassedOrFailed_delete.IsVisible = true;

            if (testPassedOrFailed.SelectedItem.ToString() == "PASS")
                await stampAnimation(passStamp);
            else
                await stampAnimation(failStamp);
        }
        else
        {
            testPassedOrFailed_x.IsVisible = false;
            testPassedOrFailed_delete.IsVisible = false;

            await stampAnimationEnd(passStamp);
            await stampAnimationEnd(failStamp);
        }
    }
    public void testPassedOrFailed_Delete(object sender, EventArgs e)
    {
        passStamp.CancelAnimations();
        failStamp.CancelAnimations();

        testPassedOrFailed.SelectedIndex = -1;
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


    public void checkAreasWithInadequateVentilationYes_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationYes.IsChecked)
            DisjunctCheckboxes(checkAreasWithInadequateVentilationYes, checkAreasWithInadequateVentilationNo, checkAreasWithInadequateVentilationNA);
        else
        {
            checkAreasWithInadequateVentilationYes.Color = Colors.White;
            if (!checkAreasWithInadequateVentilationNo.IsChecked)
                DisjunctCheckboxes(checkAreasWithInadequateVentilationNA, checkAreasWithInadequateVentilationYes, checkAreasWithInadequateVentilationNo);
        }
    }
    public void checkAreasWithInadequateVentilationNo_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationNo.IsChecked)
            DisjunctCheckboxes(checkAreasWithInadequateVentilationNo, checkAreasWithInadequateVentilationYes, checkAreasWithInadequateVentilationNA);
        else
        {
            checkAreasWithInadequateVentilationNo.Color = Colors.White;
            if (!checkAreasWithInadequateVentilationYes.IsChecked)
                DisjunctCheckboxes(checkAreasWithInadequateVentilationNA, checkAreasWithInadequateVentilationYes, checkAreasWithInadequateVentilationNo);
        }
    }
    public void checkAreasWithInadequateVentilationNA_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationNA.IsChecked || !checkAreasWithInadequateVentilationYes.IsChecked && !checkAreasWithInadequateVentilationNo.IsChecked)
            DisjunctCheckboxes(checkAreasWithInadequateVentilationNA, checkAreasWithInadequateVentilationYes, checkAreasWithInadequateVentilationNo);
        else
            checkAreasWithInadequateVentilationNA.Color = Colors.White;
    }
}