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

        public bool bodyIsChecked
        {
            get
            {
                return canvasVM.bodyIsChecked.Value;
            }
            set
            {
                canvasVM.bodyIsChecked.Value = value;
            }
        }

        public bool bodyIndexIsChecked
        {
            get
            {
                return canvasVM.bodyIndexIsChecked.Value;
            }
            set
            {
                canvasVM.bodyIndexIsChecked.Value = value;
            }
        }

        public bool colorIsChecked
        {
            get
            {
                return canvasVM.colorIsChecked.Value;
            }
            set
            {
                canvasVM.colorIsChecked.Value = value;
            }
        }

        public bool depthIsChecked
        {
            get
            {
                return canvasVM.depthIsChecked.Value;
            }
            set
            {
                canvasVM.depthIsChecked.Value = value;
            }
        }

        public bool infraredIsChecked
        {
            get
            {
                return canvasVM.infraredIsChecked.Value;
            }
            set
            {
                canvasVM.infraredIsChecked.Value = value;
            }
        }
        public MainWindow()
        {
            this.canvasVM = new FrameCanvasViewModel();

            this.drawingGroup = new DrawingGroup();
            this.drawingImage = new DrawingImage(drawingGroup);

            this.canvasVM.DrawingGroup = this.drawingGroup;
            this.canvasVM.Start();

            this.DataContext = this;

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            this.canvasVM.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: start binding between stream and property
        }


    }
}
