using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>Платформа игрока.</summary>
    internal class Platform
    {
        public Rectangle Rect { get; private set; }

        public Platform(Rectangle rect)
        {
            Rect = rect;
        }

        /// <summary>Перемещает платформу по оси X, сохраняя Y и размеры.</summary>
        public void MoveToX(int x)
        {
            Rect = new Rectangle(x, Rect.Y, Rect.Width, Rect.Height);
        }
    }
}