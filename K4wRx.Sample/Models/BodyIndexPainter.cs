using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace K4wRx.Sample.Models
{
    class BodyIndexPainter
    {
        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Collection of colors to be used to display the BodyIndexFrame data.
        /// </summary>
        private static readonly uint[] BodyColor =
        {
            0x0000FF00,
            0x00FF0000,
            0xFFFF4000,
            0x40FFFF00,
            0xFF40FF00,
            0xFF808000,
        };

        public int DisplayWidth;
        public int DisplayHeight;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private uint[] bodyIndexPixels = null;

        public BodyIndexPainter()
        {
        }

        public BodyIndexPainter(KinectSensor sensor)
        {
            this.DisplayWidth = 600;
            this.DisplayHeight = 500;
            this.bodyIndexPixels = new uint[sensor.BodyIndexFrameSource.FrameDescription.Width * sensor.BodyIndexFrameSource.FrameDescription.Height];
        }

        public void Draw(DrawingContext dc, BodyIndexFrameArrivedEventArgs e)
        {
            bool bodyIndexFrameProcessed = false;

            using (BodyIndexFrame bodyIndexFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    var width = bodyIndexFrame.FrameDescription.Width;
                    var height = bodyIndexFrame.FrameDescription.Height;

                    WriteableBitmap bodyIndexBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer bodyIndexBuffer = bodyIndexFrame.LockImageBuffer())
                    {

                        // verify data and write the color data to the display bitmap
                        if (((width * height) == bodyIndexBuffer.Size) &&
                            (width == bodyIndexBitmap.PixelWidth) && (height == bodyIndexBitmap.PixelHeight))
                        {
                            this.ProcessBodyIndexFrameData(bodyIndexBuffer.UnderlyingBuffer, bodyIndexBuffer.Size);
                            bodyIndexFrameProcessed = true;
                        }
                    }

                    if (bodyIndexFrameProcessed)
                    {
                        this.RenderBodyIndexPixels(bodyIndexBitmap);
                    }

                    // write to go
                    dc.DrawImage(bodyIndexBitmap, new Rect(0, 0, this.DisplayWidth, this.DisplayHeight));
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the BodyIndexFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the bodyIndexFrameData pointer.
        /// </summary>
        /// <param name="bodyIndexFrameData">Pointer to the BodyIndexFrame image data</param>
        /// <param name="bodyIndexFrameDataSize">Size of the BodyIndexFrame image data</param>
        private unsafe void ProcessBodyIndexFrameData(IntPtr bodyIndexFrameData, uint bodyIndexFrameDataSize)
        {
            byte* frameData = (byte*)bodyIndexFrameData;

            // convert body index to a visual representation
            for (int i = 0; i < (int)bodyIndexFrameDataSize; ++i)
            {
                // the BodyColor array has been sized to match
                // BodyFrameSource.BodyCount
                if (frameData[i] < BodyColor.Length)
                {
                    // this pixel is part of a player,
                    // display the appropriate color
                    this.bodyIndexPixels[i] = BodyColor[frameData[i]];
                }
                else
                {
                    // this pixel is not part of a player
                    // display black
                    this.bodyIndexPixels[i] = 0x00000000;
                }
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderBodyIndexPixels(WriteableBitmap bodyIndexBitmap)
        {
            bodyIndexBitmap.WritePixels(
                new Int32Rect(0, 0, bodyIndexBitmap.PixelWidth, bodyIndexBitmap.PixelHeight),
                this.bodyIndexPixels,
                bodyIndexBitmap.PixelWidth * (int)BytesPerPixel,
                0);
        }
    }
}
