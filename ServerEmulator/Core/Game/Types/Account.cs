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
    enum Rights : byte
    {
        PLAYER = 0, MODERATOR = 1, ADMIN = 2
    }

    enum Gender : sbyte
    {
        NOT_SET = -1, MALE = 0, FEMALE = 1
    }

    [Serializable]
    class Account //todo: friends with custom name aliases, displayname history, blocked list, muted list,
    {
        //static XmlSerializer serializer = new XmlSerializer(typeof(Account));
        //static SHA256 hashing = SHA256.Create();
        public string email, username, password, displayname, lastIp;
        DateTime registerDate, lastLogin, membership, mutedUntil, bannedUntil;
        public int gameTime, timesLoggedIn; //time spent online in seconds
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
        
        public int[] friends = new int[200];
        public int[] ignores = new int[100];

        public Account(string username, string displayname, string password, Rights rights = Rights.PLAYER)
        {
            this.username = username;
            this.displayname = displayname;
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
            a.gender = Gender.FEMALE;

            response = LoginResponse.LOGIN_OK;
            return a;
        }


        public static void Save(Account a) //todo: maybe save into an sqlite database or look into b-trees
        {
            FileStream fs = new FileStream(ACCOUNT_PATH + a.username + ".xml", FileMode.Create);

            //serializer.Serialize(fs, a);
        }

        public bool IsMuted => DateTime.Now < mutedUntil;
        public bool IsBanned => DateTime.Now < bannedUntil;
        public bool IsMembers => DateTime.Now < membership;
    }
}
