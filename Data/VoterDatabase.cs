using System;
using System.Collections.Generic;
using EVMS.Models;
using MySql.Data.MySqlClient;

namespace EVMS.Data
{
    /// <summary>
    /// Handles all database operations for the EVMS using MariaDB
    /// </summary>
    public class VoterDatabase
    {
        private readonly string _connectionString;

        /// <summary>
        /// Pass your MariaDB connection details here.
        /// Defaults assume a local MariaDB instance with database 'evms_db'.
        /// </summary>
        public VoterDatabase(
            string server   = "localhost",
            int    port     = 3306,
            string database = "evms_db",
            string user     = "root",
            string password = "NewStrongPassword123!")
        {
            _connectionString =
                $"Server={server};Port={port};Database={database};" +
                $"User ID={user};Password={password};CharSet=utf8mb4;";

            InitializeDatabase();
        }

        /// <summary>Creates the database and voters table if they do not exist</summary>
        private void InitializeDatabase()
        {
            // Connect without specifying the database first so we can CREATE it
            var adminCs = _connectionString
                .Replace($"Database={GetDatabaseName()};", "");

            using (var adminConn = new MySqlConnection(adminCs))
            {
                adminConn.Open();
                var createDb = $"CREATE DATABASE IF NOT EXISTS `{GetDatabaseName()}` " +
                               "CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                using var dbCmd = new MySqlCommand(createDb, adminConn);
                dbCmd.ExecuteNonQuery();
            }

            // Now connect to the target database and create the table
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS Voters (
                    Id               INT          NOT NULL AUTO_INCREMENT,
                    VoterCardId      VARCHAR(50)  NOT NULL UNIQUE,
                    NationalIdNumber VARCHAR(50)  NOT NULL UNIQUE,
                    FirstName        VARCHAR(100) NOT NULL,
                    MiddleName       VARCHAR(100),
                    Surname          VARCHAR(100) NOT NULL,
                    PollingStation   VARCHAR(200) NOT NULL,
                    DateOfBirth      VARCHAR(10)  NOT NULL COMMENT 'Format: dd-mm-yyyy',
                    Gender           VARCHAR(10)  NOT NULL,
                    PRIMARY KEY (Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            using var cmd = new MySqlCommand(createTableSql, connection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Adds a new voter record to the database</summary>
        public bool AddVoter(Voter voter)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = @"INSERT INTO Voters 
                        (VoterCardId, NationalIdNumber, FirstName, MiddleName, Surname, PollingStation, DateOfBirth, Gender)
                        VALUES 
                        (@VoterCardId, @NationalIdNumber, @FirstName, @MiddleName, @Surname, @PollingStation, @DateOfBirth, @Gender)";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@VoterCardId",      voter.VoterCardId);
            cmd.Parameters.AddWithValue("@NationalIdNumber", voter.NationalIdNumber);
            cmd.Parameters.AddWithValue("@FirstName",        voter.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName",       voter.MiddleName ?? "");
            cmd.Parameters.AddWithValue("@Surname",          voter.Surname);
            cmd.Parameters.AddWithValue("@PollingStation",   voter.PollingStation);
            cmd.Parameters.AddWithValue("@DateOfBirth",      voter.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender",           voter.Gender);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Deletes a voter by their Voter Card ID</summary>
        public bool DeleteVoter(string voterCardId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "DELETE FROM Voters WHERE VoterCardId = @VoterCardId";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@VoterCardId", voterCardId);
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Retrieves a voter by Voter Card ID</summary>
        public Voter? GetVoterByCardId(string voterCardId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT * FROM Voters WHERE VoterCardId = @VoterCardId";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@VoterCardId", voterCardId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapReaderToVoter(reader);

            return null;
        }

        /// <summary>Retrieves all voters from the database</summary>
        public List<Voter> GetAllVoters()
        {
            var voters = new List<Voter>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT * FROM Voters ORDER BY Surname, FirstName";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                voters.Add(MapReaderToVoter(reader));

            return voters;
        }

        private static Voter MapReaderToVoter(MySqlDataReader reader) => new Voter
        {
            Id               = reader.GetInt32("Id"),
            VoterCardId      = reader.GetString("VoterCardId"),
            NationalIdNumber = reader.GetString("NationalIdNumber"),
            FirstName        = reader.GetString("FirstName"),
            MiddleName       = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? "" : reader.GetString("MiddleName"),
            Surname          = reader.GetString("Surname"),
            PollingStation   = reader.GetString("PollingStation"),
            DateOfBirth      = reader.GetString("DateOfBirth"),
            Gender           = reader.GetString("Gender")
        };

        /// <summary>Extracts the database name from the connection string</summary>
        private string GetDatabaseName()
        {
            foreach (var part in _connectionString.Split(';'))
            {
                var kv = part.Split('=');
                if (kv.Length == 2 && kv[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase))
                    return kv[1].Trim();
            }
            return "evms_db";
        }
    }
}