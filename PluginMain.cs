using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Hooks;


//this is a comment to see if git is working

namespace TimeCurrency
{
    [APIVersion(1, 12)]
    public class TimeCurrency : TerrariaPlugin
    {
        public static TimeCurrencyConfig ConfigFile { get; set; }
        internal static string TimeCurrencyConfigFilePath { get { return Path.Combine(TShock.SavePath, "time.json"); } }

        DateTime LastCheck = DateTime.UtcNow;
        DateTime LastCheck2 = DateTime.UtcNow;        
        int[] lasttileX = new int[256];
        int[] lasttileY = new int[256];
        long[] TimePlayed = new long[256];

        public override string Author
        {
            get { return "Loganizer"; }
        }
        public override string Description
        {
            get { return "Has a currency of time."; }
        }
        public override string Name
        {
            get { return "TimeCurrency"; }
        }
        public override Version Version
        {
            get { return new Version("1.0"); }
        }

        public TimeCurrency(Main game)
            : base(game)
        {
            Order = 10;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerHooks.Join -= OnJoin;
                ServerHooks.Leave -= OnLeave;
                GameHooks.Update -= OnUpdate; 
            }
        }
        public override void Initialize ()
        {
            Commands.ChatCommands.Add(new Command("time.default", CheckTime, "checktime"));
            Commands.ChatCommands.Add(new Command("time.default", AddTime, "givetime"));
            Commands.ChatCommands.Add(new Command("time.subtracttime", SubtractTime, "subtracttime"));
            Commands.ChatCommands.Add(new Command("time.default", CMDTime, "time"));

            ServerHooks.Join += OnJoin;
            ServerHooks.Leave += OnLeave;
            GameHooks.Update += OnUpdate;
            
            SetupConfig();

            #region Group Crap

            SqlManager.ChangeDeadPrefix(ConfigFile.DeadGroupPrefix);
            
            if(!TShock.Groups.GroupExists("dead"))
            {
                TShock.Groups.AddGroup("dead", "default", "", "255,255,255", false);
            }                    
            foreach (Group group in TShock.Groups)
            {
                if (group.Name != "superadmin")
                {                   
                    TShock.Groups.AddPermissions(group.Name, (new List<string>() { "time.checktime", "" }));         
                }
            }

            #endregion
         
        }
        
