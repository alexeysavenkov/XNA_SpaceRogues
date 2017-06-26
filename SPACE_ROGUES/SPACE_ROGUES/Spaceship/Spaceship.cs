using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Generator = SpaceshipGenerator.TextureGenerator;

namespace SPACE_ROGUES
{
    public abstract class Spaceship
    {
        public static readonly Color[] ColorCollection = new []
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.White
        };

        protected readonly static Random random = new Random();
        protected readonly Texture2D texture;
        protected readonly bool[,] collisionMap; //[height, width]

        protected readonly Vector2 origin;
        public Color Color;

        protected Color actualColor
        {
            get
            {
                float f = Health/100f;
                Color actColor = new Color((int)(Color.R*f), (int)(Color.G*f), (int)(Color.B*f));
                return actColor;
            }
        }

        public Rectangle Bounds;

        public Point Velocity;

        public Laser Laser;
        public int weaponCooldown = Laser.FireRate;
        public Rocket Rocket;

        public bool IsFloating = false;
        private float _rotation = 0f;

        public Rectangle ActualBounds // Considering origin
        {
            get { return new Rectangle(Bounds.X - 48, Bounds.Y - 48, 96, 96); }
        }

        public int Health = 100;

        public void SetPosition(int x, int y)
        {
            Bounds = new Rectangle(x, y, Bounds.Width, Bounds.Height);
        }

        public bool CollidesWith(Spaceship ship)
        {
            Rectangle Intersection = Rectangle.Intersect(Bounds, ship.Bounds);

            for (int h = Intersection.Top; h < Intersection.Bottom; h++)
            {
                for (int w = Intersection.Left; w < Intersection.Right; w++)
                {
                    if (collisionMap[h - Bounds.Y, w - Bounds.X] &&
                        ship.collisionMap[h - ship.Bounds.Y, w - ship.Bounds.X])
                    {
                        //ExportCollisionMap(new Point(w - bounds.X, h - bounds.Y), "D:\\enemy.txt");
                        //ship.ExportCollisionMap(new Point(w - ship.bounds.X, h - ship.bounds.Y), "D:\\player.txt");
                        return true;
                    }
                }
            }

            return false;
        }
        public bool CollidesWith(Bullet bullet)
        {
            if (bullet.sourceWeapon.owner.Equals(this))
            {
                return false;
            }

            Rectangle Intersection = Rectangle.Intersect(ActualBounds, bullet.Bounds);

            Rectangle actBounds = ActualBounds;

            for (int h = Intersection.Top; h < Intersection.Bottom; h++)
            {
                for (int w = Intersection.Left; w < Intersection.Right; w++)
                {
                    if (collisionMap[h - actBounds.Y, w - actBounds.X])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ExportCollisionMap(Point highlight, string filename)
        {
            StringBuilder builder = new StringBuilder();

            for (int h = 0; h < 96; h++)
            {
                for (int w = 0; w < 96; w++)
                {
                    if (h == highlight.Y && w == highlight.X)
                    {
                        builder.Append('X');
                    }
                    else
                    {
                        builder.Append(collisionMap[h, w] ? '+' : ' ');
                    }
                }
                builder.AppendLine();
            }

            File.WriteAllText(filename, builder.ToString());
        }
        
        public bool Contains(Point point)
        {
            return ActualBounds.Contains(point);
        }
        protected Spaceship(int scale, Vector2 pos, GraphicsDevice graphicDevice, Color color)
        {
            Laser = new Laser(this);
            Rocket = new Rocket(this);

            Color = color;

            texture = Generator.GetCompletedRandomSpaceshipTexture(graphicDevice, scale, Color.White);

            Bounds = new Rectangle((int)pos.X, (int)pos.Y, texture.Width, texture.Height);

            collisionMap = new bool[Bounds.Height,Bounds.Width];
            Color[] colors = new Color[Bounds.Height*Bounds.Width];
            texture.GetData<Color>(colors);
            for (int i = 0, h = 0, w = 0; i < colors.Length; i++, w++)
            {
                if (w == Bounds.Width)
                {
                    w = 0;
                    h++;
                }
                collisionMap[h, w] = (colors[i].A != 0);
            }

            bool[,] rotatedMap = new bool[96, 96];

            for (int x = 0; x < 96; x++)
            {
                for (int y = 0; y < 96; y++)
                {
                    rotatedMap[96 - 1 - y, x] = collisionMap[x, y];
                }
            }

            collisionMap = rotatedMap;

            origin = new Vector2(texture.Width/2, texture.Height/2);
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        public virtual void Update(GameTime gameTime)
        {
            Bounds.Offset(Velocity);
        }
    }

    public sealed class EnemyShip : Spaceship
    {
        public static BonusCollection BonusCollection = new BonusCollection(OwnerType.Enemy);

        public int? rocketFireIn; //Milliseconds; null = already fired
        public bool laserFiring = false;
        public int laserNextTriggerIn; //Milliseconds

        public EnemyShip(int scale, Vector2 pos, Point vel, Color color, GraphicsDevice device)
            : base(scale, pos, device, color)
        {
            laserNextTriggerIn = random.Next(500, 3000);
            rocketFireIn = random.Next(500, 5000);
            Velocity = vel;

            // Flipping collision map
            bool tmp;
            for (int x = 0; x < 48; x++)
            {
                for (int y = 0; y < 96; y++)
                {
                    tmp = collisionMap[y, 95 - x];
                    collisionMap[y, 95 - x] = collisionMap[y, x];
                    collisionMap[y, x] = tmp;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Bounds, null, actualColor, (float)Math.PI/2, origin, SpriteEffects.None, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            if (laserNextTriggerIn > 0)
            {
                laserNextTriggerIn -= gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                laserNextTriggerIn = random.Next(500, 3000);
                laserFiring = !laserFiring;
            }

            if (rocketFireIn.HasValue)
            {
                rocketFireIn -= gameTime.ElapsedGameTime.Milliseconds;
            }

            base.Update(gameTime);
        }
    }
    public sealed class AllyShip : Spaceship
    {
        public AllyShip(int scale, Vector2 pos, Point vel, Color color, GraphicsDevice device)
            : base(scale, pos, device, color)
        {
            Velocity = vel;

            // Flipping collision map
            bool tmp;
            for (int x = 0; x < 48; x++)
            {
                for (int y = 0; y < 96; y++)
                {
                    tmp = collisionMap[y, 95 - x];
                    collisionMap[y, 95 - x] = collisionMap[y, x];
                    collisionMap[y, x] = tmp;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Bounds, null, actualColor, (float)Math.PI / 2, origin, SpriteEffects.None, 1f);
        }
    }
    public sealed class PlayerShip : Spaceship
    {
        public static BonusCollection BonusCollection;
        public BonusCollection bonusCollection = BonusCollection.Random(OwnerType.Player);

        public int RocketCooldown; //Milliseconds

        public PlayerShip(int scale, Vector2 pos, GraphicsDevice device, Color color)
            : base(scale, pos, device, color)
        {}

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Bounds, null, actualColor, (float)-Math.PI/2, origin, SpriteEffects.None, 0);
        }
        public void Draw(SpriteBatch spriteBatch, float opacity = 1f, float rotation = 0)
        {
            spriteBatch.Draw(texture, Bounds, null, actualColor*opacity, rotation, origin, SpriteEffects.None, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (RocketCooldown > 0)
            {
                RocketCooldown -= gameTime.ElapsedGameTime.Milliseconds;
            }

            base.Update(gameTime);
        }
    }
}
