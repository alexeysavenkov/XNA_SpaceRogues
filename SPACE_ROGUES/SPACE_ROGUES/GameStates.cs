using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FSharpPart;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SPACE_ROGUES
{
    public static class GameStates
    {
        private enum GameState
        {
            MainMenu,
            InGame,
            Highscores,
            Credits,
            HowToPlay
        }

        private static GameState gameState = GameState.MainMenu;

        private static Texture2D cursorTexture;

        private class Button
        {
            private readonly Vector2 Position;
            private readonly Rectangle rectangle;
            private readonly SpriteFont Font;
            private readonly string Text;
            private bool Hovered;
            private bool mouseWasReleased = false;

            public event Action OnClick = delegate {  };

            public Button(string text, Vector2 position, SpriteFont font)
            {
                Text = text;
                Vector2 measure = font.MeasureString(Text);
                Position = position;
                rectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)measure.X, (int)measure.Y - 10);
                Font = font;
            }

            public void Update(MouseState mouseState)
            {
                Hovered = rectangle.Contains(new Point(mouseState.X, mouseState.Y));
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    mouseWasReleased = true;
                }
                else if (mouseWasReleased && Hovered)
                {                 
                    OnClick.Invoke();
                }
            }

            public void Reset()
            {
                mouseWasReleased = false;
            }
            public void Draw(SpriteBatch batch, float opacity = 1f)
            {
                batch.DrawString(Font, Text, Position, Color.White * (Hovered ? 1f : 0.5f) * opacity);
            }
        }
        public static class MainMenu
        {
            private static Game game;

            private class SelectableSpaceship
            {
                public PlayerShip Ship;
                private Vector2 position;

                public bool Selected;
                public void Draw(SpriteBatch batch)
                {
                    Ship.Draw(batch, (Selected ? 1f : 0.3f)*(opacity+0.25f));

                    bool b = false;

                    foreach (string bonus in Ship.bonusCollection.ToStrings())
                    {
                        string[] lines = bonus.Split('\n');

                        Vector2 measure = font20.MeasureString(lines[0]);

                        batch.DrawString(font20, lines[0], position + new Vector2(-measure.X / 2 + 48, b ? 160 : 110),
                        Color.White * opacity);

                        measure = font20.MeasureString(lines[1]);

                        batch.DrawString(font20, lines[1], position + new Vector2(-measure.X / 2 + 48, b ? 180 : 130),
                        Color.White * opacity);

                        b = !b;
                    }
                }

                public bool IsHoveredBy(Point point)
                {
                    return Ship.Contains(point);
                }

                public SelectableSpaceship(Color color, Vector2 pos, GraphicsDevice graphics)
                {
                    Ship = new PlayerShip(8, pos, graphics, color);
                    position = pos - new Vector2(48, 48);
                }
            }


            private static SelectableSpaceship[] selectableShips;

            private static Button[] buttons;

            private static SpriteFont font20, font32, font48, font60, font72;

            private static Texture2D logo;
            private static float opacity;

            public static string PlayerName;
            private const int maxPlayerLength = 13;

            private static Random random = new Random();

            private static int centerX;

            private static GraphicsDevice graphicsDevice;

            private static ContentManager Content;

            public static void LoadContent(ContentManager content, Game _game)
            {
                Content = content;
                logo = content.Load<Texture2D>("logo");
                font20 = content.Load<SpriteFont>("font20");
                font60 = content.Load<SpriteFont>("font60");
                font48 = content.Load<SpriteFont>("font48");
                font32 = content.Load<SpriteFont>("font32");
                font72 = content.Load<SpriteFont>("font72");
                game = _game;
            }

            public static void Initialize(GraphicsDevice device)
            {
                graphicsDevice = device;

                centerX = device.Viewport.Width/2;

                opacity = 0f;

                //Initializing selectable ships
                const int diff = 160;
                const int width = 96;

                selectableShips = new SelectableSpaceship[5];

                List<Color> colorLeft = new List<Color>(Spaceship.ColorCollection);

                for (int i = 0; i < 5; i++)
                {
                    int rnd = random.Next(colorLeft.Count);
                    selectableShips[i] = new SelectableSpaceship(colorLeft[rnd],
                        new Vector2(centerX + 48 - (2.5f - i)*width - (2 - i)*diff, 498), device);
                    colorLeft.RemoveAt(rnd);
                }

                selectableShips[0].Selected = true;

                //Initializing buttons
                buttons = new Button[5];

                buttons[0] = new Button("Start", new Vector2(centerX - font60.MeasureString("Start").X/2 ,660), 
                                                                                                    font60);
                buttons[0].OnClick += delegate
                {
                    opacity = .75f;
                    Leave(); 
                    gameState = GameState.InGame; 
                    InGame.Initialize(graphicsDevice, Array.Find(selectableShips, spaceship => spaceship.Selected).Ship, Content);
                };

                buttons[1] = new Button("HighScores", new Vector2(25, graphicsDevice.Viewport.Height - 50), 
                                                                                                    font60);
                buttons[1].OnClick += delegate
                {
                    opacity = .75f;
                    Leave();
                    gameState = GameState.Highscores;
                    HighScores.Initialize(graphicsDevice);
                };

                buttons[2] = new Button("Credits", new Vector2(centerX - font60.MeasureString("Credits").X / 2,
                                                            graphicsDevice.Viewport.Height - 50), font60);

                buttons[2].OnClick += delegate
                {
                    opacity = .75f;
                    Leave();
                    gameState = GameState.Credits;
                };

                buttons[3] = new Button("Exit", new Vector2(centerX*2 - 100 - font60.MeasureString("Exit").X, 
                                                            graphicsDevice.Viewport.Height - 50), font60);

                buttons[3].OnClick += delegate
                {
                    Leave(); 
                    game.Exit();
                };

                buttons[4] = new Button("How To Play", new Vector2(25, 660), font60);
                buttons[4].OnClick += delegate
                {
                    opacity = .75f;
                    Leave();
                    gameState = GameState.HowToPlay;
                    HowToPlay.Initialize(graphicsDevice);
                    HighScores.Initialize(graphicsDevice);
                };

                try
                {
                    PlayerName = System.IO.File.ReadAllText("playername");
                }
                catch
                {
                    PlayerName = "Player";
                }
            }

            private static KeyboardState oldState = Keyboard.GetState();
            public static void Update(MouseState mouseState, KeyboardState keyboardState, GameTime gameTime)
            {
                if (opacity < 0.75f)
                {
                    opacity += 0.0001f*gameTime.ElapsedGameTime.Milliseconds;
                }

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    foreach (SelectableSpaceship colorBox in selectableShips)
                    {
                        if (colorBox.IsHoveredBy(new Point(mouseState.X, mouseState.Y)))
                        {
                            for (int i = 0; i < selectableShips.Length; i++)
                            {
                                selectableShips[i].Selected = false;
                            }
                            colorBox.Selected = true;
                            break;
                        }
                    }
                }

                foreach (Keys key in keyboardState.GetPressedKeys())
                {
                    if (oldState.IsKeyDown(key))
                    {
                        continue;
                    }

                    if (key == Keys.Back)
                    {
                        PlayerName = PlayerName.Substring(0, Math.Max(0, PlayerName.Length - 1));
                    }
                    else if(PlayerName.Length < maxPlayerLength)
                    {
                        if (key == Keys.Subtract)
                        {
                            PlayerName += '-';
                            continue;
                        }

                        string key_str = key.ToString();
                        if (key_str.Length == 1)
                        {
                            PlayerName += key_str;
                        }
                        else if (key_str.Length == 2 && key_str[0] == 'D')
                        {
                            PlayerName += key_str[1];
                        }
                    }
                }

                oldState = keyboardState;

                foreach (Button button in buttons)
                {
                    button.Update(mouseState);
                }
            }

            public static void Draw(SpriteBatch batch)
            {
                batch.Draw(logo, new Vector2(centerX - logo.Width/2, 25), Color.White*opacity);

                batch.DrawString(font60, "Type Your Name", new Vector2(centerX - font60.MeasureString("Type Your Name").X/2, 175), Color.White * opacity);
                batch.DrawString(font32, "(with keyboard)", new Vector2(centerX - font32.MeasureString("(with keyboard)").X/2, 225), Color.White * opacity);
                batch.DrawString(font72, PlayerName, new Vector2(centerX - font72.MeasureString(PlayerName).X/2, 255), Color.White * opacity);

                batch.DrawString(font60, "Select Your Ship", new Vector2(centerX - font60.MeasureString("Select Your Ship").X/2, 350), Color.White * opacity);
                batch.DrawString(font32, "(with left mouse button)", new Vector2(centerX - font32.MeasureString("(with left mouse button)").X/2, 400), Color.White * opacity);

                //Drawing ships
                foreach (SelectableSpaceship ship in selectableShips)
                {
                    ship.Draw(batch);
                }

                //Drawing buttons
                foreach (Button button in buttons)
                {
                    button.Draw(batch, opacity + 0.25f);
                }
            }

            public static void Leave()
            {
                File.WriteAllText("playername", PlayerName);
                foreach (Button button in buttons)
                {
                    button.Reset();
                }
            }
        }

        public static class InGame
        {
            
            public class DrawableBonusCollection
            {
                public BonusCollection bonusCollection;

                protected int centerX, topY;
                protected Rectangle rectangle;
                protected float opacity = 0.8f;

                public void Draw(SpriteBatch batch)
                {
                    bool b = false;

                    foreach (string bonus in bonusCollection.ToStrings())
                    {
                        string[] lines = bonus.Split('\n');

                        Vector2 measure = font32.MeasureString(lines[0]);

                        batch.DrawString(font32, lines[0], new Vector2(centerX - measure.X / 2, topY + (b ? 70 : 0)),
                        Color.White * opacity);

                        measure = font32.MeasureString(lines[1]);

                        batch.DrawString(font32, lines[1], new Vector2(centerX - measure.X / 2, topY + (b ? 105 : 35)),
                        Color.White * opacity);

                        b = !b;
                    }
                }

                public DrawableBonusCollection(Rectangle rect, OwnerType type)
                {
                    bonusCollection = BonusCollection.Random(type);

                    if (type == OwnerType.Player)
                    {
                        int diff = Score - prevLevelScore;
                        bonusCollection *= (diff == 0 ? 1 : diff) / 50000f;
                    }

                    centerX = rect.X + rect.Width/2;
                    topY = rect.Y;
                    rectangle = rect;
                }
            }
            public class SelectableBonusCollection : DrawableBonusCollection
            {
                public bool Selected;
                
                public void Update(MouseState mouseState)
                {
                    if (mouseState.LeftButton == ButtonState.Pressed
                        && IsHoveredBy(new Point(mouseState.X, mouseState.Y)))
                    {
                        for (int i = 0; i < selectableBonuses.Length; i++)
                        {
                            selectableBonuses[i].Selected = false;
                        }
                        Selected = true;
                    }

                    opacity = (Selected ? 0.75f : 0.25f);
                }

                public bool IsHoveredBy(Point point)
                {
                    return rectangle.Contains(point);
                }

                public SelectableBonusCollection(Rectangle rect) : base(rect, OwnerType.Player) {}
            }
            public class Explosion
            {
                private static Texture2D[] frames;
                private static Texture2D Crop(Texture2D image, Rectangle source)
                {
                    var graphics = image.GraphicsDevice;
                    var ret = new RenderTarget2D(graphics, source.Width, source.Height);
                    var sb = new SpriteBatch(graphics);

                    graphics.SetRenderTarget(ret); // draw to image
                    graphics.Clear(new Color(0, 0, 0, 0));

                    sb.Begin();
                    sb.Draw(image, Vector2.Zero, source, Color.White);
                    sb.End();

                    graphics.SetRenderTarget(null); // set back to main window

                    return (Texture2D)ret;
                }

                public static void LoadContent(ContentManager content)
                {
                    const int frames_count = 17;
                    frames = new Texture2D[frames_count];

                    Texture2D bigTexture = content.Load<Texture2D>("Spritesheets/explosion");
                    
                    int width = bigTexture.Width/17;
                    int height = bigTexture.Height;
                    int byte_count = 4*width*height;

                    for (int i = 0; i < frames_count; i++)
                    {
                        frames[i] = Crop(bigTexture, new Rectangle(width*i, 0, width, height));
                    }
                }

                private Rectangle bounds;
                public int Frame = 1; // 0 is bad
                private TimeSpan prevUpdate = new TimeSpan(0);
                private readonly float Power;

                private void HandleExplosionDamage(Spaceship ship)
                {
                    Point diff = new Point(ship.ActualBounds.Center.X - bounds.Center.X,
                                           ship.ActualBounds.Center.Y - bounds.Center.Y);

                    int abs = Math.Abs(diff.X) + Math.Abs(diff.Y);

                    int dmg = abs == 0 ? 100 : (int)(Power * 2000 / abs);

                    ship.Health -= dmg;

                    if (ship is EnemyShip)
                    {
                        Score += (int)(Math.Min(100, dmg) * 5 * PlayerShip.BonusCollection["Score\nFrom Rocketing"]);
                    }
                    else if (ship is AllyShip)
                    {
                        Score -= Math.Min(100, dmg) * 5;
                    }

                    if (ship is PlayerShip)
                    {
                        return;
                    }

                    diff.X >>= 4;
                    diff.Y >>= 4;

                    if (Math.Abs(diff.X) < 16 && Math.Abs(diff.Y) < 16 && abs != 0)
                    {
                        Point vel = new Point((diff.X == 0 
                            ? 0
                            : Math.Sign(diff.X)*(6 - Math.Abs(diff.X/3))),
                                (diff.Y == 0 
                            ? 0
                            : Math.Sign(diff.Y)*(6 - Math.Abs(diff.Y/3))));


                        ship.Velocity = new Point(ship.Velocity.X + vel.X,
                                                  ship.Velocity.Y + vel.Y);

                        if (!ship.IsFloating)
                        {
                            for (int i = 0; i < npcShipsByY.Length; i++)
                            {
                                if (npcShipsByY[i] != null)
                                {
                                    if (npcShipsByY[i].Equals(ship))
                                    {
                                        npcShipsByY[i] = null;
                                        floatingNpcShips.Add(ship);
                                        return;
                                    }
                                }
                            }
                        }
                    }                   
                }

                public Explosion(Vector2 pos, float power = 1)
                {
                    Power = power;
                    bounds = new Rectangle((int)(pos.X - 128*power), (int)(pos.Y - 128*power), (int)(256*power), (int)(256*power));


                    for (int i = 0; i < npcShipsByY.Length; i++)
                    {
                        if (npcShipsByY[i] != null)
                        {
                            if (bounds.Intersects(npcShipsByY[i].Bounds))
                            {
                                HandleExplosionDamage(npcShipsByY[i]);
                            }
                        }
                    }

                    for (int i = 0; i < floatingNpcShips.Count; i++)
                    {
                        if (bounds.Intersects(floatingNpcShips[i].Bounds))
                        {
                            HandleExplosionDamage(floatingNpcShips[i]);
                        }
                    }

                    if (playerShip != null)
                    {
                        if (playerShip.Bounds.Intersects(bounds))
                        {
                            HandleExplosionDamage(playerShip);
                        }
                    }
                }

                public void Update(GameTime gameTime)
                {
                    if (prevUpdate.Ticks == 0)
                    {
                        prevUpdate = gameTime.TotalGameTime;
                    }

                    if ((gameTime.TotalGameTime - prevUpdate).TotalMilliseconds >= 300)
                    {
                        Frame++;
                    }
                }

                public void Draw(SpriteBatch spriteBatch)
                {
                    if (Frame < frames.Length)
                    {
                        spriteBatch.Draw(frames[Frame], bounds, Color.White);
                    }
                }
            }

            private static TimeSpan levelStart = new TimeSpan(0);

            private static int Level = 1;
            //Score = experience
            private static int Score;
            private static int prevLevelScore;
            public enum InGameState : byte
            {
                Starting,
                Playing,
                LevelEnd,
                Pause,
                GameOver
            }
            public static InGameState inGameState = InGameState.Starting;

            private static GraphicsDevice graphicsDevice;

            private static Button[] buttonsPause;
            private static Button buttonGameOver;
            private static DrawableBonusCollection enemyBonusCollection; //at level end
            private static SelectableBonusCollection[] selectableBonuses; //at level end
            private static Button buttonLevelEnd;

            private static SpriteFont font20, font32, font60;

            private static Random random = new Random();
            private static PlayerShip playerShip;
            private static Spaceship[] npcShipsByY; //
            private static int[] nextNpcIn; //Milliseconds
            private static List<Spaceship> floatingNpcShips = new List<Spaceship>(100);
            private static List<Bullet> bullets = new List<Bullet>(1000); 
            private static List<Explosion> explosions = new List<Explosion>(100);

            private static int playerSpeed = 5;

            private static class NpcKiller
            {
                private static void Kill(Spaceship ship, bool guaranteedFloating = false)
                {
                    if (!guaranteedFloating)
                    {
                        for (int i = 0; i < npcShipsByY.Length; i++)
                        {
                            if (npcShipsByY[i] != null)
                            {
                                if (npcShipsByY[i].Equals(ship))
                                {
                                    npcShipsByY[i] = null;
                                    nextNpcIn[i] = random.Next(1000 / Level, 30000 / Level);
                                    return;
                                }
                            }
                        }
                    }

                    floatingNpcShips.Remove(ship);
                }

                public static void KillNpcAtKill(Spaceship ship)
                {
                    if (ship is AllyShip)
                    {
                        Score -= 1500;
                    }
                    else
                    {
                        Score += 1500;
                    }

                    Vector2 position = new Vector2(ship.Bounds.X, ship.Bounds.Y);

                    Kill(ship);

                    explosions.Add(new Explosion(position, 1.5f));
                }

                // When NPC gets right border
                public static void KillNpcAtSuccess(Spaceship ship)
                {
                    if (ship is AllyShip)
                    {
                        Score += 1000 + 10*ship.Health;
                    }
                    else
                    {
                        Score -= 1000 + 10*ship.Health;
                    }

                    Kill(ship);
                }
                // When NPC gets any other border
                public static void KillNpcAtFlyAway(Spaceship ship)
                {
                    if (ship is AllyShip)
                    {
                        Score -= 500 - 5 * ship.Health;
                    }
                    else
                    {
                        Score += 500 - 5 * ship.Health;
                    }

                    Kill(ship, true);
                }
            }
            

            public static void Initialize(GraphicsDevice device, PlayerShip plShip, ContentManager content)
            {
                prevLevelScore = 0;
                Score = 0;
                levelStart = new TimeSpan(0);
                Level = 1;

                graphicsDevice = device;
                font60 = content.Load<SpriteFont>("font60");
                font32 = content.Load<SpriteFont>("font32");
                font20 = content.Load<SpriteFont>("font20");
                playerShip = plShip;
                playerShip.SetPosition(graphicsDevice.Viewport.Width + 48, graphicsDevice.Viewport.Height/2);
                playerShip.Velocity = new Point(-1, 0);
                PlayerShip.BonusCollection = playerShip.bonusCollection;

                RammingHandler.Init(playerShip);

                npcShipsByY = new Spaceship[(graphicsDevice.Viewport.Height-150)/100];

                nextNpcIn = new int[npcShipsByY.Length];

                for (int i = 0; i < nextNpcIn.Length; i++)
                {
                    nextNpcIn[i] = random.Next(500, 5000);
                }

                //Initializing buttons
                buttonsPause = new Button[2];

                buttonsPause[0] = new Button("Continue", new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString("Continue").X / 2, 
                    graphicsDevice.Viewport.Height/2), font60);
                buttonsPause[0].OnClick += delegate
                {
                    Background.Stopped = false;
                    inGameState = InGameState.Playing;
                };

                buttonsPause[1] = new Button("Main Menu", new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString("Main Menu").X / 2,
                    graphicsDevice.Viewport.Height / 2 + 100), font60);
                buttonsPause[1].OnClick += delegate
                {
                    MainMenu.Initialize(graphicsDevice);
                    Background.Stopped = false;
                    End();
                    gameState = GameState.MainMenu;
                };

                buttonGameOver = new Button("Main Menu", new Vector2(graphicsDevice.Viewport.Width / 2 - 
                    font60.MeasureString("Main Menu").X / 2, graphicsDevice.Viewport.Height / 2 + 200), font60);
                buttonGameOver.OnClick += delegate
                {
                    InGame.End();
                    MainMenu.Initialize(graphicsDevice);
                    gameState = GameState.MainMenu;
                    inGameState = InGameState.Starting;
                };
            }

            private static void UpdateNpcShip(Spaceship ship, GameTime gameTime)
            {
                if (ship == null)
                {
                    return;
                }

                ship.Update(gameTime);

                if (ship.Bounds.X > graphicsDevice.Viewport.Width + 50)
                {
                      NpcKiller.KillNpcAtSuccess(ship);
                      return;
                }
                
                if (ship.Bounds.X < -200 || ship.Bounds.Y < -200 || 
                    ship.Bounds.Y > graphicsDevice.Viewport.Height + 200)
                {
                      NpcKiller.KillNpcAtFlyAway(ship);
                      return;
                }
                
                if (ship is EnemyShip)
                {
                    EnemyShip enemyShip = ship as EnemyShip;
                    if (enemyShip.laserFiring)
                    {
                        if (enemyShip.weaponCooldown < 0)
                        {
                            if (enemyShip.rocketFireIn <= 0)
                            {
                                enemyShip.rocketFireIn = null;
                                enemyShip.laserFiring = false;
                                enemyShip.laserNextTriggerIn = random.Next(2500 / Level, 5000 / Level);
                                bullets.Add(ship.Rocket.Fire());
                            }
                            else
                            {
                                bullets.Add(ship.Laser.Fire());
                                enemyShip.weaponCooldown = Laser.FireRate;
                            }
                        }
                        else
                        {
                            enemyShip.weaponCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                        }
                    }
                }
            }

            private static class RammingHandler
            {
                private class PairAssiciatedBoolfield<TKey>
                {
                    private readonly Dictionary<TKey, Dictionary<TKey, bool>> dictionary = new Dictionary<TKey, Dictionary<TKey, bool>>();

                    private TKey mainElement;

                    private bool get(TKey key1, TKey key2)
                    {
                        Dictionary<TKey, bool> dict;

                        if (!dictionary.TryGetValue(key1, out dict))
                        {
                            return false;
                        }

                        bool value;

                        if (!dict.TryGetValue(key2, out value))
                        {
                            return false;
                        }

                        return value;
                    }
                    private void set(TKey key1, TKey key2, bool value)
                    {
                        if (!dictionary.ContainsKey(key1))
                        {
                            dictionary.Add(key1, new Dictionary<TKey, bool> { { key2, value } });
                        }
                        else if (!dictionary[key1].ContainsKey(key2))
                        {
                            dictionary[key1].Add(key2, value);
                        }
                        else
                        {
                            dictionary[key1][key2] = value;
                        }
                    }

                    public bool this[TKey key1, TKey key2]
                    {
                        get { return get(key1, key2) || get(key2, key2); }
                        set
                        {
                            set(key1, key2, value);
                            set(key2, key1, value);
                        }
                    }

                    public void RemoveKey(TKey key)
                    {
                        dictionary.Remove(key);
                        dictionary[mainElement].Remove(key);
                    }

                    public PairAssiciatedBoolfield(TKey MainElement)
                    {
                        mainElement = MainElement;
                        dictionary.Add(mainElement, new Dictionary<TKey, bool>());
                    }
                }

                private static PairAssiciatedBoolfield<Spaceship> RammingInProgress;

                public static void Init(PlayerShip playerShip)
                {
                    RammingInProgress = new PairAssiciatedBoolfield<Spaceship>(playerShip);
                }
                private static void HandleRamming(Spaceship ship1, Spaceship ship2)
                {
                    if (RammingInProgress[ship1, ship2])
                    {
                        return;
                    }

                    Type type1 = ship1.GetType();
                    Type type2 = ship2.GetType();

                    Point tmp_velocty = ship1.Velocity;

                    if (type1 != typeof(PlayerShip))
                    {
                        ship1.Velocity = new Point(
                                                    (ship2.Velocity.X * 4 + ship1.Bounds.X - ship2.Bounds.X) / 24,
                                                    (ship2.Velocity.Y * 4 + ship1.Bounds.Y - ship2.Bounds.Y) / 24);

                        ship1.IsFloating = true;
                    }

                    if (type2 != typeof(PlayerShip))
                    {
                        ship2.Velocity = new Point(
                                                    (tmp_velocty.X * 4 + ship2.Bounds.X - ship1.Bounds.X) / 24,
                                                    (tmp_velocty.Y * 4 + ship2.Bounds.Y - ship1.Bounds.Y) / 24);

                        ship2.IsFloating = true;
                    }

                    ship1.Health -= (int)(10 * (type2 == typeof(EnemyShip)
                        ? EnemyShip.BonusCollection["Ramming\nDamage"] * EnemyShip.BonusCollection["Overall\nDamage"]
                        : PlayerShip.BonusCollection["Ramming\nDamage"] * PlayerShip.BonusCollection["Overall\nDamage"])
                        /
                        (type1 == typeof(EnemyShip)
                        ? EnemyShip.BonusCollection["Ramming\nProtection"] * EnemyShip.BonusCollection["Overall\nProtection"]
                        : PlayerShip.BonusCollection["Ramming\nProtection"] * PlayerShip.BonusCollection["Overall\nProtection"]));

                    ship2.Health -= (int)(10 * (type1 == typeof(EnemyShip)
                        ? EnemyShip.BonusCollection["Ramming\nDamage"] * EnemyShip.BonusCollection["Overall\nDamage"]
                        : PlayerShip.BonusCollection["Ramming\nDamage"] * PlayerShip.BonusCollection["Overall\nDamage"])
                        /
                        (type2 == typeof(EnemyShip)
                        ? EnemyShip.BonusCollection["Ramming\nProtection"] * EnemyShip.BonusCollection["Overall\nProtection"]
                        : PlayerShip.BonusCollection["Ramming\nProtection"] * PlayerShip.BonusCollection["Overall\nProtection"]));

                    Score += (Convert.ToByte(type1 == typeof(EnemyShip)) + Convert.ToByte(type2 == typeof(EnemyShip)) - 
                                   Convert.ToByte(type1 == typeof(AllyShip)) - Convert.ToByte(type2 == typeof(AllyShip))) *
                                    (int)(500 * PlayerShip.BonusCollection["Score\nFrom Ramming"] *
                                                PlayerShip.BonusCollection["Overall\nScore"]);
                }

                /// <summary>
                /// If <paramref name="ship1"/> and <paramref name="ship2"/> are colliding,
                /// collision is handled and <paramref name="action"/> is executed.
                /// Otherwise nothing is done.
                /// </summary>
                public static void HandlePossibleRamming(Spaceship ship1, Spaceship ship2, Action action = null)
                {
                    if (ship1.CollidesWith(ship2))
                    {
                        HandleRamming(ship1, ship2);
                        RammingInProgress[ship1, ship2] = true;
                        if (action != null)
                        {
                            action.Invoke();
                        }
                    }
                    else
                    {
                        RammingInProgress[ship1, ship2] = false;
                    }
                }
            }

            private static void HandleShot(Spaceship ship, Bullet bullet)
            {
                bool enemyDef = (ship is EnemyShip);
                bool enemySht = (bullet.sourceWeapon.owner is EnemyShip);
                bool weaponIsLaser = (bullet.sourceWeapon is Laser);

                float damage = weaponIsLaser ? Laser.DefaultDamage : 1;

                string defSkill = (weaponIsLaser ? "Laser\nProtection" : "Rocket\nProtection");
                string shtSkill = (weaponIsLaser ? "Laser\nDamage" : "Rocket\nDamage");

                damage *= (enemySht 
                    ? (EnemyShip.BonusCollection[shtSkill]*EnemyShip.BonusCollection["Overall\nDamage"])
                    : (PlayerShip.BonusCollection[shtSkill]*PlayerShip.BonusCollection["Overall\nDamage"]));

                damage /= (enemyDef
                    ? (EnemyShip.BonusCollection[defSkill]*EnemyShip.BonusCollection["Overall\nProtection"])
                    : (PlayerShip.BonusCollection[defSkill]*PlayerShip.BonusCollection["Overall\nProtection"]));

                if (!(ship is PlayerShip))
                {
                    string scoreSkill = (weaponIsLaser ? "Score\nFrom Lasering" : "Score\nFrom Rocketing");
                    int defScore = (weaponIsLaser ? 100 : 500);
                    bool allyDef = (ship is AllyShip);
                    Score += (allyDef ? -1 : 1)*(int)(defScore * PlayerShip.BonusCollection[scoreSkill] *
                                    PlayerShip.BonusCollection["Overall\nScore"]);
                }

                if (!weaponIsLaser)
                {
                    explosions.Add(new Explosion(new Vector2(ship.Bounds.X, ship.Bounds.Y), damage));
                }
                else
                {
                    ship.Health -= (int)damage;
                }

                bullets.Remove(bullet);
            }

            private static void HandleBulletCollision(Bullet bullet1, Bullet bullet2)
            {
                Bullet[] bullet =
                {
                    bullet1,
                    bullet2
                };

                bool[] isRocket = bullet.Select(bul => bul.sourceWeapon is Rocket).ToArray();

                float power = isRocket.Sum(b => b ? 1 : 0);

                if (power != 0)
                {
                    Vector2 position = Vector2.Zero;

                    for (int i = 0; i < 2; i++)
                    {
                        if (isRocket[i])
                        {
                            power *= (bullet[i].sourceWeapon.owner is EnemyShip)
                                ? EnemyShip.BonusCollection["Rocket\nDamage"]
                                : PlayerShip.BonusCollection["Rocket\nDamage"];

                            position = bullet[i].Center;
                        }
                    }

                    explosions.Add(new Explosion(position, power));
                    bullets.Remove(bullet1);
                    bullets.Remove(bullet2);
                }
            }

            private static bool escapeWasReleased = false;
            public static void Update(GameTime gameTime, KeyboardState keyboardState)
            {
                if (inGameState != InGameState.Pause)
                {
                    // Updating Ships, Bullets and Explosions
                    if (playerShip != null)
                    {
                        playerShip.Update(gameTime);
                        foreach (Bullet bullet in bullets)
                        {
                            if (playerShip.CollidesWith(bullet))
                            {
                                HandleShot(playerShip, bullet);
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < npcShipsByY.Length; i++)
                    {
                        if (npcShipsByY[i] != null)
                        {
                            foreach (Bullet bullet in bullets)
                            {
                                if (npcShipsByY[i].CollidesWith(bullet))
                                {
                                    HandleShot(npcShipsByY[i], bullet);
                                    break;
                                }
                            }
                            UpdateNpcShip(npcShipsByY[i], gameTime);
                        }
                    }

                    for (int i = floatingNpcShips.Count - 1; i >= 0; i--)
                    {
                        foreach (Bullet bullet in bullets)
                        {
                            if (floatingNpcShips[i].CollidesWith(bullet))
                            {
                                HandleShot(floatingNpcShips[i], bullet);
                                break;
                            }
                        }
                        floatingNpcShips[i].Update(gameTime);
                        UpdateNpcShip(floatingNpcShips[i], gameTime);
                    }

                    for (int i = bullets.Count - 1; i >= 0; i--)
                    {
                        if (i >= bullets.Count)
                        {
                            continue;
                        }
                        Point position = bullets[i].Position;
                        if (position.X < -10 || position.X > graphicsDevice.Viewport.Width + 10)
                        {
                            bullets.RemoveAt(i);
                        }
                        else
                        {
                            bullets[i].Update();
                            for (int j = i - 1; j >= 0; j--)
                            {
                                if (i >= bullets.Count)
                                {
                                    i--;
                                    break;
                                }
                                if (bullets[i].CollideWith(bullets[j]))
                                {
                                    HandleBulletCollision(bullets[i], bullets[j]);
                                }
                            }
                        }
                    }

                    for (int i = explosions.Count - 1; i >= 0; i--)
                    {
                        explosions[i].Update(gameTime);
                        if (explosions[i].Frame == 18)
                        {
                            explosions.RemoveAt(i);
                        }
                    }
                }

                switch (inGameState)
                {
                    case InGameState.Starting:
                        if (playerShip.Bounds.X <= graphicsDevice.Viewport.Width - 96)
                        {
                            playerShip.Velocity = new Point(0, 0);
                            inGameState = InGameState.Playing;
                        }
                        break;
                    case InGameState.Playing:
                        // Checking for playerShip death
                        if (playerShip.Health <= 0)
                        {
                            OnGameOver();
                            return;
                        }

                        // Checking for npc deaths
                        for (int i = 0; i < npcShipsByY.Length; i++)
                        {
                            if (npcShipsByY[i] != null)
                            {
                                if (npcShipsByY[i].Health <= 0)
                                {
                                    NpcKiller.KillNpcAtKill(npcShipsByY[i]);
                                }
                            }
                        }

                        for (int i = floatingNpcShips.Count - 1; i >= 0; i--)
                        {
                            if (floatingNpcShips[i].Health <= 0)
                            {
                                NpcKiller.KillNpcAtKill(floatingNpcShips[i]);
                            }
                        }

                        // Ending & Adding Enemies
                        if (levelStart.Ticks == 0)
                        {
                            levelStart = gameTime.TotalGameTime;
                        }
                        else if ((gameTime.TotalGameTime - levelStart).TotalMinutes < 1.0)
                        {
                            for (int i = 0; i < nextNpcIn.Length; i++)
                            {
                                if (npcShipsByY[i] != null)
                                {
                                    continue;
                                }

                                if (nextNpcIn[i] <= 0)
                                {
                                    Color color = Spaceship.ColorCollection[random.Next(Spaceship.ColorCollection.Length)];
                                    if (color == playerShip.Color)
                                    {
                                        npcShipsByY[i] = new AllyShip(8, new Vector2(-48, i*100 + 100),
                                            new Point( random.Next(Math.Min(Math.Max(2, Level/2), 4), 6) , 0), color,
                                            graphicsDevice);
                                    }
                                    else
                                    {
                                        npcShipsByY[i] = new EnemyShip(8, new Vector2(-48, i*100 + 100),
                                            new Point(random.Next(Math.Min(Math.Max(2, Level/2), 4), 6), 0), color,
                                            graphicsDevice);
                                    }

                                    nextNpcIn[i] = random.Next(1000, 30000)/Level;
                                }
                                else
                                {
                                    nextNpcIn[i] -= gameTime.ElapsedGameTime.Milliseconds;
                                }
                            }
                        }
                        else if (floatingNpcShips.Count == 0 && npcShipsByY.All(ship => ship == null))
                        {
                            inGameState = InGameState.LevelEnd;
                            OnLevelEnd();
                            return;
                        }

                        // Pausing
                        if (keyboardState.IsKeyUp(Keys.Escape))
                        {
                            escapeWasReleased = true;
                        }
                        else if (escapeWasReleased && keyboardState.IsKeyDown(Keys.Escape))
                        {
                            inGameState = InGameState.Pause;
                            Background.Stopped = true;
                            escapeWasReleased = false;
                        }

                        //Shooting
                        playerShip.weaponCooldown -= gameTime.ElapsedGameTime.Milliseconds;

                        if (playerShip.weaponCooldown < 0)
                        {
                            if (keyboardState.IsKeyDown(Keys.LeftControl) && playerShip.RocketCooldown <= 0)
                            {
                                bullets.Add(playerShip.Rocket.Fire());
                                playerShip.RocketCooldown = (int)(30000 / playerShip.bonusCollection["Rocket\nReload Speed"]);
                            }
                            else if (keyboardState.IsKeyDown(Keys.Space))
                            {
                                bullets.Add(playerShip.Laser.Fire());
                                playerShip.weaponCooldown = Laser.FireRate;
                            }
                        }

                        // Collision checking

                        for (int i = 0; i < npcShipsByY.Length; i++)
                        {
                            if (npcShipsByY[i] == null)
                            {
                                continue;
                            }

                            RammingHandler.HandlePossibleRamming(npcShipsByY[i], playerShip, delegate
                            {
                                floatingNpcShips.Add(npcShipsByY[i]);

                                npcShipsByY[i] = null;
                                nextNpcIn[i] = random.Next(1000 / Level, 30000 / Level);
                            });

                            for (int j = 0; j < floatingNpcShips.Count; j++)
                            {
                                if (npcShipsByY[i] == null)
                                {
                                    break;
                                }

                                RammingHandler.HandlePossibleRamming(npcShipsByY[i], floatingNpcShips[j], delegate
                                {
                                    floatingNpcShips.Add(npcShipsByY[i]);

                                    npcShipsByY[i] = null;
                                    nextNpcIn[i] = random.Next(1000 / Level, 30000 / Level);
                                });
                            }
                        }

                        for (int i = 0; i < floatingNpcShips.Count; i++)
                        {
                            RammingHandler.HandlePossibleRamming(floatingNpcShips[i], playerShip);

                            for (int j = i + 1; j < floatingNpcShips.Count; j++)
                            {
                                RammingHandler.HandlePossibleRamming(floatingNpcShips[i], floatingNpcShips[j]);
                            }
                        }

                        // Moving
                        int velX = 0, velY = 0;

                        if ((keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A)) 
                            && playerShip.Bounds.X > 48)
                        {
                            velX -= playerSpeed;
                        }

                        if ((keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                            && playerShip.Bounds.X + 48 < graphicsDevice.Viewport.Width)
                        {
                            velX += playerSpeed;
                        }

                        if ((keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                            && playerShip.Bounds.Y > 48)
                        {
                            velY -= playerSpeed;
                        }

                        if ((keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                            && playerShip.Bounds.Y + 48 < graphicsDevice.Viewport.Height)
                        {
                            velY += playerSpeed;
                        }

                        if (velX != 0 && velY != 0)
                        {
                            velX = (int)(velX * .75f);
                            velY = (int)(velY * .75f);
                        }

                        playerShip.Velocity = new Point(velX, velY);

                        break;
                    case InGameState.LevelEnd:
                        MouseState mouseState = Mouse.GetState();
                        buttonLevelEnd.Update(mouseState);
                        foreach (SelectableBonusCollection selectableBonus in selectableBonuses)
                        {
                            selectableBonus.Update(mouseState);
                        }
                        break;
                    case InGameState.Pause:
                        if (keyboardState.IsKeyUp(Keys.Escape))
                        {
                            escapeWasReleased = true;
                        }
                        else if (escapeWasReleased && keyboardState.IsKeyDown(Keys.Escape))
                        {
                            inGameState = InGameState.Playing;
                            Background.Stopped = false;
                            escapeWasReleased = false;
                        }

                        foreach (Button button in buttonsPause)
                        {
                            button.Update(Mouse.GetState());
                        }
                        break;
                    case InGameState.GameOver:

                        buttonGameOver.Update(Mouse.GetState());

                        break;
                }
            }

            private static void OnLevelEnd()
            {
                playerShip.Velocity = new Point(-3, 0);

                Point bonusVisualSize = new Point(350, 250);
                enemyBonusCollection = new DrawableBonusCollection(new Rectangle(
                    graphicsDevice.Viewport.Width / 2 - bonusVisualSize.X / 2, 175, bonusVisualSize.X, bonusVisualSize.Y),
                    OwnerType.Enemy);

                EnemyShip.BonusCollection += enemyBonusCollection.bonusCollection;

                if (Score - prevLevelScore > 0)
                {
                    selectableBonuses = new[]
                    {
                        new SelectableBonusCollection(new Rectangle(
                            graphicsDevice.Viewport.Width/2 - 100 - bonusVisualSize.X, 425,
                            bonusVisualSize.X, bonusVisualSize.Y)),
                        new SelectableBonusCollection(new Rectangle(
                            graphicsDevice.Viewport.Width/2 + 100, 425,
                            bonusVisualSize.X, bonusVisualSize.Y)),
                    };
                    selectableBonuses[0].Selected = true;
                }
                else
                {
                    selectableBonuses = new SelectableBonusCollection[0];
                }

                //Hint.Create(new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2 + 150),
                //    font20);

                buttonLevelEnd = new Button("Next Level", new Vector2(graphicsDevice.Viewport.Width / 2 -
                    font60.MeasureString("Next Level").X / 2, graphicsDevice.Viewport.Height / 2 + 320), font60);
                buttonLevelEnd.OnClick += delegate
                {
                    playerShip.Health = 100;
                    Level++;
                    levelStart = new TimeSpan(0);
                    prevLevelScore = Score;

                    if (selectableBonuses.Any())
                    {
                        playerShip.bonusCollection += selectableBonuses.First(collection => collection.Selected).bonusCollection;
                    }

                    playerShip.SetPosition(graphicsDevice.Viewport.Width + 48, graphicsDevice.Viewport.Height / 2);
                    playerShip.Velocity = new Point(-1, 0);
                    inGameState = InGameState.Starting;
                };
            }
            private static void OnGameOver()
            {
                inGameState = InGameState.GameOver;
                explosions.Add(new Explosion(new Vector2(playerShip.Bounds.X, playerShip.Bounds.Y)));
                playerShip = null;
                HighScores.Add(MainMenu.PlayerName, Score, Level);
                Hint.Create(new Vector2(graphicsDevice.Viewport.Width/2, graphicsDevice.Viewport.Height/2 + 50),
                    font20);
            }

            public static void Draw(SpriteBatch spriteBatch)
            {
                // Drawing UI
                if (inGameState != InGameState.GameOver && inGameState != InGameState.LevelEnd)
                {
                    string toWrite = string.Format("Score {0:N0}", Score);
                    spriteBatch.DrawString(font60, toWrite, new Vector2(10, 0), Color.White);

                    toWrite = string.Format("Level {0:N0}", Level);
                    spriteBatch.DrawString(font60, toWrite, new Vector2(graphicsDevice.Viewport.Width -
                        font60.MeasureString(toWrite).X - 10, 0), Color.White);

                    toWrite = "Rocket Ready" +
                              ((playerShip.RocketCooldown <= 0)
                                  ? ""
                                  : string.Format(" in {0:N0}", playerShip.RocketCooldown));
                    spriteBatch.DrawString(font60, toWrite, new Vector2(10, graphicsDevice.Viewport.Height -
                        font60.MeasureString(toWrite).Y + 10), Color.White);

                    toWrite = string.Format("Health {0}%", playerShip.Health);
                    spriteBatch.DrawString(font60, toWrite,
                        new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height) + new Vector2(-10, 10) -
                        font60.MeasureString(toWrite), Color.White);
                }

                // Drawing bullets
                foreach (Bullet bullet in bullets)
                {
                    bullet.Draw(spriteBatch);
                }

                // Drawing ships
                if (playerShip != null)
                {
                    playerShip.Draw(spriteBatch, 1f, (float)-Math.PI / 2);
                }

                foreach (Spaceship npcShip in npcShipsByY)
                {
                    if (npcShip != null)
                    {
                        npcShip.Draw(spriteBatch);
                    }
                }

                foreach (Spaceship npcShip in floatingNpcShips)
                {
                    npcShip.Draw(spriteBatch);
                }

                // Drawing explosions
                foreach (Explosion explosion in explosions)
                {
                    explosion.Draw(spriteBatch);
                }

                // Drawing hints
                if (inGameState == InGameState.GameOver /*|| inGameState == InGameState.LevelEnd*/ )
                {
                    Hint.Draw(spriteBatch);
                }

                // Drawing buttons if paused
                if (inGameState == InGameState.Pause)
                {
                    foreach (Button button in buttonsPause)
                    {
                        button.Draw(spriteBatch);
                    }
                    spriteBatch.DrawString(font32, "(You will lose all current progress)",
                        new Vector2(graphicsDevice.Viewport.Width / 2 - font32.MeasureString("(You will lose all current progress)").X / 2,
                        graphicsDevice.Viewport.Height / 2 + 150), Color.White*0.5f);
                }

                // Drawing Level End

                if (inGameState == InGameState.LevelEnd)
                {
                    string toWrite = string.Format("Level {0} finished!", Level);

                    spriteBatch.DrawString(font60, toWrite,
                        new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString(toWrite).X / 2, 25), 
                        Color.White * 0.9f);

                    toWrite = string.Format("Enemy ships now have these bonuses");

                    spriteBatch.DrawString(font60, toWrite,
                        new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString(toWrite).X / 2, 100),
                        Color.White * 0.9f);

                    enemyBonusCollection.Draw(spriteBatch);

                    if (selectableBonuses.Length != 0)
                    {
                        toWrite = "Select your bonuses";
                        spriteBatch.DrawString(font60, toWrite,
                            new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString(toWrite).X / 2, 350),
                                Color.White * 0.9f);

                        foreach (SelectableBonusCollection selectableBonus in selectableBonuses)
                        {
                            selectableBonus.Draw(spriteBatch);
                        }
                    }

                    buttonLevelEnd.Draw(spriteBatch);
                }

                // Drawing Game Over

                if (inGameState == InGameState.GameOver)
                {
                    spriteBatch.DrawString(font60, "Game Over",
                        new Vector2(graphicsDevice.Viewport.Width / 2 - font60.MeasureString("Game Over").X / 2,
                        graphicsDevice.Viewport.Height/2), Color.White*0.9f);

                    buttonGameOver.Draw(spriteBatch);
                }
            }

            public static void End()
            {
                inGameState = InGameState.Starting;
                npcShipsByY = null;
                floatingNpcShips = new List<Spaceship>(100);
                bullets = new List<Bullet>(1000);
            }
        }

        public static class HighScores
        {
            private struct HighScore
            {
                public string Name;
                public int Score;
                public int LevelReached;
                public HighScore(string name, int score, int levelReached)
                {
                    Name = name;
                    Score = score;
                    LevelReached = levelReached;
                }
            }

            private static SpriteFont font48, font60;
            private static int centerX;

            private static Button[] buttons;

            private static readonly List<HighScore> highscores = new List<HighScore>();

            public enum ShowType : byte
            {
                Score,
                Level
            }

            public static ShowType ShowBy = ShowType.Score;

            private static List<HighScore> highscoresSortedByScore = new List<HighScore>();
            private static List<HighScore> highscoresSortedByLevel = new List<HighScore>();
            private static void Sort()
            {
                highscoresSortedByScore = new List<HighScore>(highscores);
                highscoresSortedByScore.Sort((score1, score2) => (int)(score2.Score - score1.Score));

                highscoresSortedByLevel = new List<HighScore>(highscores);
                highscoresSortedByLevel.Sort((score1, score2) => (score2.LevelReached == score1.LevelReached
                    ? score2.Score - score1.Score
                    : score2.LevelReached - score1.LevelReached));
            }

            public static void Initialize(GraphicsDevice graphicsDevice)
            {
                //Initializing buttons
                buttons = new[]
                {
                    new Button("Main Menu", new Vector2(25, graphicsDevice.Viewport.Height - 50), font60),
                    new Button("Sort by " + (ShowBy == ShowType.Level ? "Score" : "Level"),
                        new Vector2(graphicsDevice.Viewport.Width - 500, graphicsDevice.Viewport.Height - 50), font60)
                };

                buttons[0].OnClick += delegate
                {
                    gameState = GameState.MainMenu;
                };

                buttons[1].OnClick += delegate
                {
                    ShowBy = (ShowBy == ShowType.Level ? ShowType.Score : ShowType.Level);
                    buttons[1].Reset();
                    Initialize(graphicsDevice);
                };

                // Set text alignment by X
                AlignByX = new int[]
                {
                    200,
                    graphicsDevice.Viewport.Width/2 - 100,
                    graphicsDevice.Viewport.Width - 300
                };

                // Calculate count
                CountY = graphicsDevice.Viewport.Height/50 - 4;
            }
            public static void LoadContent(ContentManager content, GraphicsDevice device)
            {
                font48 = content.Load<SpriteFont>("font48");
                font60 = content.Load<SpriteFont>("font60");
                centerX = device.Viewport.Width / 2;

                try
                {
                    using (BinaryReader reader = new BinaryReader(File.Open("highscores", FileMode.OpenOrCreate)))
                    {
                        int count = ~(reader.ReadInt32());

                        for (int i = 0; i < count; i++)
                        {
                            int checksum = count;

                            int score = ~(reader.ReadInt32());
                            checksum ^= score;

                            int level = ~(reader.ReadInt32());
                            checksum ^= level;

                            int length = ~(reader.ReadInt32());
                            checksum ^= length;

                            string name = new string(reader.ReadChars(length));
                            checksum ^= name.Sum(c => (byte) c);

                            if (checksum == reader.ReadInt32())
                            {
                                highscores.Add(new HighScore(name, score, level));
                            }
                        }
                    }
                }
                catch(Exception e)
                {}

                Sort();
                Save();
            }

            /// <summary>
            /// Adds parameter highscore to highscores
            /// </summary>
            /// <returns>
            /// String containing informationg about parameter highscore position in highscores
            /// </returns>
            public static string Add(string name, int score, int levelReached)
            {
                if (score >= 0)
                {
                    highscores.Add(new HighScore(name, score, levelReached));

                    Save();
                    Sort();

                    return string.Format("Your score {0} is on position {1} out of total {2}!",
                        score,
                        highscoresSortedByLevel.FindIndex(hscore => hscore.Score == score && hscore.Name == name
                                                                    && hscore.LevelReached == levelReached) + 1,
                        highscores.Count);
                }
                else
                {
                    return string.Empty;
                }
            }
            public static void Save()
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open("highscores", FileMode.Create)))
                {
                    writer.Write(~highscores.Count);

                    foreach (HighScore highscore in highscores)
                    {
                        int checksum = highscores.Count;

                        writer.Write(~(highscore.Score));
                        checksum ^= highscore.Score;

                        writer.Write(~(highscore.LevelReached));
                        checksum ^= highscore.LevelReached;

                        writer.Write(~(highscore.Name.Length));
                        checksum ^= highscore.Name.Length;

                        writer.Write(highscore.Name.ToCharArray());
                        checksum ^= highscore.Name.Sum(c => (byte)c);

                        writer.Write(checksum);
                    }
                }
            }

            private static int[] AlignByX;
            private static int CountY;
            public static void Draw(SpriteBatch spriteBatch)
            {
                // Drawing headers
                spriteBatch.DrawString(font60, "Name", new Vector2(AlignByX[0] - font60.MeasureString("Name").X/2, 50), 
                    Color.White * 0.75f);
                spriteBatch.DrawString(font60, "Score", new Vector2(AlignByX[1] - font60.MeasureString("Score").X/2, 50), 
                    Color.White * 0.75f);
                spriteBatch.DrawString(font60, "Reached level", new Vector2(AlignByX[2] - font60.MeasureString("Reached level").X/2, 50), 
                    Color.White * 0.75f);

                // Drawing body


                List<HighScore> list = (ShowBy == ShowType.Score) ? highscoresSortedByScore : highscoresSortedByLevel;

                for (int i = 0; i < Math.Min(CountY, list.Count); i++)
                {
                    spriteBatch.DrawString(font48, list[i].Name, new Vector2(AlignByX[0] - font48.MeasureString(list[i].Name).X/2, 
                        150 + 50*i), Color.White * 0.75f);
                    spriteBatch.DrawString(font48, list[i].Score.ToString(), new Vector2(AlignByX[1] - 
                        font48.MeasureString(list[i].Score.ToString()).X / 2, 150 + 50 * i), Color.White * 0.75f);
                    spriteBatch.DrawString(font48, list[i].LevelReached.ToString(), new Vector2(AlignByX[2] -
                        font48.MeasureString(list[i].LevelReached.ToString()).X / 2, 150 + 50 * i), Color.White * 0.75f);
                }

                foreach (Button button in buttons)
                {
                    button.Draw(spriteBatch);
                }
            }

            public static void Update(MouseState mouseState)
            {
                foreach (Button button in buttons)
                {
                    button.Update(mouseState);
                }
            }
        }

        public static class Credits
        {
            private static readonly Tuple<string, string>[] credits = new[]
            {
                Tuple.Create("Programmer, Game Creator", "Alexey Savenkov aka Capture_A_Lag"),
                Tuple.Create("Music", "Alexander Startsev aka Magic C"),
                Tuple.Create("Background Pixel Art", "Squirrelsquid"),
                Tuple.Create("Random Pixel Spaceships Idea", "David Bollinger")
            };

            private static Button button;
            private static int centerX;
            private static SpriteFont font48, font60;
            public static void Initialize(GraphicsDevice graphicsDevice, ContentManager content)
            {
                centerX = graphicsDevice.Viewport.Width/2;
                font48 = content.Load<SpriteFont>("font48");
                font60 = content.Load<SpriteFont>("font60");

                button = new Button("Main Menu", new Vector2(centerX - font60.MeasureString("Main Menu").X / 2,
                                                            graphicsDevice.Viewport.Height - 50), font60);

                button.OnClick += delegate
                {
                    button.Reset();
                    gameState = GameState.MainMenu;
                };
            }

            public static void Update(MouseState mouseState)
            {
                button.Update(mouseState);
            }

            public static void Draw(SpriteBatch spriteBatch)
            {
                for (int i = 0; i < credits.Length; i++)
                {
                    spriteBatch.DrawString(font60, credits[i].Item1, new Vector2(centerX - font60.MeasureString(credits[i].Item1).X/2,
                        100 + 150 * i), Color.White * .75f);
                    spriteBatch.DrawString(font48, credits[i].Item2, new Vector2(centerX - font48.MeasureString(credits[i].Item2).X/2, 
                        150 + 150 * i), Color.White * .75f);
                }

                button.Draw(spriteBatch);
            }
        }

        public static class HowToPlay
        {
            private abstract class Frame
            {
                protected Tuple<Texture2D, Vector2>[] illustrations;
                protected Tuple<string, Vector2, SpriteFont>[] text;

                public void Draw(SpriteBatch spriteBatch)
                {
                    foreach (Tuple<Texture2D, Vector2> illustration in illustrations)
                    {
                        spriteBatch.Draw(illustration.Item1, illustration.Item2, Color.White*0.75f);
                    }

                    foreach (var line in text)
                    {
                        spriteBatch.DrawString(line.Item3, line.Item1, line.Item2 - new Vector2(line.Item3.MeasureString(line.Item1).X/2, 0), 
                            Color.White*0.75f);
                    }
                }
            }
            private class Frame0 : Frame
            {
                public Frame0()
                {
                    text = new Tuple<string, Vector2, SpriteFont>[]
                    {
                        Tuple.Create("Welcome To Space Rogues!", new Vector2(graphicsDevice.Viewport.Width/2, 50),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("This game is a side-scrolling space shooter", new Vector2(graphicsDevice.Viewport.Width/2, 200),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("where the main task is to get", new Vector2(graphicsDevice.Viewport.Width/2, 250),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("the highest score", new Vector2(graphicsDevice.Viewport.Width/2, 300),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("before the death of your ship.", new Vector2(graphicsDevice.Viewport.Width/2, 350),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("Click 'next' to learn more!", new Vector2(graphicsDevice.Viewport.Width/2, 450),
                        Content.Load<SpriteFont>("font48"))
                    };
                    illustrations = new Tuple<Texture2D, Vector2>[0];
                }
            }

            private class Frame1 : Frame
            {
                public Frame1()
                {
                    text = new Tuple<string, Vector2, SpriteFont>[]
                    {
                        Tuple.Create("Controls", new Vector2(graphicsDevice.Viewport.Width/2, 50),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("Move - Arrows", new Vector2(graphicsDevice.Viewport.Width/2, 250),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("Laser - Space", new Vector2(graphicsDevice.Viewport.Width/2, 350),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("Rocket - Left Control", new Vector2(graphicsDevice.Viewport.Width/2, 450),
                        Content.Load<SpriteFont>("font60")),
                    };
                    illustrations = new Tuple<Texture2D, Vector2>[0];
                }

            }

            private class Frame2 : Frame
            {
                public Frame2()
                {
                    text = new Tuple<string, Vector2, SpriteFont>[]
                    {
                        Tuple.Create("Ways to earn and lose score", new Vector2(graphicsDevice.Viewport.Width/2, 25),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("You gain score when enemy ship dies or when", new Vector2(graphicsDevice.Viewport.Width/2, 100),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("ally ship gets to the right side of the screen.", new Vector2(graphicsDevice.Viewport.Width/2, 140),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("You lose score when ally ship dies or when", new Vector2(graphicsDevice.Viewport.Width/2, 180),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("enemy ship gets to the right side of the screen.", new Vector2(graphicsDevice.Viewport.Width/2, 220),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("Also you gain(lose) some score when enemy(ally) ship", new Vector2(graphicsDevice.Viewport.Width/2, 260),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("flies uncontrollably to non-right side of the screen", new Vector2(graphicsDevice.Viewport.Width/2, 300),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("(because of ramming or explosion)", new Vector2(graphicsDevice.Viewport.Width/2, 320),
                        Content.Load<SpriteFont>("font20")),
                        Tuple.Create("but not as much as when they die.", new Vector2(graphicsDevice.Viewport.Width/2, 340),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("How to distinguish enemy and ally ships? Easy!", new Vector2(graphicsDevice.Viewport.Width/2, 390),
                        Content.Load<SpriteFont>("font32")),
                        Tuple.Create("Ally ships are the same color as you. Enemies are different.", new Vector2(graphicsDevice.Viewport.Width/2, 430),
                        Content.Load<SpriteFont>("font32"))
                    };
                    illustrations = new[]
                    {
                        Tuple.Create(Content.Load<Texture2D>("Howto/yes"), new Vector2(graphicsDevice.Viewport.Width/2 - 400, 420)),
                        Tuple.Create(Content.Load<Texture2D>("Howto/no"), new Vector2(graphicsDevice.Viewport.Width/2 + 100, 420)),
                    };
                }
            }

            private class Frame3 : Frame
            {
                public Frame3()
                {
                    text = new Tuple<string, Vector2, SpriteFont>[]
                    {
                        Tuple.Create("Congratulations!", new Vector2(graphicsDevice.Viewport.Width/2, 50),
                        Content.Load<SpriteFont>("font60")),
                        Tuple.Create("You now know the basics of space roguing!", new Vector2(graphicsDevice.Viewport.Width/2, 200),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("You will probably get the rest by yourself!", new Vector2(graphicsDevice.Viewport.Width/2, 250),
                        Content.Load<SpriteFont>("font48")),
                        Tuple.Create("Have fun!", new Vector2(graphicsDevice.Viewport.Width/2, 300),
                        Content.Load<SpriteFont>("font48"))
                    };
                    illustrations = new Tuple<Texture2D, Vector2>[0];
                }
            }

            private const int frames_count = 4;
            private static int frame_index;

            private static SpriteFont font48, font60;

            private static Frame[] frames;
            private static Button[] buttons;

            private static GraphicsDevice graphicsDevice;
            private static ContentManager Content;

            public static void Initialize(GraphicsDevice device, ContentManager content = null)
            {
                if (content != null)
                {
                    Content = content;
                    font48 = Content.Load<SpriteFont>("font48");
                    font60 = Content.Load<SpriteFont>("font60");
                }

                graphicsDevice = device;

                frame_index = 0;

                frames = new Frame[]
                {
                    new Frame0(),
                    new Frame1(),
                    new Frame2(),
                    new Frame3()
                };


                int centerX = graphicsDevice.Viewport.Width/2;

                buttons = new[]
                {
                    new Button("Main Menu", new Vector2(25, graphicsDevice.Viewport.Height - 50), font60),
                    new Button("Back", new Vector2(centerX - font60.MeasureString("Back").X/2,
                        graphicsDevice.Viewport.Height - 50), font60),
                    new Button("Next", new Vector2(centerX*2 - 100 - font60.MeasureString("Next").X,
                        graphicsDevice.Viewport.Height - 50), font60)
                };


                buttons[0].OnClick += delegate
                {
                    gameState = GameState.MainMenu;
                };

                buttons[1].OnClick += delegate
                {
                    buttons[1].Reset();
                    frame_index--;
                };

                buttons[2].OnClick += delegate
                {
                    buttons[2].Reset();
                    frame_index++;
                };
            }
            public static void Update(MouseState mouseState)
            {
                buttons[0].Update(mouseState);
                if (frame_index != 0)
                {
                    buttons[1].Update(mouseState);
                }
                if (frame_index != frames_count - 1)
                {
                    buttons[2].Update(mouseState);
                }
            }
            public static void Draw(SpriteBatch spriteBatch)
            {
                frames[frame_index].Draw(spriteBatch);

                buttons[0].Draw(spriteBatch);
                if (frame_index != 0)
                {
                    buttons[1].Draw(spriteBatch);
                }
                if (frame_index != frames_count - 1)
                {
                    buttons[2].Draw(spriteBatch);
                }
            }
        }

        public static void LoadContent(ContentManager content, GraphicsDevice device, Game game)
        {
            cursorTexture = content.Load<Texture2D>("cursor");

            MainMenu.LoadContent(content, game);
            HighScores.LoadContent(content, device);
            InGame.Explosion.LoadContent(content);
        }

        public static void UnloadContent()
        {
            HighScores.Save();
        }

        public static void Update(GameTime gameTime)
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    MainMenu.Update(Mouse.GetState(), Keyboard.GetState(), gameTime);
                    break;
                case GameState.InGame:
                    InGame.Update(gameTime, Keyboard.GetState());
                    break;
                case GameState.Highscores:
                    HighScores.Update(Mouse.GetState());
                    break;
                case GameState.Credits:
                    Credits.Update(Mouse.GetState());
                    break;
                case GameState.HowToPlay:
                    HowToPlay.Update(Mouse.GetState());
                    break;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    MainMenu.Draw(spriteBatch);
                    break;
                case GameState.InGame:
                    InGame.Draw(spriteBatch);
                    break;
                case GameState.Highscores:
                    HighScores.Draw(spriteBatch);
                    break;
                case GameState.Credits:
                    Credits.Draw(spriteBatch);
                    break;
                case GameState.HowToPlay:
                    HowToPlay.Draw(spriteBatch);
                    break;
            }

            //Drawing cursor
            if (!(gameState == GameState.InGame ^
                (InGame.inGameState == InGame.InGameState.Pause 
                || InGame.inGameState == InGame.InGameState.LevelEnd
                || InGame.inGameState == InGame.InGameState.GameOver)))
            {
                spriteBatch.Draw(cursorTexture, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White * 0.5f);
            }
        }
    }
}
