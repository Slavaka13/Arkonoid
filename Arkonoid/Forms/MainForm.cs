using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Arkanoid.Classes;
using UiTimer = System.Windows.Forms.Timer;

namespace Arkanoid
{

    /// <summary>√лавна€ форма игры Arkanoid.</summary>
    public partial class MainForm : Form
    {

        // ЧЧЧ  онстанты/настройки (без Ђмагических чиселї) ЧЧЧ
        private const int TargetFps = 60;
        private const int InitialLives = 3;

        private const int BallSize = 20;
        private const int PlatformWidth = 110;
        private const int PlatformHeight = 20;

        private const int BlockRows = 10;
        private const int BlockCols = 6;
        private const int BlocksTopOffset = 50;

        // ЧЧЧ —осто€ние игры ЧЧЧ
        private readonly UiTimer _gameTimer = new();
        private readonly Random _random = new();

        private bool _isGameStarted;
        private int _minX, _maxX, _minY, _maxY;

        private Ball _ball = default!;
        private Platform _platform = default!;
        private List<Block> _blocks = new();
        private int _lives;
        private int _score;

        public MainForm()
        {

            // фикс корректного старта окна
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;        // без DPI-масштабировани€ формы
            ClientSize = new Size(800, 600);           // целевой рабочий размер
            MinimumSize = new Size(800 + 16, 600 + 39); // (опц.) запрет уменьшать: клиент + рамка
            FormBorderStyle = FormBorderStyle.FixedSingle;

            DoubleBuffered = true;
            BackColor = Color.Black;

            _gameTimer.Interval = 1000 / TargetFps;
            _gameTimer.Tick += OnGameTick;

            Load += OnFormLoad;
            Paint += OnFormPaint;
            MouseMove += OnFormMouseMove;
            MouseClick += OnFormMouseClick;
        }

        /// <summary>»нициализаци€ размеров окна и стартовых объектов.</summary>
        private void OnFormLoad(object? sender, EventArgs e)
        {
            _minX = 0;
            _maxX = ClientSize.Width;
            _minY = 0;
            _maxY = ClientSize.Height;

            ResetWorld(hardReset: true);
        }

        /// <summary>ѕолна€ или частична€ переинициализаци€ мира.</summary>
        private void ResetWorld(bool hardReset)
        {
            // платформа
            int startPlatformX = (_maxX - PlatformWidth) / 2;
            int startPlatformY = _maxY - 155;
            _platform = new Platform(new Rectangle(startPlatformX, startPlatformY, PlatformWidth, PlatformHeight));

            // м€ч на платформе
            int startBallX = _platform.Rect.X + (_platform.Rect.Width - BallSize) / 2;
            int startBallY = _platform.Rect.Y - BallSize - 4;
            _ball = new Ball(new Rectangle(startBallX, startBallY, BallSize, BallSize))
            {
                SpeedX = _random.Next(0, 2) == 0 ? -4 : 4,
                SpeedY = -7
            };

            if (hardReset)
            {
                _lives = InitialLives;
                _score = 0;
                _blocks = BuildBlocksGrid();
                _isGameStarted = false;
            }
        }

        /// <summary>—оздаЄт сетку блоков равномерно по экрану.</summary>
        private List<Block> BuildBlocksGrid()
        {
            var result = new List<Block>();

            int cellWidth = _maxX / BlockCols;
            int cellHeight = (_maxY / BlockRows) / 2;  // верхн€€ половина экрана

            for (int row = 0; row < BlockRows; row++)
            {
                for (int col = 0; col < BlockCols; col++)
                {
                    int x = col * cellWidth;
                    int y = BlocksTopOffset + row * cellHeight;

                    var rect = new Rectangle(
                        x + 1, y + 1,
                        cellWidth - 2, cellHeight - 2);

                    
                    int health = 1 + row / 4;
                    result.Add(new Block(rect, health));
                }
            }
            return result;
        }

        /// <summary>ќтрисовка текущего состо€ни€.</summary>
        private void OnFormPaint(object? sender, PaintEventArgs e)
        {
            // блоки
            foreach (var block in _blocks)
            {
                if (!block.IsDestroyed)
                {
                    Color color = block.Health switch
                    {
                        >= 3 => Color.IndianRed,
                        2 => Color.Peru,
                        _ => Color.SkyBlue
                    };

                    using var brush = new SolidBrush(color);
                    e.Graphics.FillRectangle(brush, block.Rect);
                    e.Graphics.DrawRectangle(Pens.Black, block.Rect);
                }
            }

            // платформа и м€ч
            e.Graphics.FillRectangle(Brushes.Orange, _platform.Rect);
            e.Graphics.FillEllipse(Brushes.White, _ball.Rect);

            // HUD
            using var hudBrush = new SolidBrush(Color.White);
            e.Graphics.DrawString($"Score: {_score}", Font, hudBrush, 8, 8);
            e.Graphics.DrawString($"Lives: {_lives}", Font, hudBrush, 8, 28);
            if (!_isGameStarted)
            {
                e.Graphics.DrawString("Click to start", Font, hudBrush, _maxX / 2 - 50, _maxY - 40);
            }
        }

