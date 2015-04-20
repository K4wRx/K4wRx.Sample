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
    public class KinectSensorModel : INotifyPropertyChanged
    {
        private IDisposable disposer;

        public CoordinateMapper CoordinateMapper;
        public KinectSensor sensor;

        private Body[] bodies;
        public Body[] Bodies {
            get
            {
                return this.bodies;
            }
            private set
            {
                this.bodies = value;
                var h = this.PropertyChanged;
                if (h != null)
                {
                    h(this, new PropertyChangedEventArgs("Bodies"));
                }
            }
        }

        public KinectSensorModel()
        {
            this.sensor = KinectSensor.GetDefault();
            this.CoordinateMapper = this.sensor.CoordinateMapper;
        }

        public void Start()
        {
            this.disposer = this.sensor.BodyAsObservable().Subscribe(bodies =>
            {
                this.Bodies = bodies.ToArray();
            });

            this.sensor.Open();
        }

        public void Stop()
        {
            if (this.disposer != null)
            {
                this.disposer.Dispose();
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
