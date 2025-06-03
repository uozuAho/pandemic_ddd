namespace pandemic.drawing;

public class DrawerGraph
{
    // todo: fix me later: don't use List<T>
#pragma warning disable CA1002
    public readonly List<DrawerNode> Nodes = [];
    public readonly List<DrawerEdge> Edges = [];
#pragma warning restore CA1002

    public DrawerNode CreateNode(string label = "")
    {
        var node = new DrawerNode { Label = label };
        Nodes.Add(node);
        return node;
    }

    public DrawerEdge CreateEdge(DrawerNode from, DrawerNode to, string? label)
    {
        var edge = new DrawerEdge(from, to, label);
        Edges.Add(edge);
        return edge;
    }
}
