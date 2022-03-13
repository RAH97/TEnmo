using RestSharp;
using System.Collections.Generic;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TenmoApiService : AuthenticatedApiService
    {
        public readonly string ApiUrl;
        public TenmoApiService(string apiUrl) : base(apiUrl) { }


        // Add methods to call api here...
        // public Transfer GetTransferById()
        // {
        //
        // }
        public List<Transfer> ListTransfers(int accountId)
        {
            string url = $"transfer/all/{accountId}";

            RestRequest request = new RestRequest(url);
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }
        public Transfer GetTransferById(int transferId)
        {
            string url = $"transfer/{transferId}";

            RestRequest request = new RestRequest(url);
            IRestResponse<Transfer> response = client.Get<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
        public decimal GetBalanceByAccountId(int id)
        {
            string url = $"account/{id}/balance";

            RestRequest request = new RestRequest(url);
            IRestResponse<decimal> response = client.Get<decimal>(request);
            CheckForError(response);
            return response.Data;
        }
        public List<Transfer> GetPendingTransfers(int id)
        {
            string url = $"transfer/pending/{id}";

            RestRequest request = new RestRequest(url);
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            CheckForError(response);
            return response.Data;
        }
        public Transfer ApproveTransfer(Transfer transferToApprove)
        {
            string url = $"transfer/approve";

            RestRequest request = new RestRequest(url);
            request.AddJsonBody(transferToApprove);
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
        public Transfer DenyTransfer(Transfer transferToDeny)
        {
            string url = $"transfer/deny";

            RestRequest request = new RestRequest(url);
            request.AddJsonBody(transferToDeny);
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
        public Account GetAccountByUserId(int id)
        {
            string url = $"account/user/{id}";
            RestRequest request = new RestRequest(url);
            IRestResponse<Account> response = client.Get<Account>(request);
            CheckForError(response);
            return response.Data;
        }
        public List<User> GetUsers()
        {
            string url = "user";
            RestRequest request = new RestRequest(url);
            IRestResponse<List<User>> response = client.Get<List<User>>(request);
            CheckForError(response);
            return response.Data;

        }
        public bool SendTransfer(Transfer transfer)
        {
            string url = "transfer/send";
            RestRequest request = new RestRequest(url);
            request.AddJsonBody(transfer);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            CheckForError(response);

            if (response.Data != null)
            { 
                return true;
            }
            return false;

        }
        public bool RequestTransfer(Transfer transfer)
        {
            string url = "transfer/request";
            RestRequest request = new RestRequest(url);
            request.AddJsonBody(transfer);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            CheckForError(response);

            if (response.Data != null)
            {
                return true;
            }
            return false;
        }
    }
}
