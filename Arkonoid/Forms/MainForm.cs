using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Arkanoid.Classes;
using UiTimer = System.Windows.Forms.Timer;

namespace Arkanoid
{
    /// <summary>������� ����� ���� Arkanoid.</summary>
    public partial class MainForm : Form
    {
        // ��� ���������/��������� (��� ����������� �����) ���
        private const int TargetFps = 60;
        private const int InitialLives = 3;

        private const int BallSize = 20;
        private const int PlatformWidth = 110;
        private const int PlatformHeight = 20;

        private const int BlockRows = 10;
        private const int BlockCols = 6;
        private const int BlocksTopOffset = 50;

        // ��� ��������� ���� (camelCase, ��� �������������) ���
        private readonly UiTimer gameTimer = new();
        private readonly Random random = new();

        private bool isGameStarted;
        private int minX, maxX, minY, maxY;

        private Ball ball = default!;
        private Platform platform = default!;
        private List<Block> blocks = new();
        private int lives;
        private int score;

        public MainForm()
        {
            // �����: �������������, ������� ���������� ��������
            

            // ���� ����������� ������ ����
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;         // ��� DPI-��������������� �����
            ClientSize = new Size(800, 600);            // ������� ������� ������
            MinimumSize = new Size(800 + 16, 600 + 39); // (���.) ������ ���������: ������ + �����
            FormBorderStyle = FormBorderStyle.FixedSingle;

            DoubleBuffered = true;
            BackColor = Color.Black;

            gameTimer.Interval = 1000 / TargetFps;
            gameTimer.Tick += OnGameTick;

            Load += OnFormLoad;
            Paint += OnFormPaint;
            MouseMove += OnFormMouseMove;
            MouseClick += OnFormMouseClick;
        }

        /// <summary>������������� �������� ���� � ��������� ��������.</summary>
        private void OnFormLoad(object? sender, EventArgs e)
        {
            minX = 0;
            maxX = ClientSize.Width;
            minY = 0;
            maxY = ClientSize.Height;

            ResetWorld(hardReset: true);
        }

        /// <summary>������ ��� ��������� ����������������� ����.</summary>
        private void ResetWorld(bool hardReset)
        {
            // ���������
            int startPlatformX = (maxX - PlatformWidth) / 2;
            int startPlatformY = maxY - 155;
            platform = new Platform(new Rectangle(startPlatformX, startPlatformY, PlatformWidth, PlatformHeight));

            // ��� �� ���������
            int startBallX = platform.Rect.X + (platform.Rect.Width - BallSize) / 2;
            int startBallY = platform.Rect.Y - BallSize - 4;
            ball = new Ball(new Rectangle(startBallX, startBallY, BallSize, BallSize))
            {
                SpeedX = random.Next(0, 2) == 0 ? -4 : 4,
                SpeedY = -7
            };

            if (hardReset)
            {
                lives = InitialLives;
                score = 0;
                blocks = BuildBlocksGrid();
                isGameStarted = false;
            }
        }

