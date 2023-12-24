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

                // Use this constructor to make the internal buffer publicly visible
                MemoryStream imageStream = new MemoryStream(imageBytes, 0, imageBytes.Length, true, true);
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
        public static async Task<byte[]> GasRisk(Dictionary<string, string> dic)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Check";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 8);

            XImage image = await ConvertToXImage(@"gas.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["nameAndSiteAdress"], font, XBrushes.Black, new XRect(136, 127, 280, 4), XStringFormats.Center);//name and site adress
            gfx.DrawString(dic["client"], font, XBrushes.Black, new XRect(445, 127, 115, 4), XStringFormats.Center);//client
            gfx.DrawString(dic["meterLocation"], font, XBrushes.Black, new XRect(99, 171, 270, 4), XStringFormats.Center);//meter location
            gfx.DrawString(dic["commentsOnOverallMeter"], font, XBrushes.Black, new XRect(183, 188, 374, 4), XStringFormats.Center);//coments on overall mater condition

            if (dic["checkInternalMeter"] == "True")
            {
                gfx.DrawString("Internal", font, XBrushes.Black, new XRect(478, 171, 77, 4), XStringFormats.Center);//eternal or external matter
            }
            else
            {
                gfx.DrawString("External", font, XBrushes.Black, new XRect(478, 171, 77, 4), XStringFormats.Center);//eternal or external matter
            }


            gfx.DrawString(dic["pipeworkLocation"], font, XBrushes.Black, new XRect(111, 335, 444, 4), XStringFormats.Center);//pipeline
            gfx.DrawString(dic["commentsOnOverallPipework"], font, XBrushes.Black, new XRect(195, 351, 360, 4), XStringFormats.Center);//overall pipeline
            gfx.DrawString(dic["reasonForWarningNotice"], font, XBrushes.Black, new XRect(370, 614, 185, 4), XStringFormats.Center);//warning noter
            gfx.DrawString(dic["warningNoticeRefNo"], font, XBrushes.Black, new XRect(357, 629, 197, 4), XStringFormats.Center);//warnig noter reff number
            gfx.DrawString(dic["dateOfLastTightnessTest"], font, XBrushes.Black, new XRect(370, 644, 185, 4), XStringFormats.Center);//date last tightness
            gfx.DrawString(dic["recordTightnessTestResult"], font, XBrushes.Black, new XRect(373, 660, 68, 4), XStringFormats.Center);//tightness true result
            gfx.DrawString(dic["dropRecorded"], font, XBrushes.Black, new XRect(506, 660, 49, 4), XStringFormats.Center);//drop recorded

            XRect boundingBox = new XRect(273, 391, 277, 209);
            string text = dic["pipeworkComments"];
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);//coments 1

            boundingBox = new XRect(273, 226, 277, 83);
            text = dic["meterComments"];
            layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);//coments 2


            double x = 185;
            double y = 221;
            if (dic["checkGeneralMeterConditionYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkGeneralMeterConditionNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;


            if (dic["checkEarthBondingYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkEarthBondingNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;

            if (dic["ccheckEmergencyControlsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkEmergencyControlsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;


            if (dic["checkMeterVentilationYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkMeterVentilationNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;


            if (dic["checkGasLineDiagramYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkGasLineDiagramNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;


            if (dic["checkEmergencyContractNumberYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkEmergencyContractNumberNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.6;


            if (dic["checkNoticesAndLabelsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkNoticesAndLabelsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }

            x = 185;
            y = 384;
            if (dic["checkPipeworkIdentifiedYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkIdentifiedNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeworkBuriedYesYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkBuriedYesNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeworkSurfaceYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkSurfaceNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeworkEarthBondingYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkEarthBondingNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkJointingMethodsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkJointingMethodsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeworkSupportsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkSupportsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkFixingsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkFixingsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkSupportSepparationDistancesYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkSupportSepparationDistancesNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeworkInVoidsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeworkInVoidsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeSleevesYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeSleevesNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPipeSleevesSealedYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPipeSleevesSealedNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkServiceValvesYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkServiceValvesNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkAdditionalEmergencyControlValvesYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkAdditionalEmergencyControlValvesNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkIsolationValveYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkIsolationValveNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkTestPointYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkTestPointNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkPurgePointsYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkPurgePointsNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }
            y += 13.2;


            if (dic["checkGeneralPipeworkConditionYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 13), XStringFormats.Center);
            }
            else if (dic["checkGeneralPipeworkConditionNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 13), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 13), XStringFormats.Center);
            }



            y = 610;
            if (dic["checkinstallationSafeToOperateYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 14), XStringFormats.Center);
            }
            else if (dic["checkinstallationSafeToOperateNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 14), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 14), XStringFormats.Center);
            }
            y += 15;


            if (dic["checkWarningNoticeIssuedYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 14), XStringFormats.Center);
            }
            else if (dic["checkWarningNoticeIssuedNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 14), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 14), XStringFormats.Center);
            }


            y = 640.5;
            if (dic["checkGasTightnessTestRecommendedYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 14), XStringFormats.Center);
            }
            else if (dic["checkGasTightnessTestRecommendedNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 14), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 14), XStringFormats.Center);
            }
            y += 15;
            if (dic["checkGuessTightnessTestCarriedOutYes"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x, y, 26, 14), XStringFormats.Center);
            }
            else if (dic["checkGuessTightnessTestCarriedOutNo"] == "True")
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 27, y, 26, 14), XStringFormats.Center);
            }
            else
            {
                gfx.DrawString("√", font, XBrushes.Black, new XRect(x + 54, y, 26, 14), XStringFormats.Center);
            }

            gfx.DrawString(dic["engineersName"], font, XBrushes.Black, new XRect(36, 724, 200, 22), XStringFormats.Center);//engeniers name
            gfx.DrawString(dic["clientsName"], font, XBrushes.Black, new XRect(36, 762, 200, 22), XStringFormats.Center);//clients name
            gfx.DrawString(dic["gasSafeOperativeIdNo"], font, XBrushes.Black, new XRect(376, 724, 182, 22), XStringFormats.Center);//gas safe operative
            gfx.DrawString(dic["completionDate"], font, XBrushes.Black, new XRect(376, 762, 182, 22), XStringFormats.Center);//completion date
            gfx.DrawString(dic["engineersSignature"], font, XBrushes.Black, new XRect(240, 724, 133, 22), XStringFormats.Center);//engineers signature
            gfx.DrawString(dic["clientsSignature"], font, XBrushes.Black, new XRect(240, 762, 133, 22), XStringFormats.Center);//clients signature



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "output.pdf");

            document.Save(filePath);

            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public static async Task<byte[]> CheckPage(Dictionary<string,string> dic)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Check";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"check_page.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["WarningNoticeRefNo"], font, XBrushes.Black, new XRect(467, 50, 90, 21.5), XStringFormats.Center);//warning notice no
            gfx.DrawString(dic["SheetNo"], font, XBrushes.Black, new XRect(510, 29, 46, 16.5), XStringFormats.Center);//sheet number
            if (dic["checkRemedialToWorkRequiredYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(490.5, 77, 15, 15));//remidial work
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(519.5, 77, 15, 15));//remidial work
            if (dic["checkTestsCompletedSatisfactoryYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(490.5, 100, 15, 15));//test complited
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(519.5, 100, 15, 15));//test complited
            double y = 329.5;
            List<string> prviFor = new List<string>()
            {
                dic["checkFluesFittedYes"],
                dic["checkFluesFittedNo"],
                dic["checkFluesSupportedYes"],
                dic["checkFluesSupportedNo"],
                dic["checkFluesInLineYes"],
                dic["checkFluesInLineNo"],
                dic["checkFacilitiesYes"],
                dic["checkFacilitiesNo"],
                dic["checkFlueGradientsYes"],
                dic["checkFlueGradientsNo"],
                dic["checkFluesInspectionYes"],
                dic["checkFluesInspectionNo"],
                dic["checkFlueJointsYes"],
                dic["checkFlueJointsNo"],
              


            };
            for (int i = 0; i < 7; i+=2)//prvi for
            {
                if (prviFor[i]=="True")
                gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(435.5, y, 21, 10));
                else if(prviFor[i+1]=="True")
                gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(475.5, y, 21, 10));
                else
                gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(513.5, y, 21, 10));
                y += 14.5;
            }
            y = 505;
            List<string> drugiFor = new List<string>()
            {
                dic["checkInterlocksProvidedYes"],
                dic["checkInterlocksProvidedNo"],
                dic["checkEmergencyShutOffButtonYes"],
                dic["checkEmergencyShutOffButtonNo"],
                dic["checkPlantInterlinkYes"],
                dic["checkPlantInterlinkNo"],
                dic["checkFuelShutOffYes"],
                dic["checkFuelShutOffNo"],
                dic["checkFuelFirstEntryYes"],
                dic["checkFuelFirstEntryNo"],
                dic["checkSystemStopYes"],
                dic["checkSystemStopNo"],
                dic["checkTestAndResetYes"],
                dic["checkTestAndResetNo"],
            

            };
            for (int i = 0; i < 8; i+=2)//drugi for
            {
                if (i == 2)
                {

                }
                else
                {
                    if (drugiFor[i]=="True")
                    gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(435.5, y, 21, 10));
                    else if (drugiFor[i+1]=="True")
                    gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(475.5, y, 21, 10));
                    else
                    gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(514, y, 21, 10));
                }
                y += 15.3;
            }
            double x = 325;
            y = 283;
            List<string> jedan = new List<string>()
            {
                dic["checkA_ID1"],
                dic["checkB_AR1"],
                dic["checkC_NCS1"],
            };
            for (int i = 0; i < 3; i++)
            {
                if (jedan[i]=="True")
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(x, y, 24, 19), XStringFormats.Center);
                // gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y), new XPoint(x + 24, y + 19));
                // gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y + 19), new XPoint(x + 23, y));
                x += 104.5;
            }
            x = 325;
            y = y + 176;
            List<string> dva = new List<string>()
            {
                dic["checkA_ID2"],
                dic["checkB_AR2"],
                dic["checkC_NCS2"],
            };
            for (int i = 0; i < 3; i++)
            {
                if (dva[i]=="True")
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(x, y, 24, 19), XStringFormats.Center);
              //  gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y), new XPoint(x + 24, y + 19));
              //  gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y + 19), new XPoint(x + 23, y));
                x += 104.5;
            }
            x = 325;
            y = y + 176 + 22;
            List<string> tri = new List<string>()
            {
                dic["checkA_ID3"],
                dic["checkB_AR3"],
                dic["checkC_NCS3"],
            };
            for (int i = 0; i < 3; i++)
            {
                if (tri[i]=="True")
                gfx.DrawString("\u221A", font, XBrushes.Black, new XRect(x, y, 24, 19),XStringFormats.Center);
               // gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y), new XPoint(x + 24, y + 19));
               // gfx.DrawLine(new XPen(XColor.FromArgb(0, 0, 0)), new XPoint(x, y + 19), new XPoint(x + 23, y));
                x += 104.5;
            }
            XRect boundingBox = new XRect(80, 431, 475, 26);
            string text = dic["nameAndAddressOfPremises"];
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);
            text = dic["location"];
            XRect boundingBox1 = new XRect(80, 255, 475, 26);
            XRect layoutRectangle1 = boundingBox1;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle1, XStringFormats.TopLeft);
            text = dic["ventilationCalculations"];
            XRect boundingBox2 = new XRect(80, 629, 475, 26);
            XRect layoutRectangle2 = boundingBox2;
            tf.DrawString(text, new XFont("Arial", 8), XBrushes.Black, layoutRectangle2, XStringFormats.TopLeft);

            x = 38;
            y = 750;

            gfx.DrawString(dic["engineersName"], font, XBrushes.Black, new XRect(x, y, 200, 13), XStringFormats.Center);
            gfx.DrawRectangle(XBrushes.White, new XRect(x + 378, y + 1, 140, 11));
          //  gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(x + 203, y, 171, 13), XStringFormats.Center);
            gfx.DrawString("Ashwell Maintenance Ltd", font, XBrushes.Black, new XRect(x + 377, y, 142, 13), XStringFormats.Center);
            y += 25;
            gfx.DrawRectangle(XBrushes.White, new XRect(x + 1, y + 1, 198, 11));
            gfx.DrawString(dic["companyGasSafeRegistrationNo"], font, XBrushes.Black, new XRect(x, y, 200, 13), XStringFormats.Center);
            gfx.DrawString(dic["engineersGasSafeIDNo"], font, XBrushes.Black, new XRect(x + 203, y, 171, 13), XStringFormats.Center);
            gfx.DrawString(dic["inspectionDate"], font, XBrushes.Black, new XRect(x + 377, y, 142, 13), XStringFormats.Center);
            y += 25;
            gfx.DrawRectangle(XBrushes.White, new XRect(x - 1, y - 1, 198, 11));
            gfx.DrawString(dic["clientsName"], font, XBrushes.Black, new XRect(x, y, 200, 13), XStringFormats.Center);
           // gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(x + 203, y, 171, 13), XStringFormats.Center);
            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(x + 377, y, 142, 13), XStringFormats.Center);
            if (dic["checkSystemDosingFacilitiesYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(488, 690, 21, 10));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(516, 690, 21, 10));
            if (dic["checkVentilationAtTheCorrectHeightYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 226, 21, 10));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(529, 226, 21, 10));
            if (dic["checkVentilationArrangementsYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(502, 240, 21, 10));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(529, 240, 21, 10));
            if (dic["checkVentilationCorrectlySizedYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(235, 226, 21, 10));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(260, 226, 21, 10));

            XRect boundingBox3 = new XRect(417, 534, 138, 13);
            XRect layoutRectangle3 = boundingBox3;
            text = dic["ventilationChecksComments"];
            tf.DrawString(text, new XFont("Arial", 5), XBrushes.Black, layoutRectangle3, XStringFormats.TopLeft);

            boundingBox3 = new XRect(150, 74, 212, 20);
            layoutRectangle3 = boundingBox3;
            text = dic["flueChecksComments"];
            tf.DrawString(text, new XFont("Arial", 5), XBrushes.Black, layoutRectangle3, XStringFormats.TopLeft);

            boundingBox3 = new XRect(74, 98, 288, 18);
            layoutRectangle3 = boundingBox3;
            text = dic["emergencyStopButtonComment"];
            tf.DrawString(text, new XFont("Arial", 5), XBrushes.Black, layoutRectangle3, XStringFormats.TopLeft);

            boundingBox3 = new XRect(125, 147, 430, 36);
            layoutRectangle3 = boundingBox3;
            text = dic["safetyInterlocksComments"];
            tf.DrawString(text, new XFont("Arial", 5), XBrushes.Black, layoutRectangle3, XStringFormats.TopLeft);
            //checkExistingHighLevelCM,checkExistingLowLevelCM,checkRequiredHighLevelCM,checkRequiredLowLevelCM
            gfx.DrawString(dic["existingHighLevel"], font, XBrushes.Black, new XRect(112, 188, 90, 13), XStringFormats.Center);
            gfx.DrawString(dic["requiredHighLevel"], font, XBrushes.Black, new XRect(112, 207, 90, 13), XStringFormats.Center);
            gfx.DrawString(dic["existingLowLevel"], font, XBrushes.Black, new XRect(367, 188, 90, 13), XStringFormats.Center);
            gfx.DrawString(dic["requiredLowLevel"], font, XBrushes.Black, new XRect(367, 207, 90, 13), XStringFormats.Center);

            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "ConformityCheckPage.pdf");

            document.Save(filePath);

            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public static async Task<byte[]> Boiler(Dictionary<string,string> dic)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "And i wonder";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Arial", 20);



            XImage image = await ConvertToXImage(@"boiler.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

           
            if (dic["checkRemedialToWorkRequiredYes"] == "True")
            {
                gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(488.5, 82, 15, 15));
            }
            else
            {
                gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(518.7, 82, 15, 15));
            }
            if (dic["checkTestsCompletedSatisfactoryYes"] == "True")
            {
                gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(488.5, 114.5, 15, 15));
            }
            else
            {
                gfx.DrawEllipse(new XPen(XColor.FromArgb(30, 50, 200)), new XRect(518.7, 114.5, 15, 15));
            }
            gfx.DrawString(dic["SheetNo"], new XFont("Arial", 11), XBrushes.Black, new XRect(506, 27, 64, 26), XStringFormats.Center);
            gfx.DrawString(dic["nameOfPremises"], new XFont("Arial", 8), XBrushes.Black, new XPoint(100, 81.8));
            gfx.DrawString(dic["adressOfPremises"], new XFont("Arial", 8), XBrushes.Black, new XPoint(107, 113.8));
            gfx.DrawString(dic["WarningNoticeRefNo"], new XFont("Arial", 8), XBrushes.Black, new XRect(439, 53, 131, 20), XStringFormats.Center);
            gfx.DrawString(dic["appliancesCoveredByThisCheck"], new XFont("Arial", 8), XBrushes.Black, new XPoint(162.75, 158.92));
            // add vert ventilationLocation
            double x = 193;
            double y = 249;
            List<string> list = new List<string>()
            {
                dic["checkPipeworkToGasMeterYes"],
                dic["checkRegulatorAndOrMeterYes"],
                dic["checkSafetyNoticesLabelsYes"],
                dic["checkLineDiagramYes"],
                dic["checkColorCodingIndicationTapeYes"],
                dic["checkMeterHouseVentilationYes"],

            };
            List<string> lista = new List<string>()
            {
                dic["checkPipeworkToGasMeterNA"],
                dic["checkRegulatorAndOrMeterNA"],
                dic["checkSafetyNoticesLabelsNA"],
                dic["checkLineDiagramNA"],
                dic["checkColorCodingIndicationTapeNA"],
                dic["checkMeterHouseVentilationNA"],

            };
            for (int i = 0; i < 6; i++)
            {
                if (list[i]=="True")
                gfx.DrawString("\u221A", new XFont("Arial", 15), XBrushes.Black, new XRect(x, y, 34, 17), XStringFormats.Center);
                else if (lista[i]=="True")
                gfx.DrawString("\u221A", new XFont("Arial", 15), XBrushes.Black, new XRect(x + 34.5, y, 34, 17), XStringFormats.Center);
                else
                gfx.DrawString("\u221A", new XFont("Arial", 15), XBrushes.Black, new XRect(x + 69, y, 34, 17), XStringFormats.Center);

                y += 18.5;
            }
            x = 193;
            y = 465;
            List<string> list1 = new List<string>()
            {
                dic["checkMainFlueYes"],
                dic["checkChimneyFlueTerminalPositionYes"],
                dic["checkStubFluersToBoildersYes"],
                dic["checkIdFanYes"],
                dic["checkFanBoilerSafetyInterlockYes"],
                dic["checkGeneralComplianceOfGasPipeYes"],
                dic["checkVentilationYes"],

            };
            List<string> lista1 = new List<string>()
            {
                dic["checkMainFlueNA"],
                dic["checkChimneyFlueTerminalPositionNA"],
                dic["checkStubFluersToBoildersNA"],
                dic["checkIdFanNA"],
                dic["checkFanBoilerSafetyInterlockNA"],
                dic["checkGeneralComplianceOfGasPipeNA"],
                dic["checkVentilationNA"],

            };
            for (int i = 0; i < 7; i++)
            {
                if (list1[i]=="True")
                gfx.DrawString("√", new XFont("Arial", 15), XBrushes.Black, new XRect(x, y, 34, 17), XStringFormats.Center);
                else if (lista1[i]=="True")
                gfx.DrawString("√", new XFont("Arial", 15), XBrushes.Black, new XRect(x + 34.5, y, 34, 17), XStringFormats.Center);
                else 
                gfx.DrawString("√", new XFont("Arial", 15), XBrushes.Black, new XRect(x + 69, y, 34, 17), XStringFormats.Center);

                y += 18.5;
            }
            //checkFreeAirExistingCM,checkFreeAirExistingMH,checkFreeAirRequiredCM,checkcheckFreeAirRequiredMH,

            gfx.DrawString(dic["meterHouseLocation"], new XFont("Arial", 8), XBrushes.Black, new XPoint(232.20, 218.5));
            gfx.DrawString(dic["freeAirExistingHighLevel"], new XFont("Arial", 8), XBrushes.Black, new XRect(298, 393, 100, 17), XStringFormats.Center);
            gfx.DrawString(dic["freeAirExistingLowLevel"], new XFont("Arial", 8), XBrushes.Black, new XRect(464, 393, 100, 17), XStringFormats.Center);
            gfx.DrawString(dic["freeAirRequiredHighLevel"], new XFont("Arial", 8), XBrushes.Black, new XRect(298, 410, 100, 17), XStringFormats.Center);
            gfx.DrawString(dic["freeAirRequiredLowLevel"], new XFont("Arial", 8), XBrushes.Black, new XRect(464, 410, 100, 17), XStringFormats.Center);
          
            gfx.DrawRectangle(XBrushes.White, new XRect(133, 394, 92, 15));
            gfx.DrawString("1000  cm2   or", new XFont("Arial", 8), XBrushes.Black, new XPoint(134.7, 403));
            gfx.DrawString("1000  m3/h", new XFont("Arial", 8), XBrushes.Black, new XPoint(185, 403));
          
            gfx.DrawRectangle(XBrushes.White, new XRect(133, 413, 92, 15));
            gfx.DrawString("1000  cm2   or", new XFont("Arial", 8), XBrushes.Black, new XPoint(134.7, 422));
            gfx.DrawString("1000  m3/h", new XFont("Arial", 8), XBrushes.Black, new XPoint(185, 422));
           
            gfx.DrawString(dic["inletWorkingPressureTestFullLoad"], new XFont("Arial", 8), XBrushes.Black, new XRect(194, 614, 102, 17), XStringFormats.Center);
            gfx.DrawString(dic["standingPressure"], new XFont("Arial", 8), XBrushes.Black, new XRect(194, 632, 102, 17), XStringFormats.Center);
            gfx.DrawString(dic["inletWorkingPressureTestPartLoad"], new XFont("Arial", 8), XBrushes.Black, new XRect(464, 614, 102, 17), XStringFormats.Center);
            gfx.DrawString(dic["plantGasInstallationVolume"], new XFont("Arial", 8), XBrushes.Black, new XRect(464, 632, 102, 17), XStringFormats.Center);
            if (dic["checkAIVYes"] =="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(312.5, 596.5, 15, 15));
            else if (dic["checkAIVNo"] =="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(341, 596.5, 15, 15));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(368.7, 596.5, 15, 15));

            if (dic["checkManualYes"] =="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(481.1, 596.5, 15, 15));
            else if (dic["checkManualNo"] =="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(509.5, 596.5, 15, 15));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(538, 596.5, 15, 15));

            if (dic["checkPlantGasTightnessTestYes"]=="True")
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(487.5, 654, 21, 10));
            else
            gfx.DrawEllipse(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(527, 654, 21, 10));

            y = 712;
            List<string> finalno = new List<string>()
            {
                dic["engineersName"]," ",
                dic["contractor"],
                dic["companyGasSafeRegistrationNo"],
                dic["engineersGasSafeIDNo"],
                dic["inspectionDate"],
                dic["clientsName"]," ",
                dic["date"],
            };
            for (int i = 0; i < 3; i++)
            {
                gfx.DrawRectangle(XBrushes.White, new XRect(27, y + 2, 198, 24));
                gfx.DrawRectangle(XBrushes.White, new XRect(230, y + 2, 168, 24));
                gfx.DrawRectangle(XBrushes.White, new XRect(402, y + 2, 168, 24));


                gfx.DrawString(finalno[i], new XFont("Arial", 11), XBrushes.Black, new XRect(26, y, 200, 25), XStringFormats.Center);
                gfx.DrawString(finalno[i+1], new XFont("Arial", 11), XBrushes.Black, new XRect(228, y, 170, 25), XStringFormats.Center);
                gfx.DrawString(finalno[i+2], new XFont("Arial", 11), XBrushes.Black, new XRect(400, y, 170, 25), XStringFormats.Center);
                y += 36;
            }

            //Coments


            XRect boundingBox = new XRect(300, 465, 269, 127);
            string text = dic["meterHouseComment"];
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Arial", 11), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);

            XRect boundingBox2 = new XRect(300, 250, 269, 107);
            XRect layoutRectangle2 = boundingBox2;
            text = dic["boilerHousePlantRoomComments"];
            tf.DrawString(text, new XFont("Arial", 11), XBrushes.Black, layoutRectangle2, XStringFormats.TopLeft);

            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "output.pdf");

            document.Save(filePath);
            // Save to MemoryStream
            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        public static async Task<byte[]> _1Up(Dictionary<string, string> dic)
        {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "OnePage";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"ige_up_1.jpg");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["site"], font, XBrushes.Black, new XRect(86, 125, 483, 14), XStringFormats.Center);
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

            List<string> prviFor = new List<string>
            {
                dic["steel1"],
                dic["steel2"],
                dic["steel3"],
                dic["steel4"],
                dic["steel5"],
                dic["steel6"],
                dic["steel7"],
                dic["steel8"],
                dic["steel9"],
                dic["steel10"],
                dic["steel11"],
                dic["steel12"],
                dic["steel13"],
                dic["copper1"],
                dic["copper2"],
                dic["copper3"],
                dic["copper4"],
                dic["copper5"],
                dic["copper6"],
                dic["copper7"],
                dic["presdr1"],
                dic["presdr2"],
                dic["presdr3"],
                dic["presdr4"],
                dic["presdr5"],
                dic["presdr6"],
                dic["presdr7"],
                dic["presdr8"],
            };
            List<string> drugiFor = new List<string>
            {
                dic["steel1Total"],
                dic["steel2Total"],
                dic["steel3Total"],
                dic["steel4Total"],
                dic["steel5Total"],
                dic["steel6Total"],
                dic["steel7Total"],
                dic["steel8Total"],
                dic["steel9Total"],
                dic["steel10Total"],
                dic["steel11Total"],
                dic["steel12Total"],
                dic["steel13Total"],
                dic["copper1Total"],
                dic["copper2Total"],
                dic["copper3Total"],
                dic["copper4Total"],
                dic["copper5Total"],
                dic["copper6Total"],
                dic["copper7Total"],
                dic["presdr1Total"],
                dic["presdr2Total"],
                dic["presdr3Total"],
                dic["presdr4Total"],
                dic["presdr5Total"],
                dic["presdr6Total"],
                dic["presdr7Total"],
                dic["presdr8Total"],
                dic["totalPipeworkVolume"],
                dic["pipeworkFittingsIV"],
                dic["meterVolume"],
                dic["totalVolumeForTesting"],
                dic["testMediumPicker"],
                dic["testMediumFactor"], // test medium factor
                dic["installationPicker"],
                dic["checkIsWeatherTemperatureStableYes"],
                dic["checkMeterBypassYes"],
                dic["testGaugeUsed"],
                dic["gaugeReadableMovement"], // gauge readable movement
                dic["tightnessTestPressure"],
                dic["maximumPermittedLeakRate"], //max permited leak rate
                dic["checkBarometricPressureCorrectionYes"], // barometric pressure 
            };

           

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
                gfx.DrawString(prviFor[ouchL1++], font, XBrushes.Black, new XRect(x, y, 53, 11), XStringFormats.Center);
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
                gfx.DrawString(drugiFor[ouchL1++], font, XBrushes.Black, new XRect(x, y, 65, 11), XStringFormats.Center);
                y += 11.95;
            }
            x = 508;
            y = 412;
            List<string> StrenghtTesting = new List<string>
            {
                dic["strenghtTestPressure"],
                dic["componentsNotSuitable"],
                dic["stabilisationPeriod"],
                dic["strenghtTestDuration"],
                dic["permittedPressureDrop"],
                dic["actualPressureDrop"],
                dic["testPassedOrFailed"]
            };
            for (int i = 0; i < 7; i++)
            {
                gfx.DrawRectangle(XBrushes.White, new XRect(x, y + 1, 60, 5));
                gfx.DrawString(StrenghtTesting[i], font, XBrushes.Black, new XRect(x, y, 65, 10), XStringFormats.Center);
                y += 11.85;
            }

            if (dic["checkAreaA"] == "True")
                gfx.DrawString(dic["AreaA_Value"], font, XBrushes.Black, new XRect(510, 520, 60, 30), XStringFormats.Center);
            else if (dic["checkAreaB"] == "True")
            {
                gfx.DrawString(dic["AreaB_Value"], font, XBrushes.Black, new XRect(510, 555, 60, 30), XStringFormats.Center);
                gfx.DrawString(dic["roomVolume"], font, XBrushes.Black, new XRect(510, 590, 60, 10), XStringFormats.Center);
            }
            else
                gfx.DrawString(dic["AreaCD_Value"], font, XBrushes.Black, new XRect(510, 603, 60, 30), XStringFormats.Center);
           
            
            
            
            gfx.DrawString(dic["letByDuration"], font, XBrushes.Black, new XRect(510, 638, 60, 20), XStringFormats.Center);
            gfx.DrawString(dic["stabilisationDuration"], font, XBrushes.Black, new XRect(510, 658, 60, 20), XStringFormats.Center);
            gfx.DrawString(dic["testDuration"], font, XBrushes.Black, new XRect(510, 680, 60, 20), XStringFormats.Center);

            gfx.DrawString(dic["actualPressureDropResult"], font, XBrushes.Black, new XRect(510, 730, 60, 10), XStringFormats.Center);
            gfx.DrawString(dic["actualLeakRateResult"], font, XBrushes.Black, new XRect(510, 741, 60, 10), XStringFormats.Center);
            gfx.DrawString(dic["checkAreasWithInadequateVentilationYes"], font, XBrushes.Black, new XRect(510, 752, 60, 10), XStringFormats.Center);

            gfx.DrawString(dic["testPassedOrFailed"], font, XBrushes.Red, new XRect(322, 790, 571 - 322, 811 - 790), XStringFormats.Center);



            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "IGE_UP_1 Sheet.pdf");

            document.Save(filePath);

            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();

        }
        public static async Task<byte[]> _1A(Dictionary<string,string> dic)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "OneApage";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"one_a_sheet.png");
            gfx.DrawImage(image, 0, 0, 595, 842);

            gfx.DrawString(dic["site"], font, XBrushes.Black, new XRect(86, 104, 476, 14), XStringFormats.Center);
            gfx.DrawString(dic["location"], font, XBrushes.Black, new XRect(69, 120, 308, 13), XStringFormats.Center);
            gfx.DrawString(dic["date"], font, XBrushes.Black, new XRect(406, 120, 162, 13), XStringFormats.Center);


            gfx.DrawString(dic["engineer"], font, XBrushes.Black, new XRect(68, 136, 150, 13), XStringFormats.Center);
            gfx.DrawString(dic["cardNumber"], font, XBrushes.Black, new XRect(283, 136, 92, 13), XStringFormats.Center);
        //    gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(423, 136, 162, 13), XStringFormats.Center);


            gfx.DrawString(dic["clientsName"], font, XBrushes.Black, new XRect(81, 152, 137, 13), XStringFormats.Center);
          //  gfx.DrawString(dic[""], font, XBrushes.Black, new XRect(292, 152, 84, 13), XStringFormats.Center);
            gfx.DrawString(dic["WarningNoticeRefNo"], font, XBrushes.Black, new XRect(442, 152, 128, 13), XStringFormats.Center);

            double x, y;
            x = 106;
            y = 230;
            int firstFor = 0;

            List<string> prviFor = new List<string>
            {
                dic["steel1"],
                dic["steel2"],
                dic["steel3"],
                dic["steel4"],
                dic["steel5"],
                dic["steel6"],
                dic["steel7"],
                dic["steel8"],
                dic["steel9"],
                dic["steel10"],
                dic["steel11"],
                dic["steel12"],
                dic["steel13"],
                dic["copper1"],
                dic["copper2"],
                dic["copper3"],
                dic["copper4"],
                dic["copper5"],
                dic["copper6"],
                dic["copper7"],
                dic["presdr1"],
                dic["presdr2"],
                dic["presdr3"],
                dic["presdr4"],
                dic["presdr5"],
                dic["presdr6"],
                dic["presdr7"],
                dic["presdr8"],
            };

            for (int i = 0; i < 30; i++)
            {
                if (i == 13 || i == 21)
                {
                    y += 10.0;
                    continue;
                }
                gfx.DrawString(prviFor[firstFor++], font, XBrushes.Black, new XRect(x, y, 53, 10), XStringFormats.Center);
                y += 10.95;
            }
            y = 230;
            x = 244;
            List<string> drugiFor = new List<string>
            {
                dic["steel1Total"],
                dic["steel2Total"],
                dic["steel3Total"],
                dic["steel4Total"],
                dic["steel5Total"],
                dic["steel6Total"],
                dic["steel7Total"],
                dic["steel8Total"],
                dic["steel9Total"],
                dic["steel10Total"],
                dic["steel11Total"],
                dic["steel12Total"],
                dic["steel13Total"],
                dic["copper1Total"],
                dic["copper2Total"],
                dic["copper3Total"],
                dic["copper4Total"],
                dic["copper5Total"],
                dic["copper6Total"],
                dic["copper7Total"],
                dic["presdr1Total"],
                dic["presdr2Total"],
                dic["presdr3Total"],
                dic["presdr4Total"],
                dic["presdr5Total"],
                dic["presdr6Total"],
                dic["presdr7Total"],
                dic["presdr8Total"],
                dic["totalPipeworkVolume"],
                dic["pipeworkFittingsIV"],
                dic["meterVolume"],
                dic["totalVolumeForTesting"],
                dic["testMediumPicker"],
                dic["installationPicker"],
                dic["checkIsWeatherTemperatureStableYes"],
                dic["checkMeterBypassYes"],
                dic["testGaugeUsed"],
                dic["tightnessTestPressure"],
                dic["roomVolumeOfSmallestOccupiedSpace"],
                dic["maximumAllowablePressureDrop"],
                dic["checkInadequateVentilationYes"],
                dic["strengthTestPressure"],
                dic["checkComponentsRemovedBypassedYes"],
                dic["stabilisationPeriod"],
                dic["strenghtTestDuration"],
                dic["permittedPressureDrop"],
                dic["actualPressureDrop"],
                dic["checkTestPassedOrFailedPass"],
            };
            int secondFor = 0;
            for (int i = 0; i < 54; i++)
            {
                if (i == 33) y -= 2;
                if (i == 13 || i == 21 || i == 34 || i == 35 || i == 45 || i == 46)
                {

                    y += 10.25;
                    continue;
                }
                gfx.DrawString(drugiFor[secondFor++], font, XBrushes.Black, new XRect(x, y, 62, 10), XStringFormats.Center);
                y += 10.95;
            }
            gfx.DrawString(dic["letByDuration"], font, XBrushes.Black, new XRect(506, 698, 56, 20), XStringFormats.Center);
            gfx.DrawString(dic["stabilisationDuration"], font, XBrushes.Black, new XRect(506, 720, 56, 20), XStringFormats.Center);
            gfx.DrawString(dic["testDuration"], font, XBrushes.Black, new XRect(506, 740, 56, 20), XStringFormats.Center);

            gfx.DrawString(dic["actualPressureDropResult"], font, XBrushes.Black, new XRect(506, 774, 56, 8), XStringFormats.Center);

            gfx.DrawString(dic["testPassedOrFailed"], font, XBrushes.Black, new XRect(338, 795, 224, 18), XStringFormats.Center);






            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "IGE_UP_1A Sheet.pdf");

            document.Save(filePath);

            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();


        }
        public static async Task<byte[]> CreateServiceRecordPDF(Dictionary<string, string> dic, byte[] inzenjer, byte[] clijent)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Ashwell Service Report";


            PdfPage page = document.AddPage();
            page.Height = 595;
            page.Width = 842;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font;

            try
            {
                // Try to create an XFont with the desired font
                font = new XFont("Helvetica", 8);
            }
            catch (Exception)
            {
                // If an exception is thrown, fall back to a different font
                font = new XFont("Times New Roman", 8); // Replace with any other font you have
            }



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
        public static async Task<byte[]> CreateEngineersReport(Dictionary<string, string> dic )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 842;
            page.Width = 595;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);

            XImage image = await ConvertToXImage(@"engineers_report_sheet.jpg");

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
            // Save to MemoryStream
            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();

        }
        public static async Task<byte[]> CDM(Dictionary<string,string> dic)
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
            document.Save(filePath);
            // Save to MemoryStream
            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
        public static async Task<byte[]> PressurisationReport(Dictionary<string, string> dic)
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

            // Save to MemoryStream
            using MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();

        }
       
    }

}