using Godot;

// ChatGPT сказал: Хороший код!

public partial class IPlayer : CharacterBody3D
{
	[Export] public float 
        Speed = 5f,  // Обычная скорость
        Stamina = 100f, // Текущая стамина
        MaxStamina = 150f, // Макс. стамина
        StaminaDepletionRate = 15f, // Как быстро уменьшается стамина
        StaminaRecoveryRate = 8f, // Как быстро восстонавливается стамина
        NextStaminaRecoveryTime = 0f, // Когда восстановится стамина
	    SpeedBoost = 8f, // Скорость при беге
	    Acceleration = 0.6f, //...
	    AccelerationAir = 0.3f,
	    AccelerationStop = 0.7f, // При остановке
	    AccelerationStopAir = 0.4f,
	    JumpStrength = 8f, // Сила прыжка
	    Gravity = 0.214f; // Гравитация
    
	public float CurrentSpeed = 0f; // Текущая скорость игрока, дальше по коду я её изменяю

	private Vector3 velocity,
	    direction;
	private Vector2 inputDirection;

	private Label DebugText;
	private IPlayerCamera PlayerCamera;
	private MeshInstance3D PlayerModel;

	public override void _Ready()
	{
		SetPhysicsProcess(true);
		SetProcess(false);
		SetProcessInput(false);

		PlayerCamera = GetNode<IPlayerCamera>("camera");

		DebugText = GetNode<Label>("UI/Control/Debug");
		Timer DebugTextTimer = new Timer();
		AddChild(DebugTextTimer);
		DebugTextTimer.Autostart = true;
		DebugTextTimer.Start(0.2);
		DebugTextTimer.Connect("timeout", new Callable(this, "TimerTimeout"));
	}

    public override void _PhysicsProcess(double delta)
    {
        inputDirection = new Vector2(
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
            Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward")
        );
        if (inputDirection.Length() > 1)
            inputDirection /= inputDirection.LengthSquared();

        // Здесь я делаю так, чтобы игрок двигался по направлению камеры
        Vector3 forward = PlayerCamera.GlobalTransform.Basis.Z.Normalized(); // Эти две переменные служат - не повтворять значения
        Vector3 right = PlayerCamera.GlobalTransform.Basis.X.Normalized();
        forward.Y = 0f;
        right.Y = 0f;
        direction = (forward * inputDirection.Y + right * inputDirection.X).Normalized(); // Кому нужно забирайте (написал ChatGPT)

        bool isRunning = Input.IsActionPressed("move_run") && Stamina > 0f;
        bool isJumping = Input.IsActionJustPressed("move_jump") && Stamina >= 25f;
        bool isOnFloor = IsOnFloor();

        if (isOnFloor)
            if (isRunning)
            {
                Stamina -= StaminaDepletionRate * (float)delta;
                CurrentSpeed = SpeedBoost;
            }
            else if (!isRunning && !isJumping)
            {
                Stamina += StaminaRecoveryRate * (float)delta;
                CurrentSpeed = Speed;
            }


        if (isJumping)
        {
            velocity.Y = JumpStrength;
            Stamina -= 25f;
        }

        Stamina = Stamina < 0f ? 0f : Stamina;
        Stamina = Stamina > MaxStamina ? MaxStamina : Stamina;
        
        // Простоя логика движения
        if (direction != Vector3.Zero)
        {
            velocity.X = velocity.MoveToward(CurrentSpeed * direction, isOnFloor ? Acceleration : AccelerationAir).X;
            velocity.Z = velocity.MoveToward(CurrentSpeed * direction, isOnFloor ? Acceleration : AccelerationAir).Z;
        }
        else
        {
            velocity.X = velocity.MoveToward(0f * direction, isOnFloor ? AccelerationStop : AccelerationStopAir).X;
            velocity.Z = velocity.MoveToward(0f * direction, isOnFloor ? AccelerationStop : AccelerationStopAir).Z;
        }

        velocity.Y -= !isOnFloor ? Gravity : 0f;

        Velocity = velocity;
        
        MoveAndSlide();
    }

	private void TimerTimeout()
	{
		DebugText.Text = $"Velocity: {Velocity}\nDirection: {direction}\nSpeed: {CurrentSpeed}\nStamina: {Stamina}";
	}
}
