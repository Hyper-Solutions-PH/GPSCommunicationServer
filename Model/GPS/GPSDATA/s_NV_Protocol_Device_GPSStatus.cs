using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Model.GPS.GPSDATA
{
    public class s_NV_Protocol_Device_GPSStatus
    {
        //Bit0: ACC signal (0:low,1:high)
        //Bit1: brake  signal (0:non,1:active)
        //Bit2:turn left (0:non,1:active)
        //Bit3:turn right (0:non,1:active)
        //Bit4:GPS exist(0:non,1:exist)
        //Bit5~Bit7:reserve
        public string ucValid;			//valid

    }
}
