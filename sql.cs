using System.Data.Common;
using System.Xml;
using MySql.Data.MySqlClient;

namespace PetBotCs
{
    internal class sql
    {
        private MySqlConnection db;

        public sql(string connection)
        {
            db = new MySqlConnection(connection);
        }


        public async Task<List<Dictionary<string, object>>> Read(MySqlCommand Command)
        {
            try
            {
                db.Open();
                Command.Connection = db;
                using DbDataReader reader = await Command.ExecuteReaderAsync();
                List<Dictionary<string, object>> results = new();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    results.Add(row);
                }
                return results;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public List<string> Read(string query, string columnName)
        {
            try
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.Connection = db;
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<List<string>> data = new();
                        List<string> rows = new();
                        while (reader.Read())
                        {
                            rows.Add(reader[columnName].ToString());
                        }
                        reader.Close();
                        db.Close();
                        return rows;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
        public async Task<List<Dictionary<string, object>>> ReadTopPets(int topCount, long GroupId)
        {
            try
            {
                db.Open();

                string query = $"SELECT `firstname`, `size` FROM `group{GroupId}` ORDER BY `size` DESC LIMIT {topCount};";
                MySqlCommand command = new MySqlCommand(query, db);

                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        results.Add(row);
                    }
                }
                db.Close();
                return results;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> ExtRead(string query, string[] columnNames)
        {
            try
            {
                db.Open();
                using var cmd = db.CreateCommand();
                cmd.Connection = db;
                cmd.CommandText = query;
                using var reader = cmd.ExecuteReader();
                List<Dictionary<string, object>> data = new();

                while (reader.Read())
                {
                    Dictionary<string, object> row = new();
                    foreach (var columnName in columnNames)
                    {
                        row[columnName] = reader[columnName];
                    }
                    data.Add(row);
                }
                db.Close();
                reader.Close();
                return data;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public bool TableExists(long tableName)
        {
            try
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.Connection = db;
                    cmd.CommandText = $"SHOW TABLES LIKE 'group{tableName}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        bool tableExists = reader.HasRows;
                        reader.Close();
                        db.Close();
                        return tableExists;
                    }
                }
                
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public List<string> GetTableNames()
        {
            try
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.Connection = db;
                    cmd.CommandText = "SHOW TABLES";
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<string> tableNames = new List<string>();
                        while (reader.Read())
                        {
                            tableNames.Add(reader[0].ToString());
                        }
                        reader.Close();
                        db.Close();
                        return tableNames;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
    }
}
