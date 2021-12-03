using NUnit.Framework;

namespace pandemic.drawing.test;

public class Tests
{
    [Test]
    public void From_graph_works()
    {
        var graph = new DrawerGraph();

        var from = graph.CreateNode("a");
        var to = graph.CreateNode("b");
        var edge = graph.CreateEdge(from, to, "a to b");

        CsDotDrawer.FromGraph(graph);
    }
}
