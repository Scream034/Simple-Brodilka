using Godot;

public partial class IPlayerCamera : Camera3D
{
    [Export(PropertyHint.Range, "0.05, 1.0")] 
    public float Sensitivity = 0.4f;
    
    private Vector3 rotation; 

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(true);  
    
        GlobalFunctions GF = new GlobalFunctions();
        GF.SetMouseMode(Input.MouseModeEnum.Captured);

        rotation = Rotation;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion e)
        {
            if (Rotation.X <= 1.57f && Rotation.X >= -1.57f) rotation.X += -e.Relative.Y / 1200 * Sensitivity;
            else if (Rotation.X > 1.57f) rotation.X = 1.57f;
            else if (Rotation.X < -1.57f) rotation.X = -1.57f;
           rotation.Y += -e.Relative.X / 1200 * Sensitivity;

            if (Rotation.Y > 3.14f) rotation.Y = -3.14f;
            else if (Rotation.Y < -3.14f) rotation.Y = 3.14f;

            Rotation = rotation;
        }
    }
}
