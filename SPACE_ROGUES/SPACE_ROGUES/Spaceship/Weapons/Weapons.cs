using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SPACE_ROGUES
{
    public class Bullet
    {
        public Weapon sourceWeapon;

        private static Texture2D textureEmpty;
        private Color color;
        public Rectangle Bounds;
        private int velocity;

        public Vector2 Center 
        {
            get
            {
                return new Vector2(Bounds.X + Bounds.Width/2, Bounds.Y + Bounds.Height/2);
            }
        }

        public Bullet(Rectangle rect, int vel, Color col, Weapon srcWeapon)
        {
            Bounds = rect;
            velocity = vel;
            color = col;
            sourceWeapon = srcWeapon;
        }

        public bool CollideWith(Bullet bullet)
        {
            return Bounds.Intersects(bullet.Bounds);
        }

        public Point Position
        {
            get { return new Point(Bounds.Left, Bounds.Top);}
        }
        public static void LoadContent(ContentManager content)
        {
            textureEmpty = content.Load<Texture2D>("Textures/empty");
        }
        public void Update()
        {
            Bounds.Offset(new Point(velocity, 0));
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureEmpty, Bounds, color);
        }
    }

    public abstract class Weapon
    {
        public Spaceship owner;
        public abstract Bullet Fire();

        public int VelocitySign
        {
            get { return -1 + Convert.ToInt32(owner is EnemyShip)*2; }
        }

        protected Weapon(Spaceship ownr)
        {
            owner = ownr;
        }
    }

    public sealed class Laser : Weapon
    {
        public const int DefaultDamage = 10;
        public const int FireRate = 100; // Milliseconds 
        public override Bullet Fire()
        {
            return new Bullet(new Rectangle(owner.Bounds.X + VelocitySign * 48, owner.Bounds.Y - 2, 10, 4), 
                VelocitySign*16, Color.Tomato, this);
        }

        public Laser(Spaceship ownr) : base(ownr) {}
    }

    public sealed class Rocket : Weapon
    {
        public const int DefaultDamage = 30;
        public override Bullet Fire()
        {
            return new Bullet(new Rectangle(owner.Bounds.X + VelocitySign * 48, owner.Bounds.Y - 6, 24, 12),
                VelocitySign * 8, owner.Color, this);
        }
        public Rocket(Spaceship ownr) : base(ownr) {}
    }
}
