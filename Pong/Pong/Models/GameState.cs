using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong.Models
{
    public class GameState
    {
        public bool Paused { get; set; }
        public bool ShowStats { get; set; }

        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public SpriteFont Font { get; set; }

        public KeyboardState CurrentKeyState { get; set; }
        public KeyboardState PreviousKeyState { get; set; }



        public GameState()
        {
            PreviousKeyState = Keyboard.GetState();
        }
    }
}
