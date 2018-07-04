using GPSCommunicationServer.Model.GPS.GPSDATA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer
{
   public class GPSData
    {
        public s_NV_Message header;
        public s_NV_Protocol_Device_GPSHeader gpsHeader;
        public s_NV_Protocol_GPS_BaseInfo baseInfo;
        public s_NV_Protocol_Device_GPSStatus gpsStatus;
        public s_NV_Protocol_GPS_GSensor gSensor;
        public s_NV_Protocol_GPS_GyroSensor gyroSensor;
        public s_NV_Protocol_GPS_ExternInfoEx externInfoEx;
        public s_NV_Protocol_GPS_TimeInfo timeInfo;
    }
}
