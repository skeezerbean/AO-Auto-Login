using Caliburn.Micro;
using PropertyChanged;

namespace AO_Auto_Login
{
	// This fires off property event changes as needed
	[AddINotifyPropertyChangedInterface]
	// Conductor from Caliburn - enables 1 child in the view at a time
	public class BaseViewModel : Conductor<object>.Collection.OneActive
	{

	}
}
