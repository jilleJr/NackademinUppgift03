﻿using System;
using System.Globalization;
using BankApp.BankObjects;
using BankApp.Exceptions;
using BankApp.UI.Elements;

namespace BankApp.UI.Menus
{
	public class MenuAccountTransfer : IMenuItem
	{
		public string Title { get; } = "Transfer money";
		public bool Done => ErrorMessage == null;
		public string ErrorMessage { get; set; }

		private readonly Database db;
		private readonly Account accountSource;
		private readonly Account accountTarget;

		private readonly InputGroup inputGroup;

		private readonly TextField elementInput = new TextField("Amount") {Validator = TextField.ValidatorNumber};
		private readonly Button elementOK = new Button("Transfer");
		private readonly Button elementCancel = new Button("Cancel");

		public MenuAccountTransfer(Database db, Account accountSource, Account accountTarget)
		{
			this.db = db;
			this.accountSource = accountSource;
			this.accountTarget = accountTarget;

			inputGroup = new InputGroup(
				elementInput,
				elementOK,
				elementCancel
			);
		}

		public void Run()
		{
			if (ErrorMessage != null)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ErrorMessage);
				Console.WriteLine();
				ErrorMessage = null;
			}

			accountSource.PrintProfile(db, "Sending account");
			Console.WriteLine();
			accountTarget.PrintProfile(db, "Receiving account");
			Console.WriteLine();

			UIUtilities.PrintHeader("Transfer money");
			inputGroup.Run();
			Element selected = inputGroup.Selected;

			if (selected == elementOK)
			{
				string result = elementInput.TrimmedResult.Replace(',', '.');
				if (decimal.TryParse(result, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
					CultureInfo.InvariantCulture, out decimal amount))
				{
					try
					{
						accountSource.TransferMoney(amount, accountTarget, db);

						UIUtilities.PromptSuccess($"Successfully transferred {amount:C}");
					}
					catch (AccountTransferException e)
					{
						ErrorMessage = e.Message;
					}
				}
				else
				{
					ErrorMessage = "Unable to parse number! Please check your input.";
				}
			}
		}
	}
}