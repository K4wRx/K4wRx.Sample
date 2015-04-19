using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using K4wRx.Extensions;
using System.ComponentModel;

namespace K4wRx.Sample.Models
{
    public class KinectSensorModel : INotifyPropertyChanged
    {
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
        }

        public void Start()
        {
            this.sensor.BodyAsObservable().Subscribe(bodies =>
            {
                this.Bodies = bodies.ToArray();
            });

            this.sensor.Open();
        }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
