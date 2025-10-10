using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>
    /// Представляет игровой мяч, который участвует в столкновениях и движении.
    /// Содержит информацию о положении и скорости по осям X и Y.
    /// </summary>
    internal class Ball
    {
        /// <summary>
        /// Получает или задаёт прямоугольник, определяющий позицию и размеры мяча.
        /// </summary>
        public Rectangle Rect { get; private set; }

        /// <summary>
        /// Получает или задаёт скорость мяча по оси X (пикселей за тик).
        /// Положительное значение — движение вправо, отрицательное — влево.
        /// </summary>
        public int SpeedX { get; set; }

        /// <summary>
        /// Получает или задаёт скорость мяча по оси Y (пикселей за тик).
        /// Положительное значение — движение вниз, отрицательное — вверх.
        /// </summary>
        public int SpeedY { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Ball"/>.
        /// </summary>
        /// <param name="rect">Прямоугольник, задающий начальное положение и размер мяча.</param>
        public Ball(Rectangle rect)
        {
            Rect = rect;
        }

        /// <summary>
        /// Перемещает мяч в указанные координаты, сохраняя его размеры.
        /// </summary>
        /// <param name="x">Новая координата X.</param>
        /// <param name="y">Новая координата Y.</param>
        public void MoveTo(int x, int y)
        {
            Rect = new Rectangle(x, y, Rect.Width, Rect.Height);
        }
    }
}
