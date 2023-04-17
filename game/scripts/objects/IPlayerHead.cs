
// License by paralax (6/04/2023)

using Godot;

public partial class IPlayerHead : Node3D
{
    [Export(PropertyHint.Range, "0.05, 1.0")] 
    public float Sensitivity = 0.4f; // Сенса мыши
    
    private Vector3 rotation;

    public override void _Ready()
    {
        // Отключаем обработку физики
        SetPhysicsProcess(false);
        // Отключаем обработку сигналов
        SetProcess(false);
        // Включаем обработку пользовательского ввода
        SetProcessInput(true);  

        // Создаем экземпляр класса GlobalFunctions
        GlobalFunctions GF = new GlobalFunctions();
        // Устанавливаем режим захвата мыши
        GF.SetMouseMode(Input.MouseModeEnum.Captured);

        // Инициализируем переменную rotation значением текущего поворота камеры
        rotation = Rotation;
    }

    public override void _Input(InputEvent @event)
    {
        // Проверяем, было ли событие перемещения мыши
        if (@event is InputEventMouseMotion e)
        {
            // Изменяем поворот камеры по оси X в зависимости от перемещения мыши
            if (Rotation.X <= 1.57f && Rotation.X >= -1.57f) rotation.X += -e.Relative.Y / 1200f * Sensitivity;
            else if (Rotation.X > 1.57f) rotation.X = 1.57f;
            else if (Rotation.X < -1.57f) rotation.X = -1.57f;
            
            // Изменяем поворот камеры по оси Y в зависимости от перемещения мыши
            rotation.Y += -e.Relative.X / 1200f * Sensitivity;

            // Ограничиваем поворот камеры по оси Y в диапазоне от -π до π
            if (Rotation.Y > 3.14f) rotation.Y = -3.14f;
            else if (Rotation.Y < -3.14f) rotation.Y = 3.14f;

            // Устанавливаем новый поворот камеры
            Rotation = rotation;
        }
    }
}
