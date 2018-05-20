# LipReadingRecognition

Project Description：
The project bases on Intel Realsense 3D camera, detecting and extracting the three-dimensional lip movement characteristics accurately, using long-term and short-term memory networks to achieve dynamic recognition of lip language, so that the system can recognize the user's lip content and dynamic characteristics to achieve silent Double password authentication.
It can be widely used for various types of authentication.

Due to the small number of public datasets in this field, we used the WPF programming of C# to call some of the realsense SDK's interfaces to record the 3D lip data feature set.Our team has undergone preliminary collation and now publishes the code of recording system  and this 3D lip-reading feature datasets.

The sample size of this data set was 46 people. A total of 0-9 10 numbers were recorded and stored in 10 compression packages named 0-9. The data naming rule in each package is number + sample number + recording number,For example, "N0_P01_T1" indicates the first recording of the first sample for the number 0. The following is the structure and content information of this data set:

Data Structure:
struct LandmarkPoint {

    LandmarkPointSource source;       //The LandmarkPointSource structure describes the landmark point name.

    pxcI32              confidenceImage;   //The confidence score from 0 to 100 of the image coordinates.

    pxcI32              confidenceWorld;   //The confidence score from 0 to 100 of the world coordinates.

    PXCPoint3DF32       world;             //The color image coordinates of the landmark point.

    PXCPointF32         image;             //The world coordinates of the landmark point, with units in meters.

};	

class LandmarkPointSource {

    Int32        index;        //The index that this landmark belongs.

    LandmarkType alias;        //The LandmarkType enumerator enumerates supported face landmark points.

};

The LandmarkType enumerator enumerates supported face landmark points.
Content Information:
{
LANDMARK_NOT_NAMED			            Unspecified.

LANDMARK_EYE_RIGHT_CENTER		        The center of the right eye.

LANDMARK_EYE_LEFT_CENTER		        The center of the left eye.

LANDMARK_EYELID_RIGHT_TOP		        The right eye lid points.

LANDMARK_EYELID_RIGHT_BOTTOM

LANDMARK_EYELID_RIGHT_RIGHT

LANDMARK_EYELID_RIGHT_LEFT

LANDMARK_EYELID_LEFT_TOP		        The left eye lid points.

LANDMARK_EYELID_LEFT_BOTTOM

LANDMARK_EYELID_LEFT_RIGHT

LANDMARK_EYELID_LEFT_LEFT

LANDMARK_EYEBROW_RIGHT_CENTER		    The right eye brow points.

LANDMARK_EYEBROW_RIGHT_RIGHT

LANDMARK_EYEBROW_RIGHT_LEFT		

LANDMARK_EYEBROW_LEFT_CENTER		    The left eye brow points.

LANDMARK_EYEBROW_LEFT_RIGHT

LANDMARK_EYEBROW_LEFT_LEFT

LANDMARK_NOSE_TIP			The nose points.

LANDMARK_NOSE_TOP

LANDMARK_NOSE_BOTTOM               （The nose top point is the top 					 
LANDMARK_NOSE_RIGHT			           most point of the nose vertically.
LANDMARK_NOSE_LEFT			           The nose tip point is the top most point of the nose in the Z dimension.）

LANDMARK_LIP_RIGHT			           The middle lip points.

LANDMARK_LIP_LEFT

LANDMARK_UPPER_LIP_CENTER		       The upper lip points.

LANDMARK_UPPER_LIP_RIGHT

LANDMARK_UPPER_LIP_LEFT

LANDMARK_LOWER_LIP_CENTER		       The lower lip points

LANDMARK_LOWER_LIP_RIGHT

LADNMARK_LOWER_LIP_LEFT

LADNMARK_FACE_BORDER_TOP_RIGHT		 The face border points

LADNMARK_FACE_BORDER_TOP_LEFT

LADNAMRK_CHIN				               The bottom chin point.
};	

struct PXCMPoint3DF32 {

    Single x;     //The x coordinate of the point.
    
    Single y;     //The y coordinate of the point.
    
    Single z;     //The z coordinate of the point.
    
};

struct PXCMPointF32 {

    Single x;     //The x coordinate of the image pixel.

    Single y;     //The y coordinate of the image pixel.

};

You can also see more detailed data structures on this website.
https://software.intel.com/sites/landingpage/realsense/camera-sdk/v1.1/documentation/html/index.html?landmarkpoint_pxcfacedata.html

This data set was collected for the study of lip reading and was freely available for download to other researchers to facilitate the progress of related research. After downloading, the lip contour can be extracted through the coordinate information in the txt file.
In the era of data-driven technology, we look forward to larger-scale data in the first place. After the data size increases or the generated model is optimized to automatically expand the data set, the problem of lip language recognition will be better resolved. In addition, lip language recognition not only has a wide range of applications in lip reading, but can also be widely used in password input, auxiliary speech recognition, and video subtitle generation. 

After the technology matures in the future, lip language recognition will focus more on real life and serve more market-oriented and specific application scenarios. In the future research, we will focus on solutions for non-living attacks. We will use methods based on depth maps and infrared maps to detect if the target is a living body. At present, in the recognition of counterattacks using depth cameras and infrared cameras, the recognition rate of expert systems is relatively low (50% or less). We will use a multi-feature depth model to solve this problem. We believe there is still a lot of research space in this area.

This project is supported by the National Natural Science Foundation of China, the Natural Science Foundation of Jiangsu Province and the "Cloud Number Fusion Science, Education and Innovation" Fund Project of the People's Republic of China ministry of education science and technology development center; Six major peak talents projects in Jiangsu Province; Key Laboratory of Symbol Computing and Knowledge Engineering of the Ministry of Education of Jilin University; Provincial Innovation Project for Undergraduate Students “Lip language cryptosystem under 3D realsense camera” , Key Laboratory of Computer Information Processing Technology, Suzhou University.

Commercial application please contact 
author 1: Zhang Lumin
13776285260
1530805045@stu.suda.edu.cn

Author 2: Wang Damu
18862110140
1527403001@stu.suda.edu.cn
