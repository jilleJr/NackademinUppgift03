﻿using System;
using System.Collections.Generic;
using BankApp.BankObjects;
using BankApp.UI.Elements;

namespace BankApp.UI.Menus
{
	public class MenuCustomerPage : IMenuItem
	{
		public string Title { get; } = "Customer page";
		public bool Done { get; private set; }
		public string ErrorMessage { get; set; }

		private readonly Database db;
		private readonly Customer customer;
		private readonly InputGroup inputGroup;

		private AccountButton[] elementsAccounts;
		private readonly Button elementEditCustomer = new Button("Edit customer") {PaddingAbove = true};
		private readonly Button elementNewAccount = new Button("Open new account") {PaddingAbove = false};
		private readonly Button elementRemoveCustomer = new Button("Remove customer") {PaddingAbove = false};
		private readonly Button elementBack = new Button("Back to main menu");

		public MenuCustomerPage(Customer customer, Database db)
		{
			this.customer = customer;
			this.db = db;

			inputGroup = new InputGroup();
		}

		private void ResetAccountButtons()
		{
			inputGroup.RemoveAll();

			List<Account> accounts = customer.FetchAccounts(db);
			elementsAccounts = new AccountButton[accounts.Count];

			for (int i = 0; i < accounts.Count; i++)
			{
				elementsAccounts[i] = new AccountButton(accounts[i]) {PaddingAbove = false};
				inputGroup.AddElement(elementsAccounts[i]);
			}

			inputGroup.AddElement(elementEditCustomer);
			inputGroup.AddElement(elementNewAccount);
			inputGroup.AddElement(elementRemoveCustomer);
			inputGroup.AddElement(elementBack);
		}

		public void Run()
		{
			Done = false;

			if (ErrorMessage != null)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ErrorMessage);
				Console.WriteLine();
				ErrorMessage = null;
			}

			customer.PrintProfile(db);
			Console.WriteLine();

			ResetAccountButtons();

			inputGroup.Run();

			Element selected = inputGroup.Selected;

			if (selected is AccountButton accountButton)
			{
				// Open account
				MenuMain.RunMenuItem(new MenuAccountPage(accountButton.account, db));
			}
			else if (selected == elementEditCustomer)
			{
				// Edit customer
				MenuMain.RunMenuItem(new MenuCustomerCreateEdit(db, customer));
			}
			else if (selected == elementNewAccount)
			{
				// Create account
				MenuMain.RunMenuItem(new MenuAccountCreateEdit(db, customer));
			}
			else if (selected == elementRemoveCustomer)
			{
				// Remove customer
				RemoveCustomer();
			}
			else if (selected == elementBack)
			{
				// Back
				Done = true;
			}
		}

		private void RemoveCustomer()
		{
			const string title = "This is a permanent action. Are you sure?";
			const string yes = "Yes, remove the customer";
			const string no = "No, cancel";

			if (UIUtilities.PromptWarning(title, yes, no) == yes)
			{
				try
				{
					db.RemoveCustomer(customer);
					Done = true;
				}
				catch (ApplicationException e)
				{
					ErrorMessage = e.Message;
				}
			}
		}

		private class AccountButton : Button
		{
			public readonly Account account;

			public AccountButton(Account account) : base("")
			{
				this.account = account;
			}

			protected override void OnDraw()
			{
				if (Selected)
				{
					Console.BackgroundColor = ConsoleColor.DarkCyan;
					Console.ForegroundColor = ConsoleColor.White;

					Write("[ ");
					WriteAccount();
					Write(" ]");
				}
				else
				{
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Cyan;

					Write("< ");
					WriteAccount();
					Write(" >");
				}
			}

			private void WriteAccount()
			{
				ConsoleColor fg = Console.ForegroundColor;

				Write("Account ");
				Console.ForegroundColor = Selected ? ConsoleColor.Black : ConsoleColor.DarkYellow;
				Write(account.ID);
				Console.ForegroundColor = Selected ? ConsoleColor.White : ConsoleColor.Green;
				Write($" {account.Money:C}");

				Console.ForegroundColor = fg;
			}
		}
	}
}