using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer
{
 
    public class s_NV_Protocol_Device_SignalRegister
    {
       
        public s_NV_Message header;
        public String szID;          //device ID
        public int nNetAdpaterType;    //Network adapter type(1:3G,2:WIFI,3:Wired)	
        public String szModel;       //Product number
        public int nType;          // Device type
        public String szSoftware;     //Software version
        public String szHardware;		//Software version

       

    }
}
