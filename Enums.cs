using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Folder
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Timestamp { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
    }
}
