using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Fonts;
using PdfSharp.Drawing.Layout;
using System.IO;
using Microsoft.Maui.Storage;


namespace Ashwell_Maintenance
{
    public static class PdfCreation
    {

        private static async Task<XImage> ConvertToXImage(string filename)
        {
            try
            {
                string fullResourcePath = $"Ashwell_Maintenance.Resources.Images.{filename}";

                Image image = new Image { Source = ImageSource.FromResource(fullResourcePath) };
                if (image?.Source is not StreamImageSource streamImageSource)
                {
                    Console.WriteLine("The image's source is not a StreamImageSource.");
                    return null;
                }

                using (var originalStream = await streamImageSource.Stream(CancellationToken.None))
                using (var memoryStream = new MemoryStream())
                {
                    await originalStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    XImage xImage = XImage.FromStream(memoryStream);
                    return xImage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }


        public static async Task CreateServiceRecordPDF(string workingInletPressure, string site, string location, string applianceNumber,
string recordedBurnerPressure,
string assetNumber,
string measuredGasRate,
bool heatExhanger,
bool heatExhangerNA,
string heatExhangerComments,
bool flueFlowTest,
bool flueFlowTestNA,
string flueFlowTestComments,
bool spillageTest,
bool spillageTestNA,
string spillageTestComments,
bool safetyShutOffValve,
bool safetyShutOffValveNA,
string safetyShutOffValveComments,
bool plantroomGasTightnessTest,
bool plantroomGasTightnessTestNA,
string plantroomGasTightnessTestComments,
bool AECVPlantIsolationCorrect,
bool AECVPlantIsolationCorrectNA,
string AECVPlantIsolationCorrectComments,
string stateApplianceConditionComments,
string workingInletPressureComments,
string recordedBurnerPressureComments,
string measuredGasRateComments,
bool testsCompleted,
bool remedialWorkRequired,
string applianceMake,
string applianceModel,
string applianceSerialNumber,
string gcNumber,
string stateApplianceCondition,
string burnerMake,
string burnerModel,
string burnerSerialNumber,
string Type,
string Spec,
bool OpenFlue,
bool Roomsealed,
bool ForcedDraft,
bool Flueless,
bool Heating,
bool HotWater,
bool Both,
string badgedBurnerPressure,
bool ventilationSatisfactory,
string gasType,
bool flueConditionSatisfactory,
string approxAgeOfAppliance,
string badgedInput,
string badgedOutput,
bool applianceServiceValveSatisfactory,
bool governorsSatisfactory,
bool gasSolenoidValvesSatisfactory,
bool controlBoxPcbSatisfactory,
bool gasketSealsSatisfactory,
bool burnerSatisfactory,
bool burnerJetsSatisfactory,
bool electrodesTransformerSatisfactory,
bool flameFailureDeviceSatisfactory,
bool systemBoilerControlsSatisfactory,
bool boilerCasingSatisfactory,
bool thermalInsulationSatisfactory,
bool combustionFanIdFanSatisfactory,
bool airFluePressureSwitchSatisfactory,
bool controlLimitStatsSatisfactory,
bool pressureTempGaugesSatisfactory,
bool circulationPumpsSatisfactory,
bool condenseTrapSatisfactory,
bool applianceServiceValveSatisfactoryNA,
bool governorsSatisfactoryNA,
bool gasSolenoidValvesSatisfactoryNA,
bool controlBoxPcbSatisfactoryNA,
bool gasketSealsSatisfactoryNA,
bool burnerSatisfactoryNA,
bool burnerJetsSatisfactoryNA,
bool electrodesTransformerSatisfactoryNA,
bool flameFailureDeviceSatisfactoryNA,
bool systemBoilerControlsSatisfactoryNA,
bool boilerCasingSatisfactoryNA,
bool thermalInsulationSatisfactoryNA,
bool combustionFanIdFanSatisfactoryNA,
bool airFluePressureSwitchSatisfactoryNA,
bool controlLimitStatsSatisfactoryNA,
bool pressureTempGaugesSatisfactoryNA,
bool circulationPumpsSatisfactoryNA,
bool condenseTrapSatisfactoryNA,
string gasSolenoidValvesComments,
string controlBoxPcbComments,
string gasketSealsComments,
string burnerComments,
string burnerJetsComments,
string electrodesTransformerComments,
string flameFailureDeviceComments,
string systemBoilerControlsComments,
string boilerCasingComments,
string thermalInsulationComments,
string combustionFanIdFanComments,
string airFluePressureSwitchComments,
string controlLimitStatsComments,
string pressureTempGaugesComments,
string circulationPumpsComments,
string condenseTrapComments,
string HighFireCO2,
string HighFireCO,
string HighFireO2,
string HighFireFlueTemp,
string HighFireEfficiency,
string HighFireExcessAir,
string HighFireRoomTemp,
string HighFireRatio,
string LowFireCO2,
string LowFireCO,
string LowFireO2,
string LowFireFlueTemp,
string LowFireEfficiency,
string LowFireExcessAir,
string LowFireRoomTemp,
string LowFireRatio,
string warningNoticeIssueNumber,
string engineersName,
string engineersSignature,
string engineersGasSafeID,
string clientsName,
string clientsSignature,
string inspectionDate,
        string commetsDefects,
       string applianceServiceValveSatisfactoryComments,
   string  governorsComments
        )


        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 595;
            page.Width = 842;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);


            XImage image = await ConvertToXImage("ashwell_service_report.jpg");

            //gfx.DrawImage(image, 0, 0);
            //site
            gfx.DrawString(site, font, XBrushes.Black, new XRect(51, 67, 337 - 51, 95 - 67), XStringFormats.Center);
            //location
            gfx.DrawString(location, font, XBrushes.Black, new XRect(68, 98, 269 - 68, 124 - 98), XStringFormats.Center);
            //asset no
            gfx.DrawString(assetNumber, font, XBrushes.Black, new XRect(272, 107, 337 - 272, 123 - 107), XStringFormats.Center);
            //Appliance number
            gfx.DrawString(applianceNumber, font, XBrushes.Black, new XRect(76, 128, 111 - 76, 139 - 128), XStringFormats.Center);
            //Tests completed
            if (testsCompleted)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(186, 130, 11, 5), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(213, 130, 11, 5), 0, 360);
            }

