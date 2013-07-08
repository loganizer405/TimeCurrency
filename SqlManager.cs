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
        private SqlManager()
        {
        }
        public static void EnsureTableExists(IDbConnection db)
        {
            database = db;
           
            var table = new SqlTable("TimeCurrency",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("Time", MySqlDbType.Int32),
                new SqlColumn("TimePlayed", MySqlDbType.Int32),             
                new SqlColumn("Dead", MySqlDbType.Int32),
                new SqlColumn("LastSeen", MySqlDbType.Text)             
            );
            var creator = new SqlTableCreator(db,
             db.GetSqlType() == SqlType.Sqlite
             ? (IQueryBuilder)new SqliteQueryCreator()
             : new MysqlQueryCreator());

            creator.EnsureExists(table);

        }       
        public static bool AddSeconds(string name, int seconds)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET Time = Time + @0 WHERE Name = @1", seconds, name);
                return true;

            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
                return false;

            }
        }
        public static bool CheckForEntry(string name)
        {
            string stuff = "";
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM TimeCurrency WHERE Name = @0", name))
                {
                    if (reader.Read())
                        stuff = reader.Get<string>("Name");
                    /*if (!reader.Read())
                    {
                        Log.Error("Write to SQL exception:(TimeCurrency)");
                        return true;
                    }*/
                    if (stuff == "" || stuff == null)
                    {
                        return false;
                    }
                    else
                    {
                        throw new Exception("Failed to write to sql database");
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.ToString());
                return true;
            }
        }
        public static bool AddUserEntry(string name)
        {
            try
            {
                database.Query("INSERT INTO TImeCurrency (Name, Time, TimePlayed, Dead, LastSeen) VALUES (@0, 604800, 0, 0, @1)", name, DateTime.Now.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
            }
            return 0;
        }
        public static string GetLastSeen(string name)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT LastSeen FROM TimeCurrency WHERE Name = @0", name))
                {
                    if (reader.Read())
                        return reader.Get<string>("LastSeen");
                }

            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.ToString());
                return null;
            }
            return null;
        }
        public static bool AddTimePlayed(string name, int seconds)
        {
            try
            {
                database.Query("UPDATE TimeCurrency SET TimePlayed = TimePlayed + @0 WHERE Name = @1", seconds, name);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Write to SQL exception:(TimeCurrency)");
                Log.Error(ex.ToString());
                return false;
            }
        }
        public static int GetTimePlayed(string name)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT TimePlayed FROM TimeCurrency WHERE Name = @0", name))
                {
                    if (reader.Read())
                    {
                        return reader.Get<int>("TimePlayed");
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
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
                return false;
            }
        }
    }
}