using Godot;

public partial class GameOver : Control
{
    private void _on_restart_pressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/main.tscn");
    }

    private void _on_quit_pressed()
    {
        GetTree().Quit();
    }
}

