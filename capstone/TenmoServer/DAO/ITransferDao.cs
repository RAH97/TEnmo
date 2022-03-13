using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        List<Transfer> ListTransfers(int accountId);
        Transfer GetTransferById(int transferId); 
        Transfer SendTransfer(Transfer transferToSend);
        Transfer RequestTransfer(Transfer transferToRequest);
        List<Transfer> GetPendingTransfers(int accountId);
        Transfer ApproveTransfer(Transfer transferApproval);
        Transfer DenyTransfer(Transfer transferDenied);
        public List<Transfer> ListSentTransfers(int accountId);
        public List<Transfer> ListReceivedTransfers(int accountId);

    }
}
