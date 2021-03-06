using csdot;
using csdot.Attributes.DataTypes;
using csdot.Attributes.Types;
using Color = csdot.Attributes.DataTypes.Color;

namespace pandemic.drawing;

public class CsDotDrawer
{
    public Graph _graph = new Graph { type = "digraph" };

    private CsDotDrawer()
    {
    }

    public static CsDotDrawer FromGraph(DrawerGraph graph)
    {
        var drawer = new CsDotDrawer();
        var nodeId = 0;
        Dictionary<DrawerNode, IDot> nodeLookup = new();

        foreach (var node in graph.Nodes)
        {
            var dotNode = ToDotNode(node, nodeId++);
            nodeLookup[node] = dotNode;

            drawer._graph.AddElement(dotNode);
        }

        foreach (var edge in graph.Edges)
        {
            var @from = nodeLookup[edge.From];
            var to = nodeLookup[edge.To];
            var edge1 = ToDotEdge(@from, to, edge);

            drawer._graph.AddElement(edge1);
        }

        return drawer;
    }

    public void SaveToFile(string path)
    {
        new DotDocument().SaveToFile(_graph, path);
    }

    private static IDot ToDotNode(DrawerNode node, int nodeId)
    {
        var node1 = new Node(nodeId.ToString());
        node1.Attribute.label.Value = node.Label;

        if (node.Colour != null)
        {
            node1.Attribute.color.Value = node.Colour.Value switch
            {
                Colour.Red => Color.SVG.red,
                _ => throw new ArgumentException()
            };
        }

        return node1;
    }

    private static Edge ToDotEdge(IDot @from, IDot to, DrawerEdge edge)
    {
        var dotEdge = new Edge(new List<Transition>
        {
            new(@from, EdgeOp.directed),
            new(to, EdgeOp.unspecified)
        })
        {
            Attribute =
            {
                label = new Label
                {
                    Value = edge.Label
                }
            }
        };
        if (edge.Colour != null)
        {
            dotEdge.Attribute.color.Value = edge.Colour switch
            {
                Colour.Red => Color.SVG.red,
                _ => throw new ArgumentException()
            };
        }

        return dotEdge;
    }
}
