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

    enum Gender : sbyte //*triggered*
    {
        None = -1, Male = 0, Female = 1
    }

    class Account
    {
        //static XmlSerializer serializer = new XmlSerializer(typeof(Account));
        //static SHA256 hashing = SHA256.Create();

        string email, username, password, displayname, lastIp;
        DateTime registerDate, lastLogin, membership, mutedUntil, bannedUntil;
        public int gameTime; //time spent online in seconds
        public bool flagged;
        public Rights rights;
        public Gender gender;

        public int x = 3222, y = 3222, z = 0;
        public int energy = 100; //special attack?
        public int[] equipment = new int[14];
        public int[] playerLook = new int[6];

        public Skill[] skill = new Skill[21]; //contains prayer, hp etc.
        public ItemStack[] inventory = new ItemStack[28];
        public ItemStack[] bank = new ItemStack[300];
        public string[] friends = new string[200];
        public string[] ignores = new string[100];       


        private Account() { }

        public static void Create(string username, string password, Rights rights = Rights.PLAYER)
        {
            Account a = new Account()
            {
                username = username,
                password = password,
                registerDate = DateTime.Now,
                lastLogin = DateTime.MinValue,
                rights = Rights.PLAYER,
            };
        }

        public static sbyte Load(string username, string password, ref Account acc)
        {
            acc = new Account() { username = username, password = password, displayname = "Player", rights = Rights.PLAYER, gender = Gender.Male };

            return LoginResponse.LOGIN_OK;
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
