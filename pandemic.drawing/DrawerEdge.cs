namespace pandemic.drawing;

public class DrawerEdge(DrawerNode from, DrawerNode to, string? label)
{
    public DrawerNode From { get; } = @from;
    public DrawerNode To { get; } = to;
    public string Label { get; } = label ?? "";
    public Colour? Colour { get; set; }
}
