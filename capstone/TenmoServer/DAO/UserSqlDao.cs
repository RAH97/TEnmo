using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class UserSqlDao : IUserDao
    {
        private readonly string connectionString;
        const decimal startingBalance = 1000;

        public UserSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public User GetUserByAccountId(int accountId)
        {
            User returnUser = null;
            AccountSqlDao accountDao = new AccountSqlDao(connectionString);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT tenmo_user.user_id, username, password_hash, salt FROM tenmo_user JOIN account ON account.user_id = tenmo_user.user_id WHERE account_id = @user_id", conn);
                    cmd.Parameters.AddWithValue("@user_id", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Account a = new Account();
                        returnUser = GetUserFromReader(reader);
                        a = accountDao.GetAccountFromAccountId(accountId);
                        returnUser.UserAccount = a;
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;

        }
        public User GetUser(string username)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM tenmo_user WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnUser = GetUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        public List<User> GetUsers()
        {
            List<User> returnUsers = new List<User>();
            AccountSqlDao accountDao = new AccountSqlDao(connectionString);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT tenmo_user.user_id, username, password_hash, salt, account_id FROM tenmo_user JOIN account ON account.user_id = tenmo_user.user_id", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        User u = GetUserFromReader(reader);
                        Account a = new Account()
                        {
                            AccountId = Convert.ToInt32(reader["account_id"])

                        };
                        a = accountDao.GetAccountFromAccountId(a.AccountId);
                        u.UserAccount = a;
                        returnUsers.Add(u);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUsers;
        }

        public User AddUser(string username, string password)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO tenmo_user (username, password_hash, salt) VALUES (@username, @password_hash, @salt)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT @@IDENTITY", conn);
                    int userId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("INSERT INTO account (user_id, balance) VALUES (@userid, @startBalance)", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@startBalance", startingBalance);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetUser(username);
        }

        private User GetUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
            };

            return u;
        }
    }
}
