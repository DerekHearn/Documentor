using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Documentor
{
	class ServiceContract
	{
		public enum State
		{
			search,
			xml,
			web,
			service,
		}

		private SortedSet<string> _files = new SortedSet<string>();

		/// <summary>
		/// file path or directory path
		/// </summary>
		/// <param name="path"></param>
		public void setPath(string path)
		{
			getFiles(path);
		}

		/// <summary>
		/// get
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string[] getFiles(string path)
		{
			var list = new List<string>();
			var dirFiles = Directory.GetFiles(path, "*.cs");

			foreach (string file in dirFiles)
			{
				if (!file.Contains("Xml"))
					list.Add(file);
			}

			var ds = Directory.GetDirectories(path);

			foreach (string dir in ds)
			{
				var files = getFiles(dir);

				foreach (string file in files)
					list.Add(file);
			}

			foreach (string file in list)
				_files.Add(file);

			return list.ToArray();
		}

		public string[] toHTML()
		{
			var list = new List<string>();

			foreach (string file in _files)
			{
				var doc = parse(file);
				list.Add(doc.toHTML());
				var writePath = file.Replace(".cs", ".html");
				try
				{
					File.WriteAllText(writePath, doc.toHTML());
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			return list.ToArray();
		}

		private static void writeHTML(string path, Doc doc)
		{
			try
			{
				File.WriteAllText(path, doc.toHTML());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static Doc parse(string path)
		{
			var doc = new Doc();

			try
			{
				using (var r = File.OpenText(path))
				{
					IDocPart part = new Service();
					var state = State.search;

					while (!r.EndOfStream)
					{
						string line = r.ReadLine();

						if (state == State.search)
						{
							if (line.Contains("<summary>"))
							{
								part = new XMLComment();
								state = State.xml;
							}
							else if (line.Contains("[Web"))
							{
								part = new EndPoint();
								state = State.web;
							}
							else
								continue;
						}

						switch (state)
						{
							case State.xml:
								if (!String.IsNullOrWhiteSpace(line))
									part.add(line);
								if (line.Contains("</summary>"))
								{
									state = State.search;
									doc.add(part);
								}
								break;
							case State.web:

								if (!String.IsNullOrWhiteSpace(line))
									part.add(line);
								if (line.Contains("]"))
								{
									state = State.service;
									doc.add(part);
									part = new Service();
								}
								break;
							case State.service:
								if (!String.IsNullOrWhiteSpace(line))
								{
									part.add(line);
								}
								if (line.Contains(";"))
								{
									state = State.search;
									doc.add(part);
								}
								break;

							default:
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				//Console.WriteLine(e);
				throw e;
			}

			return doc;
		}

		class Doc
		{
			List<IDocPart> _items = new List<IDocPart>();

			public void add(IDocPart part)
			{
				_items.Add(part);
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				foreach (IDocPart dp in _items)
				{
					if (dp is XMLComment)
					{
						continue;
					}
					sb.Append(dp.toHTML());
					sb.Append("<br>");
					if (dp is Service)
					{
						sb.Append("<br>");
					}
				}
				return sb.ToString();
			}
		}

		class XMLComment : IDocPart
		{
			List<string> _items = new List<string>();

			public void add(string s)
			{
				_items.Add(s);
			}

			public string toString()
			{
				var sb = new StringBuilder();
				foreach (string s in _items)
				{
					sb.Append(s);
				}
				return sb.ToString();
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				foreach (string s in _items)
				{
					sb.Append(s);
					sb.Append("<br>");
				}
				return sb.ToString();
			}
		}

		class EndPoint : IDocPart
		{
			List<string> _items = new List<string>();

			public void add(string s)
			{
				_items.Add(s);
			}

			public string toString()
			{
				var sb = new StringBuilder();
				foreach (string s in _items)
				{
					sb.Append(s);
				}
				return sb.ToString();
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				var ts = this.toString();
				if (ts.Contains("WebMessageFormat.Json"))
				{
					sb.Append("Content-Type: application/json<br>");
				}

				if (ts.Contains("WebInvoke"))
				{
					sb.Append("POST<br>");
				}
				else if (ts.Contains("WebGet"))
				{
					sb.Append("GET<br>");
				}

				var i = ts.IndexOf("UriTemplate");
				var ps = ts.Substring(i);
				var pss = ps.Split('\"');
				if (pss.Length > 0)
				{
					sb.Append("Endpoint: .../");
					sb.Append(pss[1]);
				}

				return sb.ToString();
			}
		}

		class Service : IDocPart
		{
			List<string> _items = new List<string>();

			public void add(string s)
			{
				var htm = s.Replace("<", "&lt;").Replace(">", "&gt;");
				_items.Add(htm);
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				foreach (string s in _items)
				{
					sb.Append(s);
				}
				return sb.ToString();
			}
		}

		interface IDocPart
		{
			void add(string s);
			string toHTML();
		}
	}
}
