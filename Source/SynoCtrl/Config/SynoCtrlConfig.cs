﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tommy;

namespace SynoCtrl.Config
{
	public class SynoCtrlConfig
	{
		public SingleDeviceConfig Selected;

		private SingleDeviceConfig _default;
		private List<SingleDeviceConfig> _configs;

		public static SynoCtrlConfig Load(string filename)
		{
			string content;
			try
			{
				content = File.ReadAllText(filename);
			}
			catch (IOException e)
			{
				throw new SynoCtrlConfigParseException("Could not load file: " + e.Message);
			}

			return Parse(content);
		}

		public static SynoCtrlConfig Parse(string content)
		{
			using(var reader = new StringReader(content))
			{
				TomlTable data;

				try
				{
					data = TOML.Parse(reader);
				}
				catch (TomlParseException e)
				{
					throw new SynoCtrlConfigParseException("Error in Toml Syntax: " + e.Message);
				}

				string defaultName = null;
				List<SingleDeviceConfig> devices = new List<SingleDeviceConfig>();

				if (data.HasKey("Default"))
				{
					if (data["Default"] is TomlString s)
					{
						if (!string.IsNullOrWhiteSpace(s)) defaultName = s;
					}
					else
					{
						throw new SynoCtrlConfigParseException("Key [Default] must be a string");
					}
				}

				if (data["Device"] is TomlArray a)
				{
					foreach (TomlNode n in a)
					{
						if (n is TomlTable t)
						{
							string cfgName = null;
							if (!t.HasKey("Name")) throw new SynoCtrlConfigParseException("Missing key [Name]");
							if (t["Name"] is TomlString vName) cfgName = vName;

							var cfg = new SingleDeviceConfig(cfgName, false);

							if (t.HasKey("IP")      ) { if (t["IP"]       is TomlString vIP)       cfg.IPAddress  = vIP.Value.Trim();       else throw new SynoCtrlConfigParseException("Key [IP] must be a string"); }
							if (t.HasKey("Mac")     ) { if (t["Mac"]      is TomlString vMac)      cfg.MACAddress = vMac.Value.Trim();      else throw new SynoCtrlConfigParseException("Key [Mac] must be a string"); }
							if (t.HasKey("Username")) { if (t["Username"] is TomlString vUsername) cfg.Username   = vUsername.Value.Trim(); else throw new SynoCtrlConfigParseException("Key [Username] must be a string"); }
							if (t.HasKey("Password")) { if (t["Password"] is TomlString vPassword) cfg.Password   = vPassword.Value.Trim(); else throw new SynoCtrlConfigParseException("Key [Password] must be a string"); }

							devices.Add(cfg);
						}
					}
				}

				var result = new SynoCtrlConfig { _configs = devices };
				if (defaultName != null)
				{
					result._default = result._configs.FirstOrDefault(c => string.Equals(c.Name, defaultName, StringComparison.CurrentCultureIgnoreCase));
					if (result._default == null) throw new SynoCtrlConfigParseException($"Device '{defaultName}' not found");
				} 
				else
				{
					result._default = new SingleDeviceConfig("%%AutoGenerated%%", true);
					result._configs.Add(result._default);
				}

				if (SynoCtrlProgram.Arguments["<name>"] != null)
				{
					var selName = $"{SynoCtrlProgram.Arguments["<name>"].Value}";
					result.Selected = result._configs.FirstOrDefault(c => string.Equals(c.Name, selName, StringComparison.CurrentCultureIgnoreCase));
					if (result._default == null) throw new SynoCtrlConfigParseException($"Device '{selName}' not found");
				}
				else
				{
					result.Selected = result._default;
				}

				return result;
			}
		}

		public static SynoCtrlConfig CreateEmpty()
		{
			var result = new SynoCtrlConfig();
			result._configs = new List<SingleDeviceConfig>();
			result._default = new SingleDeviceConfig("%%AutoGenerated%%", true);
			result._configs.Add(result._default);
			result.Selected = result._default;
			return result;
		}
	}
}
