using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
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

		public BindableCollection<AccountModel> ServerAccounts { get { return (SelectedServer == null) ? new BindableCollection<AccountModel>() : SelectedServer.AccountList; } }
		public BindableCollection<ServerModel> ServerList { get { return GeneralSettingsManager.GeneralSettings.ServerList; } }
		public static BindableCollection<AccountModel> SelectedAccounts { get; set; } = new BindableCollection<AccountModel>();
		public int LaunchDelayTime { get { return GeneralSettingsManager.GeneralSettings.LaunchDelayTime; } set { GeneralSettingsManager.GeneralSettings.LaunchDelayTime = value; } }
		public int KeyPressDelayTime { get { return GeneralSettingsManager.GeneralSettings.KeyPressDelayTime; } set { GeneralSettingsManager.GeneralSettings.KeyPressDelayTime = value; } }
		public bool MultipleEnters { get { return GeneralSettingsManager.GeneralSettings.MultipleEnters; } set { GeneralSettingsManager.GeneralSettings.MultipleEnters = value; } }
		public bool SetAOWindowPosition { get { return GeneralSettingsManager.GeneralSettings.SetAOWindowPosition; } set { GeneralSettingsManager.GeneralSettings.SetAOWindowPosition = value; } }
		public double AOWindowTop { get { return GeneralSettingsManager.GeneralSettings.AOWindowTop; } set { GeneralSettingsManager.GeneralSettings.AOWindowTop = value; } }
		public double AOWindowLeft { get { return GeneralSettingsManager.GeneralSettings.AOWindowLeft; } set { GeneralSettingsManager.GeneralSettings.AOWindowLeft = value; } }
		public ServerModel SelectedServer { get { return GeneralSettingsManager.GeneralSettings.SelectedServer; } set { GeneralSettingsManager.GeneralSettings.SelectedServer = value; } }
		public BindableCollection<AccountModel> ListedAccounts { get { return ServerAccounts; } set { ListedAccounts = value; } }
		private InputSimulator sim = new InputSimulator();

		public MainPageViewModel(IDialogCoordinator instance)
		{
			_dialogCoordinator = instance;
			ActivateLaunchCommand = (ICommand)new RelayParameterizedCommand(async (parameter) => await ActivateLaunch(parameter));
		}

		public async Task ActivateLaunch(object parameter)
		{
			string path = SelectedServer.ClientPath + "\\anarchyonline.exe";

			// Make sure the AO folder is legit before anything else
			if (!File.Exists(path))
			{
				MessageBox.Show("AO Folder Path does not contain anarchyonline.exe");
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
					myProcess.StartInfo.FileName = path;
					myProcess.StartInfo.WorkingDirectory = SelectedServer.ClientPath;
					myProcess.StartInfo.UseShellExecute = false;
					myProcess.StartInfo.RedirectStandardInput = true;
					myProcess.StartInfo.Arguments = SelectedServer.LaunchArguments;
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
					for (int i = 0; i < 30; i++)
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
	}
}
