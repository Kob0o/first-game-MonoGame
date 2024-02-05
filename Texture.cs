// Map.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace first_game
{
    /// <summary> toto </summary>
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness = 1)
        {
            spriteBatch.DrawPixel(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color); // Top
            spriteBatch.DrawPixel(new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color); // Bottom
            spriteBatch.DrawPixel(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color); // Left
            spriteBatch.DrawPixel(new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color); // Right
        }

        public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(pixelTexture, rectangle, color);
        }

        private static Texture2D pixelTexture;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        private static void DrawPixel(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(pixelTexture, rectangle, color);
        }
    }

}