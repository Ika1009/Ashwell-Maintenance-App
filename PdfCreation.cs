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

namespace Ashwell_Maintenance
{
    public static class PdfCreation
    {

        private static async Task<XImage> ConvertToXImage(string resourceId)
        {
            var imageSource = ImageSource.FromResource(resourceId);
            var streamImageSource = imageSource as StreamImageSource;
            if (streamImageSource != null)
            {
                using (var stream = await streamImageSource.Stream(CancellationToken.None))
                {
                    return XImage.FromStream(stream);
                }
            }
            return null;
        }

        public static async void CreateServiceRecordPDF(string site
         
          )
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            PdfDocument document = new PdfDocument(); document.Info.Title = "Engineers Report Sheet";


            PdfPage page = document.AddPage();
            page.Height = 595;
            page.Width = 842;

            XGraphics gfx = XGraphics.FromPdfPage(page);


            XFont font = new XFont("Arial", 10);
       
            XImage image = await ConvertToXImage("Ashwell Maintenance.Images.myImage.png"); ;
            gfx.DrawImage(image, 0, 0, 842, 595);
            //site
            gfx.DrawString(site, font, XBrushes.Black, new XRect(51, 67, 337 - 51, 95 - 67), XStringFormats.Center);
            //location
            gfx.DrawString(location, font, XBrushes.Black, new XRect(68, 98, 269 - 68, 124 - 98), XStringFormats.Center);
            //asset no
            gfx.DrawString(assetNo, font, XBrushes.Black, new XRect(272, 107, 337 - 272, 123 - 107), XStringFormats.Center);
            //Appliance number
            gfx.DrawString(ApplianceNumber, font, XBrushes.Black, new XRect(76, 128, 111 - 76, 139 - 128), XStringFormats.Center);
         //Tests completed
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(186, 130, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(213, 130, 11, 5), 0, 360);
            //remedial work required
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(329, 130, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(356, 130, 11, 5), 0, 360);
            //apliance serial number //apliance model
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(82, 142, 196 - 82, 154 - 142), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(250, 142, 376 - 250, 154 - 142), XStringFormats.Center);
            //apliance serial no  //GC no
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(93, 157, 268 - 93, 169 - 157), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(300, 157, 76, 169 - 157), XStringFormats.Center);
            //Burner Make   //Burner model
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(70, 172, 196 - 70, 183 - 172), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(243, 172, 376 - 243, 183 - 172), XStringFormats.Center);
            //Burner serial no  // type    //spec
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(85, 187, 195 - 85, 199 - 187), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(218, 187, 269 - 218, 199 - 187), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(293, 187, 376 - 293, 199 - 187), XStringFormats.Center);
            //open flue   //room sealed  //forced draft   //flueless
            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(159, 201, 177 - 159, 214 - 201), XStringFormats.Center);
            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(226, 201, 244 - 226, 13), XStringFormats.Center);
            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(294, 201, 310 - 294, 13), XStringFormats.Center);
            gfx.DrawString("Yes", font, XBrushes.Black, new XRect(361, 201, 376 - 361, 13), XStringFormats.Center);
            //heating   //hotwater  // both
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(182, 217, 199 - 182, 229 - 217), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(269, 217, 289 - 269, 229 - 217), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(360, 217, 376 - 360, 229 - 217), XStringFormats.Center);
            //badged bumer pressure
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(113, 232, 176 - 113, 243 - 232), XStringFormats.Center);
            //ventilation satisfactory
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(322, 235, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(354, 235, 11, 5), 0, 360);
            //gas type
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(133, 250, 11, 6), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(160, 250, 17, 6), 0, 360);
            //flue condition satisfactory
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(322, 250, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(354, 250, 11, 5), 0, 360);
            //approx age of apliance   //badged input   //badget output
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(114, 263, 145 - 114, 273 - 263), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(214, 263, 255 - 214, 10), XStringFormats.Center);
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(320, 263, 362 - 320, 10), XStringFormats.Center);
            double y = 307;
            //apliance component checklist
            for (int i = 0; i < 18; i++)
            {
                gfx.DrawString("Yes", font, XBrushes.Black, new XRect(114, y, 140 - 114, 13), XStringFormats.Center);
                gfx.DrawString("No", font, XBrushes.Black, new XRect(142, y, 168 - 142, 13), XStringFormats.Center);
                gfx.DrawString("N/A", font, XBrushes.Black, new XRect(170, y, 197 - 170, 13), XStringFormats.Center);
                y += 15;
            }
            //State appliance condition
            gfx.DrawString("Tekst", font, XBrushes.Black, new XRect(497, 187, 576 - 497, 13), XStringFormats.Center);
            y = 307;
            //remarks/ comments left page
            for (int i = 0; i < 18; i++)
            {
                if (i == 8)
                {
                    gfx.DrawString("Komentar", font, XBrushes.Black, new XRect(247, y, 346 - 247, 13), XStringFormats.Center);
                }
                else
                {
                    gfx.DrawString("Komentar", font, XBrushes.Black, new XRect(198, y, 346 - 198, 13), XStringFormats.Center);
                }
                y += 15;
            }
            //heat exchanger/fluent clear
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(507, 55, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(532, 55, 11, 5), 0, 360);
            gfx.DrawArc(new XPen(XColor.FromArgb(0, 0, 0)), new XRect(556, 55, 11, 5), 0, 360);
            //working inlet pressure   //recorded burner pressure    //measured gas rate   
            gfx.DrawString("N/A", font, XBrushes.Black, new XRect(497, 67, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString("N/A", font, XBrushes.Black, new XRect(497, 83, 557 - 497, 12), XStringFormats.Center);
            gfx.DrawString("N/A", font, XBrushes.Black, new XRect(497, 99, 557 - 497, 12), XStringFormats.Center);
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
            for (int i = 0; i < 10; i++)
            {
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x, y, 231, 11), XStringFormats.Center);
                y += 15;
            }
            y = 504;
            x = 390;
            // i = 0; // engineers name //enginiers singature // enginieers gas safe id number
            // i = 1 //clients name // clients signature  // inspection date
            for (int i = 0; i < 2; i++)
            {
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x, y, 137, 25), XStringFormats.Center);
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x + 142, y, 137, 25), XStringFormats.Center);
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x + 284, y, 137, 25), XStringFormats.Center);
                y += 45;
            }

            //comments/defects
            XRect boundingBox = new XRect(393, 287, 414, 166);
            string text = "This is a long text that needs to be split to fit within the bounding box. This is a long text that needs to be split to fit within the bounding box. This is a long text that needs to be split to fit within the bounding box. This is a long text that needs to be split to fit within the bounding box. This is a long text that needs to be split to fit within the bounding box.";
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect layoutRectangle = boundingBox;
            tf.DrawString(text, new XFont("Calibri", 11), XBrushes.Black, layoutRectangle, XStringFormats.TopLeft);
            //unsafe situations warning notice issue number
            gfx.DrawString("1234", font, XBrushes.Black, new XRect(610, 459, 200, 9), XStringFormats.Center);

            x = 432;
            y = 218.5;
            //co2 //flue temp  //co2 // flue temp
            //co  //effisency // co // effisency
            //o2 // excess air
            //ratio  //room temp
            for (int i = 0; i < 4; i++)
            {
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x, y, 43, 10), XStringFormats.Center);
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x + 106, y, 43, 10), XStringFormats.Center);
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x + 212, y, 43, 10), XStringFormats.Center);
                gfx.DrawString("1234", font, XBrushes.Black, new XRect(x + 318, y, 43, 10), XStringFormats.Center);
                y += 15;
            }




            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
            string filePath = System.IO.Path.Combine(downloadsFolder, "Ashwell_Service_Report.pdf");

            document.Save(filePath);
        }
    }
}
