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
    [Route("user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private IUserDao userDao;
        public UserController(IUserDao dao)
        {
            this.userDao = dao;
        }
        [HttpGet()]
       public ActionResult<List<User>> GetUsersToSendMoneyTo()
        {
            List<User> allUsersExceptCurrent = new List<User>();
            List<User> allusers = userDao.GetUsers();
            
            if (allusers == null)
            {
                return NotFound();
            }
            else if (allusers != null)
            {
                foreach (User user in allusers)
                {
                    if(user.Username != User.Identity.Name)
                    {
                        allUsersExceptCurrent.Add(user);
                    }
                }
                return allUsersExceptCurrent;

            }
            return StatusCode(500);
        }
    }
}
