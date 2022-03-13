using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;
using TenmoServer.DAO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TenmoServer.Controllers
{
    [Route("transfer")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private ITransferDao transferDao;
        private IAccountDao accountDao;

        public TransferController(ITransferDao transferDao, IAccountDao accountDao)
        {
            this.transferDao = transferDao;
            this.accountDao = accountDao;
        }

        [HttpGet("{transferId}")]
        public ActionResult<Transfer> GetTransferById(int transferId)
        {
            Transfer transfer = transferDao.GetTransferById(transferId);
            
            if (transfer != null)
            {
                if( transfer.FromUser == null)
                {
                    transfer.FromUser = User.Identity.Name;
                }
                else if (transfer.ToUser == null)
                {
                    transfer.ToUser = User.Identity.Name;
                }
                return transfer;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("all/{accountId}")]
        public ActionResult<List<Transfer>> ListTransfers(int accountId)
        {
            IUserDao userDao = new UserSqlDao("Server=.\\SQLEXPRESS;Database=tenmo;Trusted_Connection=True;");
            User currentUser = userDao.GetUser(User.Identity.Name);
            User testForCurrentUser = userDao.GetUserByAccountId(accountId);
            if (currentUser.UserId == testForCurrentUser.UserId)
            {
                List<Transfer> allTransfers = transferDao.ListTransfers(accountId);
               // foreach (Transfer xfer in allTransfers)
               // {
               //     if (xfer.ToUser == null)
               //     {
               //         xfer.ToUser = User.Identity.Name;
               //     }
               //     else if (xfer.FromUser == null)
               //     {
               //         xfer.FromUser = User.Identity.Name;
               //     }
               // }

                return allTransfers;
            }
            else if (currentUser.UserId != testForCurrentUser.UserId)
            {
                return BadRequest("You cannot view another user's transfers.");
            }
            return StatusCode(500);

        }

        [HttpPost("send")]
        public ActionResult<Transfer> SendTransfer(Transfer transferToSend)
        {
            IUserDao userDao = new UserSqlDao("Server=.\\SQLEXPRESS;Database=tenmo;Trusted_Connection=True;");
            User currentUser = userDao.GetUser(User.Identity.Name);
            User testForCurrentUser = userDao.GetUserByAccountId(transferToSend.AccountFromId);
            Account accountTo = accountDao.GetAccountFromAccountId(transferToSend.AccountToId);
            Account accountFrom = accountDao.GetAccountFromAccountId(transferToSend.AccountFromId);
            if (accountTo != null)
            {

                if (accountFrom.Balance < transferToSend.Amount)
                {
                    return BadRequest("You do not have enough money to transfer.");
                }
                else if (transferToSend.Amount <= 0)
                {
                    return BadRequest("You must send more than 0.00.");
                }
                else if (transferToSend.AccountFromId == transferToSend.AccountToId)
                {
                    return BadRequest("Cannot transfer money to same account.");
                }
                else if (currentUser.UserId != testForCurrentUser.UserId)
                {
                    return Forbid("You cannot take money from someone elses account without a request.");
                }
                Transfer added = transferDao.SendTransfer(transferToSend);
                accountFrom.Balance -= transferToSend.Amount;
                accountTo.Balance += transferToSend.Amount;
                return Created($"transfer/{added.TransferId}", added);

            }
            else if (accountTo == null)
            {
                return NotFound();
            }
            return StatusCode(500);

        }

        [HttpPost("request")]
        public ActionResult<Transfer> RequestTransfer(Transfer transferToRequest)
        {

            Account accountFrom = accountDao.GetAccountFromAccountId(transferToRequest.AccountFromId);

            if (accountFrom.AccountId == transferToRequest.AccountToId)
            {
                return BadRequest("Cannot transfer money to same account.");
            }
       
            else if (transferToRequest.Amount <= 0)
            {
                return BadRequest("Request must be greater than 0.00.");
            }

            Transfer added = transferDao.RequestTransfer(transferToRequest);
            return Created($"transfer/{added.TransferId}", added);
        }
        [HttpPut("approve")]
        public ActionResult<Transfer> ApproveTransfer(Transfer transferApproval)
        {
            IUserDao userDao = new UserSqlDao("Server=.\\SQLEXPRESS;Database=tenmo;Trusted_Connection=True;");
            Account accountFrom = accountDao.GetAccountFromAccountId(transferApproval.AccountFromId);
            User receivingUser = userDao.GetUserByAccountId(transferApproval.AccountToId);
            if (User.Identity.Name == receivingUser.Username)
            {
                return BadRequest("You cannot approve a request for a transfer being sent to your account.");
            }

            if (accountFrom.Balance < transferApproval.Amount)
            {
                return BadRequest("The sender did not have enough money.");
            }

            Transfer transferApproved = transferDao.ApproveTransfer(transferApproval);
            return Ok(transferApproved);
        }
        [HttpPut("deny")]
        public ActionResult<Transfer> DenyTransfer(Transfer transferDenied)
        {
            Transfer transferDeny = transferDao.DenyTransfer(transferDenied);
            return Ok(transferDeny);
        }
        [HttpGet("pending/{accountId}")]
        public ActionResult<List<Transfer>> GetPendingTransfers(int accountId)
        {
            List<Transfer> pendingTransfers = transferDao.GetPendingTransfers(accountId);
            
            return Ok(pendingTransfers);
        }
        [HttpGet("/account/{accountId}/transfer/sent")]
        public ActionResult<List<Transfer>> GetSentTransfers(int accountId)
        {
            List<Transfer> sentTransfers = transferDao.ListSentTransfers(accountId);
            if (sentTransfers == null)
            {
                return NotFound();
            }
         
            return Ok(sentTransfers);
        }
        [HttpGet("/account/{accountId}/transfer/received")]
        public ActionResult<List<Transfer>> GetReceivedTransfers(int accountId)
        {
            List<Transfer> recTransfers = transferDao.ListReceivedTransfers(accountId);
            if (recTransfers == null)
            {
                return NotFound();
            }
            return Ok(recTransfers);
        }
    }
}
