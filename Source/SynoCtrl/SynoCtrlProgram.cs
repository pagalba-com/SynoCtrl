﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DocoptNet;
using SynoCtrl.Logger;
using SynoCtrl.Tasks.Impl;

namespace SynoCtrl
{
	public static class SynoCtrlProgram
	{
		public static readonly Version VERSION = GetInformationalVersion();

		public static IDictionary<string, ValueObject> Arguments;

		public static SLogger Logger;

		public static int Main(string[] args)
		{
			try
			{
				Arguments = new Docopt().Apply(Properties.Resources.cmd_schema, args, true, VERSION, false, true);

				Logger = new SLogger(Arguments["--silent"].IsTrue, Arguments["--verbose"].IsTrue);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error during Initialization:\n{e}");
				return -1;
			}

			try
			{
				if (Arguments["wol"].IsTrue)           return new WakeOnLANTask().Run();
				if (Arguments["getmac"].IsTrue)        return new GetMACTask().Run();
				if (Arguments["getip"].IsTrue)         return new GetIPTask().Run();
				if (Arguments["ping"].IsTrue)          return new PingTask().Run();
				if (Arguments["status"].IsTrue)        return new StatusTask().Run();
				if (Arguments["status-all"].IsTrue)    return new StatusTask().Run();
				if (Arguments["status-list"].IsTrue)        return new StatusListTask().Run();
				if (Arguments["status"].IsTrue)        return new StatusTask().Run();
				if (Arguments["shutdown"].IsTrue)      return new ShutdownTask().Run();
				if (Arguments["reboot"].IsTrue)        return new RebootTask().Run();
				if (Arguments["create-config"].IsTrue) return new ConfigExampleTask(Arguments["<output_filename>"].Value?.ToString()).Run();
			}
			catch (Exception e)
			{
				Logger.WriteError($"Internal error in program execution: {e.Message}", e);
				return -1;
			}

			Logger.WriteError("No subcommand specified");
			return -1;
		}

		private static Version GetInformationalVersion()
		{
			try
			{
				var assembly = Assembly.GetAssembly(typeof(SynoCtrlProgram));

				var loc = assembly.Location;
				var vi = FileVersionInfo.GetVersionInfo(loc);
				return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}
	}
}


//TODO List Shares (EP already in StatusAPIValues)
//TODO List Drives (EP already in StatusAPIValues)
//TODO Status Info about specific share (EP already in StatusAPIValues)