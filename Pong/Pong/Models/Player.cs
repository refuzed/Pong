using Microsoft.Xna.Framework.Input;

namespace Pong.Models
{
    public class Player
    {
        public Keys MoveUp { get; set; }
        public Keys MoveDown { get; set; }
        public Keys Shoot { get; set; }
        public int Score { get; set; }
        public bool HasControl { get; set; }



        public Player()
        {
        }

        public Player(Keys moveUp, Keys moveDown, Keys shoot)
        {
            MoveUp = moveUp;
            MoveDown = moveDown;
            Shoot = shoot;
            Score = 0;
        }
    }
}
