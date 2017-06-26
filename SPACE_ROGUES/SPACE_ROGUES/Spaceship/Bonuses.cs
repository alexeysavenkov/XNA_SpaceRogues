using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SPACE_ROGUES
{
    public enum OwnerType : byte
    {
        Player,
        Enemy
    }
    public class BonusCollection
    {
        /// <summary>
        /// Shows if BonusCollection is player's or enemy's
        /// </summary>
        public OwnerType ownerType;

        /// <summary>
        /// Array of tuples that describe each bonus by index
        /// </summary>
        /// <remarks>
        /// String is name. Bool means if this bonus has lowered increase rate.
        /// </remarks>
        public static readonly Tuple<string, bool>[] BonusNames = new[]
        {
            Tuple.Create("Overall\nDamage", true),
            Tuple.Create("Overall\nProtection", true),
            Tuple.Create("Ramming\nDamage", false),
            Tuple.Create("Ramming\nProtection", false),
            Tuple.Create("Laser\nDamage", false),
            Tuple.Create("Laser\nProtection", false),
            Tuple.Create("Rocket\nDamage", false),
            Tuple.Create("Rocket\nProtection", false),
            
            // Only player bonuses next
            
            Tuple.Create("Rocket\nReload Speed", false),
            Tuple.Create("Overall\nScore", true),
            Tuple.Create("Score\nFrom Ramming", false),
            Tuple.Create("Score\nFrom Lasering", false),
            Tuple.Create("Score\nFrom Rocketing", false)
        };

        /// <summary>
        /// Minimum bonus increase
        /// </summary>
        private static readonly float[] BonusMin = {0.1f, 0.3f};

        /// <summary>
        /// Maximum bonus increase
        /// </summary>
        private static readonly float[] BonusMax = {0.4f, 0.9f};

        /// <summary>
        /// Contains values of bonuses by index.
        /// </summary>
        private float[] Values;

        /// <summary>
        /// Gets or sets bonus value by name
        /// </summary>
        /// <param name="name">Name of the bonus</param>
        /// <returns></returns>
        public float this[string name]
        {
            get { return Values[Array.FindIndex(BonusNames, tuple => (tuple.Item1 == name))]; }
            set { Values[Array.FindIndex(BonusNames, tuple => (tuple.Item1 == name))] = value; }
        }

        public static BonusCollection operator +(BonusCollection collection1, BonusCollection collection2)
        {
            if (collection1.ownerType != collection2.ownerType)
            {
                throw new Exception("Trying to add BonusCollections of different types");
            }

            BonusCollection new_collection = new BonusCollection(collection1.ownerType);
            
            for (int i = 0; i < collection1.Values.Length; i++)
            {
                new_collection.Values[i] = collection1.Values[i]*collection2.Values[i];
            }
            
            return new_collection;
        }

        public static BonusCollection operator *(BonusCollection collection, float f)
        {
            BonusCollection new_collection = new BonusCollection(collection.ownerType);

            for (int i = 0; i < collection.Values.Length; i++)
            {
                if (collection.Values[i] != 1f)
                {
                    new_collection.Values[i] = 1f + (collection.Values[i] - 1f) * f;
                }
            }

            return new_collection;
        }

        /// <summary>
        /// Creates BonusCollection with all bonuses set to 100%
        /// </summary>
        /// <param name="type">If BonusCollection is player's or enemy's</param>
        public BonusCollection(OwnerType type)
        {
            ownerType = type;
            Values = Enumerable.Repeat(1f, (ownerType == OwnerType.Enemy) ? 8 : 13).ToArray();
        }
    
        private static readonly Random random = new Random();

        public static BonusCollection Random(OwnerType type)
        {
            BonusCollection collection = new BonusCollection(type);
            List<int> range = Enumerable.Range(0, collection.Values.Length).ToList();

            const int bonusCount = 2;

            float sum = 0;

            for (int i = 1; i <= bonusCount; i++)
            {
                int rnd = random.Next(range.Count);
                int index = range[rnd];
                range.RemoveAt(rnd);

                int j = BonusNames[index].Item2 ? 0 : 1;

                collection.Values[index] += (float) random.NextDouble()*(BonusMax[j] - BonusMin[j]) + BonusMin[j];

                sum += (collection.Values[index] - 1f) * (j == 0 ? 2.5f : 1);

                if (i == bonusCount)
                {
                    collection *= ((float)random.NextDouble() * 0.3f + 0.75f) / sum;
                }
            }

            return collection;
        }

        public IEnumerable<string> ToStrings()
        {
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] != 1f)
                {
                    yield return string.Format("{0} {1}{2:P0}", BonusNames[i].Item1, (Values[i] > 1f ? '+' : '-'), Values[i]-1f);
                }
            }
        }
    }
}
