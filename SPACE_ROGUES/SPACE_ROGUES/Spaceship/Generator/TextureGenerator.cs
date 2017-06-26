using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceshipGenerator
{
    public static class TextureGenerator
    {
        public static Texture2D GetCompletedRandomSpaceshipTexture(GraphicsDevice dev, int scale, Color foregroundColor)
        {
            return BitmapToTexture2D(dev,
                PixelMaskGenerator.GetCompletedRandomSpaceshipImage(scale,
                    System.Drawing.Color.FromArgb(foregroundColor.GetHashCode())));
        }
        private static Texture2D BitmapToTexture2D(GraphicsDevice dev, System.Drawing.Bitmap bmp)
        {
            Texture2D customTexture = new Texture2D(dev, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            // calculate the byte size: for PixelFormat.Format32bppArgb (standard for GDI bitmaps) it's the hight * stride
            int bufferSize = data.Height * data.Stride; // stride already incorporates 4 bytes per pixel

            // create buffer
            byte[] bytes = new byte[bufferSize];

            // copy bitmap data into buffer
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            // copy our buffer to the texture
            customTexture.SetData(bytes);

            // unlock the bitmap data
            bmp.UnlockBits(data);

            return customTexture;
        }
    }
}
