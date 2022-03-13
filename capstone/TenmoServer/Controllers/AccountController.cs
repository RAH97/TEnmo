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
    [Route("account")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private IAccountDao accountDao;
        public AccountController(IAccountDao accountDao)
        {
            this.accountDao = accountDao;
        }

        [HttpGet("{id}/balance")]
        public decimal GetAccountBalance(int id)
        {
            decimal balance = accountDao.GetBalanceFromAccountId(id);
            return balance;

        }
        [HttpGet("user/{id}")]
        public ActionResult<Account> GetAccountByUserId(int id)
        {
            Account account = accountDao.GetAccountFromUserId(id);
            return account;
        }
        
        
    

    }
}
