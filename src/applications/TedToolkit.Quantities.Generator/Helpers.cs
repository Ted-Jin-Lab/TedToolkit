using TedToolkit.Quantities.Data;
using VDS.RDF;

namespace TedToolkit.Quantities.Generator;


public static class Helpers
{
    public static string GetUrlName(this IUriNode uriNode) => uriNode.Uri.AbsolutePath.Split('/').Last().Replace("_", "");
    
    public static string GetDescription(this IUriNode quantity, Graph g)
    {
        return GetString("qudt:plainTextDescription")
               ?? GetString("dcterms:description") //TODO: Latex Description  
               ?? "Nothing";

        string? GetString(string predicate)
        {
            return g.GetTriplesWithSubjectPredicate(quantity,
                    g.CreateUriNode(predicate))
                .Select(t => t.Object)
                .OfType<ILiteralNode>()
                .FirstOrDefault()?.Value;
        }
    }
    
    public static IReadOnlyList<Link> GetLinks(this IUriNode quantity, Graph g)
    {
        var links = new List<Link>();
        AddLinks("qudt:dbpediaMatch", "DBpedia");
        AddLinks("qudt:informativeReference", "Link");
        AddLinks("qudt:isoNormativeReference", "ISO");
        AddLinks("qudt:wikidataMatch", "WikiData");

        return links;

        void AddLinks(string predicate, string name)
        {
            links.AddRange(g.GetTriplesWithSubjectPredicate(quantity, g.CreateUriNode(predicate))
                .Select(n => n.Object)
                .OfType<ILiteralNode>()
                .Select(literalNode => new Link(name, literalNode.Value)));
        }
    }

    public static string LabelToName(this string label)
    {
        return string.Join(null, label.Split(' ').Select(i => char.ToUpperInvariant(i[0]) + i[1..]));
    }
    
    public static IEnumerable<ILiteralNode> GetLabels(this IUriNode node, Graph g)
    {
        return g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("rdfs:label"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>();
    }
}