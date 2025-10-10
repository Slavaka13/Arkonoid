using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>
    /// Представляет платформу, управляемую игроком.
    /// Платформа используется для отражения мяча и перемещается только по оси X.
    /// </summary>
    /// <remarks>
    /// Положение платформы хранится в виде прямоугольника (<see cref="Rectangle"/>),
    /// который задаёт координаты и размеры объекта на игровом поле.
    /// </remarks>
    internal class Platform
    {
        /// <summary>
        /// Получает или задаёт прямоугольник, определяющий позицию и размеры платформы.
        /// </summary>
        public Rectangle Rect { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Platform"/>.
        /// </summary>
        /// <param name="rect">Прямоугольник, задающий начальное положение и размер платформы.</param>
        public Platform(Rectangle rect)
        {
            Rect = rect;
        }

        /// <summary>
        /// Перемещает платформу по оси X, сохраняя координату Y и размеры.
        /// </summary>
        /// <param name="x">Новая координата X верхнего левого угла платформы.</param>
        public void MoveToX(int x)
        {
            Rect = new Rectangle(x, Rect.Y, Rect.Width, Rect.Height);
        }
    }
}
