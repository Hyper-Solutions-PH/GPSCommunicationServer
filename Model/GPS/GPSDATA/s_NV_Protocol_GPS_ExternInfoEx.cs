using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Model.GPS.GPSDATA
{
   public class s_NV_Protocol_GPS_ExternInfoEx
    {
        public short sHigh;            // Altitude
        public short sTemperature;     //temperature
        public decimal sOilWear; // Oil consumption //(Actual data received *10)
        public string cSatelliteNumber;  //satellite number
        public string ucReserved;		//reserve

    }
}
