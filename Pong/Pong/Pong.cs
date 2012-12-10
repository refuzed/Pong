using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        private Dictionary<GameObject.Name, GameObject> _gameObjects;
        private Dictionary<Player.Name, Player> _players;

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

            _players = new Dictionary<Player.Name, Player>
                {
                    {Player.Name.Player1, new Player(Keys.A, Keys.Z, Keys.Space)},
                    {Player.Name.Player2, new Player(Keys.Up, Keys.Down, Keys.Space)}
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

            _gameObjects = new Dictionary<GameObject.Name, GameObject>
                {
                    {
                        GameObject.Name.Ball,
                        new GameObject(new Rectangle(90, 10, 15, 15), 
                                       new Vector2(rand.Next(3,5), rand.Next(3,5)), 
                                       dummyTexture, 
                                       Color.White,
                                       new Player())
                    },
                    {
                        GameObject.Name.Paddle1,
                        new GameObject(new Rectangle(50, 50, 15, 100), 
                                       new Vector2(0, 5), 
                                       dummyTexture, 
                                       Color.White,
                                       _players[Player.Name.Player1])
                    },
                    {
                        GameObject.Name.Paddle2,
                        new GameObject(new Rectangle(_gameState.MaxX - 65, 50, 15, 100), 
                                       new Vector2(0, 5), 
                                       dummyTexture,
                                       Color.White, 
                                       _players[Player.Name.Player2])
                    },
                    {
                        GameObject.Name.Line,
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
                UpdatePaddle(_gameObjects[GameObject.Name.Paddle1]);
                UpdatePaddle(_gameObjects[GameObject.Name.Paddle2]);

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
        }

        private void UpdatePaddle(GameObject paddle)
        {
            if (_gameState.CurrentKeyState.IsKeyDown(paddle.Player.MoveUp) && paddle.Shape.Y > 0)
            {
                paddle.Position -= paddle.Speed;
                if (paddle.Player.HasControl)
                    _gameObjects[GameObject.Name.Ball].Position -= paddle.Speed; 
            }
            if (_gameState.CurrentKeyState.IsKeyDown(paddle.Player.MoveDown) && paddle.Shape.Y < _gameState.MaxY - paddle.Shape.Height)
            {
                paddle.Position += paddle.Speed;
                if (paddle.Player.HasControl)
                    _gameObjects[GameObject.Name.Ball].Position += paddle.Speed; 
            }
            if(_gameState.CurrentKeyState.IsKeyDown(paddle.Player.Shoot) && paddle.Player.HasControl)
            {
                var rand = new Random();
                _gameObjects[GameObject.Name.Ball].Speed = new Vector2(rand.Next(3,5), rand.Next(3,5));
                paddle.Player.HasControl = false;
            }
        }

        private void UpdateBall()
        {
            _gameObjects[GameObject.Name.Ball].Position += _gameObjects[GameObject.Name.Ball].Speed;
        }

        private bool CheckPaddleBounce()
        {
            if (_gameObjects[GameObject.Name.Ball].Shape.Intersects(_gameObjects[GameObject.Name.Paddle1].Shape) && _gameObjects[GameObject.Name.Ball].Speed.X < 0 ||
                _gameObjects[GameObject.Name.Ball].Shape.Intersects(_gameObjects[GameObject.Name.Paddle2].Shape) && _gameObjects[GameObject.Name.Ball].Speed.X > 0 )
            {
                _gameObjects[GameObject.Name.Ball].Speed = new Vector2(-_gameObjects[GameObject.Name.Ball].Speed.X * 1.05f, _gameObjects[GameObject.Name.Ball].Speed.Y * 1.05f);
                return true;
            }
            return false;
        }

        private void CheckWallBounce()
        {
            var ball = _gameObjects[GameObject.Name.Ball];

            if (ball.Position.Y < 0 || ball.Position.Y > _gameState.MaxY - ball.Shape.Height)
            {
                ball.Speed = new Vector2(ball.Speed.X, -ball.Speed.Y);
            }
            else if (ball.Position.X < 0)
            {
                _players[Player.Name.Player2].Score++;
                _players[Player.Name.Player1].HasControl = true;
                ResetBall(ball, GameObject.Name.Paddle1);
            }
            else if (ball.Position.X > _gameState.MaxX - ball.Shape.Width)
            {
                _players[Player.Name.Player1].Score++;
                _players[Player.Name.Player2].HasControl = true;
                ResetBall(ball, GameObject.Name.Paddle2);
            }
        }

        private void ResetBall(GameObject ball, GameObject.Name paddle)
        {
            ball.Speed = new Vector2(0, 0);
            ball.Position = _gameObjects[paddle].Position +
                new Vector2(paddle == GameObject.Name.Paddle1 ? _gameObjects[paddle].Shape.Width : -ball.Shape.Width,
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
                                    _players[Player.Name.Player1].Score.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(_gameState.MaxX / 2 - 30, 50), Color.White);
            _spriteBatch.DrawString(_gameState.Font,
                                    _players[Player.Name.Player2].Score.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(_gameState.MaxX / 2 + 25, 50), Color.White);
        }

        private void StatsDisplay()
        {
            if (_gameState.ShowStats)
            {
                var ballStats = new StringBuilder();
                ballStats.Append("X Speed: " + _gameObjects[GameObject.Name.Ball].Speed.X.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nY Speed: " + _gameObjects[GameObject.Name.Ball].Speed.Y.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nX Pos: " + _gameObjects[GameObject.Name.Ball].Position.X.ToString(CultureInfo.InvariantCulture));
                ballStats.Append("\nY Pos: " + _gameObjects[GameObject.Name.Ball].Position.Y.ToString(CultureInfo.InvariantCulture));
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
