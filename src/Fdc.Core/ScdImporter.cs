using System.Xml;
using System.Xml.Linq;

namespace Fdc.Core;

public static class ScdImporter
{
    public static IReadOnlyList<Ied> Read(Stream input)
    {
        using var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 10_000_000 });
        var doc = XDocument.Load(reader); var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
        return doc.Descendants(ns + "ConnectedAP").Select(ap =>
        {
            var name = (string?)ap.Attribute("iedName") ?? "";
            var ip = ap.Element(ns + "Address")?.Elements(ns + "P").FirstOrDefault(p => (string?)p.Attribute("type") == "IP")?.Value ?? "";
            return new Ied(name, name, ip);
        }).Where(x => Identity.SafeId(x.Id) && System.Net.IPAddress.TryParse(x.Host, out _)).DistinctBy(x => x.Id).ToArray();
    }
}
