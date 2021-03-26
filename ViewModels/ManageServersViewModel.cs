using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using System.IO;

namespace AO_Auto_Login
{
	public class ManageServersViewModel : Screen
	{
		// IDialogCoordinator is for metro message boxes
		private readonly IDialogCoordinator _dialogCoordinator;

		public BindableCollection<AccountModel> ServerAccounts { get { return (SelectedServer == null) ? new BindableCollection<AccountModel>() : SelectedServer.AccountList; } }
		public BindableCollection<ServerModel> ServerList { get { return GeneralSettingsManager.GeneralSettings.ServerList; } set { GeneralSettingsManager.GeneralSettings.ServerList = value; } }
		public static AccountModel SelectedAccount { get; set; }
		public ServerModel SelectedServer { get { return GeneralSettingsManager.GeneralSettings.SelectedServer; } set { GeneralSettingsManager.GeneralSettings.SelectedServer = value; } }
		public BindableCollection<AccountModel> AccountList { get { return (SelectedServer == null) ? new BindableCollection<AccountModel>() : ServerAccounts; } }
		public string SelectedServerFolderLocation { get { return (SelectedServer == null) ? string.Empty : SelectedServer.ClientPath; } set { if(SelectedServer != null) SelectedServer.ClientPath = value; } }
		public string SelectedServerLaunchArgs { get { return (SelectedServer == null) ? string.Empty : SelectedServer.LaunchArguments; } set { if(SelectedServer != null) SelectedServer.LaunchArguments = value; } }

		public ManageServersViewModel (IDialogCoordinator instance)
		{
			_dialogCoordinator = instance;
		}

		public async void RemoveAccount()
		{
			if (SelectedServer == null || SelectedAccount == null)
				return;

			string msg = $"⚠ ARE YOU SURE?? Deleting Entry: [{SelectedAccount.AccountName}]";

			// Prompt, verify to continue
			MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, "Confirm", msg, MessageDialogStyle.AffirmativeAndNegative);

			if (result == MessageDialogResult.Canceled || result == MessageDialogResult.Negative)
				return;

			AccountList.Remove(SelectedAccount);
		}

		public async void AddAccount()
		{
			if (SelectedServer == null)
				return;

			// Prompt for login
			string accountName = await _dialogCoordinator.ShowInputAsync(this, "Add Account", "Enter the Account Login Name");

			// Hit cancel/escape
			if (string.IsNullOrEmpty(accountName))
				return;

			// Prompt for login
			string accountPass = await _dialogCoordinator.ShowInputAsync(this, "Add Account", "Enter the Account Password");

			// Hit cancel/escape
			if (string.IsNullOrEmpty(accountPass))
				return;

			AccountModel account = new AccountModel();
			account.AccountName = accountName;
			account.AccountPassword = accountPass;
			ServerAccounts.Add(account);
		}

		public async void RemoveSelectedServer()
		{
			if (SelectedServer == null)
				return;

			string msg = $"⚠ ARE YOU SURE?? Deleting Entry: [{SelectedServer.Description}]";

			// Prompt, verify to continue
			MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, "Confirm", msg, MessageDialogStyle.AffirmativeAndNegative);

			if (result == MessageDialogResult.Canceled || result == MessageDialogResult.Negative)
				return;

			ServerList.Remove(SelectedServer);
		}

		public async void AddServer()
		{
			bool pathIsGood = false;
			string location = string.Empty;

			while (!pathIsGood)
			{
				// Browse for location
				location = BrowseFolder();

				// If it's empty, then it was cancelled
				if (string.IsNullOrEmpty(location))
					return;

				string path = location + "\\anarchyonline.exe";

				// Make sure the AO folder is legit before anything else
				if (!File.Exists(path))
					await _dialogCoordinator.ShowMessageAsync(this, "Alert", "AO Folder Path does not contain anarchyonline.exe");
				else
					pathIsGood = true;
			}

			// Prompt for Description
			string description = await _dialogCoordinator.ShowInputAsync(this, "Add Entry", "Enter the Decription");

			// Hit cancel/escape
			if (string.IsNullOrEmpty(description))
				return;

			// Prompt for command line args
			MetroDialogSettings dialogSettings = new MetroDialogSettings() { DefaultText = "IA700453413 IP7505 DU" };
			string args = await _dialogCoordinator.ShowInputAsync(this, "Add Entry", "Enter the command line arguments", dialogSettings);

			// Hit cancel/escape
			if (
				string.IsNullOrEmpty(args))
				return;

			// Add our entry to the list
			ServerModel entry = new ServerModel();
			entry.Description = description;
			entry.ClientPath = location;
			entry.LaunchArguments = args;
			entry.AccountList = new BindableCollection<AccountModel>();
			ServerList.Add(entry);
		}

		// Method to browse to a folder
		public string BrowseFolder()
		{
			const string baseFolder = @"C:\";
			string result = string.Empty;
			try
			{
				VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
				dialog.Description = "Please select a folder.";
				dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
				dialog.SelectedPath = baseFolder; // place to start search
				if ((bool)dialog.ShowDialog())
					result = dialog.SelectedPath;
			}
			catch { return string.Empty; }

			return result;
		}
	}
}
