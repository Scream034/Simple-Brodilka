
// License by paralax (6/04/2023)

using Godot;

public partial class WeaponStick : Weapon
{
    private AnimationPlayer animation;

    public override void _Ready()
    {
        if (InWorld)
        {
            SetPhysicsProcess(false);
            SetProcessInput(false);
            SetProcess(false);
        }
        else 
        {
            SetPhysicsProcess(true);
            SetProcessInput(false);
            SetProcess(false);

            Freeze = true;
            FreezeMode = FreezeModeEnum.Static;
            Sleeping = true;
            ContactMonitor = true;
            MaxContactsReported = 1;
        }
    }
}


