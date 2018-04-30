
//--------------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace FaceTracking
{
    public partial class MainWindow : Window
    {
        private PXCMSession session;
        private PXCMSenseManager senseManager;
        private PXCMFaceData faceData;
        private Thread update;
        private string alertMsg;
        private const int FaceAlignment = 0;
        public Int32 CountFrame = 0;//记录当前帧数（1到30帧保持RGB与depth和txt一致）
        public Int32 RandomNumber = -1;//记录随机数的产生（保证在从1-30帧期间数字不变化）
        //public Int32 StartMmtime = 0;//开始记录的毫秒数
        string username = null;
        DirectoryInfo di = null;
        int countQAQ = 0;
        //string TIME = DateTime.Now.ToString("ddhhmmss");
        string TIME = 0.ToString();
        int confidence = 0;//总置信度，用来判断当前记录的数字是否有效

        
        public MainWindow()
        {
            InitializeComponent();

            //启动SenseManager并配置面部模块
            ConfigureRealSense();

            //启动Update线程
            update = new Thread(new ThreadStart(Update));
            update.Start();

        }

        private void ConfigureRealSense()
        {
            PXCMFaceModule faceModule;
            PXCMFaceConfiguration faceConfig;

            // 启动SenseManager和会话
            session = PXCMSession.CreateInstance();
            senseManager = session.CreateSenseManager();

            // 启用 color stream 和depth stream
           

            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 0, 0, 0);
            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 0, 0, 0);

            // 启用 face module
            senseManager.EnableFace();
            faceModule = senseManager.QueryFace();
            faceConfig = faceModule.CreateActiveConfiguration();
            //配置landmarks便于摘录点的数据
            faceConfig.landmarks.isEnabled = true;
            // 配置 for 3D face tracking
            faceConfig.SetTrackingMode(PXCMFaceConfiguration.TrackingModeType.FACE_MODE_COLOR_PLUS_DEPTH);

            // 必须将Pose isEnabled设置为false才能使R200的脸部跟踪正常工作
            faceConfig.pose.isEnabled = false;

            // 根据场景中的外观来跟踪脸部
            faceConfig.strategy = PXCMFaceConfiguration.TrackingStrategyType.STRATEGY_APPEARANCE_TIME;


            // 启用警报监视并订阅警报事件处理程序
            faceConfig.EnableAllAlerts();
            faceConfig.SubscribeAlert(FaceAlertHandler);

            // 应用更改并初始化
            //将任何参数的改变反馈给faceModule 
            faceConfig.ApplyChanges();
            senseManager.Init();
            //创建人脸数据的实例 
            faceData = faceModule.CreateOutput();

            // 释放资源
            faceConfig.Dispose();
            faceModule.Dispose();
        }

        private void FaceAlertHandler(PXCMFaceData.AlertData alert)
        {
            alertMsg = Convert.ToString(alert.label);
        }

        private void Update()
        {

            Int32 facesDetected = 0;
            Int32 faceH = 0;
            Int32 faceW = 0;
            Int32 faceX = 0;
            Int32 faceY = 0;

            

            // 启动 AcquireFrame-ReleaseFrame 循环
            while (senseManager.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {

                // 获取彩色图像数据(depth数据目前已经仿照写在了后边)
                PXCMCapture.Sample sample = senseManager.QuerySample();

                Bitmap colorBitmap;
                PXCMImage.ImageData colorData;
                sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_ANY, out colorData);
                //colorBitmap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);
                colorBitmap = colorData.ToBitmap(0, 480, 360);

                PXCMImage.ImageData depthData;
                sample.depth.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, out depthData);
                //var depthBitmap = depthData.ToUShortArray(0, sample.depth.info.width * sample.depth.info.height );  //480*360 


                //////////////
                //如何处理深度数据。它以16位灰度级图像的形式提供，PictureBox控件无法显示该图像。
                //我们将其转换为假彩色位图。 尽管它有点欺骗，但仅仅使用支持的Format16bppRgb555格式。
                //将处理一个16位值，以便每个R，G和B使用5位，并忽略最高位。
                //要执行此转换，不能简单地分配给PixelFormat属性 - 它是只读的。
                //相反，我们必须创建一个正确大小的新Bitmap对象和PixelFormat，并将原始数据移入其中。 

                //首先，我们需要深度数据作为标准的16位整型数组：
                Int16[] buffer = depthData.ToShortArray(0, sample.depth.info.width * sample.depth.info.height);
                //接下来，我们需要一个合适的大小和格式的位图：
                Bitmap bm2 = new Bitmap(sample.depth.info.width, sample.depth.info.height,System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                //最后，我们需要将数据缓冲区锁定在内存中，并使用Marshal.copy方法将数据传输到缓冲区中：
                System.Drawing.Imaging.BitmapData bmapdata = bm2.LockBits(new Rectangle(0, 0,sample.depth.info.width,sample.depth.info.height), System.Drawing.Imaging.ImageLockMode.WriteOnly,bm2.PixelFormat);
                IntPtr ptr = bmapdata.Scan0;
                Marshal.Copy(buffer, 0, ptr,sample.depth.info.width * sample.depth.info.height);
                bm2.UnlockBits(bmapdata);
                /////////////////////////


                // 获取脸部数据
                if (faceData != null)
                {
                    faceData.Update();
                    facesDetected = faceData.QueryNumberOfDetectedFaces();

                    if (facesDetected > 0 && RandomNumber >= 0)
                    {

                        //预判帧数并处理
                        if (CountFrame == 0)
                        {
                            di = new DirectoryInfo("C:\\RealsenseData\\"+ RandomNumber + "\\N" + RandomNumber + "_P" + username +"_T" + TIME);//number_person_time
                            di.Create();
                        }
                        CountFrame++;

                        //预判结束，进入图像处理及保存
                        if (di == null)
                        {
                            continue;
                        }
                        //此处写rgb和depth图像（保证是在统一循环中）
                        string prefix = di.ToString() + "\\" + "F" + CountFrame.ToString();
                        colorBitmap.Save(prefix + "_color.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        bm2.Save(prefix + "_depth.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                        ////深度图的.dat数据 可选 
                        using (FileStream fsWrite = new FileStream(prefix + "_depth.dat", FileMode.Create))
                        {
                            using (StreamWriter writer = new StreamWriter(fsWrite))
                            {
                                foreach (var v in buffer)
                                {
                                    writer.WriteLine(v);
                                }
                            }
                        };
                        ////

                        //获取第一个脸部检测（索引0）
                        PXCMFaceData.Face face = faceData.QueryFaceByIndex(0);

                        // 检索脸部位置数据
                        PXCMFaceData.DetectionData faceDetectionData = face.QueryDetection();
                        if (faceDetectionData != null)
                        {
                            PXCMRectI32 faceRectangle;

                            faceDetectionData.QueryBoundingRect(out faceRectangle);
                            faceH = faceRectangle.h;
                            faceW = faceRectangle.w;
                            faceX = faceRectangle.x;
                            faceY = faceRectangle.y;
                        }

                        //打开文件，准备读写当前帧的数据
                        FileStream fs = new FileStream(prefix + ".txt", FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine("坐标点     名称               3D坐标置信度     X        Y        Z        2D坐标置信度     X        Y ");
                        //打点
                       
                        PXCMFaceData.LandmarksData landmarks = face.QueryLandmarks();
                        if (landmarks != null)
                        {
                            using (Graphics graphics = Graphics.FromImage(colorBitmap))
                            using (var brush = new SolidBrush(Color.White))
                            using (var lowConfidenceBrush = new SolidBrush(Color.Red))
                            using (var font = new Font("黑体", 5))
                            {
                                PXCMFaceData.LandmarkPoint[] points;
                                //只单独获取唇部坐标点信息（33-44-45-52）
                                bool res = landmarks.QueryPointsByGroup(PXCMFaceData.LandmarksGroupType.LANDMARK_GROUP_MOUTH, out points);
                                //获取全部的坐标点信息
                                //bool res = landmarks.QueryPoints( out points);
                                var point = new PointF();
                                string num = null;
                                //在图像中打出来
                                int tempConfidence = 0;
                                foreach (PXCMFaceData.LandmarkPoint landmark in points)
                                {
                                    point.X = landmark.image.x + FaceAlignment;
                                    point.Y = landmark.image.y + FaceAlignment;


                                    if (landmark.confidenceImage == 0)
                                        graphics.DrawString("x", font, lowConfidenceBrush, point);
                                    else
                                        graphics.DrawString("•", font, brush, point);

                                    tempConfidence += landmark.confidenceWorld;//计算该点的可信度
                                    //每一个点的数据都写入文件其中
                                    num = landmark.source.index.ToString() + "    " + landmark.source.alias.ToString() + "     " + landmark.confidenceWorld + "     " + landmark.world.x + "  " + landmark.world.y + "  " + landmark.world.z + "     " + "     " + landmark.confidenceImage + "     " + landmark.image.x + "  " + landmark.image.y;

                                    sw.WriteLine(num);
                                }
                                //我知道只有二十个点直接除了算该帧的置信度,并赋值给总数字

                                tempConfidence /= 20;
                                confidence += tempConfidence;
                                //清空缓冲区        
                            }
                        }
                        //打点结束

                        sw.Flush();
                        //关闭此文件
                        sw.Close();
                        fs.Close();
                    }
                }

                // 更新 UI
                Render(colorBitmap, facesDetected, faceH, faceW, faceX, faceY, CountFrame, RandomNumber);

                // 释放color流
                colorBitmap.Dispose();
                sample.color.ReleaseAccess(colorData);
                sample.depth.ReleaseAccess(depthData);
                senseManager.ReleaseFrame();
            }


        }

        private void Render(Bitmap bitmap, Int32 count, Int32 h, Int32 w, Int32 x, Int32 y, Int32 CountFrame, Int32 RandomNumber)
        {
            BitmapImage bitmapImage = ConvertBitmap(bitmap);

            if (bitmapImage != null)
            {
                // 更新UI控件
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    // 更新bitmap图像
                    imgStream.Source = bitmapImage;

                    // 更新数据（标签内容）

                    lblFacesDetected.Content = string.Format("检测人脸数目: {0}", count);
                    lblFaceH.Content = string.Format("脸部高度: {0}", h);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                    lblFaceW.Content = string.Format("脸部宽度: {0}", w);
                    lblFaceX.Content = string.Format("Face X 坐标: {0}", x);
                    lblFaceY.Content = string.Format("Face Y 坐标: {0}", y);
                    countQ.Content = string.Format("已成功录入: {0}", countQAQ);
                    lblFaceNum.Content = string.Format(RandomNumber >= 0 ? "当前数字:\n {0}" : "无数字", RandomNumber);
                    lblFaceFrame.Content = string.Format("当前写入帧: {0}", CountFrame);

                    //// 显示或者掩藏脸部标记
                    if (count > 0)
                    {
                    //    // 显示脸部标记
                        lblFaceAlert.Content = "已找到人脸 请点击按钮 开始录入数据！";

                    }
                    else
                    {
                    //    // 隐藏脸部标记
                        lblFaceAlert.Content = "尚未发现人脸，请对齐！";
                    }
                }));
            }
        }

        //转换格式,显示在xaml界面上
        private BitmapImage ConvertBitmap(Bitmap bitmap)
        {
            BitmapImage bitmapImage = null;

            if (bitmap != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ShutDown();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            ShutDown();
            this.Close();
        }

        private void ShutDown()
        {
            // 停止Update线程
            update.Abort();

            // 处理RealSense对象
            faceData.Dispose();
            senseManager.Dispose();
            session.Dispose();
        }

        //界面留空了四个按钮，函数内容可以下一步进行填充
        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            if (RandomNumber == -1)
            { 
                //int temp = (new Random()).Next(10);
                int temp = countQAQ % 10;
                MessageBox.Show("点击确定后开始录入下一个数字: " + temp);
                RandomNumber = temp;
                //TIME = DateTime.Now.ToString("ddhhmmss");//保证文件及文件夹命名规范性l
                TIME = (countQAQ / 10 + 1).ToString();
            }
        }

        private void btnTwo_Click(object sender, RoutedEventArgs e)
        { 
            if(RandomNumber != -1)
            {
                double N=0;
                RandomNumber = -1;
                if (CountFrame != 0)
                {
                     N = confidence / CountFrame;
                }
                confidence = 0;
                CountFrame = 0;
                if (N > 70)//置信水平，暂定0.8该数字有效
                {
                    MessageBox.Show("已经暂停。该数字可信 录制成功。 ");
                    countQAQ++;
                }
                else
                {
                    MessageBox.Show("已经暂停。该数字不可信 已经作废 ");
                    if (di != null) { di.Delete(true); }
                    
                }

            }

        }

        private void btnThree_Click(object sender, RoutedEventArgs e)
        {
            RandomNumber = -1;
            CountFrame = 0;
            username = textBox.Text;
            MessageBox.Show("当前用户："+username);
        }
    

        private void btnFour_Click(object sender, RoutedEventArgs e)
        {
            
        }

    }
    
}
