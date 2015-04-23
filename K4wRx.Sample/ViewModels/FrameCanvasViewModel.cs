using K4wRx.Sample.Models;
using Microsoft.Kinect;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace K4wRx.Sample.ViewModels
{
    public class FrameCanvasViewModel
    {
        private BodyPainter bodyPainter;
        private ColorPainter colorPainter;
        private BodyIndexPainter bodyIndexPainter;
        private DepthPainter depthPainter;
        private InfraredPainter infraredPainter;
        private KinectSensorModel kinectSensor;

        private IDisposable bodyDisposable;
        private IDisposable colorDisposable;
        private IDisposable bodyIndexDisposable;
        private IDisposable depthDisposable;
        private IDisposable infraredDisposable;

        private CompositeDisposable compositeDisposable;

        public FrameCanvasViewModel()
        {
            this.kinectSensor = new KinectSensorModel();
            this.kinectSensor.Start();
            this.bodyPainter = new BodyPainter();
            this.colorPainter = new ColorPainter();
            this.depthPainter = new DepthPainter();
            this.infraredPainter = new InfraredPainter();
            this.bodyIndexPainter = new BodyIndexPainter(kinectSensor.sensor);

            this.compositeDisposable = new CompositeDisposable();

            bodyIsChecked = new ReactiveProperty<bool>();
            colorIsChecked = new ReactiveProperty<bool>();
            depthIsChecked = new ReactiveProperty<bool>();
            infraredIsChecked = new ReactiveProperty<bool>();
            bodyIndexIsChecked = new ReactiveProperty<bool>();
        }

        public DrawingGroup DrawingGroup { get; set; }

        public ReactiveProperty<bool> bodyIsChecked { get; private set; }
        public ReactiveProperty<bool> colorIsChecked { get; private set; }
        public ReactiveProperty<bool> depthIsChecked { get; private set; }
        public ReactiveProperty<bool> infraredIsChecked { get; private set; }
        public ReactiveProperty<bool> bodyIndexIsChecked { get; private set; }

        /// <summary>
        /// Start drawing canvas
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            this.bodyIsChecked.Subscribe(e =>
            {
                if (e)
                {
                    bodyDisposable = this.kinectSensor.BodyStream.Subscribe(frame =>
                    {
                        this.DrawCanvas(this.DrawingGroup, frame);
                    });

                    compositeDisposable.Add(bodyDisposable);
                }
                else if (bodyDisposable != null)
                {
                    compositeDisposable.Remove(bodyDisposable);
                }
            });

            this.colorIsChecked.Subscribe(e =>
            {
                if (e)
                {
                    colorDisposable = this.kinectSensor.ColorStream.Subscribe(frame =>
                    {
                        this.DrawCanvas(this.DrawingGroup, frame);
                    });

                    compositeDisposable.Add(colorDisposable);
                }
                else if (colorDisposable != null)
                {
                    compositeDisposable.Remove(colorDisposable);
                }
            });

            this.bodyIndexIsChecked.Subscribe(e =>
            {
                if (e)
                {
                    bodyIndexDisposable = this.kinectSensor.BodyIndexStream.Subscribe(frame =>
                    {
                        this.DrawCanvas(this.DrawingGroup, frame);
                    });

                    compositeDisposable.Add(bodyIndexDisposable);
                }
                else if (bodyIndexDisposable != null)
                {
                    compositeDisposable.Remove(bodyIndexDisposable);
                }
            });

            this.depthIsChecked.Subscribe(e =>
            {
                if (e)
                {
                    depthDisposable = this.kinectSensor.DepthStream.Subscribe(frame =>
                    {
                        this.DrawCanvas(this.DrawingGroup, frame);
                    });

                    compositeDisposable.Add(depthDisposable);
                }
                else if (depthDisposable != null)
                {
                    compositeDisposable.Remove(depthDisposable);
                }
            });

            this.infraredIsChecked.Subscribe(e =>
            {
                if (e)
                {
                    infraredDisposable = this.kinectSensor.InfraredStream.Subscribe(frame =>
                    {
                        this.DrawCanvas(this.DrawingGroup, frame);
                    });

                    compositeDisposable.Add(infraredDisposable);
                }
                else if (infraredDisposable != null)
                {
                    compositeDisposable.Remove(infraredDisposable);
                }
            });
        }

        public void Stop()
        {
            this.compositeDisposable.Dispose();
        }

        /// <summary>
        /// Draw bones and hands on canvas when body frame is arrived
        /// </summary>
        /// <param name="bodies"></param>
        /// <param name="drawingGroup"></param>
        public void DrawCanvas(DrawingGroup drawingGroup, IEnumerable<Body> bodies)
        {
            using (DrawingContext dc = drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.bodyPainter.DisplayWidth, this.bodyPainter.DisplayHeight));

                int penIndex = 0;
                foreach (Body body in bodies)
                {
                    Pen drawPen = this.bodyPainter.BodyColors[penIndex++];

                    if (body.IsTracked)
                    {
                        this.bodyPainter.DrawClippedEdges(body, dc);

                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        // convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            // sometimes the depth(Z) of an inferred joint may show as negative
                            // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = BodyPainter.InferredZPositionClamp;
                            }

                            DepthSpacePoint depthSpacePoint = this.kinectSensor.CoordinateMapper.MapCameraPointToDepthSpace(position);
                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                        }

                        this.bodyPainter.DrawBody(joints, jointPoints, dc, drawPen);

                        this.bodyPainter.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                        this.bodyPainter.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                    }
                }

                // prevent drawing outside of our render area
                drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.bodyPainter.DisplayWidth, this.bodyPainter.DisplayHeight));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingGroup"></param>
        /// <param name="e"></param>
        public void DrawCanvas(DrawingGroup drawingGroup, ColorFrameArrivedEventArgs e)
        {
            using (DrawingContext dc = drawingGroup.Open())
            {
                colorPainter.Draw(dc, e);
            }
        }

        public void DrawCanvas(DrawingGroup drawingGroup, BodyIndexFrameArrivedEventArgs e)
        {
            using (DrawingContext dc = drawingGroup.Open())
            {
                bodyIndexPainter.Draw(dc, e);
            }
        }

        public void DrawCanvas(DrawingGroup drawingGroup, DepthFrameArrivedEventArgs e)
        {
            using (DrawingContext dc = drawingGroup.Open())
            {
                depthPainter.Draw(dc, e);
            }
        }

        public void DrawCanvas(DrawingGroup drawingGroup, InfraredFrameArrivedEventArgs e)
        {
            using (DrawingContext dc = drawingGroup.Open())
            {
                infraredPainter.Draw(dc, e);
            }
        }
    }
}
