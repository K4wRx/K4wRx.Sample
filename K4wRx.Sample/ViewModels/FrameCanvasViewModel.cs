using K4wRx.Sample.Models;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace K4wRx.Sample.ViewModels
{
    public class FrameCanvasViewModel
    {
        private BodyPainter bodyPainter;

        private KinectSensorModel kinectSensor;

        public FrameCanvasViewModel()
        {
            this.kinectSensor = new KinectSensorModel();
            this.bodyPainter = new BodyPainter();
        }

        /// <summary>
        /// Draw bones and hands on canvas when body frame is arrived
        /// </summary>
        /// <param name="bodies"></param>
        public void DrawCanvas(DrawingGroup drawingGroup, Body[] bodies)
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
    }
}
