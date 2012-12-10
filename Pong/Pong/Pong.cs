using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pong.Models;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Pong : Game
    {
        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        private GameState _gameState;
        private Dictionary<PlayerName, Player> _players;
        private Dictionary<ObjectName, GameObject> _gameObjects;

        public enum PlayerName
        {
            Player1,
            Player2
        }

        public enum ObjectName
        {
            Ball,
            Line,
            Paddle1,
            Paddle2
        }

        public Pong()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;
            _gameState = new GameState();
            _gameState.MaxX = _graphics.GraphicsDevice.Viewport.Width;
            _gameState.MaxY = _graphics.GraphicsDevice.Viewport.Height;
            _gameState.Font = Content.Load<SpriteFont>("DefaultFont");

            _players = new Dictionary<PlayerName, Player>
                {
                    {PlayerName.Player1, new Player(Keys.A, Keys.Z, Keys.Space)},
                    {PlayerName.Player2, new Player(Keys.Up, Keys.Down, Keys.Space)}
                };

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var rand = new Random();

            var dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
            dummyTexture.SetData(new[] { Color.White });

            _gameObjects = new Dictionary<ObjectName, GameObject>
                {
                    {
                        ObjectName.Ball,
                        new GameObject(new Rectangle(90, 10, 15, 15), 
                                       new Vector2(rand.Next(3,5), rand.Next(3,5)), 
                                       dummyTexture, 
                                       Color.White,
                                       new Player())
                    },
                    {
                        ObjectName.Paddle1,
                        new GameObject(new Rectangle(50, 50, 15, 100), 
                                       new Vector2(0, 5), 
                                       dummyTexture, 
                                       Color.White,
                                       _players[PlayerName.Player1])
                    },
                    {
                        ObjectName.Paddle2,
                        new GameObject(new Rectangle(_gameState.MaxX - 65, 50, 15, 100), 
                                       new Vector2(0, 5), 
                                       dummyTexture,
                                       Color.White, 
                                       _players[PlayerName.Player2])
                    },
                    {
                        ObjectName.Line,
                        new GameObject(new Rectangle(_gameState.MaxX / 2, 0, 5, _gameState.MaxY), 
                                       new Vector2(0, 0), 
                                       dummyTexture,
                                       Color.White, 
                                       new Player())
                    }
                };
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
            _spriteBatch.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _gameState.CurrentKeyState = Keyboard.GetState();

            CheckMiscKeys();

            if(!_gameState.Paused)
            {
                UpdatePaddle(_gameObjects[ObjectName.Paddle1]);
                UpdatePaddle(_gameObjects[ObjectName.Paddle2]);

                UpdateBall();

                if (!CheckPaddleBounce())
                    CheckWallBounce();
            }

            _gameState.PreviousKeyState = _gameState.CurrentKeyState;

            base.Update(gameTime);
        }

        public void CheckMiscKeys()
        {
            if (_gameState.CurrentKeyState.IsKeyDown(Keys.S) && _gameState.PreviousKeyState.IsKeyUp(Keys.S))
            {
                _gameState.ShowStats = !_gameState.ShowStats;
            }
            if (_gameState.CurrentKeyState.IsKeyDown(Keys.P) && _gameState.PreviousKeyState.IsKeyUp(Keys.P))
            {
                _gameState.Paused = !_gameState.Paused;
            }
            if(_gameState.CurrentKeyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
        }

        private void UpdatePaddle(GameObject paddle)
        {
            if (_gameState.CurrentKeyState.IsKeyDown(paddle.Player.MoveUp) && paddle.Shape.Y > 0)
            {
                paddle.Position -= paddle.Speed;
                if (paddle.Player.HasControl)
                    _gameObjects[ObjectName.Ball].Position -= paddle.Speed; 
            }
            if (_gameState.CurrentKeyState.IsKeyDown(paddle.Player.MoveDown) && paddle.Shape.Y < _gameState.MaxY - paddle.Shape.Height)
            {
                paddle.Position += paddle.Speed;
                if (paddle.Player.HasControl)
                    _gameObjects[ObjectName.Ball].Position += paddle.Speed; 
            }
            if(_gameState.CurrentKeyState.IsKeyDown(paddle.Player.Shoot) && paddle.Player.HasControl)
            {
                var rand = new Random();
                _gameObjects[ObjectName.Ball].Speed = new Vector2(rand.Next(3,5), rand.Next(3,5));
                paddle.Player.HasControl = false;
            }
        }

        private void UpdateBall()
        {
            _gameObjects[ObjectName.Ball].Position += _gameObjects[ObjectName.Ball].Speed;
        }

        private bool CheckPaddleBounce()
        {
            if (_gameObjects[ObjectName.Ball].Shape.Intersects(_gameObjects[ObjectName.Paddle1].Shape) && _gameObjects[ObjectName.Ball].Speed.X < 0 ||
                _gameObjects[ObjectName.Ball].Shape.Intersects(_gameObjects[ObjectName.Paddle2].Shape) && _gameObjects[ObjectName.Ball].Speed.X > 0 )
            {
                _gameObjects[ObjectName.Ball].Speed = new Vector2(-_gameObjects[ObjectName.Ball].Speed.X * 1.05f, _gameObjects[ObjectName.Ball].Speed.Y * 1.05f);
                return true;
            }
            return false;
        }

        private void CheckWallBounce()
        {
            var ball = _gameObjects[ObjectName.Ball];

            if (ball.Position.Y < 0 || ball.Position.Y > _gameState.MaxY - ball.Shape.Height)
            {
                ball.Speed = new Vector2(ball.Speed.X, -ball.Speed.Y);
            }
            else if (ball.Position.X < 0)
            {
                _players[PlayerName.Player2].Score++;
                _players[PlayerName.Player1].HasControl = true;
                ResetBall(ball, ObjectName.Paddle1);
            }
            else if (ball.Position.X > _gameState.MaxX - ball.Shape.Width)
            {
                _players[PlayerName.Player1].Score++;
                _players[PlayerName.Player2].HasControl = true;
                ResetBall(ball, ObjectName.Paddle2);
            }
        }

        private void ResetBall(GameObject ball, ObjectName paddle)
        {
            ball.Speed = new Vector2(0, 0);
            ball.Position = _gameObjects[paddle].Position +
                new Vector2(paddle == ObjectName.Paddle1 ? _gameObjects[paddle].Shape.Width : -ball.Shape.Width,
                                        _gameObjects[paddle].Shape.Height / 2.0f - ball.Shape.Height / 2.0f);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            foreach (var o in _gameObjects)
            {
                _spriteBatch.Draw(o.Value.Texture, o.Value.Shape, o.Value.Color);
            }

            ScoreDisplay();
            StatsDisplay();
            PauseDisplay();

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ScoreDisplay()
        {
            _spriteBatch.DrawString(_gameState.Font,
                                    _players[PlayerName.Player1].Score.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(_gameState.MaxX / 2 - 30, 50), Color.White);
            _spriteBatch.DrawString(_gameState.Font,
                                    _players[PlayerName.Player2].Score.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(_gameState.MaxX / 2 + 25, 50), Color.White);
        }

        private void StatsDisplay()
        {
            if (_gameState.ShowStats)
            {
                var ballStats = new StringBuilder();
                ballStats.Append("X Speed: " + _gameObjects[ObjectName.Ball].Speed.X.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nY Speed: " + _gameObjects[ObjectName.Ball].Speed.Y.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nX Pos: " + _gameObjects[ObjectName.Ball].Position.X.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nY Pos: " + _gameObjects[ObjectName.Ball].Position.Y.ToString(CultureInfo.InvariantCulture));
                _spriteBatch.DrawString(_gameState.Font, ballStats, new Vector2(5, 5), Color.White);
            }
        }

        private void PauseDisplay()
        {
            if (_gameState.Paused)
            {
                var texture = new Texture2D(GraphicsDevice, 1, 1);
                texture.SetData(new[] { Color.Black });
                _spriteBatch.Draw(texture, new Rectangle(_gameState.MaxX / 2 - 30, _gameState.MaxY / 2 - 2, 70, 20), Color.Black);
                _spriteBatch.DrawString(_gameState.Font, "PAUSED", new Vector2(_gameState.MaxX / 2.0f - 28, _gameState.MaxY / 2.0f), Color.White);
            }
        }
    }
}
