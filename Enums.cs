using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public class Folder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Timestamp { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
    }
    public class Report
    {
        public string ReportId { get; set; }

        public Enums.ReportType ReportType { get; set; }

        public string ReportName { get; set; }

        public Dictionary<string, string> ReportData { get; set; }

        public string CreatedAt { get; set; }

        public string FolderId { get; set; }

        public Report()
        {
            CreatedAt = DateTime.UtcNow.ToString("o");
        }
    }

    public class Image
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
    }

}
