using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>Разрушаемый блок с хитпоинтами.</summary>
    internal class Block
    {
        public Rectangle Rect { get; }
        public int Health { get; private set; }
        public bool IsDestroyed { get; private set; }

        public Block(Rectangle rect, int health = 1)
        {
            Rect = rect;
            Health = health;
        }

        /// <summary>Наносит 1 урон блоку. Помечает как уничтоженный при HP ≤ 0.</summary>
        public void Hit()
        {
            Health -= 1;
            if (Health <= 0)
            {
                IsDestroyed = true;

            }
        }
    }
}