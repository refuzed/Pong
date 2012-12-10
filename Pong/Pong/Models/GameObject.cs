using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong.Models
{
    public class GameObject
    {
        private Vector2 _position;
        private Rectangle _shape;

        public Vector2 Speed { get; set; }
        public Color Color { get; set; }
        public Texture2D Texture { get; set; }
        public Player Player { get; set; }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _shape.X = (int) value.X;
                _shape.Y = (int) value.Y;
            }
        }

        public Rectangle Shape
        {
            get { return _shape; }
            set
            {
                _shape = value;
                Position = new Vector2(value.X, value.Y);
            }
        }

        public enum Name
        {
            Ball,
            Line,
            Paddle1,
            Paddle2
        }

        public GameObject(Rectangle shape, Vector2 speed, Texture2D texture, Color color, Player player)
        {
            Shape = shape;
            Speed = speed;
            Color = color;
            Texture = texture;
            Player = player;
        }
    }
}