        /// <summary>������ ����� ������ ���������� �� ������.</summary>
        private List<Block> BuildBlocksGrid()
        {
            var result = new List<Block>();

            int cellWidth = maxX / BlockCols;
            int cellHeight = (maxY / BlockRows) / 2;  // ������� �������� ������

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

        /// <summary>��������� �������� ���������.</summary>
        private void OnFormPaint(object? sender, PaintEventArgs e)
        {
            // �����
            foreach (var block in blocks)
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

            // ��������� � ���
            e.Graphics.FillRectangle(Brushes.Orange, platform.Rect);
            e.Graphics.FillEllipse(Brushes.White, ball.Rect);

            // HUD
            using var hudBrush = new SolidBrush(Color.White);
            e.Graphics.DrawString($"Score: {score}", Font, hudBrush, 8, 8);
            e.Graphics.DrawString($"Lives: {lives}", Font, hudBrush, 8, 28);
            if (!isGameStarted)
            {
                e.Graphics.DrawString("Click to start", Font, hudBrush, maxX / 2 - 50, maxY - 40);
            }
        }

        /// <summary>������� ��������� �����. �� ������ � ���� �� ��� ���.</summary>
        private void OnFormMouseMove(object? sender, MouseEventArgs e)
        {
            int clampedX = Math.Max(minX, Math.Min(maxX - platform.Rect.Width, e.X - platform.Rect.Width / 2));
            platform.MoveToX(clampedX);

            if (!isGameStarted)
            {
                int ballX = platform.Rect.X + (platform.Rect.Width - ball.Rect.Width) / 2;
                ball.MoveTo(Math.Max(minX, Math.Min(maxX - ball.Rect.Width, ballX)), ball.Rect.Y);
                Invalidate();
            }
        }

        /// <summary>����� ���� �� �����.</summary>
        private void OnFormMouseClick(object? sender, MouseEventArgs e)
        {
            if (!isGameStarted)
            {
                isGameStarted = true;
                gameTimer.Start();
            }
        }

        /// <summary>������� ������� ���: ��������, ��������, �������� ���������.</summary>
        private void OnGameTick(object? sender, EventArgs e)
        {
            UpdateBallPosition();
            HandleWallCollisions();
            HandlePlatformCollision();
            HandleBlocksCollisions();

            if (AllBlocksDestroyed())
            {
                gameTimer.Stop();
                MessageBox.Show("������! ������� �������.", "Arkanoid");
                ResetWorld(hardReset: true);
                Invalidate();
                return;
            }

            if (ball.Rect.Top > maxY)
            {
                gameTimer.Stop();
                lives--;

                if (lives <= 0)
                {
                    MessageBox.Show($"���� ��������.\n����: {score}", "Arkanoid");
                    ResetWorld(hardReset: true);
                    Invalidate();
                    return;
                }

                // ����� �����: ������ ��� �� ��������� � ����� ����������
                ResetWorld(hardReset: false);
                isGameStarted = true;
                gameTimer.Start();
                Invalidate();
                return;
            }

            Invalidate();
        }

        /// <summary>��������� ������� ���� �� ��������� ��������.</summary>
        private void UpdateBallPosition()
        {
            ball.MoveTo(ball.Rect.X + ball.SpeedX, ball.Rect.Y + ball.SpeedY);
        }

        /// <summary>������� �� ���� � �������.</summary>
        private void HandleWallCollisions()
        {
            if (ball.Rect.Left <= minX || ball.Rect.Right >= maxX)
            {
                ball.SpeedX = -ball.SpeedX;
            }

            if (ball.Rect.Top <= minY)
            {
                ball.SpeedY = -ball.SpeedY;
            }
        }

        /// <summary>������ �� ��������� � ������������ ���� �� ����� ���������.</summary>
        private void HandlePlatformCollision()
        {
            if (ball.Rect.IntersectsWith(platform.Rect) && ball.SpeedY > 0)
            {
                int hitCenterX = ball.Rect.X + ball.Rect.Width / 2;
                int relativeX = hitCenterX - platform.Rect.X; // ���������� �� ������ ���� ���������
                int segment = platform.Rect.Width / 3;

                if (relativeX < segment)
                {
                    ball.SpeedX = -random.Next(3, 6);
                }
                else if (relativeX < 2 * segment)
                {
                    ball.SpeedX = 0;
                }
                else
                {
                    ball.SpeedX = random.Next(3, 6);
                }

                ball.SpeedY = -Math.Abs(ball.SpeedY);
                // ����������� ��� ��� ����������, ����� �� �����
                ball.MoveTo(ball.Rect.X, platform.Rect.Y - ball.Rect.Height - 1);
            }
        }

        /// <summary>��������� ������������ ���� � �������.</summary>
        private void HandleBlocksCollisions()
        {
            foreach (var block in blocks)
            {
                if (block.IsDestroyed)
                {
                    continue;
                }

                if (ball.Rect.IntersectsWith(block.Rect))
                {
                    // ������� ����������� ������� �����: �� ������� ����������
                    Rectangle overlap = GetOverlap(ball.Rect, block.Rect);
                    if (overlap.Width < overlap.Height)
                    {
                        ball.SpeedX = -ball.SpeedX;
                    }
                    else
                    {
                        ball.SpeedY = -ball.SpeedY;
                    }

                    block.Hit();
                    if (block.IsDestroyed)
                    {
                        score += 100;
                    }
                    break; // �� ��� � �������� ���� ����
                }
            }
        }

        /// <summary>���������, ��� �� ����� ����������.</summary>
        private bool AllBlocksDestroyed() => blocks.All(b => b.IsDestroyed);

        /// <summary>��������� ������������� ���������� ���� ���������������.</summary>
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
