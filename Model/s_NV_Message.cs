using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer
{
    public class s_NV_Message
    {
        public uint uiType;        //message type
        public uint uiLength;      //message length
        public ushort uiVersion;       //version
        public ushort uiSequeue;	//message number

    }
}
