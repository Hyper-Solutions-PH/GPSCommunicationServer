using GPSCommunicationServer.Model;
using GPSCommunicationServer.Model.GPS.GPSDATA;
using GPSCommunicationServer.Model.GPS.PassengerCounterData;
using GPSCommunicationServer.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GPSCommunicationServer
{
    class CommunicationThread
    {
        NetworkStream stream;
        TcpClient client;
        int ucnum = 1;//Passanger counter response variable INT
        byte[] num = new byte[4];// Passanger counter response variable BYTE
        gpsEntities db;

        //I keep in constant the hex of the types of possible messages that can reach me to have a greater legibility in the code
        const String registerType = "20001001";
        const String gpsType = "20001012";
        const String counterType = "20005001";
        String messageType = "";

        public CommunicationThread(NetworkStream stream, TcpClient client)
        {
            this.stream = stream;
            this.client = client;
        }

        public void ThreadReceiveGPS()
        {

            db = new gpsEntities();
            String DeviceID = "";
            try
            {
                //Remote ip and remote port
                String remoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                String remotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();

                // Buffer for reading data
                Byte[] bytes = new Byte[250];
                string hex = "";
                String invertedHex = "";


                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {

                    /*
                     IMPORTANT: the data sent by the GPS can be ordered in two formats Little-Endian or Big-Endian 
                     (If you have not heard these two terms look for information about Endianness), providers should
                     tell us in what format they were written the most common is the Big-Endian but for this case I play a
                     GPS that ordered them in Little-Endian and I had to invert the data so that it has some meaning.
                     
                    If your provider has no idea of how this information is sent or for one reason or another you can not communicate 
                    with your provider, you can make trial and error test. It will not take you long just to have the hex in two ways as 
                    you can see below and use the form that gives you meaningful data.

                    The way you know that data is correct, is seeing the protocol document that comes with the device or that your provider provides you.

                    There are many types of protocols and it is important to make sure that you have the right one so as not to waste your time and think 
                    that you are the one programming the server in an erroneous way.
                     */

                    // Console.WriteLine("Info: " + message);
                    hex = BitConverter.ToString(bytes, 0, i).Replace("-", " ");
                    invertedHex = Strings.Invert(hex, ' ');

                    //If you want to see the hex before and after being inverted uncomment the two lines below 

                    //Console.WriteLine("Received HEX: [ {1}:{2} ] {0} ", hex, remoteAddress, remotePort);
                    //Console.WriteLine("Received HEX: [ {1}:{2} ] {0} ", invertedHex, remoteAddress, remotePort);
                    
                    //
                    messageType = Strings.Right(invertedHex, 8);
                    /*
                    These are nested if they verify the type of message that the GPS sends to execute the appropriate code for each one (the types of messages are in the gps protocol)

                    The process that is carried out with each one of these messages is the following:

                        1.The hex that is sent via TCP / IP is captured.
                        2.It is verified in the GPS protocol in which order the variables come and what size each one occupies in bytes.
                        3.A place is created where each of these data is stored, the variable is matched to the corresponding bytes and these are eliminated from the main 
                        hex and the corresponding data type is parsed.
                        4.The data that interests us is saved in the database.

                     */
                    if (messageType == registerType)
                    {
                        Console.WriteLine("Received HEX: [ {1}:{2} ] {0} ", invertedHex, remoteAddress, remotePort);
                        uint uiType = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        uint uiLength = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        ushort uiVersion = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        ushort uiSequeue = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);


                        var header = new s_NV_Message
                        {
                            uiLength = uiLength,
                            uiType = uiType,
                            uiVersion = uiVersion,
                            uiSequeue = uiSequeue
                        };


                        String szID = Strings.FromHexLittleEndianToString(invertedHex, 24);
                        invertedHex = Strings.Shorten(invertedHex, 24);
                        DeviceID = szID;
                        int nNetAdpaterType = int.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        String szModel = Strings.FromHexLittleEndianToString(invertedHex, 48);
                        invertedHex = Strings.Shorten(invertedHex, 48);
                        int nType = int.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        String szSoftware = Strings.FromHexLittleEndianToString(invertedHex, 16);
                        invertedHex = Strings.Shorten(invertedHex, 16);
                        String szHardware = Strings.FromHexLittleEndianToString(invertedHex, 16);
                        invertedHex = Strings.Shorten(invertedHex, 16);

                        var s_NV_Protocol_Device_SignalRegister = new s_NV_Protocol_Device_SignalRegister
                        {
                            header = header,
                            szID = szID,
                            nNetAdpaterType = nNetAdpaterType,
                            szModel = szModel,
                            nType = nType,
                            szSoftware = szSoftware,
                            szHardware = szHardware
                        };



                    }
                    else if (messageType == gpsType)
                    {
                        Console.WriteLine("Received HEX from Device [{3}]: [ {1}:{2} ] {0} ", invertedHex, remoteAddress, remotePort, DeviceID);
                        uint uiType = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        uint uiLength = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        ushort uiVersion = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        ushort uiSequeue = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);


                        var header = new s_NV_Message
                        {
                            uiLength = uiLength,
                            uiType = uiType,
                            uiVersion = uiVersion,
                            uiSequeue = uiSequeue
                        };

                        string szTimeStamp = Strings.FromHexLittleEndianToString(invertedHex, 16);
                        invertedHex = Strings.Shorten(invertedHex, 16);
                        char usDataType = (char)Int16.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);

                        var gpsHeader = new s_NV_Protocol_Device_GPSHeader
                        {
                            szTimeStamp = szTimeStamp,
                            usDataType = usDataType
                        };

                        ushort usDirect = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        ushort usSpeed = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        decimal ucLongitude_degree = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLongitude_Score = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        ucLongitude_Score /= 60;
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLatitude_degree = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLatitude_Score = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        ucLatitude_Score /= 60;
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal unLongitude_Second = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        unLongitude_Second /= 3600;
                        unLongitude_Second /= 10000000;
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        decimal unLatitude_Second = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        unLatitude_Second /= 3600;
                        unLatitude_Second /= 10000000;
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        string ucStatus = Strings.FromHexToBinary(Strings.Right(invertedHex, 2));
                        invertedHex = Strings.Shorten(invertedHex, 2);

                        string north = "1";
                        string west = "1";

                        decimal latitude = ucLatitude_degree + ucLatitude_Score + unLatitude_Second;
                        decimal longitude = ucLongitude_degree + ucLongitude_Score + unLongitude_Second;

                        if (ucStatus.Substring(1, 1) != north)
                        {
                            latitude *= -1;
                        }


                        if (ucStatus.Substring(2, 1) == west)
                        {
                            longitude *= -1;
                        }


                        var gpsBaseinfo = new s_NV_Protocol_GPS_BaseInfo
                        {
                            usDirect = usDirect,
                            usSpeed = usSpeed,
                            ucLongitude_degree = ucLongitude_degree,
                            ucLongitude_Score = ucLongitude_Score,
                            ucLatitude_degree = ucLatitude_degree,
                            ucLatitude_Score = ucLatitude_Score,
                            unLongitude_Second = unLongitude_Second,
                            unLatitude_Second = unLatitude_Second,
                            latitude_decimal_degree = latitude,
                            longitude_decimal_degree = longitude,
                            ucStatus = ucStatus
                        };

                        string ucValid = Strings.FromHexToBinary(Strings.Right(invertedHex, 2));
                        invertedHex = Strings.Shorten(invertedHex, 2);

                        var gpsStatus = new s_NV_Protocol_Device_GPSStatus
                        {
                            ucValid = ucValid
                        };

                        short sAccelate_X = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        short sAccelate_Y = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        short sAccelate_Z = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        short sAccelate_Unit = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);

                        var gSensor = new s_NV_Protocol_GPS_GSensor
                        {
                            sAccelate_X = sAccelate_X,
                            sAccelate_Y = sAccelate_Y,
                            sAccelate_Z = sAccelate_Z,
                            sAccelate_Unit = sAccelate_Unit
                        };

                        short sCorner = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        short sUnit = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);

                        var gyroSensor = new s_NV_Protocol_GPS_GyroSensor
                        {
                            sCorner = sCorner,
                            sUnit = sUnit
                        };

                        short sHigh = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        short sTemperature = short.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        decimal sOilWear = uint.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier); // Oil consumption //(Actual data received *10)
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        string cSatelliteNumber = Strings.FromHexLittleEndianToString(invertedHex, 2);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        // string ucReserved = Strings.FromHexLittleEndianToString(invertedHex, 2);
                        invertedHex = Strings.Shorten(invertedHex, 2);

                        var gpsExternInfo = new s_NV_Protocol_GPS_ExternInfoEx
                        {
                            sHigh = sHigh,
                            sTemperature = sTemperature,
                            sOilWear = (sOilWear * 10),// Oil consumption //(Actual data received *10)
                            cSatelliteNumber = cSatelliteNumber,
                            // ucReserved = ucReserved
                        };

                        devices devices = new devices
                        {
                            attributes = "{Tank:{" + gpsExternInfo.sOilWear + "}}"
                        };

                      

                        uint usYear = uint.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);          //year
                        uint ucMonth = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);           //month
                        uint ucDay = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);             //day(1~31)
                        uint ucHour = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);           //hour(0~23)
                        uint ucMinute = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);         //minute(0~59)
                        uint ucSecond = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);          //second(0~59)
                                                                                // uint uReserved= uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);     //reserve

                        var gpsTimeInfo = new s_NV_Protocol_GPS_TimeInfo
                        {
                            usYear = usYear,
                            ucMonth = ucMonth,
                            ucDay = ucDay,
                            ucHour = ucHour,
                            ucMinute = ucMinute,
                            ucSecond = ucSecond,
                            // ucReserved=uReserved
                        };

                        var gpsData = new GPSData
                        {
                            gpsHeader = gpsHeader,
                            gpsStatus = gpsStatus,
                            gSensor = gSensor,
                            gyroSensor = gyroSensor,
                            baseInfo = gpsBaseinfo,
                            externInfoEx = gpsExternInfo,
                            timeInfo = gpsTimeInfo,
                            header = header
                        };

                        var devices_id = (from dev in db.devices
                                          where dev.uniqueid == DeviceID
                                          select dev.id).First();


                        positions positions = new positions
                        {
                            deviceid = devices_id,
                            servertime = DateTime.Now,
                            devicetime = DateTime.ParseExact(gpsTimeInfo.ucDay.ToString() + "-" + gpsTimeInfo.ucMonth.ToString() + "-" + gpsTimeInfo.usYear.ToString()
                                               + " " + gpsTimeInfo.ucHour.ToString() + ":" + gpsTimeInfo.ucMinute.ToString() + ":" + gpsTimeInfo.ucSecond.ToString(),
                                                "d-M-yyyy H:m:s",
                                       System.Globalization.CultureInfo.InvariantCulture),
                            fixtime = DateTime.Now,
                            latitude = decimal.ToDouble(gpsBaseinfo.latitude_decimal_degree),
                            longitude = decimal.ToDouble(gpsBaseinfo.longitude_decimal_degree),
                            speed = gpsBaseinfo.usSpeed,
                            protocol = "sodimax-mdvr",
                            altitude = 0,
                            valid = true,
                            course = 0,
                            accuracy = 0,
                            address = "",
                            network = ""

                        };

                        db.positions.Add(positions);
                        db.SaveChanges();




                    }
                    else if (messageType == counterType)
                    {
                        Console.WriteLine("Received HEX from Device [{3}]: [ {1}:{2} ] {0} ", invertedHex, remoteAddress, remotePort, DeviceID);
                        uint uiType = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        uint uiLength = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        ushort uiVersion = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        ushort uiSequeue = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);


                        var header = new s_NV_Message
                        {
                            uiLength = uiLength,
                            uiType = uiType,
                            uiVersion = uiVersion,
                            uiSequeue = uiSequeue
                        };


                        uint usYear = uint.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);          //year
                        uint ucMonth = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);           //month
                        uint ucDay = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);             //day(1~31)
                        uint ucHour = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);           //hour(0~23)
                        uint ucMinute = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);         //minute(0~59)
                        uint ucSecond = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);          //second(0~59)
                                                                                // uint uReserved= uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                                                                                //invertedHex = Strings.Shorten(invertedHex, 2);     //reserve

                        var gpsTimeInfo = new s_NV_Protocol_GPS_TimeInfo
                        {
                            usYear = usYear,
                            ucMonth = ucMonth,
                            ucDay = ucDay,
                            ucHour = ucHour,
                            ucMinute = ucMinute,
                            ucSecond = ucSecond,
                            // ucReserved=uReserved
                        };

                        DateTime counterDate = DateTime.ParseExact(gpsTimeInfo.ucDay.ToString() + "-" + gpsTimeInfo.ucMonth.ToString() + "-" + gpsTimeInfo.usYear.ToString()
                                                + " " + gpsTimeInfo.ucHour.ToString() + ":" + gpsTimeInfo.ucMinute.ToString() + ":" + gpsTimeInfo.ucSecond.ToString(),
                                                 "d-M-yyyy H:m:s",
                                        System.Globalization.CultureInfo.InvariantCulture);

                        string mesType = Strings.FromHexToBinary(Strings.Right(invertedHex, 8));
                        invertedHex = Strings.Shorten(invertedHex, 8);

                        var MesType = new MesType()
                        {
                            mesType = mesType
                        };

                        ushort usDirect = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);
                        ushort usSpeed = ushort.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLongitude_degree = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLongitude_Score = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        ucLongitude_Score /= 60;
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLatitude_degree = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal ucLatitude_Score = uint.Parse(Strings.Right(invertedHex, 2), NumberStyles.AllowHexSpecifier);
                        ucLatitude_Score /= 60;
                        invertedHex = Strings.Shorten(invertedHex, 2);
                        decimal unLongitude_Second = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        unLongitude_Second /= 3600;
                        unLongitude_Second /= 10000000;
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        decimal unLatitude_Second = uint.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        unLatitude_Second /= 3600;
                        unLatitude_Second /= 10000000;
                        invertedHex = Strings.Shorten(invertedHex, 8);
                        string ucStatus = Strings.FromHexToBinary(Strings.Right(invertedHex, 2));
                        invertedHex = Strings.Shorten(invertedHex, 2);

                        string north = "1";
                        string west = "1";

                        decimal latitude = ucLatitude_degree + ucLatitude_Score + unLatitude_Second;
                        decimal longitude = ucLongitude_degree + ucLongitude_Score + unLongitude_Second;

                        if (ucStatus.Substring(1, 1) != north)
                        {
                            latitude *= -1;
                        }


                        if (ucStatus.Substring(2, 1) == west)
                        {
                            longitude *= -1;
                        }

                        //invertedHex = Strings.Shorten(invertedHex, 6); //3 bytes de reserva

                        var gpsBaseinfo = new s_NV_Protocol_GPS_BaseInfo
                        {
                            usDirect = usDirect,
                            usSpeed = usSpeed,
                            ucLongitude_degree = ucLongitude_degree,
                            ucLongitude_Score = ucLongitude_Score,
                            ucLatitude_degree = ucLatitude_degree,
                            ucLatitude_Score = ucLatitude_Score,
                            unLongitude_Second = unLongitude_Second,
                            unLatitude_Second = unLatitude_Second,
                            latitude_decimal_degree = latitude,
                            longitude_decimal_degree = longitude,
                            ucStatus = ucStatus
                        };

                        ushort ucfIn = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);     // Number of passengers on board
                        ushort ucfOut = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);  //  The number of passengers getting off
                        ushort ucbIn = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);// The total number of passengers on board
                        ushort ucbOut = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4); //The total number of passengers getting off 
                        ushort ucnCunt = ushort.Parse(Strings.Right(invertedHex, 4), NumberStyles.AllowHexSpecifier);
                        invertedHex = Strings.Shorten(invertedHex, 4);//Current total number of passengers on //board
                                                                      // string ucReserved = Strings.FromHexToBinary(Strings.Right(invertedHex, 4));
                                                                      // invertedHex = Strings.Shorten(invertedHex, 4);      // reserve
                        ucnum = int.Parse(Strings.Right(invertedHex, 8), NumberStyles.AllowHexSpecifier);
                        num = Strings.FromHex(Strings.Right(invertedHex, 8));//Parsing hex to byte to response to the device

                        invertedHex = Strings.Shorten(invertedHex, 8);        //  identifier—-- The server needs to return this number //to the device by response，if this number is 1, it is void data ,do not need response


                        var peopleCount = new s_NV_PeopleCount()
                        {
                            ucbIn = ucfOut, // How many people entered Up-in down-out
                            ucbOut = ucbOut,
                            ucfIn = (ushort)(ucfOut - ucbIn), // How many people are there
                            ucfOut = ucbIn, // How many people went up-in down-out
                            ucnCunt = ucnCunt,
                            ucnum = ucnum, // Number to send
                                           // ucReserved = ucReserved
                        };

                        var devices_id = (from dev in db.devices
                                          where dev.uniqueid == DeviceID
                                          select dev.id).First();

                        var route_id = (from dev in db.devices
                                        where dev.uniqueid == DeviceID
                                        select dev.routeId).First();



                        int lastPassengerIn = (from counter in db.passenger_counters
                                               orderby counter.servertime descending
                                               select counter.totalPassengerIn).First();

                        int lastPassengerOut = (from counter in db.passenger_counters
                                                orderby counter.servertime descending
                                                select counter.totalPassengerOut).First();

                        int invalidData = 1;

                        if (ucnum != invalidData && (peopleCount.ucbIn != lastPassengerIn || peopleCount.ucfOut != lastPassengerOut))
                        {


                            passenger_counters passenger_Counters = new passenger_counters
                            {
                                deviceid = devices_id,
                                servertime = DateTime.Now,
                                devicetime = counterDate,
                                totalPassengerIn = peopleCount.ucbIn,
                                totalPassengerOut = peopleCount.ucfOut,
                                currentPassengerIn = peopleCount.ucfIn,
                                routeid = (int)route_id
                                //currentPassengerIn= ucfIn
                            };



                            db.passenger_counters.Add(passenger_Counters);
                            db.SaveChanges();
                        }
                    }



                    stream.Flush();
                    //  Thread.Sleep(10000);
                }




            }
            catch (SocketException ex)
            {
                //If an error occur the client connection will close 
                Console.WriteLine("Error!\n" + ex.Message + "\nClosing connection...");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!\n" + ex.Message + "\n" + ex.StackTrace);
                client.Close();
            }

        }

        public void ThreadSendGPS()
        {
            byte[] heartbeat = { 0x01, 0x10, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00 };
            byte[] gpsRegister = { 0x01, 0x20, 0x00, 0x10, 0x0D, 0x00, 0x00, 0x00, 0x01, 0x00, 0x03, 0x00, 0xFF };
            byte[] responseCounter = { 0x20, 0x00, 0x50, 0x02, 0x0D, 0x00, 0x00, 0x00, 0x01, 0x00, 0x03, 0x00, num[0], num[1], num[2], num[3] };
            //Set the network heartbeat interval, GPS data upload time interval
            byte[] hearbeatinterval = { 0x10, 0x00, 0x60, 0x01, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x01, 0x00, 0x01, 0x00, 0x08, 0x00, 0x00, 0x0a, 0x19, 0x0f, 0x01, 0x05, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };

            //while the client still connected will send a message every 10 seconds
            do
            {
                if (stream.CanWrite)
                {

                    try
                    {
                        //Remote ip and remote port
                        String remoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        String remotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                        //local ip and local port
                        String localAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                        String localPort = ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString();

                        if (ucnum != 1)
                        {
                            stream.Write(responseCounter, 0, responseCounter.Length);
                            //ucnum = 1;
                        }

                        stream.Write(gpsRegister, 0, gpsRegister.Length);
                        //  Console.WriteLine("Info: [  {1}:{2} ] {0} ", "Has been registered.", remoteAddress, remotePort);

                        //sending heartbeat to the client
                        stream.Write(heartbeat, 0, heartbeat.Length);
                        //  Console.WriteLine("A {0} was sent from  [{4}:{3}] to [{1}:{2}]", "hearbeat", remoteAddress, remotePort, localPort, localAddress);


                    }
                    catch (SocketException ex)
                    {
                        //If an error occur the client connection will close 
                        Console.WriteLine("Error!\n" + ex.Message + "\nClosing connection...");
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        //If an error occur the client connection will close 
                        Console.WriteLine("Error!\n" + ex.Message + "\nClosing connection...");
                        client.Close();
                    }



                    //waiting 10000 milliseconds (10 seconds) to send the message again
                    Thread.Sleep(10000);

                }
            } while (client.Connected);








        }





    }
}
