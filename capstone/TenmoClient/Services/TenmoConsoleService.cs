using System;
using System.Collections.Generic;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TenmoConsoleService : ConsoleService
    {
        /************************************************************
            Print methods
        ************************************************************/
        public void PrintLoginMenu()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("Welcome to TEnmo!");
            Console.WriteLine("1: Login");
            Console.WriteLine("2: Register");
            Console.WriteLine("0: Exit");
            Console.WriteLine("---------");
        }

        public void PrintMainMenu(string username)
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine($"Hello, {username}!");
            Console.WriteLine("1: View your past transfers");
            Console.WriteLine("2: View your pending requests");
            Console.WriteLine("3: Send TE bucks");
            Console.WriteLine("4: Request TE bucks");
            Console.WriteLine("5: Log out");
            Console.WriteLine("0: Exit");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("---------");
        }
        public LoginUser PromptForLogin()
        {
            string username = PromptForString("User name");
            if (String.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            string password = PromptForHiddenString("Password");

            LoginUser loginUser = new LoginUser
            {
                Username = username,
                Password = password
            };
            return loginUser;
        }

        // Add application-specific UI methods here...
        public void PrintListTransfers(List<Transfer> listOfTransfers)
        {

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Transfers");
            Console.WriteLine("ID          From/To                 Amount");
            Console.WriteLine("-------------------------------------------");
            foreach (Transfer transfer in listOfTransfers)
            {
                int numberOfSpacesFrom = 18;
                int numberOfSpacesTo = 20;
                Console.Write($"{transfer.TransferId}");
                if (transfer.FromUser == null)
                {

                    Console.Write($"          To: {transfer.ToUser}");

                    numberOfSpacesTo = numberOfSpacesTo - transfer.ToUser.Length;
                    for (int i = 1; i <= numberOfSpacesTo; i++)
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine($"$ {transfer.Amount.ToString("c")} ");
                }
                else if (transfer.ToUser == null)
                {
                    Console.Write($"          From: {transfer.FromUser}");
                    numberOfSpacesFrom = numberOfSpacesFrom - transfer.FromUser.Length;
                    for (int i = 1; i <= numberOfSpacesFrom; i++)
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine($"$ {transfer.Amount.ToString("c") } ");
                }


            }

        }
        public void PrintBalance(decimal balance)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"      Your Balance: {balance.ToString("c")}         ");
            Console.WriteLine("-----------------------------------");
        }

        public void PrintTransfer(Transfer transfer)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Transfer Details");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"Id: {transfer.TransferId}");
            Console.WriteLine($"From: {transfer.FromUser}");
            Console.WriteLine($"To: {transfer.ToUser}");
            Console.WriteLine($"Type: {transfer.Type.TransferTypeDescription}");
            Console.WriteLine($"Status: {transfer.Status.TransferStatusDescription}");
            Console.WriteLine($"Amount: {transfer.Amount.ToString("c")}");
        }
        public void PrintPendingTransfers(List<Transfer> pendingTransfers)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Pending Transfers");
            Console.WriteLine("ID          To                            Amount");
            Console.WriteLine("--------------------------------------------------");
            foreach (Transfer transfer in pendingTransfers)
            {

                int numberOfSpacesTo = 27 - transfer.ToUser.Length;
                Console.Write($"{transfer.TransferId}");


                Console.Write($"          To: {transfer.ToUser}");

                for (int i = 1; i <= numberOfSpacesTo; i++)
                {
                    Console.Write(" ");
                }

                Console.WriteLine($"$ {transfer.Amount.ToString("c")} ");

            }

        }
        public void ApproveOrDenyMenu()
        {
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Deny");
            Console.WriteLine("0: Cancel ");
            Console.WriteLine("-------------");
        }
        public void PrintUsers(List<User> users)
        {
            Console.WriteLine("|-------------- Users --------------|");
            Console.WriteLine("|    Id | Username                  |");
            Console.WriteLine("|-------+---------------------------|");

            foreach (User user in users)
            {
                int numberOfSpaces = 27 - (user.Username.Length);


                Console.Write($"| {user.UserId}  | ");
                Console.Write($"{user.Username}");
                for (int i = 1; i < numberOfSpaces; i++)
                {
                    Console.Write(" ");
                }
                //add spaces afer username
                Console.WriteLine("|");

            }
            Console.WriteLine("|-----------------------------------|");
        }
        public Transfer PromptForTransferData(int userid)
        {
            Transfer emptyTransfer = null;
            TenmoApiService tenmoApiService = new TenmoApiService("https://localhost:5001/");

            Account accountFrom = tenmoApiService.GetAccountByUserId(userid);
            PrintBalance(accountFrom.Balance);
            int userIdToSend = PromptForInteger("Please enter the User ID of the person you would like to send a transfer to or press 0 to cancel", 0, int.MaxValue);
            while (userIdToSend != 0)
            {
                Account accounttosend = tenmoApiService.GetAccountByUserId(userIdToSend);
                if (accounttosend == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid User ID");
                    Console.ForegroundColor = ConsoleColor.White;
                    Pause();
                    return emptyTransfer;
                }
                Transfer transfer = new Transfer();
                transfer.AccountToId = accounttosend.AccountId;
                transfer.AccountFromId = accountFrom.AccountId;

                decimal amountToSend = PromptForDecimal("Please enter the amount you would like to send");
                if (amountToSend < 0.01M)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You must send at least $0.01 TE bucks.");
                    Console.ResetColor();
                    Pause();
                    return emptyTransfer;
                }
                if (amountToSend > accountFrom.Balance)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You can not send more money than what you have in your balance.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Pause();
                    return emptyTransfer;
                }
                transfer.Amount = amountToSend;
                return transfer;
            }
            return emptyTransfer;


        }

        public Transfer PromptForTransferRequestData(int userid)
        {

            Transfer nullCheck = null;
            int userIdToSend = PromptForInteger("Please enter the UserId of the person you would like to request a transfer from or press 0 to cancel", 0, int.MaxValue);
            while (userIdToSend != 0)
            {

                TenmoApiService tenmoApiService = new TenmoApiService("https://localhost:5001/");
                Account accounttosend = tenmoApiService.GetAccountByUserId(userIdToSend); 
                if (accounttosend == null)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid User ID");
                    Console.ForegroundColor = ConsoleColor.White;
                    Pause();
                    return nullCheck;

                }
                Account accountFrom = tenmoApiService.GetAccountByUserId(userid);
                Transfer transfer = new Transfer();
                if (accounttosend != null)
                {
                    if (accounttosend.AccountId == accountFrom.AccountId)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You can not send money to yourself");
                        Console.ForegroundColor = ConsoleColor.White;
                        Pause();
                        return nullCheck;
                    }
                    transfer.AccountToId = accountFrom.AccountId;
                    transfer.AccountFromId = accounttosend.AccountId;
                    decimal amountToSend = PromptForDecimal("Please enter the amount you would like to request");
                    if(amountToSend < 0.01M)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You must request at least $0.01 TE bucks");
                        Console.ResetColor();
                        Pause();
                    }
                    transfer.Amount = amountToSend;
                    
                    return transfer;
                }
               


            }
            return nullCheck;

        }
    }
}
