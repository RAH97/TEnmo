using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TenmoServer.Models;
namespace TenmoServer.DAO
{
    public class TransferSqlDao : ITransferDao
    {

        private readonly string connectionString;
        public TransferSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Transfer GetTransferById(int transferId)
        {
            Transfer transfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($@"SELECT username AS username, transfer_id, transfer.transfer_type_id, transfer_type_desc, transfer.transfer_status_id, 
                      transfer_status_desc, amount, account_from, account_to
                      FROM transfer
                      JOIN account AS fromAccount ON fromAccount.account_id = transfer.account_from
                      JOIN tenmo_user AS fromUser ON fromAccount.user_id = fromUser.user_id
                      JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id
                      JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id
                      WHERE transfer_id = @transfer_id", conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        IUserDao userDao = new UserSqlDao(connectionString);
                        transfer = GetTransferFromReader(reader);
                        User user1 = userDao.GetUserByAccountId(transfer.AccountFromId);
                        User user2 = userDao.GetUserByAccountId(transfer.AccountToId);
                        transfer.FromUser = user1.Username;
                        transfer.ToUser = user2.Username;



                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return transfer;
        }
        public List<Transfer> ListTransfers(int accountId)
        {
            List<Transfer> allTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($@"SELECT username AS username, transfer_id, transfer.transfer_type_id, transfer_type_desc, transfer.transfer_status_id, 
                      transfer_status_desc, amount, account_from, account_to
                      FROM transfer
                      JOIN account AS fromAccount ON fromAccount.account_id = transfer.account_from
                      JOIN tenmo_user AS fromUser ON fromAccount.user_id = fromUser.user_id
                      JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id
                      JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id
                      WHERE account_to = @account_to
                      union
                      SELECT username AS username, transfer_id, transfer.transfer_type_id, transfer_type_desc, transfer.transfer_status_id, transfer_status_desc, amount, account_from, account_to
                      FROM transfer
                      JOIN account AS toAccount ON toAccount.account_id = transfer.account_to
                      JOIN tenmo_user AS toUser ON toAccount.user_id = toUser.user_id
                      JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id
                      JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id
                      WHERE account_from = @account_from", conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    cmd.Parameters.AddWithValue("@account_to", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = GetTransferFromReader(reader);
                        if (transfer.AccountToId == accountId)
                        {
                            transfer.FromUser = Convert.ToString(reader["username"]);

                        }
                        else if (transfer.AccountFromId == accountId)
                        {
                            transfer.ToUser = Convert.ToString(reader["username"]);
                        }
                        allTransfers.Add(transfer);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return allTransfers;
        }
        public List<Transfer> ListSentTransfers(int accountId)
        {

            List<Transfer> allTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT transfer_id,transfer.transfer_type_id, transfer.transfer_status_id, transfer.account_from, transfer.account_to, transfer.amount, transfer_status.transfer_status_desc, transfer_type.transfer_type_desc " +
                        " FROM transfer JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id " +
                        " JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id " +
                        " WHERE account_from = @account_from AND transfer.transfer_type_id = @status_id GROUP BY transfer_id, transfer.transfer_type_id, transfer.transfer_status_id, transfer.account_from, transfer.account_to, transfer.amount, transfer_status.transfer_status_desc, transfer_type.transfer_type_desc", conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    cmd.Parameters.AddWithValue("@status_id", 2);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = GetTransferFromReader(reader);
                        allTransfers.Add(transfer);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return allTransfers;

        }
        public List<Transfer> ListReceivedTransfers(int accountId)
        {

            List<Transfer> allTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT transfer_id,transfer.transfer_type_id, transfer.transfer_status_id, transfer.account_from, transfer.account_to, transfer.amount, transfer_status.transfer_status_desc, transfer_type.transfer_type_desc " +
                        " FROM transfer JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id " +
                        " JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id " +
                        " WHERE account_from != @account_from AND account_to = @account_from AND transfer.transfer_type_id = @status_id GROUP BY transfer_id, transfer.transfer_type_id, transfer.transfer_status_id, transfer.account_from, transfer.account_to, transfer.amount, transfer_status.transfer_status_desc, transfer_type.transfer_type_desc", conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    cmd.Parameters.AddWithValue("@status_id", 2);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer transfer = GetTransferFromReader(reader);
                        allTransfers.Add(transfer);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return allTransfers;

        }
        public Transfer SendTransfer(Transfer transferToSend)
        {
            AccountSqlDao dao = new AccountSqlDao(connectionString);
            Account accountFrom = dao.GetAccountFromAccountId(transferToSend.AccountFromId);
            Account accountTo = dao.GetAccountFromAccountId(transferToSend.AccountToId);
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount)"
                                                    + " OUTPUT INSERTED.transfer_id VALUES(@transfer_type_id, @transfer_status_id, @account_from, @account_to, @ammount)", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", 2);
                    cmd.Parameters.AddWithValue("@transfer_status_id", 2);
                    cmd.Parameters.AddWithValue("@account_from", transferToSend.AccountFromId);
                    cmd.Parameters.AddWithValue("@account_to", transferToSend.AccountToId);
                    cmd.Parameters.AddWithValue("@ammount", transferToSend.Amount);
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    transferToSend.TransferId = newId;
                    cmd = new SqlCommand("UPDATE account SET balance = @balance WHERE account_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", transferToSend.AccountFromId);
                    cmd.Parameters.AddWithValue("@balance", (accountFrom.Balance - transferToSend.Amount));
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("UPDATE account SET balance = @balance WHERE account_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", transferToSend.AccountToId);
                    cmd.Parameters.AddWithValue("@balance", (accountTo.Balance + transferToSend.Amount));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transferToSend;
        }
        public Transfer RequestTransfer(Transfer transferToRequest)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount)"
                                                    + " OUTPUT INSERTED.transfer_id VALUES(@transfer_type_id, @transfer_status_id, @account_from, @account_to, @ammount)", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", 1);
                    cmd.Parameters.AddWithValue("@transfer_status_id", 1);
                    cmd.Parameters.AddWithValue("@account_from", transferToRequest.AccountFromId);
                    cmd.Parameters.AddWithValue("@account_to", transferToRequest.AccountToId);
                    cmd.Parameters.AddWithValue("@ammount", transferToRequest.Amount);
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    transferToRequest.TransferId = newId;

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transferToRequest;

        }
        public List<Transfer> GetPendingTransfers(int accountId)
        {
            List<Transfer> allTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand($@"SELECT username AS username, transfer_id, transfer.transfer_type_id, transfer_type_desc, transfer.transfer_status_id, 
                      transfer_status_desc, amount, account_from, account_to
                      FROM transfer
                      JOIN account AS fromAccount ON fromAccount.account_id = transfer.account_to
                      JOIN tenmo_user AS fromUser ON fromAccount.user_id = fromUser.user_id
                      JOIN transfer_status ON transfer.transfer_status_id = transfer_status.transfer_status_id
                      JOIN transfer_type ON transfer.transfer_type_id = transfer_type.transfer_type_id
                      WHERE account_to != @account_from AND account_from = @account_from AND transfer.transfer_status_id = 1", conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);


                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        
                        Transfer transfer = GetTransferFromReader(reader);
                        UserSqlDao userDao = new UserSqlDao(connectionString);
                        int accountIdCheck = Convert.ToInt32(reader["account_from"]);
                        User referenceUser = userDao.GetUserByAccountId(accountIdCheck);
                        if (transfer.AccountFromId ==  referenceUser.UserAccount.AccountId)
                        {
                            transfer.ToUser = Convert.ToString(reader["username"]);
                        }
                        allTransfers.Add(transfer);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return allTransfers;
        }
        public Transfer ApproveTransfer(Transfer transferApproval)
        {
            AccountSqlDao dao = new AccountSqlDao(connectionString);
            Account accountFrom = dao.GetAccountFromAccountId(transferApproval.AccountFromId);
            Account accountTo = dao.GetAccountFromAccountId(transferApproval.AccountToId);
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE transfer SET transfer_status_id = 2 WHERE transfer_id = @transfer_id", conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferApproval.TransferId);
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("UPDATE account SET balance = @balance WHERE account_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", transferApproval.AccountFromId);
                    cmd.Parameters.AddWithValue("@balance", (accountFrom.Balance - transferApproval.Amount));
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("UPDATE account SET balance = @balance WHERE account_id = @account_id", conn);
                    cmd.Parameters.AddWithValue("@account_id", transferApproval.AccountToId);
                    cmd.Parameters.AddWithValue("@balance", (accountTo.Balance + transferApproval.Amount));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return transferApproval;
        }
        public Transfer DenyTransfer(Transfer transferDenied)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE transfer SET transfer_status_id = 3 WHERE transfer_id = @transfer_id", conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferDenied.TransferId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return transferDenied;
        }



        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer retrievedTransfer = new Transfer();
            TransferStatus retrievedStatus = new TransferStatus();
            TransferType retrievedType = new TransferType();
            retrievedTransfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            retrievedTransfer.AccountFromId = Convert.ToInt32(reader["account_from"]);
            retrievedTransfer.AccountToId = Convert.ToInt32(reader["account_to"]);
            retrievedTransfer.Amount = Convert.ToDecimal(reader["amount"]);
            retrievedStatus.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            retrievedStatus.TransferStatusDescription = Convert.ToString(reader["transfer_status_desc"]);
            retrievedType.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            retrievedType.TransferTypeDescription = Convert.ToString(reader["transfer_type_desc"]);
            retrievedTransfer.Type = retrievedType;
            retrievedTransfer.Status = retrievedStatus;
            return retrievedTransfer;
        }

    }
}
