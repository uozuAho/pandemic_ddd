namespace pandemic.drawing;

public class DrawerGraph
{
    public List<DrawerNode> Nodes = new();
    public List<DrawerEdge> Edges = new();

    public DrawerNode CreateNode(string label = "")
    {
        var node = new DrawerNode {Label = label};
        Nodes.Add(node);
        return node;
    }

    public DrawerEdge CreateEdge(DrawerNode from, DrawerNode to, string label)
    {
        var edge = new DrawerEdge(from, to, label);
        Edges.Add(edge);
        return edge;
    }
}
