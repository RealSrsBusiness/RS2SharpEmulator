using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using static ServerEmulator.Core.Constants;

namespace ServerEmulator.Core.Game
{
    struct ItemStack
    {
        public int id, amount;
    }

    struct Skill
    {
        public int level, xp;
    }

    enum Rights : byte
    {
        PLAYER = 0, MODERATOR = 1, ADMIN = 2
    }

    enum Gender : sbyte
    {
        NOT_SET = -1, MALE = 0, FEMALE = 1
    }

    struct Friend //todo: categories
    {
        public int userId;
        public string alias;
        public bool blocked;
    }

    [Serializable]
    class Account
    {
        //static XmlSerializer serializer = new XmlSerializer(typeof(Account));
        //static SHA256 hashing = SHA256.Create();

        string email, username, password, displayname, lastIp;
        DateTime registerDate, lastLogin, membership, mutedUntil, bannedUntil;
        public int gameTime; //time spent online in seconds
        public bool flagged; //flagged for cheating
        public Rights rights;
        public Gender gender = Gender.NOT_SET;

        public int x = SPAWN_X, y = SPAWN_Y, z = 0;
        public int energy = 100; //special attack?
        public int[] equipment = new int[11];
        public int[] playerLook = new int[6];

        public Skill[] skills = new Skill[21]; //contains prayer, hp etc.
        public ItemStack[] bank = new ItemStack[300];
        public ItemStack[] inventory = new ItemStack[28];
        
        public Friend[] friends = new Friend[250];
        public string[] recentNames = new string[10];

        public Account(string username, string displayname, string password, Rights rights = Rights.PLAYER)
        {
            this.username = username;
            this.password = password;
            this.registerDate = DateTime.Now;
            this.lastLogin = DateTime.MinValue;
            this.rights = rights;
            
            for (int i = 0; i < skills.Length; i++)
            {
                Skill s = new Skill() { level = 50, xp = 105000 };
                skills[i] = s;
            }
        }

        public static Account Load(string username, string password, out sbyte response)
        {
            Account a = new Account(username, "Player", password, Rights.ADMIN);
            a.gender = Gender.MALE;

            response = LoginResponse.LOGIN_OK;
            return a;
        }


        public static void Save(Account a)
        {
            FileStream fs = new FileStream(ACCOUNT_PATH + a.username + ".xml", FileMode.Create);
     
            //serializer.Serialize(fs, a);
        }

        public bool IsMuted { get { return false; } }
        public bool IsBanned { get { return false; } }
        public bool IsMembers { get { return true; } }
    }
}
