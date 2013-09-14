using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Media;
namespace KinectJam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private KinectSensor mSensor;
        private WriteableBitmap mColorBitmap;
        private WriteableBitmap mDepthBitmap;
        private byte[] mColorPixels;
        private short[] mDepthPixels;

        protected override void OnInitialized(EventArgs e)
        {
            Stream str = Properties.Resources.Test;
            SoundPlayer snd = new SoundPlayer(str);
            snd.Play();

            base.OnInitialized(e);

            foreach (var kinectSensor in KinectSensor.KinectSensors)
            {
                if (kinectSensor.Status == KinectStatus.Connected)
                {
                    mSensor = kinectSensor;
                    break;
                }
            }

            if (mSensor != null) {
                mSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                mDepthPixels = new short[mSensor.DepthStream.FramePixelDataLength];
                mDepthBitmap = new WriteableBitmap(mSensor.DepthStream.FrameWidth, mSensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray8,null);

                mSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                mColorPixels = new byte[mSensor.ColorStream.FramePixelDataLength];
                mColorBitmap = new WriteableBitmap(mSensor.ColorStream.FrameWidth, mSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Rgb24, null);
                
                mImage.Source = mDepthBitmap;
                
                mSensor.DepthFrameReady += mSensor_DepthFrameReady;
                mSensor.ColorFrameReady += mSensor_ColorFrameReady;
            }
            try
            {
                mSensor.Start();
            }
            catch (IOException)
            {
                throw;
            }
        }
       
        void mSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var df = e.OpenDepthImageFrame())
            {
                if (df != null)
                {
                    df.CopyPixelDataTo(mDepthPixels);
                    mDepthBitmap.WritePixels(new Int32Rect(0, 0, mDepthBitmap.PixelWidth, mDepthBitmap.PixelHeight), mDepthPixels, mDepthBitmap.PixelWidth * df.BytesPerPixel, 0);
                }
            }

            mImage.Source = mColorBitmap;
        }

        void mSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var cf = e.OpenColorImageFrame())
            {
                if (cf != null)
                {
                    cf.CopyPixelDataTo(mColorPixels);
                    mColorBitmap.WritePixels(new Int32Rect(0, 0, mColorBitmap.PixelWidth, mColorBitmap.PixelHeight), mColorPixels, mColorBitmap.PixelWidth * cf.BytesPerPixel, 0);
                }
            }

            mImage.Source = mDepthBitmap;
        }
    }
}
