namespace pandemic.drawing;

public class DrawerEdge
{
    public DrawerNode From { get; }
    public DrawerNode To { get; }
    public string Label { get; set; }
    public Colour? Colour { get; set; }

    public DrawerEdge(DrawerNode from, DrawerNode to, string label)
    {
        From = @from;
        To = to;
        Label = label;
    }
}
