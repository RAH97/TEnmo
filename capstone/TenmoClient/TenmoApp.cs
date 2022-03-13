using System;
using System.Collections.Generic;
using TenmoClient.Models;
using TenmoClient.Services;

namespace TenmoClient
{
    public class TenmoApp
    {
        private readonly TenmoConsoleService console = new TenmoConsoleService();
        private readonly TenmoApiService tenmoApiService;

        public TenmoApp(string apiUrl)
        {
            tenmoApiService = new TenmoApiService(apiUrl);
        }

        public void Run()
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                // The menu changes depending on whether the user is logged in or not
                if (tenmoApiService.IsLoggedIn)
                {
                    keepGoing = RunAuthenticated();
                }
                else // User is not yet logged in
                {
                    keepGoing = RunUnauthenticated();
                }
            }
        }

        private bool RunUnauthenticated()
        {
            console.PrintLoginMenu();
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 2, 1);
            while (true)
            {
                if (menuSelection == 0)
                {
                    return false;   // Exit the main menu loop
                }

                if (menuSelection == 1)
                {
                    // Log in
                    Login();
                    return true;    // Keep the main menu loop going
                }

                if (menuSelection == 2)
                {
                    // Register a new user
                    Register();
                    return true;    // Keep the main menu loop going
                }
                console.PrintError("Invalid selection. Please choose an option.");
                console.Pause();
            }
        }

        private bool RunAuthenticated()
        {
            console.PrintMainMenu(tenmoApiService.Username);
            GetBalance();
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 6);
            if (menuSelection == 0)
            {
                // Exit the loop
                return false;
            }

            if (menuSelection == 1)
            {
                ViewAllTransfers();
            }

            if (menuSelection == 2)
            {
                GetPendingTransfers();

            }

            if (menuSelection == 3)
            {
                SendTransfer();
            }

            if (menuSelection == 4)
            {
                RequestTransfer();
            }

            if (menuSelection == 5)
            {
                tenmoApiService.Logout();
                console.PrintSuccess("You are now logged out");

            }

            return true;    // Keep the main menu loop going
        }

        private void Login()
        {
            LoginUser loginUser = console.PromptForLogin();
            if (loginUser == null)
            {
                return;
            }

            try
            {
                ApiUser user = tenmoApiService.Login(loginUser);
                if (user == null)
                {
                    console.PrintError("Login failed.");
                }
                else
                {
                    console.PrintSuccess("You are now logged in");
                }
            }
            catch (Exception)
            {
                console.PrintError("Login failed.");
            }
            console.Pause();
        }

        private void Register()
        {
            LoginUser registerUser = console.PromptForLogin();
            if (registerUser == null)
            {
                return;
            }
            try
            {
                bool isRegistered = tenmoApiService.Register(registerUser);
                if (isRegistered)
                {
                    console.PrintSuccess("Registration was successful. Please log in.");
                }
                else
                {
                    console.PrintError("Registration was unsuccessful.");
                }
            }
            catch (Exception)
            {
                console.PrintError("Registration was unsuccessful.");
            }
            console.Pause();
        }
        private void ViewAllTransfers()
        {
            Console.Clear();
            Account account = tenmoApiService.GetAccountByUserId(tenmoApiService.UserId);
            List<Transfer> transfers = tenmoApiService.ListTransfers(account.AccountId);
            console.PrintListTransfers(transfers);
            int transferSelection = console.PromptForInteger("Please enter the ID of the transfer you want to view", 3001, int.MaxValue);
            foreach (Transfer xfer in transfers)
            {
                if (transferSelection == xfer.TransferId)
                {

                    Transfer transfer = tenmoApiService.GetTransferById(transferSelection);
                    console.PrintTransfer(transfer);
                    break;

                }
            }
            console.Pause();
        }
        private void GetBalance()
        {
            decimal balance = tenmoApiService.GetBalanceByAccountId(tenmoApiService.UserId);
            console.PrintBalance(balance);
        }
        private void GetPendingTransfers()
        {
            Console.Clear();
            Account accessAccount = tenmoApiService.GetAccountByUserId(tenmoApiService.UserId);
            List<Transfer> transfers = tenmoApiService.GetPendingTransfers(accessAccount.AccountId);
            console.PrintPendingTransfers(transfers);
            int pendingChoice = console.PromptForInteger("Please enter the ID of the Transfer you would like to approve or deny. Press 0 to exit", 0, int.MaxValue);
            while (pendingChoice != 0)
            {
                console.ApproveOrDenyMenu();
                decimal balance = tenmoApiService.GetBalanceByAccountId(tenmoApiService.UserId);
                console.PrintBalance(balance);
                int approveDeny = console.PromptForInteger("Enter 1 to approve 2 to deny or 0 to cancel", 0, 2);
                
                
                while (approveDeny != 0)
                {
                    if (approveDeny == 1)
                    {
                        Transfer transferToApprove = tenmoApiService.GetTransferById(pendingChoice);
                        Transfer approved = tenmoApiService.ApproveTransfer(transferToApprove);
                        Console.WriteLine("Transfer Successfully Approved.");
                        console.Pause();
                        break;
                    }
                    else if (approveDeny == 2)
                    {
                        Transfer transferToDeny = tenmoApiService.GetTransferById(pendingChoice);
                        Transfer Denied = tenmoApiService.DenyTransfer(transferToDeny);
                        Console.WriteLine("Transfer Succesfully Denied");
                        console.Pause();
                        break;
                    }
                }
                break;
            }
        }

        private void SendTransfer()
        {
            Console.Clear();
            List<User> users = tenmoApiService.GetUsers();
            console.PrintUsers(users);
            Transfer transfer = console.PromptForTransferData(tenmoApiService.UserId);
            if(transfer == null)
            {
                return;
            }
            Console.WriteLine("Sending Transfer...");
            bool successful = tenmoApiService.SendTransfer(transfer);
            if (successful == true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Transfer Successful!");


                Console.ForegroundColor = ConsoleColor.White;
                console.Pause();
                return;
            }
            else if (successful != true)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Transfer unsuccessful.");
                Console.ForegroundColor = ConsoleColor.White;
                console.Pause();
                return;


            }


        }
        private void RequestTransfer()
        {
            Console.Clear();
            List<User> users = tenmoApiService.GetUsers();
            console.PrintUsers(users);
            Transfer transfer = console.PromptForTransferRequestData(tenmoApiService.UserId);
            
            if(transfer == null)
            {
                return;
            }
            bool successful = tenmoApiService.RequestTransfer(transfer);
            if (successful == true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Transfer Successful!");


                Console.ForegroundColor = ConsoleColor.White;
                console.Pause();
                return;
            }
            else if (successful != true)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Transfer unsuccessful.");
                Console.ForegroundColor = ConsoleColor.White;
                console.Pause();
                return;
            }
        }
    }
}
