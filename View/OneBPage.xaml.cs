using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace Ashwell_Maintenance.View;

public partial class OneBPage : ContentPage
{
    double pipeworkVolumeNumber;
    double totalPipeworkVolumeNumber = 0;

    string reportName = "noname";
    public ObservableCollection<Folder> Folders = new();
    private Dictionary<string, string> reportData;
    bool previewOnly = false;
    private readonly Enums.ReportType reportType = Enums.ReportType.OneB;

    public OneBPage()
	{
		InitializeComponent();

        List<Int64> numbers = new List<Int64>();
        for (Int64 i = 1; i <= 88; i++)
            numbers.Add(i);

        steel1.ItemsSource = numbers;
        steel2.ItemsSource = numbers;
        steel3.ItemsSource = numbers;
        steel4.ItemsSource = numbers;

        copper1.ItemsSource = numbers;
        copper2.ItemsSource = numbers;
        copper3.ItemsSource = numbers;
        copper4.ItemsSource = numbers;

        pesdr1.ItemsSource = numbers;
        pesdr2.ItemsSource = numbers;
        pesdr3.ItemsSource = numbers;
    }
    public OneBPage(Report report)
    {
        InitializeComponent();
        previewOnly = true;
        PreviewOneBPage(report.ReportData);
    }
    public void FolderChosen(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        OneBBackBtt.IsEnabled = false;

        string folderId = button?.CommandParameter as string;
        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }
    private async Task UploadReport(Folder folder, Dictionary<string, string> reportData)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;
        OneBBackBtt.IsEnabled = false;

