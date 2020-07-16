using System;
using System.Windows.Input;

namespace AO_Auto_Login
{
	public class RelayParameterizedCommand : ICommand
	{
		private Action<object> mAction;

		// event fired when <see cref"CanExecute(object)"/> value has changed
		public event EventHandler CanExecuteChanged = (sender, e) => { };

		// relay command can always execute
		public bool CanExecute(object parameter) { return true; }

		public RelayParameterizedCommand(Action<object> action)
		{
			mAction = action;
		}

		public void Execute(object parameter)
		{
			mAction(parameter); // run the ICommand action that was Relay'd in
		}
	}
}
