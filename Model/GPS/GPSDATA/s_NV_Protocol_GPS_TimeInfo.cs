using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Model.GPS.GPSDATA
{
    public class s_NV_Protocol_GPS_TimeInfo
    {
        public uint usYear;          //year
        public uint ucMonth;          //month
        public uint ucDay;            //day(1~31)
        public uint ucHour;           //hour(0~23)
        public uint ucMinute;         //minute(0~59)
        public uint ucSecond;         //second(0~59)
        public uint ucReserved;		//reserve

    }
}
