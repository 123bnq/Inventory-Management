using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using InventoryManagement.Entities;
using System.Data.Common;

namespace InventoryManagement.Helpers
{
    public class SqlHelpers
    {
        public const string DatabaseName = "Inventory.db";
        public const string DatabaseFolder = "Database";
        public static string DatabaseFolderPath = Path.Combine(AppContext.BaseDirectory, DatabaseFolder);
        public static string DatabasePath = Path.Combine(DatabaseFolderPath, DatabaseName);

        public static string ConnectionString = new SQLiteConnectionStringBuilder { DataSource = Path.Combine(DatabaseFolder, DatabaseName) }.ToString();

        public static string ScriptDb = File.ReadAllText(Path.Combine(DatabaseFolderPath, "Database.sql"));
        public static void GenerateNewDb()
        {
            try
            {
                if (File.Exists(DatabasePath) == false)
                {
                    Directory.CreateDirectory(DatabaseFolderPath);

                    SQLiteConnection.CreateFile(DatabasePath);

                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();
                        using (var commandCreateDb = connection.CreateCommand())
                        {
                            commandCreateDb.CommandText = ScriptDb;
                            commandCreateDb.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(DatabasePath);
            }
        }

        public static Inventory ReadInventoryFromDb(DbDataReader reader)
        {
            Inventory inventory = new Inventory();
            int.TryParse(Convert.ToString(reader["Id"]), out int id);
            inventory.Id = id;
            int.TryParse(Convert.ToString(reader["Number"]), out int number);
            inventory.Number = number;
            inventory.Object = Convert.ToString(reader["Object"]);
            inventory.InDate = Convert.ToString(reader["Incoming Date"]);
            inventory.Repack = Convert.ToString(reader["Repack"]);
            double.TryParse(Convert.ToString(reader["Price"]), out double price);
            inventory.Price = price;

            return inventory;
        }
    }
}