        /// <summary>ƒвигаем платформу мышью. ƒо старта Ч везЄм за ней м€ч.</summary>
        private void OnFormMouseMove(object? sender, MouseEventArgs e)
        {
            int clampedX = Math.Max(_minX, Math.Min(_maxX - _platform.Rect.Width, e.X - _platform.Rect.Width / 2));
            _platform.MoveToX(clampedX);

            if (!_isGameStarted)
            {
                int ballX = _platform.Rect.X + (_platform.Rect.Width - _ball.Rect.Width) / 2;
                _ball.MoveTo(Math.Max(_minX, Math.Min(_maxX - _ball.Rect.Width, ballX)), _ball.Rect.Y);
                Invalidate();
            }
        }

        /// <summary>—тарт игры по клику.</summary>
        private void OnFormMouseClick(object? sender, MouseEventArgs e)
        {
            if (!_isGameStarted)
            {
                _isGameStarted = true;
                _gameTimer.Start();
            }
        }

        /// <summary>√лавный игровой тик: движение, коллизии, проверка состо€ни€.</summary>
        private void OnGameTick(object? sender, EventArgs e)
        {
            UpdateBallPosition();
            HandleWallCollisions();
            HandlePlatformCollision();
            HandleBlocksCollisions();

            if (AllBlocksDestroyed())
            {
                _gameTimer.Stop();
                MessageBox.Show("ѕобеда! ”ровень пройден.", "Arkanoid");
                ResetWorld(hardReset: true);
                Invalidate();
                return;
            }

            if (_ball.Rect.Top > _maxY)
            {
                _gameTimer.Stop();
                _lives--;

                if (_lives <= 0)
                {
                    MessageBox.Show($"»гра окончена.\n—чЄт: {_score}", "Arkanoid");
                    ResetWorld(hardReset: true);
                    Invalidate();
                    return;
                }

                // нова€ жизнь: ставим м€ч на платформу и сразу продолжаем
                ResetWorld(hardReset: false);
                _isGameStarted = true;      // уже в игре
                _gameTimer.Start();         // продолжить без клика
                Invalidate();
                return;
            }


            Invalidate();
        }

        /// <summary>ќбновл€ет позицию м€ча на основании скорости.</summary>
        private void UpdateBallPosition()
        {
            _ball.MoveTo(_ball.Rect.X + _ball.SpeedX, _ball.Rect.Y + _ball.SpeedY);
        }

        /// <summary>ќтскоки от стен и потолка.</summary>
        private void HandleWallCollisions()
        {
            if (_ball.Rect.Left <= _minX || _ball.Rect.Right >= _maxX)
            {
                _ball.SpeedX = -_ball.SpeedX;
            }

            if (_ball.Rect.Top <= _minY)
            {
                _ball.SpeedY = -_ball.SpeedY;
            }
        }

        /// <summary>ќтскок от платформы с зависимостью угла от точки попадани€.</summary>
        private void HandlePlatformCollision()
        {
            if (_ball.Rect.IntersectsWith(_platform.Rect) && _ball.SpeedY > 0)
            {
                int hitCenterX = _ball.Rect.X + _ball.Rect.Width / 2;
                int relativeX = hitCenterX - _platform.Rect.X;               // рассто€ние от левого кра€ платформы
                int segment = _platform.Rect.Width / 3;

                if (relativeX < segment)
                {
                    _ball.SpeedX = -_random.Next(3, 6);
                }
                else if (relativeX < 2 * segment)
                {
                    _ball.SpeedX = 0;
                }
                else
                {
                    _ball.SpeedX = _random.Next(3, 6);
                }

                _ball.SpeedY = -Math.Abs(_ball.SpeedY);
                // выталкиваем м€ч над платформой, чтобы не залип
                _ball.MoveTo(_ball.Rect.X, _platform.Rect.Y - _ball.Rect.Height - 1);
            }
        }

        /// <summary>ќбработка столкновений м€ча с блоками.</summary>
        private void HandleBlocksCollisions()
        {
            foreach (var block in _blocks)
            {
                if (block.IsDestroyed)
                {
                    continue;
                }

                if (_ball.Rect.IntersectsWith(block.Rect))
                {
                    // простое определение стороны удара: по глубине перекрыти€
                    Rectangle overlap = GetOverlap(_ball.Rect, block.Rect);
                    if (overlap.Width < overlap.Height)
                    {
                        _ball.SpeedX = -_ball.SpeedX;
                    }
                    else
                    {
                        _ball.SpeedY = -_ball.SpeedY;
                    }

                    block.Hit();
                    if (block.IsDestroyed)
                    {
                        _score += 100;
                    }
                    break; // за тик Ч максимум один блок
                }
            }
        }

        /// <summary>ѕровер€ет, все ли блоки уничтожены.</summary>
        private bool AllBlocksDestroyed()
        {
            return _blocks.All(b => b.IsDestroyed);
        }

        /// <summary>¬ычисл€ет пр€моугольник перекрыти€ двух пр€моугольников.</summary>
        private static Rectangle GetOverlap(Rectangle a, Rectangle b)
        {
            int left = Math.Max(a.Left, b.Left);
            int top = Math.Max(a.Top, b.Top);
            int right = Math.Min(a.Right, b.Right);
            int bottom = Math.Min(a.Bottom, b.Bottom);

            int width = Math.Max(0, right - left);
            int height = Math.Max(0, bottom - top);
            return new Rectangle(left, top, width, height);
        }
    }
}
