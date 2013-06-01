using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Hooks;
using System.Reflection;
using System.Web;


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


            Commands.ChatCommands.Add(new Command("time.checktime", CheckTime, "checktime"));
            Commands.ChatCommands.Add(new Command("time.givetime", AddTime, "givetime"));
            Commands.ChatCommands.Add(new Command("time.subtracttime", SubtractTime, "subtracttime"));




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

                   //TimePlayed[player.Index] = SqlManager.GetLastSeen(player.Name);
                }
            }
            if ((DateTime.UtcNow - LastCheck2).TotalSeconds >= 120)//every 2 minutes
            {
                LastCheck2 = DateTime.UtcNow;
                foreach (TSPlayer player in TShock.Players)
                {

                    if (TimePlayed[player.Index] > 1)
                    {
                        SqlManager.AddSeconds(player.Name, TimePlayed[player.Index]);
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


                SqlManager.GetTimePlayed(TShock.Players[who].Name);
            }
        }
        private void OnLeave(int who)
        {
            if(TimePlayed[who] > 0)
            {
                SqlManager.AddSeconds(TShock.Players[who].Name, TimePlayed[who]);   
            }
        }
        #region Commands
        
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
        private void AddTime(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /givetime <player> <days>");
            }

            int days = 0;

            var player = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendMessage("No players matched!!", Color.OrangeRed);
            }
            else if (player.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.OrangeRed);
            }
            else
            {
                try
                {
                    days = Convert.ToInt32(args.Parameters[1]);

                    SqlManager.AddSeconds(args.Player.Name, (days * 86400));
                }
                catch
                {
                    args.Player.SendErrorMessage("Please enter a valid integer in days!");
                    args.Player.SendErrorMessage("Proper systax: /givetime <player> <days>");
                }
            }
           



        }
        private void SubtractTime(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /subtracttime <player> <days>");
            }

            int days = 0;

            var player = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendMessage("No players matched!!", Color.OrangeRed);
            }
            else if (player.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.OrangeRed);
            }
            else
            {
                try
                {
                    days = Convert.ToInt32(args.Parameters[1]);

                    SqlManager.RemoveSeconds(args.Player.Name, (days * 86400));
                }
                catch
                {
                    args.Player.SendErrorMessage("Please enter a valid integer in days!");
                    args.Player.SendErrorMessage("Proper systax: /subtracttime <player> <days>");
                }
            }
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
