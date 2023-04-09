using Godot;

public partial class GlobalFunctions : Node
{
	public Input.MouseModeEnum CurrentMouseMode { get ; set ; }

	public virtual void SetMouseMode(Input.MouseModeEnum value)
	{
		Input.MouseMode = value; 
		CurrentMouseMode = value;
	}
}
