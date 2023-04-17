
// License by paralax (6/04/2023)

using Godot;

public partial class IPlayer : CharacterBody3D
{
    [Export] public float 
        WalkingSpeed = 8f, // Скорость передвижения
        RunningSpeed = 10f, // Скорость при беге
        Acceleration = 0.6f, // Ускорение при движении по земле
        AccelerationAir = 0.11f, // Ускорение при движении в воздухе
        AccelerationStop = 0.7f, // Ускорение при остановке на земле
        AccelerationStopAir = 0.08f, // Ускорение при остановке в воздухе
        Gravity = 0.241f, // Гравитация
        JumpStrength = 12.5f, // Сила прыжка
        Stamina = 50f, // Выносливость
        MaxStamina = 100f, // Максимальное значение выносливости
        StaminaDepletionRate = 0.2f, // Скорость расходования выносливости
        StaminaRecoveryRate = 0.05f; // Скорость восстановления выносливости

    public Inventory Inventory = new Inventory(9);
    public Node ItemHand;

    private Vector3 
        velocity = Vector3.Zero, // Текущая скорость игрока
        direction = Vector3.Zero; // Текущее направление движения игрока

    private bool 
        isRunning = false, // Проверка, бежит ли игрок и хватает ли у него выносливости
        isJumping = false, // Проверка, прыгает ли игрок и хватает ли у него выносливости
        isOnFloor = false; // Проверка, находится ли игрок на земле

    private float CurrentSpeed = 0f; // Текущая скорость игрока (обычная или ускоренная)
    
    private Events _events; // Эвенты или сигналы
    private IPlayerHead PlayerHead; // Камера игрока
    private MeshInstance3D PlayerModel3D; // Модель игрока
    private CollisionShape3D PlayerShape3D; // Коллизия игрока
    private Label DebugText; // Текст для вывода отладочной информации
    private RayCast3D PickUpRaycast; // Рей для проверки достаём ли мы до предмета
    private InventoryUI InventoryUI; // Отоброжение инв-ря
    private Node3D HandPosition; // Место спавна предмета
    private Timer ItemCheckPickupTimer; // Таймер, который просто снижает нагрузку на ЦП
    private Timer DebugTextTimer; // Таймер обновления откладки

