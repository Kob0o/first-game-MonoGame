// Tile.cs
using Microsoft.Xna.Framework.Graphics;

namespace first_game
{
    public class Tile
    {
        public Texture2D Texture { get; set; }
        public int LayoutValue { get; set; }
        public bool IsWall => Texture.Name == "mur";
    }
}
