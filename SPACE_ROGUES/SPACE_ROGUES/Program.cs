using System;
using System.Windows.Forms;
using Launcher.Designed;

namespace SPACE_ROGUES
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                using (Launcher.Designed.FormDesigned launcher = new FormDesigned())
                {
                    if (launcher.ShowDialog() == DialogResult.Yes)
                    {
                        using (Game game = new Game(launcher.GetWidth(),
                                                    launcher.GetHeight(),
                                                    launcher.GetFullscr()))
                        {
                            game.Window.Title = "Space Rogues";

                            game.Run();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message - {0}\nSource - {1}\nStackTrace - {2}", e.Message, e.Source, e.StackTrace));
                throw;
            }
        }
    }
#endif
}

