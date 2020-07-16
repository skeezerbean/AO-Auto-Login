using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace AO_Auto_Login
{
	public class MainPageViewModel : Screen
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		// Flags for setting window position
		const uint SWP_NOSIZE = 0x0001;
		const uint SWP_NOZORDER = 0x0004;

		// IDialogCoordinator is for metro message boxes
		private readonly IDialogCoordinator _dialogCoordinator;

		public ICommand ActivateLaunchCommand { get; set; }
		public ICommand DeleteAccountCommand { get; set; }

		public string AOFolder { get { return GeneralSettingsManager.GeneralSettings.AOFolder; } set { GeneralSettingsManager.GeneralSettings.AOFolder = value; } }
		public int LaunchDelayTime { get { return GeneralSettingsManager.GeneralSettings.LaunchDelayTime; } set { GeneralSettingsManager.GeneralSettings.LaunchDelayTime = value; } }
		public int KeyPressDelayTime { get { return GeneralSettingsManager.GeneralSettings.KeyPressDelayTime; } set { GeneralSettingsManager.GeneralSettings.KeyPressDelayTime = value; } }
		public bool MultipleEnters { get { return GeneralSettingsManager.GeneralSettings.MultipleEnters; } set { GeneralSettingsManager.GeneralSettings.MultipleEnters = value; } }
		public bool SetAOWindowPosition { get { return GeneralSettingsManager.GeneralSettings.SetAOWindowPosition; } set { GeneralSettingsManager.GeneralSettings.SetAOWindowPosition = value; } }
		public double AOWindowTop { get { return GeneralSettingsManager.GeneralSettings.AOWindowTop; } set { GeneralSettingsManager.GeneralSettings.AOWindowTop = value; } }
		public double AOWindowLeft { get { return GeneralSettingsManager.GeneralSettings.AOWindowLeft; } set { GeneralSettingsManager.GeneralSettings.AOWindowLeft = value; } }
		public static BindableCollection<AccountModel> AllAccounts { get; set; } = new BindableCollection<AccountModel>();
		public static BindableCollection<AccountModel> SelectedAccounts { get; set; } = new BindableCollection<AccountModel>();
		public BindableCollection<AccountModel> ListedAccounts { get { return AllAccounts; } set { ListedAccounts = value; } }
		public static string AccountsXMLPath = "AccountData.xml";
		private InputSimulator sim = new InputSimulator();

		public MainPageViewModel(IDialogCoordinator instance)
		{
			LoadAccounts();
			_dialogCoordinator = instance;
			ActivateLaunchCommand = (ICommand)new RelayParameterizedCommand(async (parameter) => await ActivateLaunch(parameter));
			DeleteAccountCommand = (ICommand)new RelayParameterizedCommand(async (parameter) => await DeleteAccount(parameter));
		}

		public async Task ActivateLaunch(object parameter)
		{
			string path = AOFolder + "\\anarchy.exe";

			// Make sure the AO folder is legit before anything else
			if (!File.Exists(path))
			{
				MessageBox.Show("AO Folder Path does not contain anarchy.exe");
				return;
			}

			// parameter is SelectedItems coming from the multi-select ListBox. Comes as IList, 
			// from CommandParameter in button Command, this casts it into CharacterModel
			System.Collections.IList items = (System.Collections.IList)parameter;

			// fresh selection needed
			SelectedAccounts.Clear();

			var collection = items.Cast<AccountModel>();
			foreach (AccountModel aModel in collection)
				SelectedAccounts.Add(aModel);

			// Did we actually select anything??
			if (SelectedAccounts.Count == 0)
			{
				MessageBox.Show("You need to select at least 1 account");
				return;
			}

			foreach (var item in SelectedAccounts)
			{
				// Launch each process
				using (Process myProcess = new Process())
				{
					myProcess.StartInfo.FileName = AOFolder + "\\anarchyonline.exe";
					myProcess.StartInfo.WorkingDirectory = AOFolder;
					myProcess.StartInfo.UseShellExecute = false;
					myProcess.StartInfo.RedirectStandardInput = true;
					myProcess.StartInfo.Arguments = "IA700453413 IP7505 DU";
					myProcess.Start();

					// Wait for process to start up
					await Task.Delay(LaunchDelayTime);

					// If we are setting window position, grab the window handle of this process and adjust
					if (SetAOWindowPosition)
						if (myProcess.MainWindowHandle != IntPtr.Zero)
							SetWindowPos(myProcess.MainWindowHandle, IntPtr.Zero, (int)AOWindowLeft, (int)AOWindowTop, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

					// Process launched, send input					
					sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
					await Task.Delay(KeyPressDelayTime);
					sim.Keyboard.KeyPress(VirtualKeyCode.HOME);
					await Task.Delay(KeyPressDelayTime);

					// delete existing account name entry
					for (int i = 0; i < 50; i++)
					{
						sim.Keyboard.KeyPress(VirtualKeyCode.DELETE);
						await Task.Delay(1);
					}

					await Task.Delay(KeyPressDelayTime);
					sim.Keyboard.TextEntry(item.AccountName);
					await Task.Delay(KeyPressDelayTime);
					sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
					await Task.Delay(KeyPressDelayTime * 2);  // extra wait before password
					sim.Keyboard.TextEntry(item.AccountPassword);
					await Task.Delay(KeyPressDelayTime);
					sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

					if (MultipleEnters)
					{
						// Wait on AO login and character selection screen
						await Task.Delay(1000);
						sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
					}

					// final wait before processing next one
					await Task.Delay(500);
				}
			}
		}

		public static void SaveAccounts()
		{
			XMLManager.SaveToXML<BindableCollection<AccountModel>>(AccountsXMLPath, AllAccounts);
			GeneralSettingsManager.SaveGeneralSettings();
		}

		public static void LoadAccounts()
		{
			// Pull in our characters and sort the list
			var temp = XMLManager.LoadFromXML<BindableCollection<AccountModel>>(AccountsXMLPath);
			if (temp == null)
				AllAccounts = new BindableCollection<AccountModel>();
			else
			{
				var tempAllAccounts = temp.ToList();
				tempAllAccounts.Sort((x, y) => string.Compare(x.AccountName, y.AccountName));
				AllAccounts = new BindableCollection<AccountModel>(tempAllAccounts);
			}
		}

		public async Task DeleteAccount(object parameter)
		{
			if (parameter == null)
				return;

			// parameter is SelectedItems coming from the multi-select ListBox. Comes as IList, 
			// from CommandParameter in button Command, this casts it into CharacterModel
			System.Collections.IList items = (System.Collections.IList)parameter;
			var collection = items.Cast<AccountModel>();

			// Buddy the Elf: I love deleting! Deleting's my favorite!
			// Setting .ToList eliminated exception issue
			foreach (var item in collection.ToList())
			{
				AllAccounts.Remove(item);
				await Task.Delay(1);
			}
		}

		public async void AddAccount()
		{
			string resultAccount;
			string resultPassword;
			bool duplicates = false;
			AccountModel account = new AccountModel();

			resultAccount = await _dialogCoordinator.ShowInputAsync(this, "Account", "Enter Account Name");

			// If they hit cancel
			if (resultAccount == null)
				return;

			// Check for duplicates
			foreach (var item in AllAccounts)
				if (resultAccount == item.AccountName) { duplicates = true; }

			if (duplicates)
			{
				MessageBox.Show("Account already exists in list");
				return;
			}

			resultPassword = await _dialogCoordinator.ShowInputAsync(this, "Account", "Enter Account Password");

			// If they hit cancel
			if (resultPassword == null)
				return;

			// take input retrieved, put into a new entry
			account.AccountName = resultAccount;
			account.AccountPassword = resultPassword;
			AllAccounts.Add(account);
		}

		// Get the AO scripts folder
		public void BrowseAOFolder()
		{
			const string baseFolder = @"C:\";
			try
			{
				VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
				dialog.Description = "Please select a folder.";
				dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
				dialog.SelectedPath = baseFolder; // place to start search				
				if ((bool)dialog.ShowDialog())
					AOFolder = dialog.SelectedPath;
			}
			catch { } // who cares
		}
	}
}