            //remedial work required
            if (remedialWorkRequired)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(329, 130, 11, 5), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(356, 130, 11, 5), 0, 360);
            }

            //apliance serial number //apliance model
            gfx.DrawString(applianceNumber, font, XBrushes.Black, new XRect(82, 142, 196 - 82, 154 - 142), XStringFormats.Center);
            gfx.DrawString(applianceModel, font, XBrushes.Black, new XRect(250, 142, 376 - 250, 154 - 142), XStringFormats.Center);
            //apliance serial no  //GC no
            gfx.DrawString(applianceSerialNumber, font, XBrushes.Black, new XRect(93, 157, 268 - 93, 169 - 157), XStringFormats.Center);
            gfx.DrawString(gcNumber, font, XBrushes.Black, new XRect(300, 157, 76, 169 - 157), XStringFormats.Center);
            //Burner Make   //Burner model
            gfx.DrawString(burnerMake, font, XBrushes.Black, new XRect(70, 172, 196 - 70, 183 - 172), XStringFormats.Center);
            gfx.DrawString(burnerModel, font, XBrushes.Black, new XRect(243, 172, 376 - 243, 183 - 172), XStringFormats.Center);
            //Burner serial no  // type    //spec
            gfx.DrawString(burnerSerialNumber, font, XBrushes.Black, new XRect(85, 187, 195 - 85, 199 - 187), XStringFormats.Center);
            gfx.DrawString(Type, font, XBrushes.Black, new XRect(218, 187, 269 - 218, 199 - 187), XStringFormats.Center);
            gfx.DrawString(Spec, font, XBrushes.Black, new XRect(293, 187, 376 - 293, 199 - 187), XStringFormats.Center);
            //open flue   //room sealed  //forced draft   //flueless
            if (OpenFlue) { gfx.DrawString("Yes", font, XBrushes.Black, new XRect(159, 201, 177 - 159, 214 - 201), XStringFormats.Center); }
            else { gfx.DrawString("No", font, XBrushes.Black, new XRect(159, 201, 177 - 159, 214 - 201), XStringFormats.Center); }

            if (Roomsealed)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(226, 201, 244 - 226, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(226, 201, 244 - 226, 13), XStringFormats.Center);
            }

            if (ForcedDraft)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(294, 201, 310 - 294, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(294, 201, 310 - 294, 13), XStringFormats.Center);
            }

            if (Flueless)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(361, 201, 376 - 361, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(361, 201, 376 - 361, 13), XStringFormats.Center);
            }
            //heating   //hotwater  // both
            if (Heating)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(182, 217, 199 - 182, 229 - 217), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(182, 217, 199 - 182, 229 - 217), XStringFormats.Center);
            }

            if (HotWater)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(269, 217, 289 - 269, 229 - 217), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(269, 217, 289 - 269, 229 - 217), XStringFormats.Center);
            }

            if (Both)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(360, 217, 376 - 360, 229 - 217), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("No", font, XBrushes.Black, new XRect(360, 217, 376 - 360, 229 - 217), XStringFormats.Center);
            }


            //badged bumer pressure
            gfx.DrawString(badgedBurnerPressure, font, XBrushes.Black, new XRect(113, 232, 176 - 113, 243 - 232), XStringFormats.Center);
            //ventilation satisfactory
            if (ventilationSatisfactory)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(322, 235, 11, 5), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(354, 235, 11, 5), 0, 360);
            }
            //gas type
            if (gasType == "NG")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(133, 250, 11, 6), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(160, 250, 17, 6), 0, 360);
            }
            //flue condition satisfactory
            if (ventilationSatisfactory)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(322, 250, 11, 5), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(354, 250, 11, 5), 0, 360);
            }
            //approx age of apliance   //badged input   //badget output
            gfx.DrawString(approxAgeOfAppliance, font, XBrushes.Black, new XRect(114, 263, 145 - 114, 273 - 263), XStringFormats.Center);
            gfx.DrawString(badgedInput, font, XBrushes.Black, new XRect(214, 263, 255 - 214, 10), XStringFormats.Center);
            gfx.DrawString(badgedOutput, font, XBrushes.Black, new XRect(320, 263, 362 - 320, 10), XStringFormats.Center);
            double y = 307;
            //apliance component checklist
            List<bool> componentBooleans = new List<bool>
            {
                applianceServiceValveSatisfactory,
                governorsSatisfactory,
                gasSolenoidValvesSatisfactory,
                controlBoxPcbSatisfactory,
                gasketSealsSatisfactory,
                burnerSatisfactory,
                burnerJetsSatisfactory,
                electrodesTransformerSatisfactory,
                flameFailureDeviceSatisfactory,
                systemBoilerControlsSatisfactory,
                boilerCasingSatisfactory,
                thermalInsulationSatisfactory,
                combustionFanIdFanSatisfactory,
                airFluePressureSwitchSatisfactory,
                controlLimitStatsSatisfactory,
                pressureTempGaugesSatisfactory,
                circulationPumpsSatisfactory,
                condenseTrapSatisfactory,
                applianceServiceValveSatisfactoryNA,
                governorsSatisfactoryNA,
                gasSolenoidValvesSatisfactoryNA,
                controlBoxPcbSatisfactoryNA,
                gasketSealsSatisfactoryNA,
                burnerSatisfactoryNA,
                burnerJetsSatisfactoryNA,
                electrodesTransformerSatisfactoryNA,
                flameFailureDeviceSatisfactoryNA,
                systemBoilerControlsSatisfactoryNA,
                boilerCasingSatisfactoryNA,
                thermalInsulationSatisfactoryNA,
                combustionFanIdFanSatisfactoryNA,
                airFluePressureSwitchSatisfactoryNA,
                controlLimitStatsSatisfactoryNA,
                pressureTempGaugesSatisfactoryNA,
                circulationPumpsSatisfactoryNA,
                condenseTrapSatisfactoryNA
            };

            List<string> componentComments = new List<string>
            {
                applianceServiceValveSatisfactoryComments,
                 governorsComments,
                gasSolenoidValvesComments,
                controlBoxPcbComments,
                gasketSealsComments,
                burnerComments,
                burnerJetsComments,
                electrodesTransformerComments,
                flameFailureDeviceComments,
                systemBoilerControlsComments,
                boilerCasingComments,
                thermalInsulationComments,
                combustionFanIdFanComments,
                airFluePressureSwitchComments,
                controlLimitStatsComments,
                pressureTempGaugesComments,
                circulationPumpsComments,
                condenseTrapComments
                //Fale jos 2
            };
            for (int i = 0; i < 18; i++)
            {
                if (componentBooleans[i])
                {
                    gfx.DrawString("Yes", font, XBrushes.Black, new XRect(114, y, 140 - 114, 13), XStringFormats.Center);
                }
                else if (componentBooleans[i + 18])
                {
                    gfx.DrawString("N/A", font, XBrushes.Black, new XRect(170, y, 197 - 170, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString("No", font, XBrushes.Black, new XRect(142, y, 168 - 142, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //State appliance condition
            gfx.DrawString(stateApplianceCondition, font, XBrushes.Black, new XRect(497, 187, 576 - 497, 13), XStringFormats.Center);
            y = 307;
            for (int i = 0; i < 18; i++)
            {
                if (i == 8)
                {
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(247, y, 346 - 247, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(198, y, 346 - 198, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //heat exchanger/fluent clear
            ////heat exchanger/fluent clear
            if (heatExhanger)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(507, 55, 11, 5), 0, 360);
            }
            else if (heatExhangerNA)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 55, 11, 5), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 55, 11, 5), 0, 360);
            }
            //working inlet pressure   //recorded burner pressure    //measured gas rate   
            gfx.DrawString(workingInletPressure, font, XBrushes.Black, new XRect(497, 67, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString(recordedBurnerPressure, font, XBrushes.Black, new XRect(497, 83, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString(measuredGasRate, font, XBrushes.Black, new XRect(497, 99, 557 - 497, 12), XStringFormats.Center);
            //flue flow test
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(504, 115, 16, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 115, 12, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(557, 115, 12, 5), 0, 360);
            //spillage test
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(504, 130, 16, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 130, 12, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(557, 130, 12, 5), 0, 360);
            //AECV plant isolation correct
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(504, 145, 16, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 145, 12, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(557, 145, 12, 5), 0, 360);
            //safety shut off valve
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(504, 160, 16, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 160, 12, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(557, 160, 12, 5), 0, 360);
            //plantroom gas thightness level
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(504, 175, 16, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 175, 12, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(557, 175, 12, 5), 0, 360);

            y = 53;
            float x = 580;
            //remarks comments page right
            List<string> componentComments1 = new List<string>
            {

                heatExhangerComments,
                workingInletPressureComments,
                recordedBurnerPressureComments,
                measuredGasRateComments,
                flueFlowTestComments,
                spillageTestComments,
                AECVPlantIsolationCorrectComments,
                safetyShutOffValveComments,
                plantroomGasTightnessTestComments,
                stateApplianceConditionComments,

            };
            for (int i = 0; i < 10; i++)
            {
                gfx.DrawString(componentComments1[i], font, XBrushes.Black, new XRect(x, y, 231, 11), XStringFormats.Center);
                y += 15;
            }
            y = 504;
            x = 390;
            // i = 0; // engineers name //enginiers singature // enginieers gas safe id number
            // i = 1 //clients name // clients signature  // inspection date

            gfx.DrawString(engineersName, font, XBrushes.Black, new XRect(x, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(engineersSignature, font, XBrushes.Black, new XRect(x + 142, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(engineersGasSafeID, font, XBrushes.Black, new XRect(x + 284, y, 137, 25), XStringFormats.Center);
            y += 45;
            gfx.DrawString(clientsName, font, XBrushes.Black, new XRect(x, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(clientsSignature, font, XBrushes.Black, new XRect(x + 142, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(inspectionDate, font, XBrushes.Black, new XRect(x + 284, y, 137, 25), XStringFormats.Center);


            //comments/defects
            XRect boundingBox = new XRect(393, 287, 414, 166);
            string text = commetsDefects;
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Calibri", 11), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);
            //unsafe situations warning notice issue number
            gfx.DrawString(warningNoticeIssueNumber, font, XBrushes.Black, new XRect(610, 459, 200, 9), XStringFormats.Center);

            x = 432;
            y = 218.5;
            //co2 //flue temp  //co2 // flue temp
            //co  //effisency // co // effisency
            //o2 // excess air
            //ratio  //room temp
            List<string> debelamasnakurcinanajvecanasvetu = new List<string>
        {
             HighFireCO2,
             HighFireCO,
             HighFireO2,
             HighFireRatio,
             HighFireFlueTemp,
             HighFireEfficiency,
             HighFireExcessAir,
             HighFireRoomTemp,
             LowFireCO2,
             LowFireCO,
             LowFireO2,
             LowFireRatio,
             LowFireFlueTemp,
             LowFireEfficiency,
             LowFireExcessAir,
             LowFireRoomTemp,
        };

            for (int i = 0; i < 4; i++)
            {
                gfx.DrawString(debelamasnakurcinanajvecanasvetu[i], font, XBrushes.Black, new XRect(x, y, 43, 10), XStringFormats.Center);
                gfx.DrawString(debelamasnakurcinanajvecanasvetu[i + 4], font, XBrushes.Black, new XRect(x + 106, y, 43, 10), XStringFormats.Center);
                gfx.DrawString(debelamasnakurcinanajvecanasvetu[i + 8], font, XBrushes.Black, new XRect(x + 212, y, 43, 10), XStringFormats.Center);
                gfx.DrawString(debelamasnakurcinanajvecanasvetu[i + 12], font, XBrushes.Black, new XRect(x + 318, y, 43, 10), XStringFormats.Center);
                y += 15;
            }




            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "Ashwell_Service_Report.pdf");


        }
    }
}
