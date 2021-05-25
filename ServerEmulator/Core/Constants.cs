using System;
using System.Collections.Generic;
using System.Text;

namespace ServerEmulator.Core
{
    internal static class Constants
    {
        public const int PORT = 43594;
        public const int CYCLE_TIME = 600;
        public const int SUB_CYCLES = 2;
        public const int MAX_CONNECTIONS = 3072;
        public const int MAX_PLAYER = 2048;
        public const int MAX_PLAYERS_FOR_ADDRESS = 5;
        public const int SERVER_REV = 317;
        public const bool ENABLE_RSA_ENCRYPTION = false;

        public const string SERVER_NAME = "RS2SharpEmulator";
        public const string WELCOME_MSG = "Welcome to " + SERVER_NAME;
        public const int SPAWN_X = 3200, SPAWN_Y = 3200;
        public const byte MAP_LOADING_METHOD = 0; //0 = don't load, 1 = lazy load, 2 = preload complete map

        public const string DATA_PATH = "data\\";
        public const string PLUGIN_PATH = DATA_PATH + "plugins\\";
        public const string CONTENT_PATH = DATA_PATH + "content\\";
        public const string ACCOUNT_PATH = DATA_PATH + "accounts\\";
        public const string RSA_KEY_PAIR = DATA_PATH + "rsa.xml";

        public static char[] VALID_CHARS = {
            '_', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2',
            '3', '4', '5', '6', '7', '8', '9', '!', '@', '#', '$', '%', '^', '&', '*',
            '(', ')', '-', '+', '=', ':', ';', '.', '>', '<', ',', '"', '[', ']', '|',
            '?', '/', '`'
        };

        public static int[] OUTGOING_SIZES = {
            0, 0, 0, 0, 6, 0, 0, 0, 4, 0, // 0
		    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 10
		    0, 0, 0, 0, 1, 0, 0, 0, 0, 0, // 20
		    0, 0, 0, 0, -2, 4, 3, 0, 0, 0, // 30
		    0, 0, 0, 0, 5, 0, 0, 6, 0, 0, // 40
		    9, 0, 0, -2, 0, 0, 0, 0, 0, 0, // 50
		    -2, 1, 0, 0, 2, -2, 0, 0, 0, 0, // 60
		    6, 3, 2, 4, 2, 4, 0, 0, 0, 4, // 70
		    0, -2, 0, 0, 7, 2, 0, 6, 0, 0, // 80
		    0, 0, 0, 0, 0, 0, 0, 2, 0, 1, // 90
		    0, 2, 0, 0, -1, 4, 1, 0, 0, 0, // 100
		    1, 0, 0, 0, 2, 0, 0, 15, 0, 0, // 110
		    0, 4, 4, 0, 0, 0, -2, 0, 0, 0, // 120
		    0, 0, 0, 0, 6, 0, 0, 0, 0, 0, // 130
		    0, 0, 2, 0, 0, 0, 0, 14, 0, 0, // 140
		    0, 4, 0, 0, 0, 0, 3, 0, 0, 0, // 150
		    4, 0, 0, 0, 2, 0, 6, 0, 0, 0, // 160
		    0, 3, 0, 0, 5, 0, 10, 6, 0, 0, // 170 
		    0, 0, 0, 0, 0, 2, 0, 0, 0, 0, // 180
		    0, 0, 0, 0, 0, 0, -1, 0, 0, 0, // 190
		    4, 0, 0, 0, 0, 0, 3, 0, 2, 0, // 200
		    0, 0, 0, 0, -2, 7, 0, 0, 2, 0, // 210
		    0, 1, 0, 0, 0, 0, 0, 0, 0, 0, // 220
		    8, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 230
		    2, -2, 0, 0, 0, 0, 6, 0, 4, 3, // 240
		    0, 0, 0, -1, 6, 0, 0 // 250
	    };

        public static int[] INCOMING_SIZES = {
            0, 0, 0, 1, -1, 0, 0, 0, 0, 0, //0
		    0, 0, 0, 0, 8, 0, 6, 2, 2, 0,  //10
		    0, 2, 0, 6, 0, 12, 0, 0, 0, 0, //20
		    0, 0, 0, 0, 0, 8, 4, 0, 0, 2,  //30
		    2, 6, 0, 6, 0, -1, 0, 0, 0, 0, //40
		    0, 0, 0, 12, 0, 0, 0, 0, 8, 0, //50
		    0, 8, 0, 0, 0, 0, 0, 0, 0, 0,  //60
		    6, 0, 2, 2, 8, 6, 0, -1, 0, 6, //70
		    0, 0, 0, 0, 0, 1, 4, 6, 0, 0,  //80
		    0, 0, 0, 0, 0, 3, 0, 0, -1, 0, //90
		    0, 13, 0, -1, 0, 0, 0, 0, 0, 0,//100
		    0, 0, 0, 0, 0, 0, 0, 6, 0, 0,  //110
		    1, 0, 6, 0, 0, 0, -1, 0, 2, 6, //120
		    0, 4, 6, 8, 0, 6, 0, 0, 0, 2,  //130
		    0, 0, 0, 0, 0, 6, 0, 0, 0, 0,  //140
		    0, 0, 1, 2, 0, 2, 6, 0, 0, 0,  //150
		    0, 0, 0, 0, -1, -1, 0, 0, 0, 0,//160
		    0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  //170
		    0, 8, 0, 3, 0, 2, 0, 0, 8, 1,  //180
		    0, 0, 12, 0, 0, 0, 0, 0, 0, 0, //190
		    2, 0, 0, 0, 0, 0, 0, 0, 4, 0,  //200
		    4, 0, 0, 0, 7, 8, 0, 0, 10, 0, //210
		    0, 0, 0, 0, 0, 0, -1, 0, 6, 0, //220
		    1, 0, 0, 0, 6, 0, 6, 8, 1, 0,  //230
		    0, 4, 0, 0, 0, 0, -1, 0, -1, 4,//240
		    0, 0, 6, 6, 0, 0, 0            //250
	    };

