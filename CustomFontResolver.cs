using Ashwell_Maintenance;
using PdfSharp.Fonts;
using System.IO;
using System.Reflection;

public class CustomFontResolver : IFontResolver
{
    public string DefaultFontName => "Arial";

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Always use regular Arial
        string name = "Arial_Regular";
        return new FontResolverInfo(name);
    }

    public byte[] GetFont(string faceName)
    {
        // Namespace and resource path to the Arial font
        string fontResource = "Ashwell_Maintenance.Resources.Fonts.arial.ttf";

        var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;

        using (Stream fontStream = assembly.GetManifestResourceStream(fontResource))
        {
            if (fontStream == null)
            {
                throw new FileNotFoundException($"Font resource '{fontResource}' does not exist.");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                fontStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
