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
        public static XImage ConvertToXImage(byte[] imageBytes)
        {
            try
            {
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
        static public double MeterTypeVolume(string x)
        {
            if (x == "G4/U6")
                return 0.008;
            else if (x == "U16")
                return 0.025;
            else if (x == "U25")
                return 0.037;
            else if (x == "U40")
                return 0.067;
            else if (x == "U65")
                return 0.1;
            else if (x == "U100")
                return 0.182;
            else if (x == "U160")
                return 0.304;
            else if (x == "RD or Turnime")
                return 0.079;
            return 0.0024;
        }
        public static async Task<PdfDocument> _1Up(Dictionary<string, string> dic)
        {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"ige_up_1.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["siteAddress"], font, XBrushes.Black, new XRect(86, 125, 483, 14), XStringFormats.Center);
            gfx.DrawString(dic["location"], font, XBrushes.Black, new XRect(69, 144, 308, 13), XStringFormats.Center);
            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(406, 144, 162, 13), XStringFormats.Center);

            //engineer signature
            gfx.DrawString(dic["engineer"], font, XBrushes.Black, new XRect(68, 162, 150, 13), XStringFormats.Center);
            gfx.DrawString(dic["cardNumber"], font, XBrushes.Black, new XRect(283, 162, 92, 13), XStringFormats.Center);
            // gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(423, 162, 162, 13), XStringFormats.Center);

            //client signature
            gfx.DrawString(dic["clientName"], font, XBrushes.Black, new XRect(81, 179, 137, 13), XStringFormats.Center);
            //gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(292, 179, 84, 13), XStringFormats.Center);
            gfx.DrawString(dic["warningNoticeNo"], font, XBrushes.Black, new XRect(442, 179, 128, 13), XStringFormats.Center);
            List<string> listaDuzina = new List<string>
            { 
                dic["1/2"],
                dic["3/4"],
                dic["1"],
                dic["1 1/4"],
                dic["1 1/2"],
                dic["2"],
                dic["2 1/2"],
                dic["3"],
                dic["4"],
                dic["5"],
                dic["6"],
                dic["8"],
                dic["10"],
                dic["15mm"],
                dic["22mm"],
                dic["28mm"],
                dic["35mm"],
                dic["42mm"],
                dic["54mm"],
                dic["67mm"],
                dic["20mm"],
                dic["25mm"],
                dic["32mm"],
                dic["55mm"],
                dic["63mm"],
                dic["75mm"],
                dic["90mm"],
                dic["125mm"],
            };
            List<string> listaDuzinaDouble = new List<string>();
            List<double> pomnozaj = new List<double>
            {
                0.00024,
                0.000046,
                0.00064,
                0.0011,
                0.0015,
                0.0024,
                0.0038,
                0.0054,
                0.009,
                0.014,
                0.02,
                0.035,
                0.053,
                0.00014,
                0.00032,
                0.00054,
                0.00084,
                0.0012,
                0.0021,
                0.0033,
                0.00019,
                0.00033,
                0.00053,
                0.0016,
                0.0021,
                0.0029,
                0.004,
                0.008
            };
            int or = 0;
            foreach(var str in listaDuzina)
            {
                if (str != String.Empty)
                {
                    listaDuzinaDouble.Add((pomnozaj[or++] * Double.Parse(str)).ToString("F5"));
                }
                else listaDuzinaDouble.Add(" ");
            }
            double Total = 0;
            foreach(var str in listaDuzinaDouble)
            {
                if(str != " ")
                {
                    Total += Double.Parse(str);
                }
            }
            listaDuzinaDouble.Add(Total.ToString("F5"));
            listaDuzinaDouble.Add((1.1 * Total).ToString("F5"));
            listaDuzinaDouble.Add(MeterTypeVolume(dic["meterVolume"]).ToString());
            listaDuzinaDouble.Add(((1.1 * Total) + MeterTypeVolume(dic["meterVolume"])).ToString("F5"));//prethodna dva sabrana
            listaDuzinaDouble.Add(dic["testMedium"]);
            listaDuzinaDouble.Add(dic["testMediumFactor"]);
            listaDuzinaDouble.Add(dic["installation"]);
            listaDuzinaDouble.Add(dic["weather/temperature"]);
            listaDuzinaDouble.Add(dic["metarBypass"]);
            listaDuzinaDouble.Add(dic["testGaugeUsed"]);
            listaDuzinaDouble.Add(dic["gaudgeReadableMovment"]);
            listaDuzinaDouble.Add(dic["tightnessTestPressure"]);
            listaDuzinaDouble.Add(dic["maximumPermittedLeakRate"]);
            listaDuzinaDouble.Add(dic["barometricPressure"]);

            double x, y;
            x = 100;
            y = 269;
            int ouchL1 = 0;
            for (int i = 0; i < 30; i++)
            {
                if (i == 13 || i == 21)
                {
                    y += 10.95;
                    continue;
                }
                gfx.DrawString(listaDuzina[ouchL1++], font, XBrushes.Black, new XRect(x, y, 53, 11), XStringFormats.Center);
                y += 11.95;
            }
            y = 269;
            x = 223;
            ouchL1 = 0;
            for (int i = 0; i < 46; i++)
            {
                if (i == 33) y -= 2;
                if (i == 13 || i == 21 || i == 34 || i == 35)
                {

                    y += 10.95;
                    continue;
                }
                gfx.DrawString(listaDuzinaDouble[ouchL1++], font, XBrushes.Black, new XRect(x, y, 65, 11), XStringFormats.Center);
                y += 11.95;
            }
            x = 508;
            y = 412;
            for (int i = 0; i < 7; i++)
            {
                gfx.DrawRectangle(XBrushes.White, new XRect(x, y + 1, 60, 5));
                gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(x, y, 65, 10), XStringFormats.Center);
                y += 11.85;
            }
            double TimeTaken = double.Parse(dic["gaudgeReadableMovement"]) * ((1.1 * Total) + MeterTypeVolume(dic["meterVolume"])) * double.Parse(dic["testMediumFactor"]);
            gfx.DrawString((TimeTaken).ToString("F5"), font, XBrushes.Black, new XRect(510, 520, 60, 30), XStringFormats.Center);
            gfx.DrawString((2.8 * TimeTaken / double.Parse(dic["roomVolume"])).ToString("F5"), font, XBrushes.Black, new XRect(510, 555, 60, 30), XStringFormats.Center);
            gfx.DrawString(dic["roomVolume"], font, XBrushes.Black, new XRect(510, 590, 60, 10), XStringFormats.Center);
            gfx.DrawString((0.047*TimeTaken).ToString("F5"), font, XBrushes.Black, new XRect(510, 603, 60, 30), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 638, 60, 20), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 658, 60, 20), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 680, 60, 20), XStringFormats.Center);

            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 730, 60, 10), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 741, 60, 10), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(510, 752, 60, 10), XStringFormats.Center);

            gfx.DrawString(dic[""], font, XBrushes.Red, new XRect(322, 790, 571 - 322, 811 - 790), XStringFormats.Center);



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "IGE_UP_1 Sheet.pdf");

            document.Save(filePath);
            
            return document;

        }
        public static async Task<PdfDocument> _1A(Dictionary<string,string> dic)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

          //  XImage image = ConvertToXImage();
          //  gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(86, 104, 476, 14), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(69, 120, 308, 13), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(406, 120, 162, 13), XStringFormats.Center);


            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(68, 136, 150, 13), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(283, 136, 92, 13), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(423, 136, 162, 13), XStringFormats.Center);


            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(81, 152, 137, 13), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(292, 152, 84, 13), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(442, 152, 128, 13), XStringFormats.Center);

            double x, y;
            x = 106;
            y = 230;
            for (int i = 0; i < 30; i++)
            {
                if (i == 13 || i == 21)
                {
                    y += 10.0;
                    continue;
                }
                gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(x, y, 53, 10), XStringFormats.Center);
                y += 10.95;
            }
            y = 230;
            x = 244;
            for (int i = 0; i < 54; i++)
            {
                if (i == 33) y -= 2;
                if (i == 13 || i == 21 || i == 34 || i == 35 || i == 45 || i == 46)
                {

                    y += 10.25;
                    continue;
                }
                gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(x, y, 62, 10), XStringFormats.Center);
                y += 10.95;
            }
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(506, 698, 56, 20), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(506, 720, 56, 20), XStringFormats.Center);
            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(506, 740, 56, 20), XStringFormats.Center);

            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(506, 774, 56, 8), XStringFormats.Center);

            gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(338, 795, 224, 18), XStringFormats.Center);






            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "IGE_UP_1A Sheet.pdf");

            document.Save(filePath);

            return document;

        
    }

        //        public static async Task CreateServiceRecordPDF(string reportName, string workingInletPressure, string site, string location, string applianceNumber,
        //string recordedBurnerPressure,
        //string assetNumber,
        //string measuredGasRate,
        //bool heatExhanger,
        //bool heatExhangerNA,
        //string heatExhangerComments,
        //bool flueFlowTest,
        //bool flueFlowTestNA,
        //string flueFlowTestComments,
        //bool spillageTest,
        //bool spillageTestNA,
        //string spillageTestComments,
        //bool safetyShutOffValve,
        //bool safetyShutOffValveNA,
        //string safetyShutOffValveComments,
        //bool plantroomGasTightnessTest,
        //bool plantroomGasTightnessTestNA,
        //string plantroomGasTightnessTestComments,
        //bool AECVPlantIsolationCorrect,
        //bool AECVPlantIsolationCorrectNA,
        //string AECVPlantIsolationCorrectComments,
        //string stateApplianceConditionComments,
        //string workingInletPressureComments,
        //string recordedBurnerPressureComments,
        //string measuredGasRateComments,
        //bool testsCompleted,
        //bool remedialWorkRequired,
        //string applianceMake,
        //string applianceModel,
        //string applianceSerialNumber,
        //string gcNumber,
        //string stateApplianceCondition,
        //string burnerMake,
        //string burnerModel,
        //string burnerSerialNumber,
        //string Type,
        //string Spec,
        //bool OpenFlue,
        //bool Roomsealed,
        //bool ForcedDraft,
        //bool Flueless,
        //bool Heating,
        //bool HotWater,
        //bool Both,
        //string badgedBurnerPressure,
        //bool ventilationSatisfactory,
        //string gasType,
        //bool flueConditionSatisfactory,
        //string approxAgeOfAppliance,
        //string badgedInput,
        //string badgedOutput,
        //bool applianceServiceValveSatisfactory,
        //bool governorsSatisfactory,
        //bool gasSolenoidValvesSatisfactory,
        //bool controlBoxPcbSatisfactory,
        //bool gasketSealsSatisfactory,
        //bool burnerSatisfactory,
        //bool burnerJetsSatisfactory,
        //bool electrodesTransformerSatisfactory,
        //bool flameFailureDeviceSatisfactory,
        //bool systemBoilerControlsSatisfactory,
        //bool boilerCasingSatisfactory,
        //bool thermalInsulationSatisfactory,
        //bool combustionFanIdFanSatisfactory,
        //bool airFluePressureSwitchSatisfactory,
        //bool controlLimitStatsSatisfactory,
        //bool pressureTempGaugesSatisfactory,
        //bool circulationPumpsSatisfactory,
        //bool condenseTrapSatisfactory,
        //bool applianceServiceValveSatisfactoryNA,
        //bool governorsSatisfactoryNA,
        //bool gasSolenoidValvesSatisfactoryNA,
        //bool controlBoxPcbSatisfactoryNA,
        //bool gasketSealsSatisfactoryNA,
        //bool burnerSatisfactoryNA,
        //bool burnerJetsSatisfactoryNA,
        //bool electrodesTransformerSatisfactoryNA,
        //bool flameFailureDeviceSatisfactoryNA,
        //bool systemBoilerControlsSatisfactoryNA,
        //bool boilerCasingSatisfactoryNA,
        //bool thermalInsulationSatisfactoryNA,
        //bool combustionFanIdFanSatisfactoryNA,
        //bool airFluePressureSwitchSatisfactoryNA,
        //bool controlLimitStatsSatisfactoryNA,
        //bool pressureTempGaugesSatisfactoryNA,
        //bool circulationPumpsSatisfactoryNA,
        //bool condenseTrapSatisfactoryNA,
        //string gasSolenoidValvesComments,
        //string controlBoxPcbComments,
        //string gasketSealsComments,
        //string burnerComments,
        //string burnerJetsComments,
        //string electrodesTransformerComments,
        //string flameFailureDeviceComments,
        //string systemBoilerControlsComments,
        //string boilerCasingComments,
        //string thermalInsulationComments,
        //string combustionFanIdFanComments,
        //string airFluePressureSwitchComments,
        //string controlLimitStatsComments,
        //string pressureTempGaugesComments,
        //string circulationPumpsComments,
        //string condenseTrapComments,
        //string HighFireCO2,
        //string HighFireCO,
        //string HighFireO2,
        //string HighFireFlueTemp,
        //string HighFireEfficiency,
        //string HighFireExcessAir,
        //string HighFireRoomTemp,
        //string HighFireRatio,
        //string LowFireCO2,
        //string LowFireCO,
        //string LowFireO2,
        //string LowFireFlueTemp,
        //string LowFireEfficiency,
        //string LowFireExcessAir,
        //string LowFireRoomTemp,
        //string LowFireRatio,
        //string warningNoticeIssueNumber,
        //string engineersName,
        //string engineersSignature,
        //string engineersGasSafeID,
        //string clientsName,
        //string clientsSignature,
        //string inspectionDate,
        //        string commetsDefects,
        //       string applianceServiceValveSatisfactoryComments,
        //   string  governorsComments
        //        )

        public static async Task<byte[]> CreateServiceRecordPDF(Dictionary<string, string> dic, byte[] inzenjer, byte[] clijent)
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

            //potpis - za doncica
            //   gfx.DrawImage(ConvertToXImage(inzenjer), new XPoint(531+(671-531- ConvertToXImage(inzenjer).PixelWidth/2),548+(529-502- ConvertToXImage(inzenjer).PixelHeight/2)));
            if (inzenjer != null && inzenjer.Length != 0)
                gfx.DrawImage(ConvertToXImage(inzenjer), new XPoint(531, 502));
            //   gfx.DrawImage(ConvertToXImage(inzenjer), 531, 502, 671 - 531, 529 - 502);
            if (clijent != null && clijent.Length != 0)
                gfx.DrawImage(ConvertToXImage(clijent), 531, 548, 671 - 531, 529 - 502);

            //site
            gfx.DrawString(dic["site"], font, XBrushes.Black, new XRect(35, 72, 337 - 35, 95 - 72), XStringFormats.CenterLeft);
            //location
            gfx.DrawString(dic["location"], font, XBrushes.Black, new XRect(35, 108, 269 - 65, 124 - 108), XStringFormats.CenterLeft);
            //asset no
            gfx.DrawString(dic["assetNumber"], font, XBrushes.Black, new XRect(275, 108, 337 - 272, 123 - 108), XStringFormats.CenterLeft);
            //Appliance number
            gfx.DrawString(dic["applianceNumber"], font, XBrushes.Black, new XRect(76, 128, 111 - 76, 139 - 128), XStringFormats.Center);
            //Tests completed
            if (dic["testsCompleted"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(184, 128, 15, 9), 0, 360);//+1
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(211, 128, 15,9), 0, 360);
            }

            //remedial work required
            if (dic["remedialWorkRequired"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(327, 128, 15, 9), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(354, 128, 15, 9), 0, 360);
            }

            //apliance serial number //apliance model
            gfx.DrawString(dic["applianceNumber"], font, XBrushes.Black, new XRect(82, 142, 196 - 82, 154 - 142), XStringFormats.Center);
            gfx.DrawString(dic["applianceModel"], font, XBrushes.Black, new XRect(250, 142, 376 - 250, 154 - 142), XStringFormats.Center);
            //apliance serial no  //GC no
            gfx.DrawString(dic["applianceSerialNumber"], font, XBrushes.Black, new XRect(93, 157, 268 - 93, 169 - 157), XStringFormats.Center);
            gfx.DrawString(dic["gcNumber"], font, XBrushes.Black, new XRect(300, 157, 76, 169 - 157), XStringFormats.Center);
            //Burner Make   //Burner model
            gfx.DrawString(dic["burnerMake"], font, XBrushes.Black, new XRect(70, 172, 196 - 70, 183 - 172), XStringFormats.Center);
            gfx.DrawString(dic["burnerModel"], font, XBrushes.Black, new XRect(243, 172, 376 - 243, 183 - 172), XStringFormats.Center);
            //Burner serial no  // type    //spec
            gfx.DrawString(dic["burnerSerialNumber"], font, XBrushes.Black, new XRect(85, 187, 195 - 85, 199 - 187), XStringFormats.Center);
            gfx.DrawString(dic["Type"], font, XBrushes.Black, new XRect(218, 187, 269 - 218, 199 - 187), XStringFormats.Center);
            gfx.DrawString(dic["Spec"], font, XBrushes.Black, new XRect(293, 187, 376 - 293, 199 - 187), XStringFormats.Center);
            //open flue   //room sealed  //forced draft   //flueless
            if (dic["OpenFlue"] == "True") { gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(159, 201, 177 - 159, 214 - 201), XStringFormats.Center); }
            //else { gfx.DrawString("No", font, XBrushes.Black, new XRect(159, 201, 177 - 159, 214 - 201), XStringFormats.Center); }

            if (dic["Roomsealed"] == "True")
            {
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(226, 201, 244 - 226, 13), XStringFormats.Center);
            }
            else
            {
             //   gfx.DrawString("No", font, XBrushes.Black, new XRect(226, 201, 244 - 226, 13), XStringFormats.Center);
            }

            if (dic["ForcedDraft"] == "True")
            {
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(294, 201, 310 - 294, 13), XStringFormats.Center);
            }
            else
            {
              //  gfx.DrawString("No", font, XBrushes.Black, new XRect(294, 201, 310 - 294, 13), XStringFormats.Center);
            }

            if (dic["Flueless"] == "True")
            {
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(361, 201, 376 - 361, 13), XStringFormats.Center);
            }
            else
            {
              //  gfx.DrawString("No", font, XBrushes.Black, new XRect(361, 201, 376 - 361, 13), XStringFormats.Center);
            }
            //heating   //hotwater  // both
            if (dic["Heating"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(182, 217, 199 - 182, 229 - 217), XStringFormats.Center);
            }
            else
            {
              //  gfx.DrawString("No", font, XBrushes.Black, new XRect(182, 217, 199 - 182, 229 - 217), XStringFormats.Center);
            }

            if (dic["HotWater"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(269, 217, 289 - 269, 229 - 217), XStringFormats.Center);
            }
            else
            {
               // gfx.DrawString("No", font, XBrushes.Black, new XRect(269, 217, 289 - 269, 229 - 217), XStringFormats.Center);
            }

            if (dic["Both"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(360, 217, 376 - 360, 229 - 217), XStringFormats.Center);
            }
            else
            {
               // gfx.DrawString("No", font, XBrushes.Black, new XRect(360, 217, 376 - 360, 229 - 217), XStringFormats.Center);
            }


            //badged bumer pressure
            gfx.DrawString(dic["badgedBurnerPressure"], font, XBrushes.Black, new XRect(113, 232, 176 - 113, 243 - 232), XStringFormats.Center);
            //ventilation satisfactory
            if (dic["ventilationSatisfactory"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(319, 233, 15, 9), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(351, 233, 15, 9), 0, 360);
            }
            //gas type
            if (dic["gasType"] == "NG")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(131, 248, 15, 9), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(158, 248, 21, 10), 0, 360);
            }
            //flue condition satisfactory
            if (dic["ventilationSatisfactory"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(319, 248, 15, 9), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(351, 248, 15, 9), 0, 360);
            }
            //approx age of apliance   //badged input   //badget output
            gfx.DrawString(dic["approxAgeOfAppliance"], font, XBrushes.Black, new XRect(114, 263, 145 - 114, 273 - 263), XStringFormats.Center);
            gfx.DrawString(dic["badgedInput"], font, XBrushes.Black, new XRect(214, 263, 255 - 214, 10), XStringFormats.Center);
            gfx.DrawString(dic["badgedOutput"], font, XBrushes.Black, new XRect(320, 263, 362 - 320, 10), XStringFormats.Center);
            double y = 307;
            //apliance component checklist
            List<string> componentBooleans1 = new List<string>
            {
               dic["applianceServiceValveSatisfactory"],
               dic["governorsSatisfactory"],
               dic["gasSolenoidValvesSatisfactory"],
               dic["controlBoxPcbSatisfactory"],
               dic["gasketSealsSatisfactory"],
               dic["burnerSatisfactory"],
               dic["burnerJetsSatisfactory"],
               dic["electrodesTransformerSatisfactory"],
               dic["flameFailureDeviceSatisfactory"],
               dic["systemBoilerControlsSatisfactory"],
               dic["boilerCasingSatisfactory"],
               dic["thermalInsulationSatisfactory"],
               dic["combustionFanIdFanSatisfactory"],
               dic["airFluePressureSwitchSatisfactory"],
               dic["controlLimitStatsSatisfactory"],
               dic["pressureTempGaugesSatisfactory"],
               dic["circulationPumpsSatisfactory"],
               dic["condenseTrapSatisfactory"],
               dic["applianceServiceValveSatisfactoryNA"],
               dic["governorsSatisfactoryNA"],
               dic["gasSolenoidValvesSatisfactoryNA"],
               dic["controlBoxPcbSatisfactoryNA"],
               dic["gasketSealsSatisfactoryNA"],
               dic["burnerSatisfactoryNA"],
               dic["burnerJetsSatisfactoryNA"],
               dic["electrodesTransformerSatisfactoryNA"],
               dic["flameFailureDeviceSatisfactoryNA"],
               dic["systemBoilerControlsSatisfactoryNA"],
               dic["boilerCasingSatisfactoryNA"],
               dic["thermalInsulationSatisfactoryNA"],
               dic["combustionFanIdFanSatisfactoryNA"],
               dic["airFluePressureSwitchSatisfactoryNA"],
               dic["controlLimitStatsSatisfactoryNA"],
               dic["pressureTempGaugesSatisfactoryNA"],
               dic["circulationPumpsSatisfactoryNA"],
               dic["condenseTrapSatisfactoryNA"]
            };
            List<bool> componentBooleans = componentBooleans1.Select(s => bool.Parse(s)).ToList();
            List<string> componentComments = new List<string>
            {
               dic["applianceServiceValveSatisfactoryComments"],
               dic["governorsComments"],
               dic["gasSolenoidValvesComments"],
               dic["controlBoxPcbComments"],
               dic["gasketSealsComments"],
               dic["burnerComments"],
               dic["burnerJetsComments"],
               dic["electrodesTransformerComments"],
               dic["flameFailureDeviceComments"],
               dic["systemBoilerControlsComments"],
               dic["boilerCasingComments"],
               dic["thermalInsulationComments"],
               dic["combustionFanIdFanComments"],
               dic["airFluePressureSwitchComments"],
               dic["controlLimitStatsComments"],
               dic["pressureTempGaugesComments"],
               dic["circulationPumpsComments"],
               dic["condenseTrapComments"]
                //Fale jos 2
            };
            for (int i = 0; i < 18; i++)
            {
                if (componentBooleans[i])
                {
                    gfx.DrawString("√", font, XBrushes.Black, new XRect(114, y, 140 - 114, 13), XStringFormats.Center);
                }
                else if (componentBooleans[i + 18])
                {
                    gfx.DrawString("√", font, XBrushes.Black, new XRect(170, y, 197 - 170, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString("√", font, XBrushes.Black, new XRect(142, y, 168 - 142, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //State appliance condition
            gfx.DrawString(dic["stateApplianceCondition"], font, XBrushes.Black, new XRect(497, 187, 576 - 497, 13), XStringFormats.Center);
            y = 307;
            for (int i = 0; i < 18; i++)
            {
                if (i == 8)
                {
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(247, y, 376 - 247, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString(componentComments[i], font, XBrushes.Black, new XRect(198, y, 376 - 198, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //heat exchanger/fluent clear
            ////heat exchanger/fluent clear
            if (dic["heatExhanger"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(505, 53, 15, 9), 0, 360);
            }
            else if (dic["heatExhangerNA"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(554, 53, 15, 9), 0, 360);
            }
            else
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 53, 15, 9), 0, 360);
            }
            //working inlet pressure   //recorded burner pressure    //measured gas rate   
            gfx.DrawString(dic["workingInletPressure"], font, XBrushes.Black, new XRect(496, 66, 557 - 497+2, 12+2), XStringFormats.Center);
            gfx.DrawString(dic["recordedBurnerPressure"], font, XBrushes.Black, new XRect(496, 82, 557 - 497+2, 12+2), XStringFormats.Center);
            gfx.DrawString(dic["measuredGasRate"], font, XBrushes.Black, new XRect(496, 98, 557 - 497+2, 12+2), XStringFormats.Center);
            //flue flow test
            if (dic["flueFlowTest"] == "True")
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 113, 20, 9), 0, 360);
            }
            else if (!(dic["flueFlowTestNA"] == "True"))
            {
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 113, 16, 9), 0, 360);
            }
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 113, 16, 9), 0, 360);
            //spillage test
            if (dic["spillageTest"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 128, 20, 9), 0, 360);
            else if (!(dic["spillageTestNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 128, 16, 9), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 128, 16, 9), 0, 360);
            //AECV plant isolation correct
            if (dic["AECVPlantIsolationCorrect"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 143, 20, 9), 0, 360);
            else if (!(dic["AECVPlantIsolationCorrectNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 143, 16, 9), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 143, 16, 9), 0, 360);
            //safety shut off valve
            if (dic["safetyShutOffValve"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 158, 20, 9), 0, 360);
            else if (!(dic["safetyShutOffValveNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 158, 16, 9), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 158, 16, 9), 0, 360);
            //plantroom gas thightness level
            if (dic["plantroomGasTightnessTest"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 173, 20, 9), 0, 360);
            else if (!(dic["plantroomGasTightnessTestNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(530, 173, 16, 9), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(555, 173, 16, 9), 0, 360);

            y = 53;
            float x = 580;
            //remarks comments page right
            List<string> componentComments1 = new List<string>
            {

               dic["heatExhangerComments"],
               "",//dic["workingInletPressureComments"],
               "",//dic["recordedBurnerPressureComments"],
               "",//dic["measuredGasRateComments"],
               dic["flueFlowTestComments"],
               dic["spillageTestComments"],
               dic["AECVPlantIsolationCorrectComments"],
               dic["safetyShutOffValveComments"],
               dic["plantroomGasTightnessTestComments"],
               "",//dic["stateApplianceConditionComments"],

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

            gfx.DrawString(dic["engineersName"], font, XBrushes.Black, new XRect(x, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(dic["engineersSignature"], font, XBrushes.Black, new XRect(x + 142, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(dic["engineersGasSafeID"], font, XBrushes.Black, new XRect(x + 284, y, 137, 25), XStringFormats.Center);
            y += 45;
            gfx.DrawString(dic["clientsName"], font, XBrushes.Black, new XRect(x, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(dic["clientsSignature"], font, XBrushes.Black, new XRect(x + 142, y, 137, 25), XStringFormats.Center);
            gfx.DrawString(dic["inspectionDate"], font, XBrushes.Black, new XRect(x + 284, y, 137, 25), XStringFormats.Center);


            //comments/defects
            XRect boundingBox = new XRect(393, 287, 414, 166);
            string text = dic["commetsDefects"];
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);
            //unsafe situations warning notice issue number
            gfx.DrawString(dic["warningNoticeIssueNumber"], font, XBrushes.Black, new XRect(610, 459, 200, 9), XStringFormats.Center);

            x = 432;
            y = 218.5;
            //co2 //flue temp  //co2 // flue temp
            //co  //effisency // co // effisency
            //o2 // excess air
            //ratio  //room temp
            List<string> debelamasnakurcinanajvecanasvetu = new List<string>
        {
            dic["HighFireCO2"],
            dic["HighFireCO"],
            dic["HighFireO2"],
            dic["HighFireRatio"],
            dic["HighFireFlueTemp"],
            dic["HighFireEfficiency"],
            dic["HighFireExcessAir"],
            dic["HighFireRoomTemp"],
            dic["LowFireCO2"],
            dic["LowFireCO"],
            dic["LowFireO2"],
            dic["LowFireRatio"],
            dic["LowFireFlueTemp"],
            dic["LowFireEfficiency"],
            dic["LowFireExcessAir"],
            dic["LowFireRoomTemp"],
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
            string filePath = System.IO.Path.Combine(downloadsFolder, dic["reportName"]);
            document.Save(filePath);

            // Save to MemoryStream
            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public static async Task CreateEngineersReport(
//     string clientsName,
//string address,
//string date,
//string engineer,
//string taskTNo,
//bool checkTaskComplete,
//string applianceMake,
//string serialNumber,
//string description,
//bool checkSpillageTestPerformed,
//bool checkSpillageTestPerformedNA,
//bool checkRiskAssesmentCompleted,
//bool checkFlueFlowTest,
//bool checkFlueFlowTestNA,
//string gasOperatinPressure,
//string inletPressure,
//bool checkThightnessTestCarriedOut,
//bool checkThightnessTestCarriedOutNA,
//string thightnessTestCarriedOut,
//string totalHoursIncludingTravel,
//bool checkApplianceSafeToUse,
//bool checkWarningNoticeIssued,
//string warningNoticeNumber
Dictionary<string, string> dic
     )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);


            XImage image = await ConvertToXImage(@"engineers_report.png");

            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["clientsName"], font, XBrushes.Black, new XRect(118, 123, 288 - 118, 143 - 123), XStringFormat.Center);

            gfx.DrawString(dic["address"], font, XBrushes.Black, new XRect(6, 145, 280, 25), XStringFormat.Center);
            gfx.DrawString(" ", font, XBrushes.Black, new XRect(6, 170, 280, 25), XStringFormat.Center);
            gfx.DrawString(" ", font, XBrushes.Black, new XRect(6, 195, 280, 25), XStringFormat.Center);

            gfx.DrawString(dic["applianceMake"], font, XBrushes.Black, new XRect(79, 219, 210, 24), XStringFormat.Center);

            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(340, 125, 588 - 340, 25), XStringFormat.Center);
            gfx.DrawString(dic["engineer"], font, XBrushes.Black, new XRect(340, 145, 588 - 340, 25), XStringFormat.Center);
            gfx.DrawString(dic["taskTNo"], font, XBrushes.Black, new XRect(340, 170, 588 - 340, 25), XStringFormat.Center);


            //gfx.DrawEllipse(XBrushes.Black, new XRect(377, 194, 420 - 377, 25));
            if (dic["checkTaskComplete"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(376, 193, 396 - 377 + 2, 206 - 194 + 2), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(490, 193, 396 - 377 + 2, 206 - 194 + 2), 0, 360);

            gfx.DrawString(dic["serialNumber"], font, XBrushes.Black, new XRect(340, 219, 588 - 340, 25), XStringFormat.Center);

            string[] l = { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
            string[] TempDesc = dic["description"].Split();
            int d = 0;
            int duzina = 0;
            for (int i = 0; i < TempDesc.Length; i++)
            {
                if (duzina + TempDesc[i].Length < 112)
                {
                    l[d] += TempDesc[i] + " ";
                    duzina += TempDesc[i].Length + 1;
                }
                else
                {
                    d++;
                    l[d] += TempDesc[i] + " ";
                    duzina = TempDesc[i].Length + 1;
                }

            }
            int y = 267;
            for (int i = 0; i < 18; i++)
            {
                gfx.DrawString(l[i], font, XBrushes.Black, new XRect(7, y, 588 - 7, 23), XStringFormats.BottomLeft);
                y += 24;
            }
            if (dic["checkSpillageTestPerformed"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(115, 701, 24, 10), 0, 360);
            else if (!(dic["checkSpillageTestPerformedNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(142, 701, 22, 10), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(165, 701, 22, 10), 0, 360);
            if (dic["checkRiskAssesmentCompleted"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(308, 701, 22, 10), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(332, 701, 18, 10), 0, 360);
            if (dic["checkFlueFlowTest"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(72, 725, 25, 11), 0, 360);
            else if (!(dic["checkFlueFlowTestNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(102, 725, 23, 11), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(129, 725, 17, 11), 0, 360);
            if (dic["checkThightnessTestCarriedOut"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(126, 749, 25, 10), 0, 360);
            else if (!(dic["checkThightnessTestCarriedOutNA"] == "True"))
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(155, 749, 23, 10), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(179, 749, 17, 10), 0, 360);
            if (dic["checkApplianceSafeToUse"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(102, 773, 22, 10), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(126, 773, 20, 10), 0, 360);
            if (dic["checkWarningNoticeIssued"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(107, 796, 22, 10), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(131, 796, 17, 10), 0, 360);

            gfx.DrawString(dic["warningNoticeNumber"], font, XBrushes.Black, new XRect(267, 797, 105, 17), XStringFormats.Center);

            gfx.DrawString(dic["gasOperatinPressure"], font, XBrushes.Black, new XRect(190, 735, 47, 8), XStringFormats.Center);

            gfx.DrawString(dic["inletPressure"], font, XBrushes.Black, new XRect(300, 735, 47, 8), XStringFormats.Center);

            gfx.DrawString(dic["totalHoursIncludingTravel"], font, XBrushes.Black, new XRect(514, 748, 74, 16), XStringFormats.Center);



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"Engineers_Report_Sheet_{dateTimeString}.pdf");


            document.Save(filePath);


        }
        
        public static async Task CDM(
            Dictionary<string,string> dic
            )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "CDM Site Form";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"cdm.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["siteAdress"], font, XBrushes.Black, new XRect(93, 127, 562 - 93, 40), XStringFormats.Center);

            gfx.DrawString(dic["clinet"], font, XBrushes.Black, new XRect(64, 171, 229, 15), XStringFormats.CenterLeft);


            gfx.DrawString(dic["responsibleSiteEngineer"], font, XBrushes.Black, new XRect(415, 191, 148, 16), XStringFormats.CenterLeft);

            gfx.DrawString(dic["otherEngineers"], font, XBrushes.Black, new XRect(233, 212, 330, 16), XStringFormats.CenterLeft);

            gfx.DrawString(dic["whatInformationIssued"], font, XBrushes.Black, new XRect(204, 234, 360, 16), XStringFormats.CenterLeft);

            gfx.DrawString(dic["startDate"], font, XBrushes.Black, new XRect(83, 254, 123, 16), XStringFormats.Center);
            gfx.DrawString(dic["completionDate"], font, XBrushes.Black, new XRect(290, 254, 95, 16), XStringFormats.Center);
            gfx.DrawString(dic["other"], font, XBrushes.Black, new XRect(421, 254, 142, 16), XStringFormats.Center);
            if(dic["checkWelfareFacilitiesYes"]=="True")
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(332, 279, 17, 8), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(385, 279, 16, 8), 0, 360);
            if (dic["checkPortableWelfareFacilitiesYes"]=="True")
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(332, 292, 17, 8), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(385, 292, 16, 8), 0, 360);
            //Pocetak dugog niza
            if (dic["checkWorkingAtHeightYes"] =="True")
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 332, 17, 8), 0, 360);
            else
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 332, 15, 8), 0, 360);
            if (dic["checkPermitsToWorkRequiredYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 352, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 352, 15, 8), 0, 360);
            if (dic["checkExcavationsYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 373, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 373, 15, 8), 0, 360);
            if (dic["checkDustYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 407, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 407, 15, 8), 0, 360);
            if (dic["checkNoiseYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 420, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 420, 15, 8), 0, 360);
            if (dic["checkCOSHHYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 433, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 433, 15, 8), 0, 360);
            if (dic["checkOtherYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 446, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 446, 15, 8), 0, 360);
            if (dic["checkManagementSurveyYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 478, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 478, 15, 8), 0, 360);
            if (dic["checkFiveYearsSurveyYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 491, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 491, 15, 8), 0, 360);
            if (dic["checkElectricalYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 524, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 524, 15, 8), 0, 360);
            if (dic["checkGasYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 537, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 537, 15, 8), 0, 360);
            if (dic["checkWaterYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 550, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 550, 15, 8), 0, 360);
            if (dic["checkOtherServicesYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 563, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 563, 15, 8), 0, 360);
            if (dic["checkDangerToOthersYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 583, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 583, 15, 8), 0, 360);
            if (dic["checkDangerToPublicYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 601, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 601, 15, 8), 0, 360);
            if (dic["checkOtherDangersYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 619, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 619, 15, 8), 0, 360);
            if (dic["checkHotWorksYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 641, 17, 8), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 641, 15, 8), 0, 360);
            if (dic["checkAppointedFirstAiderYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 667, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 667, 15, 8), 0, 360);
            if (dic["checkAdditionalActionsYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 712, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 712, 15, 8), 0, 360);
            if (dic["checkIsItSafeYes"] == "True")
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(273, 732, 17, 8), 0, 360);
            else
                gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(300, 732, 15, 8), 0, 360);

            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(418, 786, 59, 7), XStringFormats.CenterLeft);



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"CDM_Site_Form{dateTimeString}.pdf");
        }
        public static async Task<PdfDocument> PressurisationReport(
          //string siteNameAndAddress,
          //string totalHeatingSystemRating,
          //string numberOfBoilers,
          //string flowTemperature,
          //string returnTemperature,
          //string currentWorkingPressure,
          //string safetyValveSetting,
          //string unitModel,
          //string serialNo,
          //string expansionVesselSize,
          //string numberOfPressureVessels,
          //string setFillPressure,
          //string ratedExpansionVesselCharge,
          //string highPressureSwitchSetting,
          //string lowPressureSwitchSetting,
          //string finalSystemPressure,
          //bool checkMainWaterSupply,
          //bool checkColdFillPressureSet,
          //bool checkElectricalSupplyWorking,
          //bool checkFillingLoopDisconnected,
          //bool checkUnitLeftOperational,
          //string notes,
          //string date,
          //string engineer
          Dictionary<string, string> dic

          )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Pressurisation Unit Service Report";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"pressurisation_unit_report.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            XRect boundingBox = new XRect(35, 222, 561 - 35, 259 - 222);
            string text = dic["siteNameAndAddress"];
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 10), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);

            //gfx.DrawString(totalHeatingSystemRating, font, XBrushes.Black, new XRect(298, 308, 543 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(numberOfBoilers, font, XBrushes.Black, new XRect(298, 323, 566 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(flowTemperature, font, XBrushes.Black, new XRect(298, 339, 548 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(returnTemperature, font, XBrushes.Black, new XRect(298, 354, 548 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(currentWorkingPressure, font, XBrushes.Black, new XRect(298, 369, 543 - 298, 14), XStringFormat.Center);
            //gfx.DrawString(safetyValveSetting, font, XBrushes.Black, new XRect(298, 385, 543 - 298, 14), XStringFormat.Center);

            gfx.DrawString(dic["totalHeatingSystemRating"], font, XBrushes.Black, new XRect(298, 308, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(dic["numberOfBoilers"], font, XBrushes.Black, new XRect(298, 323, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(dic["flowTemperature"], font, XBrushes.Black, new XRect(298, 339, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(dic["returnTemperature"], font, XBrushes.Black, new XRect(298, 354, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(dic["currentWorkingPressure"], font, XBrushes.Black, new XRect(298, 369, 566 - 298, 14), XStringFormat.Center);
            gfx.DrawString(dic["safetyValveSetting"], font, XBrushes.Black, new XRect(298, 385, 566 - 298, 14), XStringFormat.Center);

            gfx.DrawString(dic["unitModel"], font, XBrushes.Black, new XRect(99, 444, 296 - 99, 25), XStringFormat.Center);
            gfx.DrawString(dic["serialNo"], font, XBrushes.Black, new XRect(359, 444, 565 - 359, 25), XStringFormat.Center);
            gfx.DrawString(dic["expansionVesselSize"], font, XBrushes.Black, new XRect(183, 471, 296 - 183, 25), XStringFormat.Center);
            gfx.DrawString(dic["numberOfPressureVessels"], font, XBrushes.Black, new XRect(447, 471, 565 - 447, 25), XStringFormat.Center);

            List<string> PressurationUnitSetings = new List<string>
            {

                dic["setFillPressure"],
                dic["ratedExpansionVesselCharge"],
                dic["highPressureSwitchSetting"],
                dic["lowPressureSwitchSetting"],
                dic["finalSystemPressure"],

            };
            //za push
            double y = 542;
            for (int i = 0; i < 5; i++)
            {
                gfx.DrawString(PressurationUnitSetings[i], font, XBrushes.Black, new XRect(227, y, 268 - 227, 13), XStringFormat.Center);
                y += 15;
            }
            List<string> check1 = new List<string>
            {
                dic["checkMainWaterSupply"],
                dic["checkColdFillPressureSet"],
                dic["checkElectricalSupplyWorking"],
                dic["checkFillingLoopDisconnected"],
                dic["checkUnitLeftOperational"],
            };
            List<bool> check = check1.Select(s => bool.Parse(s)).ToList();
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
            string text1 = dic["notes"];
            XTextFormatter tf1 = new XTextFormatter(gfx);
            XRect layoutRectangle1 = boundingBox1;
            tf1.DrawString(text1, new XFont("Arial", 10), XBrushes.Black, layoutRectangle1, XStringFormats.TopLeft);

            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(245, 777, 378 - 245, 25), XStringFormat.Center);
            gfx.DrawString(dic["engineer"], font, XBrushes.Black, new XRect(435, 777, 565 - 435, 25), XStringFormat.Center);

            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(downloadsFolder, $"Pressurisation_Unit_Service_Report_{dateTimeString}.pdf");

            document.Save(filePath);

            return document;

        }
       
    }

}