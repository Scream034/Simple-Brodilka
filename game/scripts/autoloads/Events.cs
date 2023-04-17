
// License by paralax (6/04/2023)

using Godot;

public partial class Events : Node
{
	[Signal] 
	public delegate void HealthChangedEventHandler(int amount);
	[Signal]
	public delegate void OnChangedItemEventHandler(Item item);
	[Signal]
	public delegate void OnPickupItemEventHandler(Item item);
	[Signal]
	public delegate void OnChangedItemHandEventHandler(int index);
}