        try
        {
            await ReportManager.UploadReportAsync(reportType, reportName, folder, reportData);

            // If we reach here, both data and PDF (if any) uploaded successfully
            await DisplayAlert("Success", "Report successfully uploaded.", "OK");
        }
        catch (HttpRequestException)
        {
            // Network/server issue: saved locally by retry logic
            await DisplayAlert("Offline", "No internet or server error. Report saved locally.", "OK");

            await ReportManager.SaveReportLocallyAsync(reportType, reportName, folder, reportData);
        }
        catch (Exception ex)
        {
            // Any other error (PDF generation/upload, etc.)
            await DisplayAlert("Error", $"Upload failed: {ex.Message}. Report saved locally.", "OK");

            await ReportManager.SaveReportLocallyAsync(reportType, reportName, folder, reportData);
        }
        finally
        {
            loadingBG.IsRunning = false;
            loading.IsRunning = false;
            OneBBackBtt.IsEnabled = true;
            await Navigation.PopModalAsync();
        }
    }

    public async void NewFolder(object sender, EventArgs e)
    {
        string folderName = await Shell.Current.DisplayPromptAsync("New Folder", "Enter folder name");
        if (string.IsNullOrWhiteSpace(folderName)) return;

        loadingBG.IsRunning = true;
        loading.IsRunning = true;

        var newFolder = await FolderManager.CreateFolderAsync(folderName);
        Folders.Add(newFolder);

        // Always refresh displayed list
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            await FolderManager.LoadFoldersAsync(Folders, FoldersListView);

        loadingBG.IsRunning = false;
        loading.IsRunning = false;
    }
    private async Task LoadFolders()
    {
        await FolderManager.LoadFoldersAsync(Folders, FoldersListView);
    }

    public async void FolderEdit(object sender, EventArgs e)
    {
        loadingBG.IsRunning = true;
        loading.IsRunning = true;

        var folderId = (sender as ImageButton)?.CommandParameter as string;
        var folder = Folders.FirstOrDefault(f => f.Id == folderId);

        if (folder != null)
            await FolderManager.EditFolderAsync(folder, Folders);

        loadingBG.IsRunning = false;
        loading.IsRunning = false;
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

    public async void OneBBack(object sender, EventArgs e)
	{
        if (OBSection1.IsVisible)
        {
            OneBBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
        else if (OBSection2.IsVisible)
        {
            OBSection2.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OBSection1.ScrollToAsync(0, 0, false);
            OBSection1.IsVisible = true;
        }
        else if (OBSection3.IsVisible)
        {
            OBSection3.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OBSection2.ScrollToAsync(0, 0, false);
            OBSection2.IsVisible = true;
        }
        else if(OBSection4.IsVisible)
        {
            OBSection4.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OBSection3.ScrollToAsync(0, 0, false);
            OBSection3.IsVisible = true;
        }
        else
        {
            FolderSection.IsVisible = false;
            folderSearch.IsVisible = false;
            folderAdd.IsVisible = false;

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                await OBSection4.ScrollToAsync(0, 0, false);
            OBSection4.IsVisible = true;
        }
    }

    
    public async void OneBNext1(object sender, EventArgs e)
    {
        OBSection1.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OBSection2.ScrollToAsync(0, 0, false);
        OBSection2.IsVisible = true;
    }

    
    public async void OneBNext2(object sender, EventArgs e)
    {
        OBSection2.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OBSection3.ScrollToAsync(0, 0, false);
        OBSection3.IsVisible = true;
    }
    
    public async void OneBNext3(object sender, EventArgs e)
    {
        OBSection3.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await OBSection4.ScrollToAsync(0, 0, false);
        OBSection4.IsVisible = true;

        // Do not Show Folders if in preview of PDF page
        if (!previewOnly)
            await LoadFolders();
    }

    public async void OneBNextFinish(object sender, EventArgs e)
    {
        // Do not Show Folders if in preview of PDF page
        if (previewOnly)
            await Navigation.PopModalAsync();

        OBSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;
        folderSearch.IsVisible = true;
        folderAdd.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"1_B_Tightness_Testing_{dateTimeString}.pdf";
        reportData = GatherReportData();

        //PdfCreation.IgeUpB(GatherReportData());
    }
    public void PreviewOneBPage(Dictionary<string, string> reportData)
    {

        List<Int64> numbers = new List<Int64>();
        for (Int64 i = 1; i <= 88; i++)
            numbers.Add(i);

        steel1.ItemsSource = numbers;
        steel2.ItemsSource = numbers;
        steel3.ItemsSource = numbers;
        steel4.ItemsSource = numbers;

        copper1.ItemsSource = numbers;
        copper2.ItemsSource = numbers;
        copper3.ItemsSource = numbers;
        copper4.ItemsSource = numbers;

        pesdr1.ItemsSource = numbers;
        pesdr2.ItemsSource = numbers;
        pesdr3.ItemsSource = numbers;

        if (reportData.ContainsKey("steel1") && reportData["steel1"] != string.Empty)
        {
            steel1.SelectedItem = Int64.Parse(reportData["steel1"]);
        }

        if (reportData.ContainsKey("steel2") && reportData["steel2"] != string.Empty)
        {
            steel2.SelectedItem = Int64.Parse(reportData["steel2"]);
        }
        if (reportData.ContainsKey("steel3") && reportData["steel3"] != string.Empty)
        {
            steel3.SelectedItem = Int64.Parse(reportData["steel3"]);
        }
        if (reportData.ContainsKey("steel4") && reportData["steel4"] != string.Empty)
        {
            steel4.SelectedItem = Int64.Parse(reportData["steel4"]);
        }

        if (reportData.ContainsKey("copper1") && reportData["copper1"] != string.Empty)
        {
            copper1.SelectedItem = Int64.Parse(reportData["copper1"]);
        }
        if (reportData.ContainsKey("copper2") && reportData["copper2"] != string.Empty)
        {
            copper2.SelectedItem = Int64.Parse(reportData["copper2"]);
        }
        if (reportData.ContainsKey("copper3") && reportData["copper3"] != string.Empty)
        {
            copper3.SelectedItem = Int64.Parse(reportData["copper3"]);
        }
        if (reportData.ContainsKey("copper4") && reportData["copper4"] != string.Empty)
        {
            copper4.SelectedItem = Int64.Parse(reportData["copper4"]);
        }

        if (reportData.ContainsKey("pesdr1") && reportData["pesdr1"] != string.Empty)
        {
            pesdr1.SelectedItem = Int64.Parse(reportData["pesdr1"]);
        }
        if (reportData.ContainsKey("pesdr2") && reportData["pesdr2"] != string.Empty)
        {
            pesdr2.SelectedItem = Int64.Parse(reportData["pesdr2"]);
        }
        if (reportData.ContainsKey("pesdr3") && reportData["pesdr3"] != string.Empty)
        {
            pesdr3.SelectedItem = Int64.Parse(reportData["pesdr3"]);
        }


        // Set text fields
        site.Text = reportData.ContainsKey("site") ? reportData["site"] : string.Empty;
        date.Date = reportData.ContainsKey("date") ? DateTime.ParseExact(reportData["date"], "d/M/yyyy", null) : DateTime.Now;
        location.Text = reportData.ContainsKey("location") ? reportData["location"] : string.Empty;
        //steel1Total.Text = reportData.ContainsKey("steel1Total") ? reportData["steel1Total"] : string.Empty;
        //steel2Total.Text = reportData.ContainsKey("steel2Total") ? reportData["steel2Total"] : string.Empty;
        //steel3Total.Text = reportData.ContainsKey("steel3Total") ? reportData["steel3Total"] : string.Empty;
        //steel4Total.Text = reportData.ContainsKey("steel4Total") ? reportData["steel4Total"] : string.Empty;
        //copper1Total.Text = reportData.ContainsKey("copper1Total") ? reportData["copper1Total"] : string.Empty;
        //copper2Total.Text = reportData.ContainsKey("copper2Total") ? reportData["copper2Total"] : string.Empty;
        //copper3Total.Text = reportData.ContainsKey("copper3Total") ? reportData["copper3Total"] : string.Empty;
        //copper4Total.Text = reportData.ContainsKey("copper4Total") ? reportData["copper4Total"] : string.Empty;
        //pesdr1Total.Text = reportData.ContainsKey("pesdr1Total") ? reportData["pesdr1Total"] : string.Empty;
        //pesdr2Total.Text = reportData.ContainsKey("pesdr2Total") ? reportData["pesdr2Total"] : string.Empty;
        //pesdr3Total.Text = reportData.ContainsKey("pesdr3Total") ? reportData["pesdr3Total"] : string.Empty;
        totalPipeworkVolume.Text = reportData.ContainsKey("totalPipeworkVolume") ? reportData["totalPipeworkVolume"] : string.Empty;
        pipeworkFittingsIV.Text = reportData.ContainsKey("pipeworkFittingsIV") ? reportData["pipeworkFittingsIV"] : string.Empty;
        meterVolume.Text = reportData.ContainsKey("meterVolume") ? reportData["meterVolume"] : string.Empty;
        totalVolumeForTesting.Text = reportData.ContainsKey("totalVolumeForTesting") ? reportData["totalVolumeForTesting"] : string.Empty;
        tightnessTestPressure.Text = reportData.ContainsKey("tightnessTestPressure") ? reportData["tightnessTestPressure"] : string.Empty;
        letByDuration.Text = reportData.ContainsKey("letByDuration") ? reportData["letByDuration"] : string.Empty;
        stabilisationDuration.Text = reportData.ContainsKey("stabilisationDuration") ? reportData["stabilisationDuration"] : string.Empty;
        testDuration.Text = reportData.ContainsKey("testDuration") ? reportData["testDuration"] : string.Empty;
        actualPressureDropResult.Text = reportData.ContainsKey("actualPressureDropResult") ? reportData["actualPressureDropResult"] : string.Empty;
        engineersComments.Text = reportData.ContainsKey("engineersComments") ? reportData["engineersComments"] : string.Empty;
        engineer.Text = reportData.ContainsKey("engineer") ? reportData["engineer"] : string.Empty;
        clientsName.Text = reportData.ContainsKey("clientsName") ? reportData["clientsName"] : string.Empty;
        WarningNoticeRefNo.Text = reportData.ContainsKey("WarningNoticeRefNo") ? reportData["WarningNoticeRefNo"] : string.Empty;
        cardNumber.Text = reportData.ContainsKey("cardNumber") ? reportData["cardNumber"] : string.Empty;

        // Set checkboxes based on dictionary values
        if (reportData.ContainsKey("checkIsWeatherTemperatureStableYes"))
        {
            bool isWeatherTemperatureStableYes = reportData["checkIsWeatherTemperatureStableYes"] == "True";
            checkIsWeatherTemperatureStableYes.IsChecked = isWeatherTemperatureStableYes;
            checkIsWeatherTemperatureStableNo.IsChecked = !isWeatherTemperatureStableYes;
        }

        // Set dropdowns based on dictionary values
       
        if (reportData.ContainsKey("testMediumPicker"))
        {
            testMediumPicker.SelectedItem = reportData["testMediumPicker"];
        }

        if (reportData.ContainsKey("installationPicker"))
        {
            installationPicker.SelectedItem = reportData["installationPicker"];
        }

        if (reportData.ContainsKey("testGaugeUsed"))
        {
            testGaugeUsed.SelectedItem = reportData["testGaugeUsed"];
        }

        if (reportData.ContainsKey("maximumPermissiblePressureDrop"))
        {
            maximumPermissiblePressureDrop.SelectedItem = reportData["maximumPermissiblePressureDrop"];
        }

        if (reportData.ContainsKey("testPassedOrFailed"))
        {
            testPassedOrFailed.SelectedItem = reportData["testPassedOrFailed"];
        }

    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("site", site.Text ?? string.Empty);
        reportData.Add("date", date.Date.ToString("d/M/yyyy") ?? string.Empty);
        reportData.Add("location", location.Text ?? string.Empty);
        reportData.Add("steel1Total", steel1Total.Text ?? string.Empty);
        reportData.Add("steel2Total", steel2Total.Text ?? string.Empty);
        reportData.Add("steel3Total", steel3Total.Text ?? string.Empty);
        reportData.Add("steel4Total", steel4Total.Text ?? string.Empty);
        reportData.Add("copper1Total", copper1Total.Text ?? string.Empty);
        reportData.Add("copper2Total", copper2Total.Text ?? string.Empty);
        reportData.Add("copper3Total", copper3Total.Text ?? string.Empty);
        reportData.Add("copper4Total", copper4Total.Text ?? string.Empty);
        reportData.Add("pesdr1Total", pesdr1Total.Text ?? string.Empty);
        reportData.Add("pesdr2Total", pesdr2Total.Text ?? string.Empty);
        reportData.Add("pesdr3Total", pesdr3Total.Text ?? string.Empty);
        reportData.Add("totalPipeworkVolume", totalPipeworkVolume.Text ?? string.Empty);
        reportData.Add("pipeworkFittingsIV", pipeworkFittingsIV.Text ?? string.Empty);
        reportData.Add("meterVolume", meterVolume.Text ?? string.Empty);
        reportData.Add("totalVolumeForTesting", totalVolumeForTesting.Text ?? string.Empty);
        reportData.Add("tightnessTestPressure", tightnessTestPressure.Text ?? string.Empty);
        reportData.Add("letByDuration", letByDuration.Text ?? string.Empty);
        reportData.Add("stabilisationDuration", stabilisationDuration.Text ?? string.Empty);
        reportData.Add("testDuration", testDuration.Text ?? string.Empty);
        reportData.Add("actualPressureDropResult", actualPressureDropResult.Text ?? string.Empty);
        reportData.Add("engineersComments", engineersComments.Text ?? string.Empty);
        reportData.Add("engineer", engineer.Text ?? string.Empty);
        reportData.Add("clientsName", clientsName.Text ?? string.Empty);
        reportData.Add("WarningNoticeRefNo", WarningNoticeRefNo.Text ?? string.Empty);
        reportData.Add("cardNumber", cardNumber.Text ?? string.Empty);


        reportData.Add("checkIsWeatherTemperatureStableYes", checkIsWeatherTemperatureStableYes.IsChecked.ToString());
        reportData.Add("checkIsWeatherTemperatureStableNo", checkIsWeatherTemperatureStableNo.IsChecked.ToString());


        if (steel1.SelectedIndex != -1)
            reportData.Add("steel1", (steel1.SelectedItem).ToString());
        else
            reportData.Add("steel1", string.Empty);

        if (steel2.SelectedIndex != -1)
            reportData.Add("steel2", (steel2.SelectedItem).ToString());
        else
            reportData.Add("steel2", string.Empty);

        if (steel3.SelectedIndex != -1)
            reportData.Add("steel3", (steel3.SelectedItem).ToString());
        else
            reportData.Add("steel3", string.Empty);
        if (steel4.SelectedIndex != -1)
            reportData.Add("steel4", (steel4.SelectedItem).ToString());
        else
            reportData.Add("steel4", string.Empty);


        if (copper1.SelectedIndex != -1)
            reportData.Add("copper1", (copper1.SelectedItem).ToString());
        else
            reportData.Add("copper1", string.Empty);

        if (copper2.SelectedIndex != -1)
            reportData.Add("copper2", (copper2.SelectedItem).ToString());
        else
            reportData.Add("copper2", string.Empty);
        if (copper3.SelectedIndex != -1)
            reportData.Add("copper3", (copper3.SelectedItem).ToString());
        else
            reportData.Add("copper3", string.Empty);
        if (copper4.SelectedIndex != -1)
            reportData.Add("copper4", (copper4.SelectedItem).ToString());
        else
            reportData.Add("copper4", string.Empty);


        if (pesdr1.SelectedIndex != -1)
            reportData.Add("pesdr1", (pesdr1.SelectedItem).ToString());
        else
            reportData.Add("pesdr1", string.Empty);

        if (pesdr2.SelectedIndex != -1)
            reportData.Add("pesdr2", (pesdr2.SelectedItem).ToString());
        else
            reportData.Add("pesdr2", string.Empty);

        if (pesdr3.SelectedIndex != -1)
            reportData.Add("pesdr3", (pesdr3.SelectedItem).ToString());
        else
            reportData.Add("pesdr3", string.Empty);





        if (testMediumPicker.SelectedIndex != -1)
            reportData.Add("testMediumPicker", (testMediumPicker.SelectedItem).ToString());
        else
            reportData.Add("testMediumPicker", string.Empty);

        if (installationPicker.SelectedIndex != -1)
            reportData.Add("installationPicker", (installationPicker.SelectedItem).ToString());
        else
            reportData.Add("installationPicker", string.Empty);

        if (testGaugeUsed.SelectedIndex != -1)
            reportData.Add("testGaugeUsed", (testGaugeUsed.SelectedItem).ToString());
        else
            reportData.Add("testGaugeUsed", string.Empty);

        if (maximumPermissiblePressureDrop.SelectedIndex != -1)
            reportData.Add("maximumPermissiblePressureDrop", (maximumPermissiblePressureDrop.SelectedItem).ToString());
        else
            reportData.Add("maximumPermissiblePressureDrop", string.Empty);

        if (testPassedOrFailed.SelectedIndex != -1)
            reportData.Add("testPassedOrFailed", (testPassedOrFailed.SelectedItem).ToString());
        else
            reportData.Add("testPassedOrFailed", string.Empty);







        return reportData;
    }


    private void TotalVolumeLimit(bool full)
    {
        if (full && double.Parse(totalVolumeForTesting.Text) > 0.035)
        {
            totalVolumeExceeded.IsVisible = true;
            OneBNext_First.IsEnabled = false;
            OneBNext_First.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#707070");
            OneBNext_First.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#222225");
        }
        else
        {
            totalVolumeExceeded.IsVisible = false;
            OneBNext_First.IsEnabled = true;
            OneBNext_First.TextColor = Colors.White;
            OneBNext_First.BackgroundColor = Colors.Red;
        }
    }


    private void UpdateTotalPipeworkVolume(Label total, Picker quantificator, double k)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(quantificator.SelectedItem.ToString()) * k, 5);
        total.Text = pipeworkVolumeNumber.ToString();

        totalPipeworkVolumeNumber += pipeworkVolumeNumber;
        totalPipeworkVolume.Text = Math.Round(totalPipeworkVolumeNumber, 5).ToString();
        pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

        if (meterVolume.Text != null)
        {
            totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
            TotalVolumeLimit(true);
        }
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

            TotalVolumeLimit(false);
        }
        else
        {
            totalPipeworkVolumeNumber = subtract;
            totalPipeworkVolume.Text = subtract.ToString();
            pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

            if (meterVolume.Text != null)
            {
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
                TotalVolumeLimit(true);
            }
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
            }

            if (pipeworkFittingsIV.Text != null)
            {
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 3).ToString();
                TotalVolumeLimit(true);
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

        TotalVolumeLimit(false);
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
    public void maximumPermissiblePressureDrop_IndexChanged(object sender, EventArgs e)
    {
        if (maximumPermissiblePressureDrop.SelectedIndex != -1)
        {
            maximumPermissiblePressureDrop_x.IsVisible = true;
            maximumPermissiblePressureDrop_delete.IsVisible = true;

        }
        else
        {
            maximumPermissiblePressureDrop_x.IsVisible = false;
            maximumPermissiblePressureDrop_delete.IsVisible = false;
        }
    }
    public void maximumPermissiblePressureDrop_Delete(object sender, EventArgs e)
    {
        maximumPermissiblePressureDrop.SelectedIndex = -1;
    }

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
}