using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDao : IAccountDao
    {
        private readonly string connectionString;
        public AccountSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Account GetAccountFromAccountId(int accountId)
        {
            Account account = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM account WHERE account_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {

                        account = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return account;
            
        }
        public Account GetAccountFromUserId(int userId)
        {
            Account account = null;
            try
            {
                using (SqlConnection conn  = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM account JOIN tenmo_user ON tenmo_user.user_id = account.user_id WHERE account.user_id = @user_id", conn);
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        account = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return account;
        }
        public decimal GetBalanceFromAccountId(int accountId)
        {
            decimal accountBalance = 0.00M;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT balance FROM account JOIN tenmo_user ON account.user_id = tenmo_user.user_id WHERE account.user_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        accountBalance = Convert.ToDecimal(reader["balance"]);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return accountBalance;
        }



        public Account GetAccountFromReader(SqlDataReader reader)
        {
            Account account = new Account();
            account.AccountId = Convert.ToInt32(reader["account_id"]);
            account.UserId = Convert.ToInt32(reader["user_id"]);
            account.Balance = Convert.ToDecimal(reader["balance"]);
            return account;
        }
    }
}
