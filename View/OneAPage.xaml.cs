using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class OneAPage : ContentPage
{
    double pipeworkVolumeNumber;
    double totalPipeworkVolumeNumber = 0;

    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    public OneAPage()
    {
        InitializeComponent();

        List<Int64> numbers = new List<Int64>();
        for (Int64 i = 1; i <= 88; i++)
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
        OneABackBtt.IsEnabled = false;
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.OneA, reportName, folder.Id, report);

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

                byte[] pdfData = await PdfCreation.OneA(reportData, signature1, signature2);

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

    public async void OneABack(object sender, EventArgs e)
    {
        if (OASection1.IsVisible)
        {
            OneABackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
        else if (OASection2.IsVisible)
        {
            OASection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OASection1.ScrollToAsync(0, 0, false);
            OASection1.IsVisible = true;
        }
        else if (OASection3.IsVisible)
        {
            OASection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OASection2.ScrollToAsync(0, 0, false);
            OASection2.IsVisible = true;
        }
        else if (OASection4.IsVisible)
        {
            OASection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OASection3.ScrollToAsync(0, 0, false);
            OASection3.IsVisible = true;
        }
        else if(OASection5.IsVisible)
        {
            OASection5.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OASection4.ScrollToAsync(0, 0, false);
            OASection4.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OASection5.ScrollToAsync(0, 0, false);
            OASection5.IsVisible = true;
        }
    }

    
    public async void OneANext1(object sender, EventArgs e)
    {
        OASection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OASection2.ScrollToAsync(0, 0, false);
        OASection2.IsVisible = true;
    }
    
    public async void OneANext2(object sender, EventArgs e)
    {
        OASection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OASection3.ScrollToAsync(0, 0, false);
        OASection3.IsVisible = true;
    }
    
    public async void OneANext3(object sender, EventArgs e)
    {
        if (totalVolumeForTesting.Text != null && testMediumPicker.SelectedIndex != -1 && installationPicker.SelectedIndex != -1)
        {
            double totalVolumeForTestingValue = double.Parse(totalVolumeForTesting.Text);
            if (totalVolumeForTestingValue <= 0.5)
                letByDuration.Text = "2";
            else if (totalVolumeForTestingValue <= 0.8)
                letByDuration.Text = "3";
            else if (totalVolumeForTestingValue <= 1)
                letByDuration.Text = "4";
            else
                letByDuration.Text = null;

            if (installationPicker.SelectedItem.ToString() == "Existing")
            {
                if (testMediumPicker.SelectedItem.ToString() == "Natural Gas")
                {
                    if (totalVolumeForTestingValue <= 0.3)
                        testDuration.Text = "2";
                    else if (totalVolumeForTestingValue <= 0.45)
                        testDuration.Text = "3";
                    else if (totalVolumeForTestingValue <= 0.6)
                        testDuration.Text = "4";
                    else if (totalVolumeForTestingValue <= 0.75)
                        testDuration.Text = "5";
                    else if (totalVolumeForTestingValue <= 1)
                        testDuration.Text = "6";
                    else
                        testDuration.Text = null;
                }
                else
                {
                    if (totalVolumeForTestingValue <= 0.15)
                        testDuration.Text = "2";
                    else if (totalVolumeForTestingValue <= 0.3)
                        testDuration.Text = "3";
                    else if (totalVolumeForTestingValue <= 0.45)
                        testDuration.Text = "5";
                    else if (totalVolumeForTestingValue <= 0.6)
                        testDuration.Text = "6";
                    else if (totalVolumeForTestingValue <= 0.75)
                        testDuration.Text = "8";
                    else if (totalVolumeForTestingValue <= 0.9)
                        testDuration.Text = "9";
                    else if (totalVolumeForTestingValue <= 1)
                        testDuration.Text = "10";
                    else
                        testDuration.Text = null;
                }
            }
            else
            {
                if (totalVolumeForTestingValue <= 0.06)
                    testDuration.Text = "2";
                else if (totalVolumeForTestingValue <= 0.09)
                    testDuration.Text = "3";
                else if (totalVolumeForTestingValue <= 0.12)
                    testDuration.Text = "4";
                else if (totalVolumeForTestingValue <= 0.15)
                    testDuration.Text = "5";
                else if (totalVolumeForTestingValue <= 0.18)
                    testDuration.Text = "6";
                else if (totalVolumeForTestingValue <= 0.21)
                    testDuration.Text = "7";
                else if (totalVolumeForTestingValue <= 0.24)
                    testDuration.Text = "8";
                else if (totalVolumeForTestingValue <= 0.27)
                    testDuration.Text = "9";
                else if (totalVolumeForTestingValue <= 0.30)
                    testDuration.Text = "10";
                else if (totalVolumeForTestingValue <= 0.33)
                    testDuration.Text = "11";
                else if (totalVolumeForTestingValue <= 0.36)
                    testDuration.Text = "12";
                else if (totalVolumeForTestingValue <= 0.39)
                    testDuration.Text = "13";
                else if (totalVolumeForTestingValue <= 0.42)
                    testDuration.Text = "14";
                else if (totalVolumeForTestingValue <= 0.45)
                    testDuration.Text = "15";
                else if (totalVolumeForTestingValue <= 0.48)
                    testDuration.Text = "16";
                else if (totalVolumeForTestingValue <= 0.51)
                    testDuration.Text = "17";
                else if (totalVolumeForTestingValue <= 0.54)
                    testDuration.Text = "18";
                else if (totalVolumeForTestingValue <= 0.57)
                    testDuration.Text = "19";
                else if (totalVolumeForTestingValue <= 0.60)
                    testDuration.Text = "20";
                else if (totalVolumeForTestingValue <= 0.63)
                    testDuration.Text = "21";
                else if (totalVolumeForTestingValue <= 0.66)
                    testDuration.Text = "22";
                else if (totalVolumeForTestingValue <= 0.69)
                    testDuration.Text = "23";
                else if (totalVolumeForTestingValue <= 0.72)
                    testDuration.Text = "24";
                else if (totalVolumeForTestingValue <= 0.75)
                    testDuration.Text = "25";
                else if (totalVolumeForTestingValue <= 0.78)
                    testDuration.Text = "26";
                else if (totalVolumeForTestingValue <= 0.81)
                    testDuration.Text = "27";
                else if (totalVolumeForTestingValue <= 0.84)
                    testDuration.Text = "28";
                else if (totalVolumeForTestingValue <= 0.87)
                    testDuration.Text = "29";
                else if (totalVolumeForTestingValue <= 0.90)
                    testDuration.Text = "30";
                else
                    testDuration.Text = null;
            }
        }

        if (testDuration.Text != null)
        {
            if (double.Parse(testDuration.Text) < 6)
                stabilisationDuration.Text = "6";
            else
                stabilisationDuration.Text = testDuration.Text;
        }

        OASection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OASection4.ScrollToAsync(0, 0, false);
        OASection4.IsVisible = true;
    }
    
    public async void OneANext4(object sender, EventArgs e)
    {
        OASection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OASection5.ScrollToAsync(0, 0, false);
        OASection5.IsVisible = true;
        await LoadFolders();
    }

    public async void OneANextFinish(object sender, EventArgs e)
    {
        OASection5.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"1_A_Tightness_Testing_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //PdfCreation.OneA(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());


        reportData.Add("site", site.Text ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        if(steel1.SelectedIndex!=-1)
        reportData.Add("steel1",(steel1.SelectedItem).ToString());
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

        if (checkIsWeatherTemperatureStableYes.IsChecked)
            reportData.Add("checkIsWeatherTemperatureStableYes", "yes");
        else reportData.Add("checkIsWeatherTemperatureStableYes", "no");

        //checkMeterBypassYes,checkInadequateVentilationYes
        if (checkMeterBypassYes.IsChecked)
            reportData.Add("checkMeterBypassYes", "yes");
        else reportData.Add("checkMeterBypassYes", "no");

        if (testGaugeUsed.SelectedIndex != -1)
            reportData.Add("testGaugeUsed", (testGaugeUsed.SelectedItem).ToString());
        else
            reportData.Add("testGaugeUsed", string.Empty);

        reportData.Add("tightnessTestPressure", tightnessTestPressure.Text ?? string.Empty);
        reportData.Add("roomVolumeOfSmallestOccupiedSpace", roomVolumeOfSmallestOccupiedSpace.Text ?? string.Empty);
        reportData.Add("maximumAllowablePressureDrop", maximumAllowablePressureDrop.Text ?? string.Empty);

        if (checkInadequateVentilationYes.IsChecked)
            reportData.Add("checkInadequateVentilationYes", "yes");
        else reportData.Add("checkInadequateVentilationYes", "no");

        reportData.Add("strengthTestPressure", strengthTestPressure.Text ?? string.Empty);
        //checkComponentsRemovedBypassedYes
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
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
   

        return reportData;
    }

    private void UpdateTotalPipeworkVolume(Label total, Picker quantificator, double k)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(quantificator.SelectedItem.ToString()) * k, 5);
        total.Text = pipeworkVolumeNumber.ToString();

        totalPipeworkVolumeNumber += pipeworkVolumeNumber;
        totalPipeworkVolume.Text = Math.Round(totalPipeworkVolumeNumber, 5).ToString();
        pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

        if (meterVolume.Text != null && meterVolume.Text != "0.079d2L*")
            totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
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

            if (meterVolume.Text != null)
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
        }
    }
    public void meterVolumePicker_IndexChanged(object sender, EventArgs e)
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
                case "RD or Turbine": meterVolume.Text = "0.079d2L*"; break;
                case "Ultrasonic": meterVolume.Text = "0.0024"; break;
            }

            if (meterVolume.Text != "0.079d2L*" && pipeworkFittingsIV.Text != null)
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
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

    public void testMediumPicker_IndexChanged(object sender, EventArgs e)
    {
        if (testMediumPicker.SelectedIndex != -1)
        {
            testMediumPicker_x.IsVisible = true;
            testMediumPicker_delete.IsVisible = true;

        }
        else
        {
            testMediumPicker_x.IsVisible = false;
            testMediumPicker_delete.IsVisible = false;
        }
    }
    public void testMediumPicker_Delete(object sender, EventArgs e)
    {
        testMediumPicker.SelectedIndex = -1;
    }
    public void testGaugeUsed_IndexChanged(object sender, EventArgs e)
    {
        if (testGaugeUsed.SelectedIndex != -1)
        {
            testGaugeUsed_x.IsVisible = true;
            testGaugeUsed_delete.IsVisible = true;

        }
        else
        {
            testGaugeUsed_x.IsVisible = false;
            testGaugeUsed_delete.IsVisible = false;
        }
    }
    public void testGaugeUsed_Delete(object sender, EventArgs e)
    {
        testGaugeUsed.SelectedIndex = -1;
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

    private async static Task stampAnimation(Image image)
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
}