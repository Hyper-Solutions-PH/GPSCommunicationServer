using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Model.GPS.PassengerCounterData
{
    public class s_NV_PeopleCount
    {
        public ushort ucfIn;     // Number of passengers on board
        public ushort ucfOut;  //  The number of passengers getting off
        public ushort ucbIn;// The total number of passengers on board
        public ushort ucbOut; //The total number of passengers getting off 
        public ushort ucnCunt;//Current total number of passengers on //board
        public string ucReserved;      // reserve
        public int ucnum;        //  identifier—-- The server needs to return this number //to the device by response，if this number is 1, it is void data ,do not need response
    }
}
