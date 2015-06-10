using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Documentor
{
	class Program
	{
		static readonly string SERVICE_ARG_KEY = "/service";
		static readonly string SERVICE_ARG_KEY_SHORT = "/s";
		static readonly string DATACONTRACT_ARG_KEY = "/datacontract";
		static readonly string DATACONTRACT_ARG_KEY_SHORT = "/d";
		static readonly string WRITEPATH_ARG_KEY = "/writeto";
		static readonly string WRITEPATH_ARG_KEY_SHORT = "/w";
		static readonly string HELP_ARG_KEY = "/help";
		static readonly string HELP_ARG_KEY_SHORT = "/h";

		static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();
			if (args.Length > 0 && 
				(args[0].Equals(HELP_ARG_KEY, StringComparison.InvariantCultureIgnoreCase)) || 
				 args[0].Equals(HELP_ARG_KEY_SHORT, StringComparison.InvariantCultureIgnoreCase))
			{
				Console.WriteLine("/service path to folder containing service files \n"
					+ "/datacontract path to folder containing data contract files");
				return;
			}

			var clArgs = appStartup(args);

			//"/p" path
			if (clArgs.serviceFiles.Length > 0)
			{
				var sc = new ServiceContract(clArgs);
				var htmls = sc.toHTML();
				var wp = clArgs.writePath + "/AllServices.html";
				File.WriteAllLines(wp, htmls);
			}


			if (clArgs.dataContractFiles.Length > 0)
			{
				var dc = new DataContract(clArgs);
				var htmls = dc.toHTML();
				var wp = clArgs.writePath + "/ContractDocument.html";

				File.WriteAllLines(wp, htmls);
			}

			
			var end = DateTime.UtcNow;
			sw.Stop();
			Console.WriteLine("Finished: " + sw.ElapsedMilliseconds + "ms");
		}

		static Args appStartup(string[] args)
		{
			// Don't bother if no command line args were passed
			// NOTE: e.Args is never null - if no command line args were passed, 
			//       the length of e.Args is .0
			var length = args.Length;
			if(length == 0)
				throw new ArgumentException("arguments required in order to function");
			var serviceFiles = new List<string>();
			var dataContractFiles = new List<string>();
			string writePath = null;

			var ignoreCase = StringComparison.InvariantCultureIgnoreCase;

			for(int i = 0; i+1 < length; i+=2)
			{
				if(args[i].Equals(SERVICE_ARG_KEY_SHORT, ignoreCase) ||
					args[i].Equals(SERVICE_ARG_KEY, ignoreCase))
					serviceFiles.Add(args[i+1]);
				else if(args[i].Equals(DATACONTRACT_ARG_KEY_SHORT, ignoreCase) ||
					args[i].Equals(DATACONTRACT_ARG_KEY, ignoreCase))
					dataContractFiles.Add(args[i+1]);
				else if ((args[i].Equals(WRITEPATH_ARG_KEY_SHORT, ignoreCase) ||
					args[i].Equals(WRITEPATH_ARG_KEY, ignoreCase)))
				{
					if (writePath != null)
						throw new ArgumentException("Multiple writepaths found. Only one allowed");
					writePath = args[i + 1];
				}
			}

			if(writePath == null)
				throw new ArgumentException("No write path found");

			return new Args(serviceFiles.ToArray(), dataContractFiles.ToArray(), writePath);
		}

		

	}
}
