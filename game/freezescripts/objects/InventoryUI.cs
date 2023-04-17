
// License by paralax (6/04/2023)

using Godot;
using System.Collections.Generic;

public partial class InventoryUI : Control 
{
    [Export(PropertyHint.Range, "0.0,1.0")] public float ColorNotActiveAlpha = 0.39f; 
    [Export(PropertyHint.Range, "0.0,1.0")] public float ColorActiveAlpha = 0.19f; 

    private Inventory _inventory { get; set; }

    private Control DescriptionPanel; // панель с описанием и именем предмета, вызываеься только когда подберу предмет
    private Label DescriptionPanelName; // Имя предмета
    private RichTextLabel DescriptionPanelText; // Описание предмета
    private Timer DescriptionTimer; // Таймер, как долго будет описание
    private HBoxContainer HBox; // Здесь все ячейки
    private List<Control> Slots; // Ячейки

    public override void _Ready() 
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);

        _inventory = GetParent<Control>().GetParent<CanvasLayer>().GetParent<IPlayer>().Inventory;
        Slots = new List<Control>(_inventory.Capacity);

        DescriptionPanel = GetParent().GetNode<Control>("Item description panel");
        DescriptionPanelName = DescriptionPanel.GetNode<Label>("Info");
        DescriptionPanelText = DescriptionPanel.GetNode<RichTextLabel>("Description");
        DescriptionTimer = GetNode<Timer>("Description timer");
        HBox = GetNode<HBoxContainer>("Vbox/Hbox");
        for (var i = 0; i < _inventory.Capacity; i++)
        {
            Slots.Add(HBox.GetNode<Control>($"Slot{i}"));
        }

        GetNode<Events>("/root/Events").Connect(nameof(Events.SignalName.OnPickupItem), new Callable(this, nameof(OnPickupItem)));
        DescriptionTimer.Connect("timeout", new Callable(this, nameof(OnDescriptionTimerTimeout)));

        hideDescriptionPanel();
    }

    private void OnPickupItem(Item item)
    {
        Update();
        showDescriptionPanel(item);
    }

    private void OnDescriptionTimerTimeout()
    {
        hideDescriptionPanel();
    }

    public void showDescriptionPanel(Item item, int seconds = 10)
    {
        if (seconds != -1)
            if (DescriptionTimer.IsStopped())
                DescriptionTimer.Start(seconds);
        DescriptionPanel.Visible = true;
        DescriptionPanelName.Text = $"{item.ItemName} ({item.ItemCount}/{item.ItemMaxCount})";
        DescriptionPanelText.Text = $"{item.ItemDescription}";
    }

    public void hideDescriptionPanel()
    {
        DescriptionPanel.Visible = false;
        DescriptionPanelName.Text = "Пусто";
        DescriptionPanelText.Text = "Пусто";
    }

    public void Update()
    {
        foreach (var item in _inventory.Items)
        {
            Control slot = GetNode<Control>("Vbox/Hbox/Slot" + item.ItemPosition);
            ColorRect slotBg = slot.GetNode<ColorRect>("Bg");
            Label slotInfo = slot.GetNode<Label>("Info");
            Label slotAmount = slot.GetNode<Label>("Amount");
            
            slotBg.Visible = true;
            slotInfo.Visible = true;
            slotAmount.Visible = true;

            slotInfo.Text = item.ItemName;
            slotAmount.Text = item.ItemCount.ToString();
        }
    }
}