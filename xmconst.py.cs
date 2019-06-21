namespace CamControl
{
    public static class XMConstants
    {
        public static class PTZ
        {
            public const string LEFT = "DirectionLeft";

            public const string RIGHT = "DirectionRight";
            public const string UP = "DirectionUp";

            public const string DOWN = "DirectionDown";
        }
        public static class User
        {
            public const int LOGIN_REQ1 = 999;
            public const int LOGIN_REQ2 = 1000;
            public const int LOGIN_RSP = 1000;
            public const int LOGOUT_REQ = 1001;
            public const int LOGOUT_RSP = 1002;
            public const int FORCELOGOUT_REQ = 1003;
            public const int FORCELOGOUT_RSP = 1004;
            public const int FULLAUTHORITYLIST_GET = 1470;
            public const int FULLAUTHORITYLIST_GET_RSP = 1471;
            public const int USERS_GET = 1472;
            public const int USERS_GET_RSP = 1473;
            public const int GROUPS_GET = 1474;
            public const int GROUPS_GET_RSP = 1475;
            public const int ADDGROUP_REQ = 1476;
            public const int ADDGROUP_RSP = 1477;
            public const int MODIFYGROUP_REQ = 1478;
            public const int MODIFYGROUP_RSP = 1479;
            public const int DELETEGROUP_REQ = 1480;
            public const int DELETEGROUP_RSP = 1481;
            public const int ADDUSER_REQ = 1482;
            public const int ADDUSER_RSP = 1483;
            public const int MODIFYUSER_REQ = 1484;
            public const int MODIFYUSER_RSP = 1485;
            public const int DELETEUSER_REQ = 1486;
            public const int DELETEUSER_RSP = 1487;
            public const int MODIFYPASSWORD_REQ = 1488;
            public const int MODIFYPASSWORD_RSP = 1489;

        }


        public const int KEEPALIVE_REQ = 1006;

        public const int KEEPALIVE_RSP = 1007;
        public static class System
        {
            public const int SYSINFO_REQ = 1020;
            public const int SYSINFO_RSP = 1021;
            public const int CONFIG_SET = 1040;
            public const int CONFIG_SET_RSP = 1041;
            public const int CONFIG_GET = 1042;
            public const int CONFIG_GET_RSP = 1043;
            public const int DEFAULT_CONFIG_GET_REQ = 1044;
            public const int DEFAULT_CONFIG_GET_RSP = 1045;
            public const int CONFIG_CHANNELTILE_SET = 1046;
            public const int CONFIG_CHANNELTILE_SET_RSP = 1047;
            public const int CONFIG_CHANNELTILE_GET = 1048;
            public const int CONFIG_CHANNELTILE_GET_RSP = 1049;
            public const int CONFIG_CHANNELTILE_DOT_SET = 1050;
            public const int CONFIG_CHANNELTILE_DOT_SET_RSP = 1051;
            public const int SYSTEM_DEBUG_REQ = 1052;
            public const int SYSTEM_DEBUG_RSP = 1053;
            public const int ABILITY_GET = 1360;
            public const int ABILITY_GET_RSP = 1361;
        }

        public static class PlayBack
        {
            public const int PTZ_REQ = 1400;

            public const int PTZ_RSP = 1401;

            public const int MONITOR_REQ = 1410;

            public const int MONITOR_RSP = 1411;

            public const int MONITOR_DATA = 1412;

            public const int MONITOR_CLAIM = 1413;

            public const int MONITOR_CLAIM_RSP = 1414;

            public const int PLAY_REQ = 1420;

            public const int PLAY_RSP = 1421;

            public const int PLAY_DATA = 1422;

            public const int PLAY_EOF = 1423;

            public const int PLAY_CLAIM = 1424;

            public const int PLAY_CLAIM_RSP = 1425;

            public const int DOWNLOAD_DATA = 1426;

            public const int TALK_REQ = 1430;

            public const int TALK_RSP = 1431;

            public const int TALK_CU_PU_DATA = 1432;

            public const int TALK_PU_CU_DATA = 1433;

            public const int TALK_CLAIM = 1434;

            public const int TALK_CLAIM_RSP = 1435;

            public const int FILESEARCH_REQ = 1440;

            public const int FILESEARCH_RSP = 1441;

            public const int LOGSEARCH_REQ = 1442;

            public const int LOGSEARCH_RSP = 1443;

            public const int FILESEARCH_BYTIME_REQ = 1444;

            public const int FILESEARCH_BYTIME_RSP = 1445;

        }



        public const int SYSMANAGER_REQ = 1450;

        public const int SYSMANAGER_RSP = 1451;

        public const int TIMEQUERY_REQ = 1452;

        public const int TIMEQUERY_RSP = 1453;

        public const int DISKMANAGER_REQ = 1460;

        public const int DISKMANAGER_RSP = 1461;

        public const int GUARD_REQ = 1500;

        public const int GUARD_RSP = 1501;

        public const int UNGUARD_REQ = 1502;

        public const int UNGUARD_RSP = 1503;

        public const int ALARM_REQ = 1504;

        public const int ALARM_RSP = 1505;

        public const int NET_ALARM_REQ = 1506;

        //public const int NET_ALARM_REQ = 1507;

        public const int ALARMCENTER_MSG_REQ = 1508;

        public const int UPGRADE_REQ = 1520;

        public const int UPGRADE_RSP = 1521;

        public const int UPGRADE_DATA = 1522;

        public const int UPGRADE_DATA_RSP = 1523;

        public const int UPGRADE_PROGRESS = 1524;

        public const int UPGRADE_INFO_REQ = 1525;

        public const int UPGRADE_INFO_RSQ = 1526;

        public const int IPSEARCH_REQ = 1530;

        public const int IPSEARCH_RSP = 1531;

        public const int IP_SET_REQ = 1532;

        public const int IP_SET_RSP = 1533;

        public const int CONFIG_IMPORT_REQ = 1540;

        public const int CONFIG_IMPORT_RSP = 1541;

        public const int CONFIG_EXPORT_REQ = 1542;

        public const int CONFIG_EXPORT_RSP = 1543;

        public const int LOG_EXPORT_REQ = 1544;

        public const int LOG_EXPORT_RSP = 1545;

        public const int NET_KEYBOARD_REQ = 1550;

        public const int NET_KEYBOARD_RSP = 1551;

        public const int NET_SNAP_REQ = 1560;

        public const int NET_SNAP_RSP = 1561;

        public const int SET_IFRAME_REQ = 1562;

        public const int SET_IFRAME_RSP = 1563;

        public const int RS232_READ_REQ = 1570;

        public const int RS232_READ_RSP = 1571;

        public const int RS232_WRITE_REQ = 1572;

        public const int RS232_WRITE_RSP = 1573;

        public const int RS485_READ_REQ = 1574;

        public const int RS485_READ_RSP = 1575;

        public const int RS485_WRITE_REQ = 1576;

        public const int RS485_WRITE_RSP = 1577;

        public const int TRANSPARENT_COMM_REQ = 1578;

        public const int TRANSPARENT_COMM_RSP = 1579;

        public const int RS485_TRANSPARENT_DATA_REQ = 1580;

        public const int RS485_TRANSPARENT_DATA_RSP = 1581;

        public const int RS232_TRANSPARENT_DATA_REQ = 1582;

        public const int RS232_TRANSPARENT_DATA_RSP = 1583;

        public const int SYNC_TIME_REQ = 1590;

        public const int SYNC_TIME_RSP = 1591;

        public const int PHOTO_GET_REQ = 1600;

        public const int PHOTO_GET_RSP = 1601;
    }

}