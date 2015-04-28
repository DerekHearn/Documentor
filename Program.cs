using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;

namespace Documentor
{
	class Program
	{
		static readonly string[] CMD_ARG_KEYS = { "/help", "/service", "/datacontract" };

		static void Main(string[] args)
		{
			var connectionString = "server=172.30.3.186;"
			+ "database=MEETBALL_DITKA;"
			+ "user id=mb_sql_svc_dev;"
			+ "password=m33t8all!";

			var tbl_sql = @"SELECT *
					FROM  INFORMATION_SCHEMA.TABLES
					WHERE TABLE_TYPE='BASE TABLE'";

			var getTbl_sql = "SELECT * FROM ";

			//var sqlc = new SqlConnection(connectionString);
			var sql = "";
			using (SqlConnection sqlc = new SqlConnection(connectionString))
			{
				var result = new Dictionary<string, int>();

				//using (IDataReader rdr = GetReader())
				//{
				//	while (rdr.Read())
				//	{
				//		result[rdr[tableName + "Name"].ToString()] = (int)rdr[tableName + "ID"];
				//	}
				//}
				//return result;

				var cmd = new SqlCommand(tbl_sql, sqlc);
				
				sqlc.Open();
				var err = cmd.ExecuteReader(CommandBehavior.Default);
				Console.WriteLine(err);
			}

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
