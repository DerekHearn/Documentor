using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Documentor
{
	class Program
	{
		static readonly string[] CMD_ARG_KEYS = { "/help", "/service", "/datacontract" };

		static void Main(string[] args)
		{
			if (args.Length > 0 && args[0].Contains("/h"))
			{
				Console.WriteLine("/service path to folder containing service files \n"
					+ "/datacontract path to folder containing data contract files");
				return;
			}

			Dictionary<string, string> clArgs = appStartup(args);

			foreach (string key in clArgs.Keys)
			{
				Console.WriteLine(key + " " + clArgs[key]);
			}

			//"/p" path
			if (clArgs.ContainsKey(CMD_ARG_KEYS[1]))
			{
				var sc = new ServiceContract();

				var path = clArgs[CMD_ARG_KEYS[1]];
				sc.setPath(path);
				var htmls = sc.toHTML();
				var wp = path + "/AllServices.html";
				File.WriteAllLines(wp, htmls);
			}


			if (clArgs.ContainsKey(CMD_ARG_KEYS[2]))
			{
				string dcp = clArgs[CMD_ARG_KEYS[2]];

				var dc = new DataContract();
				dc.setPath(dcp);
				var htmls = dc.toHTML();
				var wp = dcp + "/ContractDocument.html";

				File.WriteAllLines(wp, htmls);
			}

			Console.WriteLine("Finished");
		}

		static Dictionary<string, string> appStartup(string[] args)
		{
			var clArgs = new Dictionary<string, string>();
			// Don't bother if no command line args were passed
			// NOTE: e.Args is never null - if no command line args were passed, 
			//       the length of e.Args is .0
			if (args.Length == 0) 
				return clArgs;

			var flag = false;
			var key = "";
			for (int i = 0; i < args.Length; i++)
			{
				if (flag)
				{
					clArgs.Add(key, args[i]);
					flag = !flag;
				}
				else
				{
					key = args[i];
					flag = !flag;
				}
			}

			return clArgs;
		}

	}
}
