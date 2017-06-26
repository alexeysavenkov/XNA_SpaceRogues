using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpaceshipGenerator
{
    public static class PixelMaskGenerator
    {
        //for randomizations
        private static Random rnd = new Random();


        #region Basic templates

        /// <summary>
        /// Basic template for half a robot (right column becomes center of pixel mask)
        /// Full robot is 7 pixels wide and 11 pixels tall
        /// 0 = Empty
        /// 1 = Possibly filled (random)
        /// </summary>
        static readonly int[,] BASICROBOT = new int[,] 
        {
            {0,0,0,0},
            {0,1,1,1},
            {0,1,1,1},
            {0,0,1,1},
            {0,0,0,1},
            {0,1,1,1},
            {0,1,1,1},
            {0,0,0,1},
            {0,0,0,1},
            {0,1,1,1},
            {1,1,1,1}
        };

        /// <summary>
        /// Template for half a robot (right column becomes center of pixel mask)
        /// This robot is ensured to have a spine so it's parts are connected
        /// Full robot is 7 pixels wide and 11 pixels tall
        /// 0 = Empty
        ///  1 = Randomly chosen Empty/Body
        ///  2 = Randomly chosen Border/Body
        /// </summary>
        static readonly int[,] BASIC_SPINED_ROBOT = new int[,] 
        {
            {0,0,0,0},
            {0,1,1,1},
            {0,1,1,2},
            {0,0,1,2},
            {0,0,0,2},
            {0,1,1,2},
            {0,1,1,2},
            {0,0,0,2},
            {0,0,0,2},
            {0,1,2,2},
            {1,1,1,1}
        };


        /// <summary>
        /// Basic template for half a spaceship
        /// Full spaceship is 12 pixels wide and 12 pixels tall
        /// -1 = Always border (black)
        ///  0 = Empty
        ///  1 = Randomly chosen Empty/Body
        ///  2 = Randomly chosen Border/Body
        /// </summary>
        static readonly int[,] BASIC_SPACESHIP = new int[,] 
        {
            {0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 1},
            {0, 0, 0, 0, 1,-1},
            {0, 0, 0, 1, 1,-1},
            {0, 0, 0, 1, 1,-1},
            {0, 0, 1, 1, 1,-1},
            {0, 1, 1, 1, 2, 2},
            {0, 1, 1, 1, 2, 2},
            {0, 1, 1, 1, 2, 2},
            {0, 1, 1, 1, 1,-1},
            {0, 0, 0, 1, 1, 1},
            {0, 0, 0, 0, 0, 0},
        };

        #endregion


        #region Enumerations

        /// <summary>
        /// Defines whether a mask-template is mirrorred fully or around the right-most column
        /// </summary>
        public enum MirrorMode
        {
            MirrorAll,
            MirrorOnRightColumn
        }


        /// <summary>
        /// Whether to rotate an array clockwise or counter-clockwise
        /// </summary>
        public enum RotationMode
        {
            Clockwise,
            CounterClockwise
        }

        #endregion


        #region Shades of black array
        
        //used for antialiasing - contains the opacity to use
        //depending on the number of black pixels surrounding the pixel in question
        static Color[] shadesOfBlack = new Color[] 
        { 
            Color.FromArgb(0, Color.Black),     // 0 neighborpixels
            Color.FromArgb(0, Color.Black),     // 1 neighborpixel
            Color.FromArgb(96, Color.Black),    // 2 neighborpixels
            Color.FromArgb(128, Color.Black),   // 3 neighborpixels
            Color.FromArgb(128, Color.Black)    // 4 neighborpixels
        }; 

        #endregion


        #region All-in-one code

        /// <summary>
        /// Gets a completely random and finished Spaceship with borders
        /// </summary>
        /// <returns>The spaceship as a mask</returns>
        public static int[,] GetCompletedRandomSpaceshipMask()
        {
            int[,] spaceshipMask = BasicSpaceshipMask;
            PixelMaskGenerator.RandomizeMask(spaceshipMask);
            spaceshipMask = PixelMaskGenerator.MirrorArrayHorizontally(spaceshipMask, PixelMaskGenerator.MirrorMode.MirrorAll);
            PixelMaskGenerator.AddBorderToMask(spaceshipMask);
            return spaceshipMask;
        }

        public static Bitmap GetCompletedRandomSpaceshipImage(int scale, Color foregroundColor, Color? backgroundColor, bool antialiased)
        {
            return PixelMaskGenerator.MaskToBitmap(GetCompletedRandomSpaceshipMask(), scale, foregroundColor, backgroundColor, antialiased);
        }

        public static Bitmap GetCompletedRandomSpaceshipImage(int scale, Color foregroundColor)
        {
            return PixelMaskGenerator.MaskToBitmap(GetCompletedRandomSpaceshipMask(), scale, foregroundColor, null, false);
        }


        #endregion


        #region Basic mask
        
        /// <summary>
        /// The basic spaceshipmask.
        /// It is only half a finished spaceship and needs randomization and borders added.
        /// </summary>
        public static int[,] BasicSpaceshipMask
        {
            get
            {
                //we rotate and flip before returning 
                //because the array is presented in a format that is easily hand-editable in VS.Net
                int[,] ship = RotateArray90Degrees(BASIC_SPACESHIP, RotationMode.Clockwise);
                FlipArrayHorizontally(ship);
                return ship;
            }
        }

        #endregion


        #region Drawing code

        //Dictionary to speed up getting a Brush
        private static Dictionary<Color, Brush> SolidBrushes = new Dictionary<Color, Brush>();

        //Gets a SolidBrush in the wanted color
        private static Brush GetSolidBrush(Color color)
        {
            //if we haven't got a brush in that color yet
            if (!SolidBrushes.ContainsKey(color))
            {
                //create and store it
                SolidBrushes.Add(color, new SolidBrush(color));
            }
            //return SolidBrush in the wanted color
            return SolidBrushes[color];
        }

        /// <summary>
        /// Creates a Bitmap from a pixelmask
        /// </summary>
        /// <param name="mask">The mask to use. 0=backgroundcolor (null=transparent), 1=foregroundColor, </param>
        /// <param name="scale">How much to scale the mask. E.g. a mask of 10x10 ints with a scale of 4 turn into a 40x40 pixel bitmap.
        /// Must be a positive value.</param>
        /// <param name="foregroundColor">The color of the body (the 1's)</param>
        /// <param name="backgroundColor">The color of the space around (the 0's)</param>
        /// <returns></returns>
        public static Bitmap MaskToBitmap(int[,] mask, int scale, Color foregroundColor, Color? backgroundColor, bool antialiased)
        {

            //check for invalid input
            if (scale <= 0)
            {
                throw new ArgumentException("Scale must be a positive integer!");
            }

            //get size of double array
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            //prepare a bitmap 
            Bitmap bmp = new Bitmap(width * scale, height * scale);

            //...and a Graphics handle to draw on the bitmap
            Graphics g = Graphics.FromImage(bmp);

            int value = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //find pixelvalue
                    value = mask[x, y];

                    switch (value)
                    {

                        case -1:
                            //border - black
                            g.FillRectangle(Brushes.Black, x * scale, y * scale, scale, scale);
                            break;

                        case 0:
                            //empty - transparent or backgroundcolor
                            if (backgroundColor != null)
                            {
                                g.FillRectangle(GetSolidBrush((Color)backgroundColor), x * scale, y * scale, scale, scale);
                            }
                            if (antialiased)
                            {
                                int shadeIndex = GetNumberOfNeighborBorderTiles(mask, x, y);
                                g.FillRectangle(GetSolidBrush(shadesOfBlack[shadeIndex]), x * scale, y * scale, scale, scale);
                                
                            }
                            break;

                        case 1:
                            //body - foregroundColor or white
                            if (foregroundColor != null)
                            {
                                g.FillRectangle(GetSolidBrush((Color)foregroundColor), x * scale, y * scale, scale, scale);
                            }
                            else
                            {
                                g.FillRectangle(Brushes.White, x * scale, y * scale, scale, scale);
                            }

                            break;
                    }
                }
            }

            return bmp;
        }


        /// <summary>
        /// Creates a Bitmap with transparency from a mask (double int-array).
        /// Values of -1 are black, values of 0 are transparent, values above 0 are white.
        /// </summary>
        /// <param name="mask">The mask to turn into a bitmap</param>
        /// <param name="scale">How much to scale the mask. E.g. a mask of 10x10 ints with a scale of 4 turn into a 40x40 pixel bitmap.
        /// Must be a positive value.</param>
        /// <returns>The bitmap represented by the mask</returns>
        public static Bitmap MaskToBitmap(int[,] mask, int scale)
        {
            return MaskToBitmap(mask, scale, Color.White, null, true);
        }


        
        /// <summary>
        /// Creates a Bitmap with transparency from a mask (double int-array).
        /// Values of -1 are black, values of 0 are transparent, values above 0 are white.
        /// </summary>
        /// <param name="mask">The mask to turn into a bitmap</param>
        /// <returns>The bitmap represented by the mask</returns>
        public static Bitmap MaskToBitmap(int[,] mask)
        {
            return MaskToBitmap(mask, 1);
        }




        #endregion


        #region Randomization code

        /// <summary>
        /// Randomizes a mask.
        /// Values of 1 are set to either 0 (empty) or 1 (body).
        /// Values of 2 are set to either -1 (border) or 1 (body).
        /// </summary>
        /// <param name="mask">The mask to randomize.</param>
        public static void RandomizeMask(int[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (mask[x, y])
                    {


                        case 1:
                            //it's part of the body
                            //multiply by 1 or 0 to either keep current value or set to zero
                            mask[x, y] = mask[x, y] * rnd.Next(2);
                            break;

                        case 2:
                            //it's part of the core - mustn't be cleared
                            if (rnd.Next(2) > 0)
                            {
                                mask[x, y] = 1;
                            }
                            else
                            {
                                mask[x, y] = -1;
                            }

                            break;
                    }
                }
            }
        }

        #endregion


        #region Code for adding borders

        /// <summary>
        /// Adds a -1 value above, below, right and left of "1" values in the array
        /// </summary>
        /// <param name="mask">The mask to add borders to</param>
		public static void AddBorderToMask(int[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask[x, y] > 0)
                    {
                        AddBorderToPixel(mask, x, y);
                    }
                }
            }
        }

        //Stores the coordinates for the pixels above, below, right and left of a pixel (x and y values).
        private static int[,] Neighbours = new int[4, 2] { { 0, -1 }, { -1, 0 }, { 1, 0 }, { 0, 1 } };

        /// <summary>
        /// Puts "-1" values in the cells above, below, right and left of the cell, if they are currently empty
        /// </summary>
        /// <param name="mask">The mask to add the borders in</param>
        /// <param name="x">x-index of the cell to add borders to</param>
        /// <param name="y">y-index of the cell to add borders to</param>
        private static void AddBorderToPixel(int[,] mask, int x, int y)
        {
            //find the size
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            int newX = 0;
            int newY = 0;
            //for all four neighbours...
            for (int i = 0; i < 4; i++)
            {
                //find out where the neighbour should be
                newX = x + Neighbours[i, 0];
                newY = y + Neighbours[i, 1];

                //if the neighbour is within the array
                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    //...and is empty
                    if (mask[newX, newY] == 0)
                    {
                        //add border
                        mask[newX, newY] = -1;
                    }
                }
            }
        }

	#endregion    


        #region Array helper code

        //rotates the values in an array 90 degrees
        public static int[,] RotateArray90Degrees(int[,] array, RotationMode rotation)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            int[,] rotatedArray = new int[height, width];

            if (rotation == RotationMode.CounterClockwise)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        rotatedArray[y, width - 1 - x] = array[x, y];
                    }
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        rotatedArray[height - 1 - y, x] = array[x, y];
                    }
                }
            }
            return rotatedArray;
        }

        //flips values in an array horizontally
        public static void FlipArrayHorizontally(int[,] arrayToFlip)
        {
            int width = arrayToFlip.GetLength(0);
            int height = arrayToFlip.GetLength(1);
            int temp = 0;
            for (int x = 0; x < width / 2; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    temp = arrayToFlip[width - 1 - x, y];
                    arrayToFlip[width - 1 - x, y] = arrayToFlip[x, y];
                    arrayToFlip[x, y] = temp;
                }
            }
        }

        //Mirrors an array into a wider array
        public static int[,] MirrorArrayHorizontally(int[,] mask, MirrorMode mode)
        {
            //get dimensions
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            int newWidth = width * 2;

            //if we mirror on right column then we subtract one from the new width
            if (mode == MirrorMode.MirrorOnRightColumn)
            {
                newWidth--;
            }
            //create the new doublearray to store the pixelimage
            int[,] fullsize = new int[newWidth, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    fullsize[x, y] = mask[x, y];
                    fullsize[(newWidth - 1) - x, y] = mask[x, y];
                }
            }

            return fullsize;
        }


        /// <summary>
        /// Gets the number of neighboring indexes containing a fully black value (-1)
        /// </summary>
        /// <param name="mask">The double-array to look in</param>
        /// <param name="x">the x-coordinate of the cell to count neighbors for</param>
        /// <param name="y">the y-coordinate of the cell to count neighbors for</param>
        /// <returns>The number of neighbor-cells of the cell in question</returns>
        private static int GetNumberOfNeighborBorderTiles(int[,] mask, int x, int y)
        {
            int newX = 0;
            int newY = 0;
            int neighborCount = 0;
            for (int i = 0; i < 4; i++)
            {
                //find out where the neighbour should be
                newX = x + Neighbours[i, 0];
                newY = y + Neighbours[i, 1];

                //if the neighbour is within the array
                if (mask.IsWithinArrayBounds(newX, newY))
                {
                    //...and is border
                    if (mask[newX, newY] == -1)
                    {
                        neighborCount++;
                    }
                }
            }
            return neighborCount;
        }

        /// <summary>
        /// Extensionmethod to ascertain that a given position is within an array.
        /// To avoid the dreaded IndexOutOfBoundsException :)
        /// </summary>
        /// <param name="theArray">The array to look in</param>
        /// <param name="x">The x-position (first dimension)</param>
        /// <param name="y">The y-position (second dimension)</param>
        /// <returns>Whether the position is within the double-array</returns>
        public static bool IsWithinArrayBounds(this int[,] theArray, int x, int y)
        {
            return (x >= 0 && x < theArray.GetLength(0) && y >= 0 && y < theArray.GetLength(1));
        }

        #endregion

       
    }
}
