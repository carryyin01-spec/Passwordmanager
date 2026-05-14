using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using PasswordManagerApp.Models;

namespace PasswordManagerApp.Data
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=passwords.db";

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    CREATE TABLE IF NOT EXISTS PasswordEntries (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProjectName TEXT,
                        Address TEXT,
                        Account TEXT,
                        Password TEXT,
                        Notes TEXT,
                        ExtraCol1 TEXT,
                        ExtraCol2 TEXT,
                        ExtraCol3 TEXT,
                        Category TEXT
                    );
                    CREATE TABLE IF NOT EXISTS SystemConfig (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Libraries (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT UNIQUE,
                        Col1 TEXT, Col2 TEXT, Col3 TEXT, Col4 TEXT, 
                        Col5 TEXT, Col6 TEXT, Col7 TEXT, Col8 TEXT
                    );
                ";
                command.ExecuteNonQuery();

                // Add default password if not exists
                command.CommandText = "SELECT COUNT(*) FROM SystemConfig WHERE Key = 'LoginPassword'";
                long configCount = (long)(command.ExecuteScalar() ?? 0L);
                if (configCount == 0)
                {
                    command.CommandText = "INSERT INTO SystemConfig (Key, Value) VALUES ('LoginPassword', '')";
                    command.ExecuteNonQuery();
                }

                // Add default homepage if not exists
                command.CommandText = "SELECT COUNT(*) FROM SystemConfig WHERE Key = 'DefaultTab'";
                if ((long)(command.ExecuteScalar() ?? 0L) == 0)
                {
                    command.CommandText = "INSERT INTO SystemConfig (Key, Value) VALUES ('DefaultTab', '密码库')";
                    command.ExecuteNonQuery();
                }

                // Initialize Libraries table
                command.CommandText = "SELECT COUNT(*) FROM Libraries";
                if ((long)(command.ExecuteScalar() ?? 0L) == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Libraries (Name, Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8) VALUES 
                        ('密码库', '项目名', '地址', '账户', '密码', '备注', '', '', ''),
                        ('自定义库', '项目名', '地址', '账户', '密码', '备注', '', '', '')";
                    command.ExecuteNonQuery();
                }
            }
        }

        public static string GetLoginPassword()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM SystemConfig WHERE Key = 'LoginPassword'";
                return command.ExecuteScalar()?.ToString() ?? "";
            }
        }

        public static void SetLoginPassword(string password)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE SystemConfig SET Value = $password WHERE Key = 'LoginPassword'";
                command.Parameters.AddWithValue("$password", password);
                command.ExecuteNonQuery();
            }
        }

        public static string GetConfig(string key, string defaultValue = "")
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM SystemConfig WHERE Key = $key";
                command.Parameters.AddWithValue("$key", key);
                var result = command.ExecuteScalar();
                return result?.ToString() ?? defaultValue;
            }
        }

        public static void SetConfig(string key, string value)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO SystemConfig (Key, Value) VALUES ($key, $value)";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$value", value);
                command.ExecuteNonQuery();
            }
        }

        public static List<LibraryConfig> GetAllLibraries()
        {
            var libs = new List<LibraryConfig>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Libraries";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        libs.Add(new LibraryConfig
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Col1 = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Col2 = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Col3 = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Col4 = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Col5 = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            Col6 = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            Col7 = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            Col8 = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }
            return libs;
        }

        public static void SaveLibrary(LibraryConfig lib)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (lib.Id == 0)
                {
                    command.CommandText = @"INSERT INTO Libraries (Name, Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8) 
                                          VALUES ($name, $c1, $c2, $c3, $c4, $c5, $c6, $c7, $c8)";
                }
                else
                {
                    command.CommandText = @"UPDATE Libraries SET Name=$name, Col1=$c1, Col2=$c2, Col3=$c3, Col4=$c4, 
                                          Col5=$c5, Col6=$c6, Col7=$c7, Col8=$c8 WHERE Id=$id";
                    command.Parameters.AddWithValue("$id", lib.Id);
                }
                command.Parameters.AddWithValue("$name", lib.Name);
                command.Parameters.AddWithValue("$c1", lib.Col1 ?? "");
                command.Parameters.AddWithValue("$c2", lib.Col2 ?? "");
                command.Parameters.AddWithValue("$c3", lib.Col3 ?? "");
                command.Parameters.AddWithValue("$c4", lib.Col4 ?? "");
                command.Parameters.AddWithValue("$c5", lib.Col5 ?? "");
                command.Parameters.AddWithValue("$c6", lib.Col6 ?? "");
                command.Parameters.AddWithValue("$c7", lib.Col7 ?? "");
                command.Parameters.AddWithValue("$c8", lib.Col8 ?? "");
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteLibrary(int id)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // 1. Get name to delete entries
                command.CommandText = "SELECT Name FROM Libraries WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
                string? name = command.ExecuteScalar()?.ToString();
                
                if (name != null && name != "密码库")
                {
                    // 2. Delete entries
                    command.CommandText = "DELETE FROM PasswordEntries WHERE Category = $name";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$name", name);
                    command.ExecuteNonQuery();

                    // 3. Delete lib
                    command.CommandText = "DELETE FROM Libraries WHERE Id = $id";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<PasswordEntry> GetAllEntries()
        {
            var entries = new List<PasswordEntry>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM PasswordEntries";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new PasswordEntry
                        {
                            Id = reader.GetInt32(0),
                            ProjectName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Address = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Account = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Password = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Notes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ExtraCol1 = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            ExtraCol2 = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            ExtraCol3 = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            Category = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }
            return entries;
        }

        public static void AddEntry(PasswordEntry entry)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    INSERT INTO PasswordEntries (ProjectName, Address, Account, Password, Notes, ExtraCol1, ExtraCol2, ExtraCol3, Category)
                    VALUES ($projectName, $address, $account, $password, $notes, $extra1, $extra2, $extra3, $category)
                ";
                command.Parameters.AddWithValue("$projectName", entry.ProjectName ?? "");
                command.Parameters.AddWithValue("$address", entry.Address ?? "");
                command.Parameters.AddWithValue("$account", entry.Account ?? "");
                command.Parameters.AddWithValue("$password", entry.Password ?? "");
                command.Parameters.AddWithValue("$notes", entry.Notes ?? "");
                command.Parameters.AddWithValue("$extra1", entry.ExtraCol1 ?? "");
                command.Parameters.AddWithValue("$extra2", entry.ExtraCol2 ?? "");
                command.Parameters.AddWithValue("$extra3", entry.ExtraCol3 ?? "");
                command.Parameters.AddWithValue("$category", entry.Category ?? "密码库");
                command.ExecuteNonQuery();
            }
        }

        public static void UpdateEntry(PasswordEntry entry)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    UPDATE PasswordEntries 
                    SET ProjectName = $projectName, Address = $address, Account = $account, 
                        Password = $password, Notes = $notes, ExtraCol1 = $extra1, 
                        ExtraCol2 = $extra2, ExtraCol3 = $extra3, Category = $category
                    WHERE Id = $id
                ";
                command.Parameters.AddWithValue("$projectName", entry.ProjectName ?? "");
                command.Parameters.AddWithValue("$address", entry.Address ?? "");
                command.Parameters.AddWithValue("$account", entry.Account ?? "");
                command.Parameters.AddWithValue("$password", entry.Password ?? "");
                command.Parameters.AddWithValue("$notes", entry.Notes ?? "");
                command.Parameters.AddWithValue("$extra1", entry.ExtraCol1 ?? "");
                command.Parameters.AddWithValue("$extra2", entry.ExtraCol2 ?? "");
                command.Parameters.AddWithValue("$extra3", entry.ExtraCol3 ?? "");
                command.Parameters.AddWithValue("$category", entry.Category ?? "密码库");
                command.Parameters.AddWithValue("$id", entry.Id);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteEntry(int id)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM PasswordEntries WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

        public static List<PasswordEntry> SearchEntries(string keyword)
        {
            var entries = new List<PasswordEntry>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    SELECT * FROM PasswordEntries 
                    WHERE ProjectName LIKE $keyword OR Account LIKE $keyword OR Notes LIKE $keyword 
                       OR ExtraCol1 LIKE $keyword OR ExtraCol2 LIKE $keyword OR ExtraCol3 LIKE $keyword
                ";
                command.Parameters.AddWithValue("$keyword", $"%{keyword}%");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new PasswordEntry
                        {
                            Id = reader.GetInt32(0),
                            ProjectName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Address = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Account = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Password = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Notes = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ExtraCol1 = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            ExtraCol2 = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            ExtraCol3 = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            Category = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }
            return entries;
        }
    }
}
