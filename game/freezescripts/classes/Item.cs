
// License by paralax (6/04/2023)

using Godot;

public partial class Item : RigidBody3D 
{
    [Export] public string ItemName { get; set; }
    [Export(PropertyHint.TypeString)] public string ItemDescription { get; set; }
    public Type ItemType { get; set; }
    [Export] public int ItemCount { get; set; }
    [Export] public int ItemMaxCount { get; set; }
    public int ItemPosition { get; set; }
    [Export(PropertyHint.File)] public string LocalPath;
    [Export] public bool InWorld { get; set; }
    [Export] public bool InInventory { get; set; }
    public Inventory Inventory { get; set; }

    private Events _events;

    public enum Type 
    {
        Weapon, // оружие
        Armor, // броня
        Potion, // Зелье 
        QuestItem, // Квествый
        Trinket // Безделушка
    }

    public override void _EnterTree()
    {
        if (InWorld && ItemCount == 0)
        {
            ItemCount = 1;
            GD.PrintErr($"{ItemName}: ItemCount - не может быть 0!");
        }
        if (ItemCount > ItemMaxCount)
        {
            ItemCount = ItemMaxCount;
            GD.PrintErr($"{ItemName}: ItemCount - не может быть больше макс-количевства предмета!");
        }
        if (InWorld && InInventory)
        {
            GD.PrintErr($"{ItemName}: ItemCount - не может быть в мире и инвентаре!");
            GetTree().Quit(1);
        }
    }

    public override void _Ready()
    {
        _events = GetNode<Events>("/root/Events");
    }

    public virtual void AnimateSpawnItemPlayerView()
    {
        // Анимация достования...
    }

    public void AddToInventory(Inventory inventory)
    {
        inventory.AddItem(this);
        _events.EmitSignal(nameof(Events.SignalName.OnChangedItem), this);
    } 

    public void Remove()
    {
        if (!InInventory)
            Free();
        else
        {
            Inventory.RemoveItem(this);
            Free();
        }
    }

    public void RemoveFromInventory()
    {
        Inventory.RemoveItem(this);
    } 

    public void RemoveFromWorld()
    {
        if (InWorld)
        {
            QueueFree();
        }
    }

    public virtual void BreakAnimation() 
    {
        // Анимация ломания...
    }

    public void Break()
    {
        if (InInventory)
            RemoveFromInventory();
        BreakAnimation();
        QueueFree();
    }
}
