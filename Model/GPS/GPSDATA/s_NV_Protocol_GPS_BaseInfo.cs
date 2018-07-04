using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Model.GPS.GPSDATA
{
    public class s_NV_Protocol_GPS_BaseInfo
    {
        public ushort usDirect;            //direct
        public ushort usSpeed;         //speed(Km/H)
        public decimal ucLongitude_degree;   //longitude degree
        public decimal ucLongitude_Score;    //longitude score
        public decimal ucLatitude_degree;    //latitude degree
        public decimal ucLatitude_Score; //latitude score
        public decimal unLongitude_Second; //longitude second
        public decimal unLatitude_Second; //latitude second
        public decimal latitude_decimal_degree;//latitude decimal degree
        public decimal longitude_decimal_degree; //longitude decimal degree

        public string ucStatus;           //status
                                        //Bit0:GPS data(0:void,  1:valid)
                                        //Bit1: Longitude direction (0: East longitude,1: west longitude)
                                        //Bit2: Latitudinal direction (0: south latitude,1:north latitude)
                                        //Bit3~Bit7:reserve

    }
}
