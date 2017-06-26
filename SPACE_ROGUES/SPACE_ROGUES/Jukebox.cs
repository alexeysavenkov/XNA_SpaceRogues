using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace SPACE_ROGUES
{
    /// <summary>
    /// Class that loads and plays songs in rare-repeat order
    /// </summary>
    static class Jukebox
    {
        private static readonly List<Song> AllSongs = new List<Song>();
        private static List<Song> Queue; 

        private static readonly Random random = new Random();

        /// <summary>
        /// Loads all *.wma format files from Content/Music into AllSongs and Queue lists.
        /// </summary>
        /// <param name="content">Content Manager</param>
        public static void LoadContent(ContentManager content)
        {
            DirectoryInfo dir = new DirectoryInfo(content.RootDirectory + "/Music");

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            FileInfo[] files = dir.GetFiles("*.wma");

            if (files.Length == 0)
            {
                throw new FileNotFoundException();
            }

            foreach (FileInfo file in files)
            {
                AllSongs.Add(content.Load<Song>("Music/" + Path.GetFileNameWithoutExtension(file.Name)));
            }
            
            Queue = new List<Song>(AllSongs);
        }

        /// <summary>
        /// Checks if song has ended.
        /// If it has ended, starts next one.
        /// </summary>
        /// <remarks>
        /// When starting next song, it selects random song from Queue, starts it and then deletes it from Queue.
        /// When there are no elements in Queue, it fills it with elements from AllSongs.
        /// </remarks>
        public static void Update()
        {
            if (MediaPlayer.State != MediaState.Playing)
            {
                int rnd = random.Next(Queue.Count);

                MediaPlayer.Play(Queue[rnd]);

                Queue.RemoveAt(rnd);

                if (Queue.Count == 0)
                {
                    Queue = new List<Song>(AllSongs);
                }
            }
        }
    }
}
