using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace AO_Auto_Login
{
	/// <summary>
	/// Interaction logic for MainPageView.xaml
	/// </summary>
	public partial class MainPageView : UserControl
	{
		// Here we create the viewmodel with the current DialogCoordinator instance 
		MainPageViewModel vm = new MainPageViewModel(DialogCoordinator.Instance);

		public MainPageView()
		{
			InitializeComponent();
			DataContext = vm;
		}
	}
}
