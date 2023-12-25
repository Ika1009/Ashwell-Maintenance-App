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

        _ = UploadReport(folderId, reportData);
    }

    private async Task UploadReport(string folderId, Dictionary<string, string> report)
    {
        try
        {
            HttpResponseMessage response = await ApiService.UploadReportAsync(Enums.ReportType.ConstructionDesignManagement, reportName, folderId, report);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Successfully uploaded new sheet.", "OK");
                await Navigation.PopModalAsync();
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
                        Timestamp = element.GetProperty("created_at").GetString()
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

    public class Folder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Timestamp { get; set; }
    }
    public void CDMBack(object sender, EventArgs e)
    {
        if (CDMSection1.IsVisible)
        {
            CDMBackBtt.IsEnabled = false;
            Navigation.PopModalAsync();
        }
        else if (CDMSection2.IsVisible)
        {
            CDMSection2.IsVisible = false;

            if (Device.RuntimePlatform is Device.Android or Device.iOS)
                CDMSection1.ScrollToAsync(0, 0, false);
            CDMSection1.IsVisible = true;
        }
        else
        {
            CDMSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                CDMSection2.ScrollToAsync(0, 0, false);
            CDMSection2.IsVisible = true;
        }
    }

    [Obsolete]
    public async void CDMNext1(object sender, EventArgs e)
    {
        CDMSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await CDMSection2.ScrollToAsync(0, 0, false);
        CDMSection2.IsVisible = true;
    }
    [Obsolete]
    public async void CDMNext2(object sender, EventArgs e)
    {
        CDMSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            CDMSection3.ScrollToAsync(0, 0, false);
        CDMSection3.IsVisible = true;
        await LoadFolders();
    }
    [Obsolete]
    public async void CDMNext3(object sender, EventArgs e)
    {
        FolderSection.IsVisible = true;

        string dateTimeString = DateTime.Now.ToString("M-d-yyyy-HH-mm");
        reportName = $"Construction_Design_Management_{dateTimeString}.pdf";
        reportData = GatherReportData();
        //await DisplayAlert("MARICU", "fajl sacuvan", "cancelanko");
        //await PdfCreation.CDM(reportData);
    }
    private Dictionary<string, string> GatherReportData()
    {

        Dictionary<string, string> reportData = new Dictionary<string, string>();

        //  reportData.Add("", .Text ?? string.Empty);
        //  reportData.Add("", .IsChecked.ToString());

        reportData.Add("siteAdress", siteAdress.Text ?? string.Empty);
        reportData.Add("clinet", clinet.Text ?? string.Empty);
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