        public static class LoginResponse
        {
            public const sbyte TRY_AGAIN = -1;
            public const sbyte LOGGING_IN = 0;
            public const sbyte WAIT_2_SECS = 1;
            public const sbyte LOGIN_OK = 2;
            public const sbyte INVALID_PASSWORD = 3;
            public const sbyte BANNED = 4;
            public const sbyte ALREADY_ONLINE = 5;
            public const sbyte HAS_UPDATED = 6;
            public const sbyte WORLD_FULL = 7;
            public const sbyte LOGIN_SERVER_OFFLINE = 8;
            public const sbyte LOGIN_LIMIT_EXCEEDED = 9;
            public const sbyte BAD_SESSION_ID = 10;
            public const sbyte SESSION_REJECTED = 11;
            public const sbyte MEMBERS_REQUIRED = 12;
            public const sbyte TRY_AGAIN_USE_DIF_WORLD = 13;
            public const sbyte BEING_UPDATED = 14;
            public const sbyte NOT_LOGGED_OUT_YET = 15;
            public const sbyte TOO_MANY_INCORRECT_LOGINS = 16;
            public const sbyte STANDING_IN_MEMBERS_AREA = 17;
            public const sbyte INVALID_LOGIN_SERVER = 20;
            public const sbyte JUST_LEFT_ANOTHER_WORLD = 21;
        }

        public static class Frames //server -> client
        {
            public const byte CONFIG_SET = 36;
            public const byte MSG_SEND = 253;
            public const byte SYSTEM_UPDATE_SECS = 114;
            public const byte DISCONNECT = 109;

            public const byte PLAYER_UPDATE = 81;
            public const byte PLAYER_STATUS = 249;
            public const byte PLAYER_SKILL = 134;
            public const byte PLAYER_LOCATION = 85;
            public const byte PLAYER_RUN_ENERGY = 110;
            public const byte PLAYER_WEIGHT = 240;
            public const byte PLAYER_RIGHTCLICK = 104;
            public const byte PLAYER_INTO_OBJ = 147; //most important frame eva?

            public const byte CHAT_SETTINGS = 206;
            public const byte CHAT_PRIVATE = 196;

            public const byte ITEM_ALL_CLEAR = 72; //clear inventory
            public const byte ITEM_SET = 53;
            public const byte ITEM_SLOT_SET = 34;
            
            public const byte ANIM_ALL_RESET = 1;
            public const byte ANIM_SET = 4;
            public const byte MULTICOMBAT = 61;

            public const byte NPC_UPDATE = 65;
            public const byte NPC_ICON = 254;

            public const byte OBJ_ADD = 151;
            public const byte OBJ_REMOVE = 101;
            public const byte OBJ_ANIMATE = 160;

            public const byte PROJECTILE = 117;

            public const byte FLOORITEM_ADD = 44;
            public const byte FLOORITEM_REMOVE = 156;
            public const byte FLOORITEM_REMOVE_SPAWNED = 64;
            public const byte FLOORITEM_UNKNOWN = 84;
            public const byte FLOORITEM_UNKNOWN2 = 215;

            public const byte REGION_UPDATE = 60;
            public const byte REGION_LOAD = 73;
            public const byte REGION_CONSTRUCT = 241;

            public const byte SONG_PLAY = 74;
            public const byte SONG_QUEUE = 121;
            public const byte SOUND_PLAY = 174;
            public const byte SOUND_PLAYAT = 105;

            public const byte CAMERA_SHAKE = 35;
            public const byte CAMERA_SPIN = 166;
            public const byte CAMERA_RESET = 107;
            public const byte CAMERA_TURN = 177;

            public const byte SIDEBAR_FLASH = 24;
            public const byte SIDEBAR_SET = 106;
            public const byte SIDEBAR_INTF_ASSIGN = 71;

