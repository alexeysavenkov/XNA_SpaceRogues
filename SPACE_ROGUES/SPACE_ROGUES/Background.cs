using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SPACE_ROGUES
{
    static class Background
    {
        private static Texture2D backgroundTexture;
        private static GraphicsDevice graphicsDevice;

        private static int positionX;
        private static int sourceY;
        private const int Velocity = 1;
        public static bool Stopped = false;

        public static void LoadContent(ContentManager content)
        {
            backgroundTexture = content.Load<Texture2D>("Textures/background");

            Random rnd = new Random();
            positionX = rnd.Next(backgroundTexture.Width);
            sourceY = rnd.Next(backgroundTexture.Height - graphicsDevice.Viewport.Height);
        }

        public static void Init(GraphicsDevice device)
        {
            graphicsDevice = device;
        }

        public static void Update()
        {
            if (!Stopped)
            {
                positionX += Velocity;
                positionX %= backgroundTexture.Width;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, new Vector2(positionX - backgroundTexture.Width, 0), 
                new Rectangle(0, sourceY, backgroundTexture.Width, graphicsDevice.Viewport.Height), Color.White);
            
            spriteBatch.Draw(backgroundTexture, new Vector2(positionX, 0),
                new Rectangle(0, sourceY, backgroundTexture.Width, graphicsDevice.Viewport.Height), Color.White);
            
            spriteBatch.Draw(backgroundTexture, new Vector2(positionX + backgroundTexture.Width, 0),
                new Rectangle(0, sourceY, backgroundTexture.Width, graphicsDevice.Viewport.Height), Color.White);
        }
    }
}
