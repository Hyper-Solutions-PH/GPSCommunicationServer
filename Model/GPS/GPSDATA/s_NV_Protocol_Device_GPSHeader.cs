using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer
{
    public class s_NV_Protocol_Device_GPSHeader
    {
       public string szTimeStamp;             //time stamp
                                              //Bit0: GPS base data (0:void,  1:valid)
                                              //Bit1: status data(0:void,  1:valid)
                                              //Bit2: G-Sensor data(0:void,  1:valid)
                                              //Bit3: Gyro-Sensor data(0:void,  1:valid)
                                              //Bit4: GPS extended data(0:void ,  1: valid)
                                              //Bit5:time(0:void,  1:valid)
                                              //Bit6~Bit7: Reserve

        public char usDataType;		          //data type

    }
}
