using System.Drawing;

namespace Arkanoid.Classes
{
    /// <summary>
    /// Представляет разрушаемый блок игрового поля.
    /// Каждый блок имеет прямоугольник столкновения, количество очков прочности (HP)
    /// и флаг состояния уничтожения.
    /// </summary>
    /// <remarks>
    /// Блок считается уничтоженным, когда его прочность (Health) падает до нуля или ниже.
    /// </remarks>
    internal class Block
    {
        /// <summary>
        /// Получает прямоугольник, описывающий позицию и размеры блока.
        /// </summary>
        public Rectangle Rect { get; }

        /// <summary>
        /// Получает текущее количество очков прочности блока.
        /// При достижении нуля блок считается уничтоженным.
        /// </summary>
        public int Health { get; private set; }

        /// <summary>
        /// Возвращает значение, показывающее, уничтожен ли блок.
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Block"/>.
        /// </summary>
        /// <param name="rect">Прямоугольник, определяющий позицию и размер блока.</param>
        /// <param name="health">Начальное количество очков прочности (по умолчанию 1).</param>
        public Block(Rectangle rect, int health = 1)
        {
            Rect = rect;
            Health = health;
        }

        /// <summary>
        /// Наносит блоку единицу урона.
        /// Если после удара прочность ≤ 0 — помечает блок как уничтоженный.
        /// </summary>
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
