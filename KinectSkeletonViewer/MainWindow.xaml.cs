using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;



using Microsoft.Kinect;
using System.IO;

namespace KinectSkeletonViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor myKinect;

        public MainWindow()
        {
            InitializeComponent();
        }

        int numeroFrames = 0;
        int nbrSke = 0;
        string gesture = "null";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Check to see if a Kinect is available
            if (  KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinects detected", "Camera Viewer");
                Application.Current.Shutdown();
            }

            // Get the first Kinect on the computer
            myKinect = KinectSensor.KinectSensors[0];
            myKinect.SkeletonStream.AppChoosesSkeletons = true;

            // Start the Kinect running and select the depth camera
            try
            {
                myKinect.SkeletonStream.Enable();
                myKinect.Start();
            }
            catch
            {
                MessageBox.Show("Kinect initialise failed", "Camera Viewer");
                Application.Current.Shutdown();
            }

            // connect a handler to the event that fires when new frames are available

            myKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

        }

        Brush skeletonBrush = new SolidColorBrush(Colors.Blue);

        void addLine(Joint j1, Joint j2)
        {
            Line boneLine = new Line();
            boneLine.Stroke = skeletonBrush;
            boneLine.StrokeThickness = 5;
           // float j1X, j1Y;

            DepthImagePoint j1P = myKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(j1.Position, DepthImageFormat.Resolution640x480Fps30);
            boneLine.X1 = j1P.X;
            boneLine.Y1 = j1P.Y;

            DepthImagePoint j2P = myKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(j2.Position, DepthImageFormat.Resolution640x480Fps30);
            boneLine.X2 = j2P.X;
            boneLine.Y2 = j2P.Y;

            skeletonCanvas.Children.Add(boneLine);
        }

        void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {            
            string message = "No Skeleton Data";
            //string FistBumpMessage = "NO GESTURE;";

            // Remove the old skeleton
            skeletonCanvas.Children.Clear();
            Brush brush = new SolidColorBrush(Colors.Red);

            SkeletonFrame frame = e.OpenSkeletonFrame();

            if (frame == null) return;

            Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skeletons);

        /*    KinectSensor.KinectSensors.

            if (skeletons[0].TrackingState != null)
                nbrSke = 1;
            else
                nbrSke = 0;   */        

            foreach (Skeleton skeleton in skeletons)

            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    Joint rightHandJoint = skeleton.Joints[JointType.HandRight];
                    Joint leftHandJoint = skeleton.Joints[JointType.HandLeft];
                    float distance;

                    distance = jointDistance(rightHandJoint, leftHandJoint);
                    message = string.Format("Distance: X:{0:0.00}", distance);

                    if (distance < 0.2)
                    {
                        tb.Text = numeroFrames.ToString();
                        numeroFrames++;                        
                    }
                    else
                    {
                        if(numeroFrames > 5)
                            if (numeroFrames < 25)
                            {
                               // FistBumpMessage = "FIST BUMP!!!";
                                fistBump();
                                numeroFrames = 0;
                            }
                            else
                            {
                               // FistBumpMessage = "HAND SHAKE!!!";
                                //FistBumpBlock.Text = "HAND SHAKE!!!";
                                handShake();
                                numeroFrames = 0;
                            }
                    }
                    DrawTrackedSkeletonJoints(skeleton);                                        
                }
            }
            numberSkeletons.Text = string.Format("Number of Skeletons: {0,3}", nbrSke);
            StatusTextBlock.Text = message;
           // FistBumpBlock.Text = FistBumpMessage;
        }

        private void fistBump()
        {
            GestureBlock.Text = "FIST BUMP!!!";
            if (gesture.Equals("null") || gesture.Equals("shake"))
                awe1.Source = new Uri("http://lia.dc.ufscar.br/andre.obueno/ThingBrokerDemo/fist.html");
            else
                awe1.ExecuteJavascript("window.location.reload(true);");
            gesture = "fist";
        }

        private void handShake()
        {
            GestureBlock.Text = "HAND SHAKE!!!";
            if (gesture.Equals("null") || gesture.Equals("fist"))
                awe1.Source = new Uri("http://lia.dc.ufscar.br/andre.obueno/ThingBrokerDemo/shake.html");
            else
                awe1.ExecuteJavascript("window.location.reload(true);");
            gesture = "shake";
        }

        private float jointDistance(Joint first, Joint second)
        {
            float dX = first.Position.X - second.Position.X;
            float dY = first.Position.Y - second.Position.Y;
            float dZ = first.Position.Z - second.Position.Z;
            return (float)Math.Sqrt((dX * dX) + (dY * dY) + (dZ * dZ));
        }

        private void DrawTrackedSkeletonJoints(Skeleton s)
        {
            // Spine
            addLine(s.Joints[JointType.Head], s.Joints[JointType.ShoulderCenter]);
            addLine(s.Joints[JointType.ShoulderCenter], s.Joints[JointType.Spine]);

            // Left leg
            addLine(s.Joints[JointType.Spine], s.Joints[JointType.HipCenter]);
            addLine(s.Joints[JointType.HipCenter], s.Joints[JointType.HipLeft]);
            addLine(s.Joints[JointType.HipLeft], s.Joints[JointType.KneeLeft]);
            addLine(s.Joints[JointType.KneeLeft], s.Joints[JointType.AnkleLeft]);
            addLine(s.Joints[JointType.AnkleLeft], s.Joints[JointType.FootLeft]);

            // Right leg
            addLine(s.Joints[JointType.HipCenter], s.Joints[JointType.HipRight]);
            addLine(s.Joints[JointType.HipRight], s.Joints[JointType.KneeRight]);
            addLine(s.Joints[JointType.KneeRight], s.Joints[JointType.AnkleRight]);
            addLine(s.Joints[JointType.AnkleRight], s.Joints[JointType.FootRight]);

            // Left arm
            addLine(s.Joints[JointType.ShoulderCenter], s.Joints[JointType.ShoulderLeft]);
            addLine(s.Joints[JointType.ShoulderLeft], s.Joints[JointType.ElbowLeft]);
            addLine(s.Joints[JointType.ElbowLeft], s.Joints[JointType.WristLeft]);
            addLine(s.Joints[JointType.WristLeft], s.Joints[JointType.HandLeft]);

            // Right arm
            addLine(s.Joints[JointType.ShoulderCenter], s.Joints[JointType.ShoulderRight]);
            addLine(s.Joints[JointType.ShoulderRight], s.Joints[JointType.ElbowRight]);
            addLine(s.Joints[JointType.ElbowRight], s.Joints[JointType.WristRight]);
            addLine(s.Joints[JointType.WristRight], s.Joints[JointType.HandRight]);
        }
   
    }
}
