﻿using System;
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

        public static async Task<XImage> ConvertToXImage(string filename)
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

                byte[] imageBytes;
                using (var originalStream = await streamImageSource.Stream(CancellationToken.None))
                {
                    using (var ms = new MemoryStream())
                    {
                        await originalStream.CopyToAsync(ms);
                        imageBytes = ms.ToArray();
                    }
                }

                MemoryStream imageStream = new MemoryStream(imageBytes);
                XImage xImage = XImage.FromStream(imageStream);
                return xImage;
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

            PdfDocument document = new PdfDocument(); document.Info.Title = "Ashwell Service Report";


            PdfPage page = document.AddPage();
            page.Height = 595;
            page.Width = 842;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 8);


            XImage image = await ConvertToXImage(@"ashwell_service_report.jpg");

            gfx.DrawImage(image, 0, 0, 842, 595);
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
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(185, 129, 13, 7), 0, 360);//+1
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(212, 129, 13, 7), 0, 360);
            }

            //remedial work required
            if (remedialWorkRequired)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(328, 129, 13, 7), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(355, 129, 13, 7), 0, 360);
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
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(321, 234, 13, 7), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(353, 234, 13, 7), 0, 360);
            }
            //gas type
            if (gasType == "NG")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(132, 249, 13, 7), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(159, 249, 19, 8), 0, 360);
            }
            //flue condition satisfactory
            if (ventilationSatisfactory)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(321, 249, 13, 7), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(353, 249, 13, 7), 0, 360);
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
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(247, y, 36 - 247, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(198, y, 376 - 198, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //heat exchanger/fluent clear
            ////heat exchanger/fluent clear
            if (heatExhanger)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(506, 54, 13, 7), 0, 360);
            }
            else if (heatExhangerNA)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 54, 13, 7), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 54, 13, 7), 0, 360);
            }
            //working inlet pressure   //recorded burner pressure    //measured gas rate   
            gfx.DrawString(workingInletPressure, font, XBrushes.Black, new XRect(497, 67, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString(recordedBurnerPressure, font, XBrushes.Black, new XRect(497, 83, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString(measuredGasRate, font, XBrushes.Black, new XRect(497, 99, 557 - 497, 12), XStringFormats.Center);
            //flue flow test
            if (flueFlowTest)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(503, 114, 18, 7), 0, 360);
            }
            else if (!flueFlowTestNA)
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 114, 14, 7), 0, 360);
            }else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 114, 14, 7), 0, 360);
            //spillage test
            if (spillageTest) 
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(503, 129, 18, 7), 0, 360);
            else if(!spillageTestNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 129, 14, 7), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 129, 14, 7), 0, 360);
            //AECV plant isolation correct
            if(AECVPlantIsolationCorrect)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(503, 144, 18, 7), 0, 360);
            else if(!AECVPlantIsolationCorrectNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 144, 14, 7), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 144, 14, 7), 0, 360);
            //safety shut off valve
            if(safetyShutOffValve)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(503, 159, 18, 7), 0, 360);
            else if(!safetyShutOffValveNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 159, 14, 7), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 159, 14, 7), 0, 360);
            //plantroom gas thightness level
            if(plantroomGasTightnessTest)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(503, 174, 18, 7), 0, 360);
            else if(!plantroomGasTightnessTestNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(531, 174, 14, 7), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 174, 14, 7), 0, 360);

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
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);
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
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"Ashwell_Service_Report_{dateTimeString}.pdf");


            document.Save(filePath);
        }
        public static async Task CreateEngineersReport(
     string clientsName,
string address,
string date,
string engineer,
string taskTNo,
bool checkTaskComplete,
string applianceMake,
string serialNumber,
string description,
bool checkSpillageTestPerformed,
bool checkSpillageTestPerformedNA,
bool checkRiskAssesmentCompleted,
bool checkFlueFlowTest,
bool checkFlueFlowTestNA,
string gasOperatinPressure,
string inletPressure,
bool checkThightnessTestCarriedOut,
bool checkThightnessTestCarriedOutNA,
string thightnessTestCarriedOut,
string totalHoursIncludingTravel,
bool checkApplianceSafeToUse,
bool checkWarningNoticeIssued,
string warningNoticeNumber
     )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);


            XImage image = await ConvertToXImage(@"Engineers_report.png");

            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(clientsName, font, XBrushes.Black, new XRect(118, 123, 288 - 118, 143 - 123), XStringFormat.Center);

            gfx.DrawString(address, font, XBrushes.Black, new XRect(6, 145, 280, 25), XStringFormat.Center);
            gfx.DrawString(" ", font, XBrushes.Black, new XRect(6, 170, 280, 25), XStringFormat.Center);
            gfx.DrawString(" ", font, XBrushes.Black, new XRect(6, 195, 280, 25), XStringFormat.Center);

            gfx.DrawString(applianceMake, font, XBrushes.Black, new XRect(79, 219, 210, 24), XStringFormat.Center);

            gfx.DrawString(date, font, XBrushes.Black, new XRect(340, 125, 588 - 340, 25), XStringFormat.Center);
            gfx.DrawString(engineer, font, XBrushes.Black, new XRect(340, 145, 588 - 340, 25), XStringFormat.Center);
            gfx.DrawString(taskTNo, font, XBrushes.Black, new XRect(340, 170, 588 - 340, 25), XStringFormat.Center);


            //gfx.DrawEllipse(XBrushes.Black, new XRect(377, 194, 420 - 377, 25));
            if(checkTaskComplete)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(376, 193, 396 - 377+2, 206 - 194+2), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(490, 193, 396 - 377+2, 206 - 194+2), 0, 360);

            gfx.DrawString(serialNumber, font, XBrushes.Black, new XRect(340, 219, 588 - 340, 25), XStringFormat.Center);

            string[] l = {"  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
            string[] TempDesc = description.Split();
            int d = 0;
            int duzina = 0;
            for (int i = 0; i < TempDesc.Length; i++)
            {
                if (duzina + TempDesc[i].Length < 112)
                {
                    l[d] += TempDesc[i]+" ";
                    duzina += TempDesc[i].Length + 1;
                }
                else
                {
                    d++;
                    l[d] += TempDesc[i] + " ";
                    duzina = TempDesc[i].Length+1;
                }
              
            }
            int y = 267;
            for (int i = 0; i < 18; i++)
            {
                gfx.DrawString(l[i], font, XBrushes.Black, new XRect(7, y, 588 - 7, 23), XStringFormats.BottomLeft);
                y += 24;
            }
            if(checkSpillageTestPerformed)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(115, 701, 24, 10), 0, 360);
            else if(!checkSpillageTestPerformedNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(142, 701, 22, 10), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(165, 701, 22, 10), 0, 360);
            if(checkRiskAssesmentCompleted)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(308, 701, 22, 10), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(332, 701, 18, 10), 0, 360);
            if(checkFlueFlowTest)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(72, 725, 25, 11), 0, 360);
            else if(!checkFlueFlowTestNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(102, 725, 23, 11), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(129, 725, 17, 11), 0, 360);
            if(checkThightnessTestCarriedOut)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(126, 749, 25, 10), 0, 360);
            else if(!checkThightnessTestCarriedOutNA)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(155, 749, 23, 10), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(179, 749, 17, 10), 0, 360);
            if(checkApplianceSafeToUse)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(102, 773, 22, 10), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(126, 773, 20, 10), 0, 360);
            if(checkWarningNoticeIssued)
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(107, 796, 22, 10), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(131, 796, 17, 10), 0, 360);

            gfx.DrawString(warningNoticeNumber, font, XBrushes.Black, new XRect(267, 797, 105, 17), XStringFormats.Center);

            gfx.DrawString(gasOperatinPressure, font, XBrushes.Black, new XRect(190, 735, 47, 8), XStringFormats.Center);

            gfx.DrawString(inletPressure, font, XBrushes.Black, new XRect(300, 735, 47, 8), XStringFormats.Center);

            gfx.DrawString(totalHoursIncludingTravel, font, XBrushes.Black, new XRect(514, 748, 74, 16), XStringFormats.Center);



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"Engineers_Report_Sheet_{dateTimeString}.pdf");


            document.Save(filePath);


        }
        public static async Task PressurisationReport(
          string siteNameAndAddress,
          string totalHeatingSystemRating,
          string numberOfBoilers,
          string flowTemperature,
          string returnTemperature,
          string currentWorkingPressure,
          string safetyValveSetting,
          string unitModel,
          string serialNo,
          string expansionVesselSize,
          string numberOfPressureVessels,
          string setFillPressure,
          string ratedExpansionVesselCharge,
          string highPressureSwitchSetting,
          string lowPressureSwitchSetting,
          string finalSystemPressure,
          bool checkMainWaterSupply,
          bool checkColdFillPressureSet,
          bool checkElectricalSupplyWorking,
          bool checkFillingLoopDisconnected,
          bool checkUnitLeftOperational,
          string notes,
          string date,
          string engineer

          )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Pressurisation Unit Service Report";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"Pressurisation Unit Report.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            XRect boundingBox = new XRect(35, 222, 561 - 35, 259 - 222);
            string text = siteNameAndAddress;
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 10), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);

            //gfx.DrawString(totalHeatingSystemRating, font, XBrushes.Black, new XRect(298, 308, 543 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(numberOfBoilers, font, XBrushes.Black, new XRect(298, 323, 566 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(flowTemperature, font, XBrushes.Black, new XRect(298, 339, 548 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(returnTemperature, font, XBrushes.Black, new XRect(298, 354, 548 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(currentWorkingPressure, font, XBrushes.Black, new XRect(298, 369, 543 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(safetyValveSetting, font, XBrushes.Black, new XRect(298, 385, 543 - 298, 14), XStringFormat.Center);

            gfx.DrawString(totalHeatingSystemRating, font, XBrushes.Black, new XRect(298, 308, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(numberOfBoilers, font, XBrushes.Black, new XRect(298, 323, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(flowTemperature, font, XBrushes.Black, new XRect(298, 339, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(returnTemperature, font, XBrushes.Black, new XRect(298, 354, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(currentWorkingPressure, font, XBrushes.Black, new XRect(298, 369, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(safetyValveSetting, font, XBrushes.Black, new XRect(298, 385, 566 - 298, 14), XStringFormat.Center);

            gfx.DrawString(unitModel, font, XBrushes.Black, new XRect(99, 444, 296 - 99, 25), XStringFormat.Center);
            gfx.DrawString(serialNo, font, XBrushes.Black, new XRect(359, 444, 565 - 359, 25), XStringFormat.Center);
            gfx.DrawString(expansionVesselSize, font, XBrushes.Black, new XRect(183, 471, 296 - 183, 25), XStringFormat.Center);
            gfx.DrawString(numberOfPressureVessels, font, XBrushes.Black, new XRect(447, 471, 565 - 447, 25), XStringFormat.Center);

            List<string> PressurationUnitSetings = new List<string>
            {

                setFillPressure,
                ratedExpansionVesselCharge,
                highPressureSwitchSetting,
                lowPressureSwitchSetting,
                finalSystemPressure,

            };
            double y = 542;
            for (int i = 0; i < 5; i++)
            {
                gfx.DrawString(PressurationUnitSetings[i], font, XBrushes.Black, new XRect(227, y, 268 - 227, 13), XStringFormat.Center);
                y += 15;
            }
            List<bool> check = new List<bool>
            {
                checkMainWaterSupply,
                checkColdFillPressureSet,
                checkElectricalSupplyWorking,
                checkFillingLoopDisconnected,
                checkUnitLeftOperational,
            };
            y = 544;
            for (int i = 0; i < 5; i++)
            {
                if (check[i])
                    gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(499, y - 1, 20, 11), 0, 360);
                else
                    gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(525, y - 1, 17, 11), 0, 360);
                y += 15;

            }
            XRect boundingBox1 = new XRect(35, 659, 561 - 35, 771 - 659);
            string text1 = notes;
            XTextFormatter tf1 = new XTextFormatter(gfx);
            XRect layoutRectangle1 = boundingBox1;
            tf1.DrawString(text1, new XFont("Arial", 10), XBrushes.Black, layoutRectangle1, XStringFormats.TopLeft);

            gfx.DrawString(date, font, XBrushes.Black, new XRect(245, 777, 378 - 245, 25), XStringFormat.Center);
            gfx.DrawString(engineer, font, XBrushes.Black, new XRect(435, 777, 565 - 435, 25), XStringFormat.Center);

            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"Pressurisation_Unit_Service_Report_{dateTimeString}.pdf");

            document.Save(filePath);

        }
    }
 
}
