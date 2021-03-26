using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace AO_Auto_Login
{
	/// <summary>
	/// Interaction logic for ManageServersView.xaml
	/// </summary>
	public partial class ManageServersView : UserControl
	{
		// Here we create the viewmodel with the current DialogCoordinator instance 
		ManageServersViewModel vm = new ManageServersViewModel(DialogCoordinator.Instance);

		public ManageServersView()
		{
			InitializeComponent();
			DataContext = vm;
		}
	}
}