            //interfaces
            public const byte INTF_WELCOME = 176;
            public const byte INTF_SHOW = 97;
            public const byte INTF_CLEAR = 219;
            public const byte INTF_TEXT_ADD = 126;
            public const byte INTF_COLOR_SET = 122;
            public const byte INTF_MODEL_SET = 254;
            public const byte INTF_MODEL_ANIM = 200;
            public const byte INTF_MODEL_ROTATE = 230;
            public const byte INTF_MODEL_ZOOM = 246;
            public const byte INTF_CHAT_ADD = 164;
            public const byte INTF_HIDDEN = 171; //hidden component state
            public const byte INTF_WALKABLE = 208;
            public const byte INTF_INV_HUD = 248;
            public const byte INTF_INV_REPLACE = 142;
            public const byte INTF_OFFSET = 70; //hide and show segment?, used for special bar in PI
            public const byte INTF_SCROLL_SET = 79;
            public const byte INTF_ENTER_NAME = 187;
            public const byte INTF_ENTER_AMT = 27;

            public const byte BTN_ALL_RESET = 68;
            public const byte BTN_SET = 87;

            public const byte FRIENDLIST_STATUS = 221;
            public const byte FRIEND_ADD = 50;
            public const byte IGNORE_ADD = 214;

            public const byte MINIMAP_CLR_FLAG = 78;
            public const byte MINIMAP_STATE = 99;
        }        

        public static class Packets //Client -> Server
        {
            //items
            public const byte ITEM_EQUIP = 41;
            public const byte ITEM_UNEQUIP = 145;

            public const byte ITEM_OPT_1 = 122;
            public const byte ITEM_OPT_2 = 16;
            public const byte ITEM_OPT_3 = 75;

            public const byte ITEM_MOVE = 214;
            public const byte ITEM_DROP = 87;

            public const byte ITEM_BANK_5 = 117;
            public const byte ITEM_BANK_10 = 43;
            public const byte ITEM_BANK_X1 = 135;
            public const byte ITEM_BANK_X2 = 208;
            public const byte ITEM_BANK_ALL = 129;

            public const byte FLOORITEM_OPT_1 = 253;
            public const byte FLOORITEM_LIGHT = 79;
            public const byte FLOORITEM_PICKUP = 236;

            //objects
            public const byte OBJ_OPT_1 = 132;
            public const byte OBJ_OPT_2 = 234; //252
            public const byte OBJ_OPT_3 = 70;
            public const byte OBJ_OPT_4 = 228;

            //entities
            public const byte NPC_ATT = 72;
            public const byte NPC_OPT_1 = 155;
            public const byte NPC_OPT_2 = 17;
            public const byte NPC_OPT_3 = 21;
            public const byte NPC_OPT_4 = 18;

            //player
            public const byte PLAYER_OPT_1 = 128;
            public const byte PLAYER_OPT_2 = 153;
            public const byte PLAYER_OPT_3 = 73;
            public const byte PLAYER_OPT_4 = 139;
            public const byte PLAYER_OPT_5 = 39;
            
            //hud
            public const byte CHAT_MSG_SEND = 4;
            public const byte CHAT_CMD_SEND = 103;
            public const byte CHAT_PRIVATE = 126;
            public const byte CHAT_OPTIONS = 95;

            public const byte INTF_CONTINUE = 40;
            public const byte INTF_TYPING = 60;
            public const byte INTF_CHANGE = 130;
            public const byte INTF_ACTION_BTN = 185;
            public const byte INTF_CHARACTER_DESIGN = 101;

            public const byte PLAYER_REPORT = 218;
            public const byte FRIEND_ADD = 188;
            public const byte FRIEND_DELETE = 215;

            public const byte IGNORE_REMOVE = 74;
            public const byte IGNORE_ADD = 133;

            //interactions
            public const byte ITEM_ON_ITEM = 53;
            public const byte ITEM_ON_OBJ = 192;
            public const byte ITEM_ON_NPC = 57;
            public const byte ITEM_ON_PLAYER = 14;
            public const byte ITEM_ON_FLOORITEM = 25;

            public const byte MAGIC_ON_ITEM = 237;
            public const byte MAGIC_ON_OBJ = 35;
            public const byte MAGIC_ON_NPC = 131;
            public const byte MAGIC_ON_PLAYER = 249;
            public const byte MAGIC_ON_FLOORITEM = 181;

            //misc
            public const byte IDLE = 0;
            public const byte IDLE_LOGOUT = 202;
            public const byte ACC_FLAGGED = 45;

            public const byte WINDOW_FOCUS = 3;
            public const byte MOUSE_CLICK = 241;
            public const byte CAMERA_MOVE = 86;
            public const byte CAMERA_VALIDATE = 77; //anti-cheat?

            public const byte ANTI_CHEAT2 = 85;
            public const byte ANTI_CHEAT3 = 136;
            public const byte ANTI_CHEAT4 = 152;
            public const byte ANTI_CHEAT5 = 183;
            public const byte ANTI_CHEAT6 = 200;
            public const byte ANTI_CHEAT7 = 230;
            public const byte ANTI_CHEAT8 = 246;

            public const byte WALK_VALIDATE = 36; //anti-cheat
            public const byte WALK = 164;
            public const byte WALK_MINIMAP = 248;
            public const byte WALK_ON_COMMAND = 98;

            public const byte MAPREGION_LOADED = 121;
            public const byte MAPREGION_ENTERED = 210;

            
        }
    }
}
