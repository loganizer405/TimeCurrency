using System;
using System.Collections.Generic;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Linq;

namespace TimeCurrency
{
    public class SqlManager
    {
       

        private static IDbConnection database;

        public SqlManager(IDbConnection db)
        {
            database = db;

            var table = new SqlTable("TimeCurrency",

                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("Time", MySqlDbType.Int64),
                new SqlColumn("TimePlayed", MySqlDbType.Int64),             
                new SqlColumn("Dead", MySqlDbType.Byte)
                
            );
            var creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            creator.EnsureExists(table);

        }
        
        public static bool AddSeconds(string name, long seconds)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET Time = Time + @0 WHERE Name = @1", seconds, name);
                return true;

            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
                
            }
        }
        public static bool RemoveSeconds(string name, int seconds)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET Time = Time - @0 WHERE Name = @1", seconds, name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;

            }
        }
        public static int ReadTime(string name)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT Time FROM TimeCurrency WHERE Name = @0", name))
                {
                    if (reader.Read())
                        return reader.Get<int>("Time");
                }
                
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                
            }
            return 0;
        }
        public static bool AddTimePlayed(string name, long seconds)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET TimePlayed = TimePlayed + @0 WHERE Name = @1", seconds, name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
            }
        }
        public static long GetTimePlayed(string name)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT TimePlayed FROM TimeCurrency WHERE Name = @0", name))
                {
                    if (reader.Read())
                    {
                        return reader.Get<long>("TimePlayed");
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return 0;
            }

        }
        public static bool KillPlayer(string name)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET Dead = 1 WHERE Name = @0", name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
            }
        }
        public static bool ChangeDeadPrefix(string prefix)
        {
            try
            {
                database.Query("UPDATE Users SET Prefix = @0 WHERE GroupName = dead", prefix);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
            }
        }
        public static bool ChangeGroupToDead(string name)
        {
            try
            {
                database.Query("UPDATE Users SET Usergroup = dead WHERE Username = @0 LIMIT 1", name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
            }
        }
       
        public static bool CheckDeadStatus(string name)
        {
            int dead = 0;
            try
            {
                using (var reader = database.QueryReader("SELECT Dead FROM TimeCurrency WHERE Name = @0", name))
                {
                    if(reader.Read())
                    {
                        dead = reader.Get<int>("Dead");
                    }
                    if (!reader.Read())
                    {
                    }
                    if (dead == 1)
                    {
                        return true;
                    }
                    if (dead == 0)
                    {
                        return false;
                    }
                    else //not possible....vs is complaining about not all code paths returning values
                    {
                        return false;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.Message);
                return false;
            }
        }
               
       /* public bool DeleteWarpplate(string name)
        {
            Warpplate r = GetWarpplateByName(name);
            if (r != null)
            {
                int q = database.Query("DELETE FROM AutoRank WHERE WarpplateName=@0 AND WorldID=@1", name, Main.worldID.ToString());
                Warpplates.Remove(r);
                if (q > 0)
                    return true;
            }
            return false;
        }*/
        /*
        public bool SetWarpplateState(string name, bool state)
        {
            var Warpplate = GetWarpplateByName(name);
            if (Warpplate != null)
            {
                try
                {
                    Warpplate.DisableBuild = state;
                    database.Query("UPDATE AutoRank SET Protected=@0 WHERE WarpplateName=@1 AND WorldID=@2", state ? 1 : 0, name, Main.worldID.ToString());

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        public Warpplate FindWarpplate(string name)
        {
            try
            {
                foreach (Warpplate wp in Warpplates)
                {
                    if (wp.Name == name)
                        return wp;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return new Warpplate();
        }

        public bool InArea(int x, int y)
        {
            foreach (Warpplate Warpplate in Warpplates)
            {
                if (x >= Warpplate.Area.Left && x <= Warpplate.Area.Right &&
                    y >= Warpplate.Area.Top && y <= Warpplate.Area.Bottom &&
                    Warpplate.DisableBuild)
                {
                    return true;
                }
            }
            return false;
        }

        public string InAreaWarpplateName(int x, int y)
        {
            foreach (Warpplate Warpplate in Warpplates)
            { 
                if (x >= Warpplate.Area.Left && x <= Warpplate.Area.Right &&
                    y >= Warpplate.Area.Top && y <= Warpplate.Area.Bottom &&
                    Warpplate.DisableBuild)
                {
                    return Warpplate.Name;
                }
            }
            return null;
        }

        public static List<string> ListIDs(string MergedIDs)
        {
            return MergedIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public bool removedestination(string WarpplateName)
        {
            Warpplate r = GetWarpplateByName(WarpplateName);
            if (r != null)
            {
                int q = database.Query("UPDATE Warpplates SET WarpplateDestination=@0 WHERE WarpplateName=@1 AND WorldID=@2", "", WarpplateName, Main.worldID.ToString());
                r.WarpDest = "";
                if (q > 0)
                    return true;
            }
            return false;
        }

        public bool adddestination(string WarpplateName, String WarpDestination)
        {
            Warpplate r = GetWarpplateByName(WarpplateName);
            if (r != null)
            {
                int q = database.Query("UPDATE Warpplates SET WarpplateDestination=@0 WHERE WarpplateName=@1 AND WorldID=@2;", WarpDestination, WarpplateName, Main.worldID.ToString());
                r.WarpDest = WarpDestination;
                if (q > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all the Warpplates names from world
        /// </summary>
        /// <param name="worldid">World name to get Warpplates from</param>
        /// <returns>List of Warpplates with only their names</returns>
        public List<Warpplate> ListAllWarpplates(string worldid)
        {
            var WarpplatesTemp = new List<Warpplate>();
            try
            {
                foreach (Warpplate wp in Warpplates)
                {
                    WarpplatesTemp.Add(new Warpplate { Name = wp.Name });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return WarpplatesTemp;
        }

        public Warpplate GetWarpplateByName(String name)
        {
            return Warpplates.FirstOrDefault(r => r.Name.Equals(name) && r.WorldID == Main.worldID.ToString());
        }
    }
*/
/*        public class Warpplate
        {
            public Rectangle Area { get; set; }
            public Vector2 WarpplatePos { get; set; }
            public string Name { get; set; }
            public string WarpDest { get; set; }
            public bool DisableBuild { get; set; }
            public string WorldID { get; set; }
            public List<int> AllowedIDs { get; set; }

            public Warpplate(Vector2 warpplatepos, Rectangle Warpplate, string name, string warpdest, bool disablebuild, string WarpplateWorldIDz)
                : this()
            {
                WarpplatePos = warpplatepos;
                Area = Warpplate;
                Name = name;
                WarpDest = warpdest;
                DisableBuild = disablebuild;
                WorldID = WarpplateWorldIDz;
            }

            public Warpplate()
            {
                WarpplatePos = Vector2.Zero;
                Area = Rectangle.Empty;
                Name = string.Empty;
                WarpDest = string.Empty;
                DisableBuild = true;
                WorldID = string.Empty;
                AllowedIDs = new List<int>();
            }

            public bool InArea(Rectangle point)
            {
                if (Area.Contains(point.X, point.Y))
                {
                    return true;
                }
                return false;
            }
        }*/
    }
}