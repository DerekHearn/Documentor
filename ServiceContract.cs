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

		private Args _args;

		public ServiceContract(Args clArgs)
		{
			_args = clArgs;
			setPaths(clArgs.serviceFiles);
		}

		private SortedSet<string> _files = new SortedSet<string>();

		/// <summary>
		/// file path or directory path
		/// </summary>
		/// <param name="path"></param>
		private void setPaths(string[] paths)
		{
			foreach (string file in paths)
			{
				if (file.EndsWith(".cs"))
					_files.Add(file);
				else
					Console.WriteLine(file + " is not a C# file. It has been rejected");
			}
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

		private Doc parse(string file)
		{
			var doc = new Doc();

			try
			{
				using (var r = File.OpenText(file))
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
			private readonly string lineHeight = "125%";
			private readonly string fontFace = "courier";

			List<IDocPart> _items = new List<IDocPart>();

			public void add(IDocPart part)
			{
				_items.Add(part);
			}

			public string toHTML()
			{
				var sb = new StringBuilder();

				//general text formating
				sb.Append("<style>font.big {line-height:");
				sb.Append(lineHeight);
				sb.Append(";}</style>");
				sb.Append("<FONT FACE=\"");
				sb.Append(fontFace);
				sb.Append("\" color=\"white\" class=\"big\">");
				sb.Append("<style> body { background-color: #050A00; margin: 0 5% 0 5%;</style>");

				//header
				sb.Append("<center><hr><h2 style=\"color:white\"><span style=\"align:left;\">MeetBall App");
				sb.Append("</span><img src=\"BMB.png\" style=\"width:128px;height:128px;\" align=\"middle\">");
				sb.Append("<span style=\"align:left\">Web-Services</span></h2><hr></center><br>");

				foreach (IDocPart dp in _items)
				{
					if (dp is XMLComment)
					{
						sb.Append(dp.toHTML());
						continue;
					}
					sb.Append(dp.toHTML());
					sb.Append("<br>");
					if (dp is Service)
					{
						sb.Append("<br>");
					}
				}

				sb.Append("</FONT>");
				return sb.ToString();
			}
		}

		class XMLComment : IDocPart
		{
			private readonly string color = "#608B4E";//"#336600";

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
					var ts = s;
					ts = ts.Replace("<", "&lt;").Replace(">", "&gt;");
					sb.Append(String.Format("<span style=\"color:{0};\">", color));
					sb.Append(ts);
					sb.Append("</span>");
					sb.Append("<br>");
				}
				return sb.ToString();
			}
		}

		class EndPoint : IDocPart
		{
			private readonly string span = "<span style=\"color:{0};\">";
			private readonly string spanEnd = "</span>";

			private readonly string defaultColor = "#92CAF4";
			private readonly string GETPOSTColor = "#D85050";
			private readonly string endpointColor = "#C28566";

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
					sb.Append(String.Format(span, defaultColor));
					sb.Append("Content-Type: ");
					sb.Append(spanEnd);
					sb.Append(String.Format(span, endpointColor));
					sb.Append("application/json<br>");
					sb.Append(spanEnd);
				}

				if (ts.Contains("WebInvoke"))
				{
					sb.Append(String.Format(span, GETPOSTColor));
					sb.Append("POST<br>");
					sb.Append(spanEnd);
				}
				else if (ts.Contains("WebGet"))
				{
					sb.Append(String.Format(span, GETPOSTColor));
					sb.Append("GET<br>");
					sb.Append(spanEnd);
				}

				var i = ts.IndexOf("UriTemplate");
				var ps = ts.Substring(i);
				var pss = ps.Split('\"');
				if (pss.Length > 0)
				{
					sb.Append(String.Format(span, defaultColor));
					sb.Append("Endpoint: ");
					sb.Append(spanEnd);
					sb.Append(String.Format(span, endpointColor));
					sb.Append(".../");
					sb.Append(pss[1]);
					sb.Append(spanEnd);
				}
				sb.Append(spanEnd);

				return sb.ToString();
			}
		}

		class Service : IDocPart
		{
			private readonly string span = "<span style=\"color:{0};\">";
			private readonly string spanEnd = "</span>";

			private readonly string defaultColor = "white";
			private readonly string lookHere = "#92CAF4";
			private readonly string retColor = "#C28566";

			List<string> _items = new List<string>();

			string ret;

			string name;

			string[] parans;

			public void add(string s)
			{
				var htm = s.Replace("<", "&lt;").Replace(">", "&gt;");
				_items.Add(htm);
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				
				var sb2 = new StringBuilder();

				var sb3 = new StringBuilder();

				foreach (string s in _items)
				{
					sb2.Append(s.Replace("\t", ""));
				}

				var temp = sb2.ToString();
				var tempSplit = temp.Split(' ');
				
				//return value
				sb3.Append(String.Format(span, lookHere));
				sb3.Append("Return Value: ");
				sb3.Append(spanEnd);

				if (tempSplit[0].Contains("&lt;"))
				{
					var otherSplit = tempSplit[0].Replace("&lt;", "<")
						.Replace("&gt;", "<")
						.Split('<');
					sb3.Append(String.Format(span, retColor));
					sb3.Append(otherSplit[0]);
					sb3.Append(spanEnd);
					sb3.Append("&lt;");
					sb3.Append(String.Format(span, retColor));
					sb3.Append(otherSplit[1]);
					sb3.Append(spanEnd);
					sb3.Append("&gt;");
					
				}
				else
				{
					sb3.Append(String.Format(span, retColor));
					sb3.Append(tempSplit[0]);
					sb3.Append(spanEnd);
				}

				sb3.Append("<br>");

				var retVal = sb3.ToString();
				sb3.Clear();

				sb2.Clear();

				for (int i = 1; i < tempSplit.Length; i++)
				{

					sb2.Append(' ');
					sb2.Append(tempSplit[i]);

				}

				temp = sb2.ToString();
				tempSplit = temp.Split(new char[]{'(', ')'});
				
				//service name
				sb3.Append(String.Format(span, lookHere));
				sb3.Append("Service Name: ");
				sb3.Append(spanEnd);
				sb3.Append(String.Format(span, retColor));
				sb3.Append(tempSplit[0]);
				sb3.Append(spanEnd);
				sb3.Append("<br>");
				var serviceName = sb3.ToString();
				sb3.Clear();

				//params
				sb3.Append(String.Format(span, lookHere));
				sb3.Append("Params: ");
				sb3.Append(spanEnd);

				sb3.Append(String.Format(span, defaultColor));
				for (int i = 1; i < tempSplit.Length; i++)
				{
					sb3.Append(tempSplit[i]);
				}
				sb3.Append(spanEnd);
				var parans = sb3.ToString().Replace(";", "");

				sb.Append(serviceName);
				sb.Append(retVal);
				sb.Append(parans);
				
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
