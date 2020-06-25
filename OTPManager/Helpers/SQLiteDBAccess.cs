using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;
using OTPManager.DBModels;

namespace OTPManager.Helpers
{
    public class SQLiteDBAccess
    {
        const int DB_VERSION = 2;
        const string DEFAULT_CONNECTION_STRINGS_SECTION = "Default";

        readonly string _connectionString;
        readonly string _dbPassword;
        readonly bool _dbCompressionIsEnabled;

        public static SQLiteDBAccess Instance { get; set; }

        public SQLiteDBAccess() : this(null, false) { }

        public SQLiteDBAccess(string dbPassword) : this(dbPassword, false) { }

        public SQLiteDBAccess(string dbPassword, bool dbCompressionIsEnabled)
        {
            _dbPassword = dbPassword;
            _dbCompressionIsEnabled = dbCompressionIsEnabled;
            _connectionString = GetConnectionString();
            CheckDBExists();
            TestDBAccess();
        }

        private void CheckDBExists()
        {
            var builder = new SQLiteConnectionStringBuilder(_connectionString);
            string dbFilePath = Path.GetFullPath(builder.DataSource);

            if (!File.Exists(dbFilePath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(dbFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dbFilePath));
                }

                // create new default db
                File.WriteAllBytes(dbFilePath, Properties.Resources.DEFAULT_DB_FILE);

                if (!string.IsNullOrEmpty(_dbPassword))
                {
                    // Opens an unencrypted database
                    using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
                    cnn.Open();
                    // Encrypts the database. The connection remains valid and usable afterwards.
                    cnn.ChangePassword(_dbPassword);
                    // Removes the encryption on an encrypted database.
                    //cnn.ChangePassword(null);
                }

                // apply default db schema
                CreateDBSchema();
            }
        }

        private string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[DEFAULT_CONNECTION_STRINGS_SECTION].ConnectionString;
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Requested connection string name doesn't exist in App.config");
            }
        }

        private void TestDBAccess()
        {
            var settings = Select_Settings();

            foreach (var setting in settings)
            {
                if (setting.Key.Equals(nameof(DB_VERSION)))
                {
                    if (int.Parse(setting.Value) != DB_VERSION)
                    {
                        throw new Exception("DB version mismatch");
                    }
                }
            }
        }

        #region DB queries

        //public void ChangeDBPassword(string newPassword)
        //{
        //    using SQLiteConnection cnn = new SQLiteConnection(_connectionString);

        //    if (!string.IsNullOrEmpty(_dbPassword)) { cnn.SetPassword(_dbPassword); }

        //    cnn.Open();
        //    cnn.ChangePassword(newPassword);
        //    _dbPassword = newPassword;
        //    // Removes the encryption on an encrypted database.
        //    //cnn.ChangePassword(null);
        //}

        private void CreateDBSchema()
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string query = Properties.Resources.DB_SCHEMA_QUERY;

            cnn.Execute(query);
        }

        private IList<Settings> Select_Settings()
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string sql = "SELECT * " +
                         "FROM Settings;";

            try
            {
                return cnn.Query<Settings>(sql).ToList();
            }
            catch (Exception)
            {
                throw new Exception("DB credentials error");
            }
        }

        private void CompactDatabase()
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string query = "VACUUM main;";

            cnn.Execute(query);
        }

        public IList<OtpKeys> Select_OtpKeys()
        {
            //using IDbConnection cnn = new SQLiteConnection(ConnectionString);
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string sql = "SELECT * " +
                         "FROM OtpKeys;";
            //"ORDER BY Description ASC;";

            return cnn.Query<OtpKeys>(sql).ToList();
        }

        public void Insert_OtpKeys(OtpKeys otpKey)
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string query = "INSERT INTO OtpKeys (Description, Base32SecretKey) " +
                           "VALUES (@Description, @Base32SecretKey);";

            cnn.Execute(query, otpKey);

            if (_dbCompressionIsEnabled) { CompactDatabase(); }
        }

        public void Update_OtpKeys(OtpKeys otpKey)
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string query = "UPDATE OtpKeys " +
                           "SET Description = @Description, Base32SecretKey = @Base32SecretKey " +
                           "WHERE ID = @ID;";

            cnn.Execute(query, otpKey);

            if (_dbCompressionIsEnabled) { CompactDatabase(); }
        }

        public void Delete_OtpKeys(OtpKeys otpKey)
        {
            using SQLiteConnection cnn = new SQLiteConnection(_connectionString);
            cnn.SetPassword(_dbPassword);

            string query = "DELETE FROM OtpKeys " +
                           "WHERE ID = @ID;";

            cnn.Execute(query, otpKey);

            if (_dbCompressionIsEnabled) { CompactDatabase(); }
        }

        #endregion
    }
}
