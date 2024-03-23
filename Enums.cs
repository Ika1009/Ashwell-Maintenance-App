using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ashwell_Maintenance
{
    public class Enums
    {
        public enum ReportType
        {
            ServiceRecord,
            BoilerHouseDataSheet,
            ConformityCheck,
            ConstructionDesignManagement,
            EngineersReport,
            GasRiskAssessment,
            OneA,
            OneB,
            One,
            PressurisationUnitReport
        }
    }
    //public class Folder
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string Timestamp { get; set; }    
    //    public string Signature1 { get; set; }
    //    public string Signature2 { get; set; }
    //}

    public class Folder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string id;
    public string Id
    {
        get { return id; }
        set { id = value; OnPropertyChanged(); }
    }

    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; OnPropertyChanged(); }
    }


    private string timestamp;
    public string Timestamp
    {
        get { return timestamp; }
        set { timestamp = value; OnPropertyChanged(); }
    }
    private bool timestampvisible;
    public bool TimestampVisible
    {
        get { return timestampvisible; }
        set { timestampvisible = value; OnPropertyChanged(); }
    }



        private string signature1;
    public string Signature1
    {
        get { return signature1; }
        set { signature1 = value; OnPropertyChanged(); }
    }

    private string signature2;
    public string Signature2
    {
        get { return signature2; }
        set { signature2 = value; OnPropertyChanged(); }
    }

    private bool isEditing;
    public bool IsEditing
    {
        get { return isEditing; }
        set { isEditing = value; OnPropertyChanged(); }
    }

    private string penImageSource;
    public string PenImageSource
    {
        get { return penImageSource; }
        set { penImageSource = value; OnPropertyChanged(); }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
}
