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
    class InfraredPainter
    {
        /// <summary>
        /// Maximum value (as a float) that can be returned by the InfraredFrame
        /// </summary>
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /// <summary>
        /// The value by which the infrared source data will be scaled
        /// </summary>
        private const float InfraredSourceScale = 0.75f;

        /// <summary>
        /// Smallest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// Largest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        public int DisplayWidth = 0;

        public int DisplayHeight = 0;

        public InfraredPainter()
        {
            this.DisplayWidth = 600;
            this.DisplayHeight = 500;
        }

        public void Draw(DrawingContext dc, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    var width = infraredFrame.FrameDescription.Width;
                    var height = infraredFrame.FrameDescription.Height;

                    WriteableBitmap infraredBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Gray32Float, null);
                    // the fastest way to process the infrared frame data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        // verify data and write the new infrared frame data to the display bitmap
                        if (((width * height) == (infraredBuffer.Size / infraredFrame.FrameDescription.BytesPerPixel)) &&
                            (width == infraredBitmap.PixelWidth) && (height == infraredBitmap.PixelHeight))
                        {
                            this.ProcessInfraredFrameData(infraredBitmap, infraredBuffer.UnderlyingBuffer, infraredBuffer.Size, infraredFrame.FrameDescription.BytesPerPixel);
                        }
                    }

                    dc.DrawImage(infraredBitmap, new Rect(0, 0, this.DisplayWidth, this.DisplayHeight));
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the InfraredFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the infraredFrameData pointer.
        /// </summary>
        /// <param name="infraredFrameData">Pointer to the InfraredFrame image data</param>
        /// <param name="infraredFrameDataSize">Size of the InfraredFrame image data</param>
        private unsafe void ProcessInfraredFrameData(WriteableBitmap infraredBitmap, IntPtr infraredFrameData, uint infraredFrameDataSize, uint bytesPerPixel)
        {
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*)infraredFrameData;

            // lock the target bitmap
            infraredBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)infraredBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int)(infraredFrameDataSize / bytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, infraredBitmap.PixelWidth, infraredBitmap.PixelHeight));

            // unlock the bitmap
            infraredBitmap.Unlock();
        }
    }
}
