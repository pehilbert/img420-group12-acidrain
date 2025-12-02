using Godot;

public partial class Shelter : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player p)
            p.IsInsideShelter = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player p)
            p.IsInsideShelter = false;
    }
}
