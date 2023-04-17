
// License by paralax (6/04/2023)

using Godot;

public partial class Weapon : Item 
{
    [Export] public float Damage { get; set; }
    [Export] public float Durability { get; set; }

    public override void _EnterTree()
    {
        ItemType = Type.Weapon;
    }

    public virtual void Attack() { }
    public virtual void Defense() { }
}