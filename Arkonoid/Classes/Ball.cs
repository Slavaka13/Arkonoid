using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>Мяч: прямоугольник хитбокса и текущие скорости по осям.</summary>
    internal class Ball
    {
        public Rectangle Rect { get; private set; }
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }

        public Ball(Rectangle rect)
        {
            Rect = rect;
        }

        /// <summary>Задаёт абсолютные координаты мяча, сохраняя размеры.</summary>
        public void MoveTo(int x, int y)
        {
            Rect = new Rectangle(x, y, Rect.Width, Rect.Height);
        }
    }
}