        void OnUpdate()
        {
            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)//every second
            {
                LastCheck = DateTime.UtcNow;
                foreach(TSPlayer player in TShock.Players)
                {
                    if (lasttileX[player.Index] != 0 && lasttileY[player.Index] != 0)
                    {
                        if (player.TileX == lasttileX[player.Index] && player.TileY == lasttileY[player.Index])//if player is afk
                        {
                            TimePlayed[player.Index] = TimePlayed[player.Index] + 1;
                        }                                              
                            lasttileX[player.Index] = player.TileX;
                            lasttileY[player.Index] = player.TileY;                      
                   }
                }
            }
            if ((DateTime.UtcNow - LastCheck2).TotalSeconds >= 120)//every 2 minutes
            {
                LastCheck2 = DateTime.UtcNow;
                foreach (TSPlayer player in TShock.Players)
                {

                    if (TimePlayed[player.Index] > 1)
                    {
                        SqlManager.AddTimePlayed(player.Name, TimePlayed[player.Index]);
                    }
                }
            }
            
        }
        
        private void OnJoin(int who, HandledEventArgs e)//this should be onlogin but I don't have the dev buid
        {
            //check if the person is dead
            if (SqlManager.CheckDeadStatus(TShock.Players[who].Name))
            {
                SqlManager.ChangeGroupToDead(TShock.Players[who].Name);
            }
        }
        private void OnLeave(int who)
        {
            if(TimePlayed[who] > 0)
            {
                SqlManager.AddTimePlayed(TShock.Players[who].Name, TimePlayed[who]);   
            }
        }

        bool GetTime(string str, out int time)//thanks marioE for this method
        {
            int seconds;
            if (int.TryParse(str, out seconds))
            {
                time = seconds;
                return true;
            }

            StringBuilder timeConv = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsDigit(str[i]) || (str[i] == '-' || str[i] == '+'))
                {
                    timeConv.Append(str[i]);
                }
                else
                {
                    int num;
                    if (!int.TryParse(timeConv.ToString(), out num))
                    {
                        time = 0;
                        return false;
                    }
                    timeConv.Clear();
                    switch (str[i])
                    {
                        case 's':
                            seconds += num;
                            break;
                        case 'm':
                            seconds += num * 60;
                            break;
                        case 'h':
                            seconds += num * 60 * 60;
                            break;
                        case 'd':
                            seconds += num * 60 * 60 * 24;
                            break;
                        default:
                            time = 0;
                            return false;
                    }
                }
            }
            time = seconds;
            return true;
        }

        #region Commands

        void CMDTime(CommandArgs args)
        {
            switch(args.Parameters[0])
            {
                case "give":
                    if (args.Player.IsLoggedIn)
                    {
                        if (SqlManager.CheckDeadStatus(args.Player.Name))
                        {
                            args.Player.SendErrorMessage("You cannot transfer time because you are dead.");
                        }
                        if (!SqlManager.CheckDeadStatus(args.Player.Name))
                        {
                            if (!args.Player.Group.HasPermission("time.addtime"))
                            {
                                if (args.Parameters.Count != 2)
                                {
                                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /time give <player> <time>");
                                    args.Player.SendErrorMessage("Syntax for time: 0d0h0m0s");
                                    return;
                                }
                                int time;

                                var player = TShock.Utils.FindPlayer(args.Parameters[0]);
                                if (player.Count == 0)
                                {
                                    args.Player.SendMessage("No players matched!!", Color.OrangeRed);
                                }
                                else if (player.Count > 1)
                                {
                                    args.Player.SendMessage("More than one player matched!", Color.OrangeRed);
                                }
                                else if (player.Count == 1)
                                {
                                    if (GetTime(args.Parameters[1], out time) && time > 0)
                                    {
                                        //time is the anount of seconds

                                        if (SqlManager.RemoveSeconds(args.Player.Name, time) && SqlManager.AddSeconds(player[0].Name, time))
                                        {
                                            args.Player.SendSuccessMessage("Successfully transfered " + args.Parameters[1] + " to " + player[0].Name + " 's account.");
                                            int time2 = SqlManager.ReadTime(args.Player.Name);
                                            if (!SqlManager.CheckDeadStatus(args.Player.Name))
                                            {
                                                TimeSpan t = TimeSpan.FromSeconds(time2);
                                                args.Player.SendMessage("Your currenct balance: " + t.Days + " days, " + t.Hours + " hours, " + t.Minutes + " minutes, and " + t.Seconds + " seconds.", Color.LightGreen);
                                            }
                                        }
                                        else
                                        {
                                            args.Player.SendErrorMessage("Failed to transfer " + args.Parameters[1] + " to " + player[0].Name + " 's account.");
                                        }
                                    }
                                    else
                                    {
                                        args.Player.SendErrorMessage("Invalid time. Syntax: 0d0h0m0s");
                                    }
                                }
                            }

                        }
                        
                    }
                        
                    
                        break;
                default:
                    args.Player.SendErrorMessage("Invalid argument! Proper arguments: ");
                    break;
              
            }
        }
        void AddTime(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /subtracttime <player> <time>");
                args.Player.SendErrorMessage("Syntax for time: 0d0h0m0s");
                return;
            }
            int time;

            var player = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendMessage("No players matched!!", Color.OrangeRed);
            }
            else if (player.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.OrangeRed);
            }
            else if (player.Count == 1)
            {
                if (args.Player.Group.HasPermission("time.add"))
                {
                    if (GetTime(args.Parameters[1], out time) && time > 0)
                    {
                        //time is the anount of seconds
                        SqlManager.AddSeconds(player[0].Name, time);
                        args.Player.SendSuccessMessage("Successfully transfered " + args.Parameters[1] + " to " + player[0].Name + " 's account.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Invalid time. Syntax: 0d0h0m0s");
                    }
                }
                else 
                {
                    if (GetTime(args.Parameters[1], out time) && time > 0)
                    {
                        //time is the anount of seconds
                        if (SqlManager.RemoveSeconds(args.Player.Name, time) && SqlManager.AddSeconds(player[0].Name, time))
                        {
                            args.Player.SendSuccessMessage("Successfully transfered " + args.Parameters[1] + " to " + player[0].Name + " 's account.");
                             int time2 = SqlManager.ReadTime(args.Player.Name);
                             if (!SqlManager.CheckDeadStatus(args.Player.Name))
                             {
                                 TimeSpan t = TimeSpan.FromSeconds(time2);
                                 args.Player.SendMessage("Your currenct balance: " + t.Days + " days, " + t.Hours + " hours, " + t.Minutes + " minutes, and " + t.Seconds + " seconds.", Color.LightGreen);
                             }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Failed to transfer " + args.Parameters[1] + " to " + player[0].Name + " 's account.");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Invalid time. Syntax: 0d0h0m0s");
                    }
                }

            }
        }
        void SubtractTime(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /givetime <player> <time>");
                args.Player.SendErrorMessage("Syntax for time: 0d0h0m0s");
                return;
            }
            int time;

            var player = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendMessage("No players matched!!", Color.OrangeRed);
            }
            else if (player.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.OrangeRed);
            }
            else if (player.Count == 1)
            {

                if (GetTime(args.Parameters[1], out time) && time > 0)
                {
                    //time is the anount of seconds
                    SqlManager.AddSeconds(player[0].Name, time);
                }
                else
                {
                    args.Player.SendErrorMessage("Invalid time.");
                    args.Player.SendErrorMessage("Syntax: 0d0h0m0s");
                }
            }
        }
        private void CheckTime(CommandArgs args)
        {
            int time = SqlManager.ReadTime(args.Player.Name);
            if (!SqlManager.CheckDeadStatus(args.Player.Name))
            {
                TimeSpan t = TimeSpan.FromSeconds(time);
                args.Player.SendMessage("You have " + t.Days + " days, " + t.Hours + " hours, " + t.Minutes + " minutes, and " + t.Seconds + " seconds left.", Color.LightGreen);
            }
            if (SqlManager.CheckDeadStatus(args.Player.Name))
                args.Player.SendMessage("You are dead. Type /time dead help   for info on how to be revived.", Color.Red);
        }
        
        #endregion





        public static bool SetupConfig()
        {
            try
            {
                if (File.Exists(TimeCurrencyConfigFilePath))
                {
                    ConfigFile = TimeCurrencyConfig.Read(TimeCurrencyConfigFilePath);
                }
                else
                {
                    ConfigFile.Write(TimeCurrencyConfigFilePath);
                }
                return true;
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in TimeCurrency config file.");
                Console.ResetColor();
                Log.Error("TimeCurrency Config Exception");
                Log.Error(ex.ToString());
                return false;
            }
        }
    }
}
