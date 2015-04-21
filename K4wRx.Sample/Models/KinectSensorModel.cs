using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Reactive;
using System.Reactive.Linq;
using K4wRx.Extensions;
using System.ComponentModel;

namespace K4wRx.Sample.Models
{
    public class KinectSensorModel
    {
        public CoordinateMapper CoordinateMapper;
        public KinectSensor sensor;

        public IObservable<IEnumerable<Body>> BodyStream;

        public IObservable<ColorFrameArrivedEventArgs> ColorStream;

        public IObservable<BodyIndexFrameArrivedEventArgs> BodyIndexStream;

        public KinectSensorModel()
        {
            this.sensor = KinectSensor.GetDefault();
            this.CoordinateMapper = this.sensor.CoordinateMapper;

            this.BodyStream = this.sensor.BodyAsObservable();
            this.ColorStream = this.sensor.ColorFrameAsObservable();
            this.BodyIndexStream = this.sensor.BodyIndexFrameAsObservable();
        }

        public void Start()
        {
            this.sensor.Open();
        }

        public void Stop()
        {
        }
    }
}
