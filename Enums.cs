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
        [JsonPropertyName("report_id")]
        public int ReportId { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; }

        [JsonPropertyName("report_name")]
        public string ReportName { get; set; }

        [JsonPropertyName("report_data")]
        public Dictionary<string, string> ReportData { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("folder_id")]
        public int FolderId { get; set; }
    }
}
