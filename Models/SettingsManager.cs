using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace AO_Auto_Login
{
	/// <summary>
	/// This class handles the general settings, save/load XML
	/// </summary>
	public static class GeneralSettingsManager
	{
		public static GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
		public static string SettingsXMLPath = "Settings.xml";

		public static void LoadGeneralSettings()
		{
			GeneralSettings = XMLManager.LoadFromXML<GeneralSettings>(SettingsXMLPath);

			// If somehow settings retrieval failed, pop in default settings
			if (GeneralSettings == null)
				GeneralSettings = new GeneralSettings();
			GeneralSettings.WindowHeight = 450;
			GeneralSettings.WindowWidth = 800;
		}

		public static void SaveGeneralSettings()
		{
			XMLManager.SaveToXML<GeneralSettings>(SettingsXMLPath, GeneralSettings);
		}

		// In case the window was saved to a position no longer visible,
		// such as a second monitor disconnected
		public static void MoveIntoView()
		{
			GeneralSettingsManager.GeneralSettings.WindowWidth = Math.Min(GeneralSettingsManager.GeneralSettings.WindowWidth, SystemParameters.VirtualScreenWidth);
			GeneralSettingsManager.GeneralSettings.WindowHeight = Math.Min(GeneralSettingsManager.GeneralSettings.WindowHeight, SystemParameters.VirtualScreenHeight);
			GeneralSettingsManager.GeneralSettings.WindowLeft = Math.Min(Math.Max(GeneralSettingsManager.GeneralSettings.WindowLeft, SystemParameters.VirtualScreenLeft),
				SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - GeneralSettingsManager.GeneralSettings.WindowWidth);
			GeneralSettingsManager.GeneralSettings.WindowTop = Math.Min(Math.Max(GeneralSettingsManager.GeneralSettings.WindowTop, SystemParameters.VirtualScreenTop),
				SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - GeneralSettingsManager.GeneralSettings.WindowHeight);
		}
	}

	/// <summary>
	/// This class stores the overall general settings for the app
	/// </summary>
	public class GeneralSettings
	{
		public string AOFolder { get; set; } = string.Empty;
		public double WindowTop { get; set; } = 100;
		public double WindowLeft { get; set; } = 100;
		public double WindowHeight { get; set; }
		public double WindowWidth { get; set; }
		public int LaunchDelayTime { get; set; } = 2500;
		public int KeyPressDelayTime { get; set; } = 100;
		public bool MultipleEnters { get; set; } = false;
		public bool SetAOWindowPosition { get; set; } = false;
		public double AOWindowTop { get; set; } = 100;
		public double AOWindowLeft { get; set; } = 100;

		public GeneralSettings() { }
	}

	/// <summary>
	/// The AccountModel represents settings for a single account
	/// </summary>
	public class AccountModel
	{
		public string AccountName { get; set; } = "";
		public string AccountPassword { get; set; } = "";

		public AccountModel() { }
	}

	/// <summary>
	/// This class handles the generic load/save to XML
	/// </summary>
	public static class XMLManager
	{
		public static T LoadFromXML<T>(string filePath)
		{
			T loadResult;

			if (File.Exists(filePath))
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(T));
					StreamReader rd = new StreamReader(filePath);
					loadResult = (T)xs.Deserialize(rd);
					rd.Close();
					rd.Dispose();
				}
				catch (Exception e)
				{
					MessageBox.Show($"There was a problem loading the {filePath} XML file - {e.Message}");
					// Was a problem, kick out default values?
					return default;
				}

				return loadResult;
			}
			else	// file doesn't exist, kick out default values?
				return default;
		}

		public static T SaveToXML<T>(string filePath, T xmlObject)
		{
			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				TextWriter tw = new StreamWriter(filePath);
				xs.Serialize(tw, xmlObject);
				tw.Flush();
				tw.Close();
				tw.Dispose();
			}
			catch (Exception e)
			{
				MessageBox.Show($"There was a problem saving the {filePath} XML file - {e.Message}");
				return default; // Was a problem, kick out default values?
			}
			return default;
		}
	}
}
