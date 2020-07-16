using MahApps.Metro.Controls.Dialogs;

namespace AO_Auto_Login
{
	public class ShellViewModel : BaseViewModel
	{

		public double WindowTop { get { return GeneralSettingsManager.GeneralSettings.WindowTop; } set { GeneralSettingsManager.GeneralSettings.WindowTop = value; } }
		public double WindowLeft { get { return GeneralSettingsManager.GeneralSettings.WindowLeft; } set { GeneralSettingsManager.GeneralSettings.WindowLeft = value; } }
		public double WindowHeight { get { return GeneralSettingsManager.GeneralSettings.WindowHeight; } set { GeneralSettingsManager.GeneralSettings.WindowHeight = value; } }
		public double WindowWidth { get { return GeneralSettingsManager.GeneralSettings.WindowWidth; } set { GeneralSettingsManager.GeneralSettings.WindowWidth = value; } }
		public MainPageViewModel MainPageVM = new MainPageViewModel(DialogCoordinator.Instance);
		public string AppTitle { get; set; } = "AO Auto Login Launcher";

		public ShellViewModel()
		{
			LoadPageMain();
			GeneralSettingsManager.LoadGeneralSettings();

			// Pull in saved window position, adjust if saved settings
			// are no longer valid			
			GeneralSettingsManager.MoveIntoView();
			WindowTop = GeneralSettingsManager.GeneralSettings.WindowTop;
			WindowLeft = GeneralSettingsManager.GeneralSettings.WindowLeft;
		}

		public void LoadPageMain() { ActivateItem(MainPageVM); }
	}
}
