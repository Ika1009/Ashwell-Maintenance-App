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
    public void FolderChosen(object sender, EventArgs e)
    {
        string folderId = (sender as Button).CommandParameter as string;

        _ = UploadReport(Folders.First(folder => folder.Id == folderId), reportData);
    }

    private async Task UploadReport(Folder folder, Dictionary<string, string> report)
    {
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.OneB, reportName, folder.Id, report);

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
                byte[] signature2 = await ApiService.GetImageAsByteArrayAsync($"https://ashwellmaintenance.host/{folder.Signature1}");
                if (signature1 == null || signature2 == null)
                    throw new Exception("Couldn't retrieve signatures");

                byte[] pdfData = await PdfCreation.OneB(reportData, signature1, signature2);

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
        await Navigation.PopModalAsync();
    }

    public void NewFolder(object sender, EventArgs e)
    {
        this.ShowPopup(new NewFolderPopup(LoadFolders));
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
        await LoadFolders();
    }

    public async void OneBNextFinish(object sender, EventArgs e)
    {
        OBSection4.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            await FolderSection.ScrollToAsync(0, 0, false);
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"1_B_Tightness_Testing_{dateTimeString}.pdf";
        reportData = GatherReportData();

        //PdfCreation.IgeUpB(GatherReportData());
    }
    private Dictionary<string, string> GatherReportData()
    {
        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("site", site.Text ?? string.Empty);
        reportData.Add("date", date.Text ?? string.Empty);
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

    private void UpdateTotalPipeworkVolume(Label total, Picker quantificator, double k)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(quantificator.SelectedItem.ToString()) * k, 5);
        total.Text = pipeworkVolumeNumber.ToString();

        totalPipeworkVolumeNumber += pipeworkVolumeNumber;
        totalPipeworkVolume.Text = Math.Round(totalPipeworkVolumeNumber, 5).ToString();
        pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

        if (meterVolume.Text != null)
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
            }

            if (pipeworkFittingsIV.Text != null)
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