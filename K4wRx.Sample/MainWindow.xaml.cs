using K4wRx.Sample.Models;
using K4wRx.Sample.ViewModels;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace K4wRx.Sample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameCanvasViewModel canvasVM;
        private DrawingGroup drawingGroup;
        private DrawingImage drawingImage;

        public ImageSource ImageSource
        {
            get
            {
                return this.drawingImage;
            }
        }

        public MainWindow()
        {
            this.canvasVM = new FrameCanvasViewModel();

            this.drawingGroup = new DrawingGroup();
            this.drawingImage = new DrawingImage(drawingGroup);

            this.DataContext = this;

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // TODO: resume KinectStream
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: start binding between stream and property
        }


    }
}
