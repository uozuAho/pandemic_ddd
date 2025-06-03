namespace pandemic.drawing.test;

using NUnit.Framework;

public class Tests
{
    [Test]
    public void From_graph_works()
    {
        var graph = new DrawerGraph();

        var from = graph.CreateNode("a");
        var to = graph.CreateNode("b");
        _ = graph.CreateEdge(from, to, "a to b");

        _ = CsDotDrawer.FromGraph(graph);
    }
}