    public override void _PhysicsProcess(double delta)
    {
        isOnFloor = IsOnFloor(); // Проверка, находится ли игрок на земле

        Vector3 forward = -PlayerHead.GlobalTransform.Basis.Z;
        Vector3 right = PlayerHead.GlobalTransform.Basis.X;

        Vector2 inputDirection = Vector2.Zero;
        inputDirection.X = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        inputDirection.Y = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back");
        inputDirection = inputDirection.Normalized();

        direction = (forward * inputDirection.Y + right * inputDirection.X).Normalized();

        isRunning = Input.IsActionPressed("move_run"); // Проверка, бежит ли игрок
        isJumping = Input.IsActionJustPressed("move_jump"); // Проверка, прыгает ли игрок и хватает ли у него выносливости

        if (isOnFloor)
        {
            if (isRunning && Velocity.LengthSquared() > 0) // Если игрок бежит и у него хватает выносливости
            {
                if (Stamina > 0f)
                {
                    Stamina -= StaminaDepletionRate; // Уменьшаем выносливость
                    CurrentSpeed = RunningSpeed; // Устанавливаем текущую скорость бега
                }
                else
                {
                    CurrentSpeed = WalkingSpeed;
                    isRunning = false;
                }
            }
            else // Если игрок не бежит
            {
                // Здесь мы проверяем стоит ли игрок, если да то мы ускоряем процесс восстановления стамины и нет
                Stamina += Velocity.LengthSquared() == 0f ? StaminaRecoveryRate * 2f : StaminaRecoveryRate; 
                CurrentSpeed = WalkingSpeed; // Устанавливаем текущую скорость ходьбы
            }

            if (isJumping) // Если игрок прыгает
            {
                float jumpForce = Mathf.Lerp(Stamina / MaxStamina, JumpStrength, Stamina / MaxStamina); // небольшая логика прыжка
                velocity.Y = jumpForce; 
                Stamina -= JumpStrength + jumpForce;
            }
        }

        Stamina = Mathf.Clamp(Stamina, 0f, MaxStamina); // Ограничиваем выносливость в диапазоне от 0 до максимального значения

        if (direction != Vector3.Zero) // Если игрок движется
        {
            velocity.X = velocity.MoveToward(CurrentSpeed * direction, isOnFloor ? Acceleration : AccelerationAir).X; // Устанавливаем горизонтальную скорость в направлении движения
            velocity.Z = velocity.MoveToward(CurrentSpeed * direction, isOnFloor ? Acceleration : AccelerationAir).Z;
        }
        else // Если игрок стоит на месте
        {
            velocity.X = velocity.MoveToward(0f * direction, isOnFloor ? AccelerationStop : AccelerationStopAir).X; // Уменьшаем горизонтальную скорость до остановки
            velocity.Z = velocity.MoveToward(0f * direction, isOnFloor ? AccelerationStop : AccelerationStopAir).Z;
        }

        velocity.Y -= !isOnFloor ? Gravity : 0f; // Устанавливаем вертикальную скорость для гравитации

        Velocity = velocity; // Устанавливаем текущую скорость игрока

        MoveAndSlide(); // Перемещаем игрока с учетом физики

        if (Input.IsActionJustReleased("interact")) // Е
        {
            PickupItem(CheckItemPickup()); // Беру предмет если до него достаю
        }

        PlayerModel3D.Rotation = new Vector3(0f, PlayerHead.Rotation.Y, 0f);
        PlayerShape3D.Rotation = new Vector3(0f, PlayerHead.Rotation.Y, 0f);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("active_slot_0"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 0);
            else if (Input.IsActionJustPressed("active_slot_1"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 1);
            else if (Input.IsActionJustPressed("active_slot_2"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 2);
            else if (Input.IsActionJustPressed("active_slot_3"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 3);
            else if (Input.IsActionJustPressed("active_slot_4"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 4);
            else if (Input.IsActionJustPressed("active_slot_5"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 5);
            else if (Input.IsActionJustPressed("active_slot_6"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 6);
            else if (Input.IsActionJustPressed("active_slot_7"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 7);
            else if (Input.IsActionJustPressed("active_slot_8"))
                _events.EmitSignal(nameof(Events.SignalName.OnChangedItemHand), 8);
        }
    }

    public override void _Ready()
    {
        SetPhysicsProcess(true); // Устанавливаем флаг физической обработки
        SetProcess(false); // Устанавливаем флаг обработки сигналов
        SetProcessInput(true); // Устанавливаем флаг обработки пользовательского ввода

        Inventory.Player = this;

        _events = GetNode<Events>("/root/Events");
        PlayerHead = GetNode<IPlayerHead>("head"); // Получаем ссылку на голову игрока
        PlayerModel3D = GetNode<MeshInstance3D>("model"); // Получаем ссылку на модель игрока
        PlayerShape3D = GetNode<CollisionShape3D>("shape"); // Получаем ссылку на коллизию игрока
        PickUpRaycast = GetNode<RayCast3D>("head/camera/raycast"); // Получаем ссылку на проверку подбора предмета 
        ItemCheckPickupTimer = GetNode<Timer>("item pickup check timer"); // Получаем ссылку на таймер 
        InventoryUI = GetNode<InventoryUI>("UI/Layer 1/Inventory"); // Получаем ссылку на отоброжение инв-ря
        HandPosition = GetNode<Node3D>("head/hand position"); // Получаем ссылку на место спавна предмета
        DebugText = GetNode<Label>("UI/Layer 1/Debug"); // Получаем ссылку на текст для вывода отладочной информации
        DebugTextTimer = GetNode<Timer>("debug update text timer"); // Получаем ссылку на таймер для обновления текста
        
        // Подключаем обработчики событий
        ItemCheckPickupTimer.Connect("timeout", new Callable(this, nameof(ItemCheckPickupTimerTimeout)));
        DebugTextTimer.Connect("timeout", new Callable(this, nameof(TimerTimeout)));
        _events.Connect(nameof(Events.SignalName.OnChangedItemHand), new Callable(this, nameof(OnChangedItemHand)));
    }

    private void TimerTimeout()
    {
        DebugText.Text = $"Velocity: {Velocity}\nDirection: {direction}\nSpeed: {CurrentSpeed}\nStamina: {Stamina}"; // Обновляем текст для вывода отладочной информации
    }

    private void ItemCheckPickupTimerTimeout()
    {
        Item item = CheckItemPickup();
        if (item is not null && item.InWorld)
        {
            InventoryUI.showDescriptionPanel(item, 8);
        }
    }

    private void OnChangedItemHand(int index)
    {
        Item item;
        if (Inventory.Items.Capacity > 0)
            item = Inventory.Items[index];
        else 
            return;
        PackedScene itemScene = GD.Load<PackedScene>(item.LocalPath);

        switch (item.ItemType)
        {
            case Item.Type.Weapon:

                if (item is WeaponStick stick)
                {
                    stick = (WeaponStick)itemScene.Instantiate();
                    stick.Transform = HandPosition.Transform;
                    if (ItemHand is not WeaponStick)
                        HandPosition.AddChild(stick);
                }
                
                break;
            
            default:
                GD.PrintErr("IPlayer.cs OnChangedItemHand - Нет типа у предмета!");
                GetTree().Quit(1);
                break;
        }
        
    }

    public Item CheckItemPickup()
    {
        if (PickUpRaycast.IsColliding())
        {
            return (Item)PickUpRaycast.GetCollider();
        }
        
        return null;
    }

    public void PickupItem(Item item)
    {
        if (Inventory.AddItem(item))
        {
            _events.EmitSignal(nameof(Events.SignalName.OnPickupItem), item);
        }
    }

    public void TakeDamage(int damageAmoumt)
    {
        _events.EmitSignal(nameof(Events.HealthChangedEventHandler), damageAmoumt);
    }

    public void Heal(int damageAmoumt)
    {
        _events.EmitSignal(nameof(Events.HealthChangedEventHandler), damageAmoumt);
    }
}