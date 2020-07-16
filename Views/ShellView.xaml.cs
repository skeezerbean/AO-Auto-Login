using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AO_Auto_Login
{
	/// <summary>
	/// Interaction logic for ShellView.xaml
	/// </summary>
	public partial class ShellView : MetroWindow
	{
		// Here we create the viewmodel with the current DialogCoordinator instance 
		ShellViewModel vm = new ShellViewModel();

		public ShellView()
		{
			InitializeComponent();
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
		}
	}
}
