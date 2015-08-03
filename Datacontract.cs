using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Documentor
{
	class DataContract
	{
		private SortedSet<string> _files = new SortedSet<string>();
		private Args _args;

		public DataContract(Args clArgs)
		{
			this._args = clArgs;
			setPaths(clArgs.dataContractFiles);
		}

		/// <summary>
		/// file path or directory path
		/// </summary>
		/// <param name="paths"></param>
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
				File.WriteAllText(writePath, doc.toHTML());
			}

			return list.ToArray();
		}

		private Doc parse(string file)
		{
			//find struct or class with the DataContract attr
			//remove public and struct/class
			//grab everything between strut/class { }

			var doc = new Doc();
			try
			{
				using (var r = File.OpenText(file))
				{
					XMLComment comm = null;
					Contract cont = null;
					Member memb = null;
					
					int contractCount = 0;
					
					var state = State.search;

					while (!r.EndOfStream)
					{
						string line = r.ReadLine();
						
						if (state == State.search)
						{
							if (line.Contains("<summary>"))
							{
								comm = new XMLComment();
								state = State.xml;
							}
							else if (line.Contains("[DataContract]"))
							{
								//start contract obj
								cont = new Contract(comm);
								contractCount = 0;
								//if last think was xmlcomment add that to our contract
								state = State.contract;
							}
						}

						switch (state)
						{
							case State.xml:
								if (!String.IsNullOrWhiteSpace(line))
								{
									var mline = line.Replace("///", "");
									if(!string.IsNullOrWhiteSpace(mline))
										comm.add(line);
								}
								if (line.Contains("</summary>"))
								{
									state = State.search;
								}
								break;
							case State.contract:
								if (!String.IsNullOrWhiteSpace(line))
								{
									if (contractCount == 1)
									{
										cont.add(line);
										state = State.contractSearch;
									}

									contractCount++;
								}
								break;
							case State.contractSearch:
								if (line.Contains("{"))
								{
									cont.parens++;
								}
								if (line.Contains("}"))
								{
									if (cont.parens == 1)
									{
										doc.add(cont);
										state = State.search;
										break;
									}
											
									cont.parens--;
								}
								if (line.Contains("<summary>"))
								{
									comm = new XMLComment();
									comm.add(line);

									if (!line.Contains("</summary>"))
										state = State.memberXML;
									
								}
								if (line.Contains("[DataMember]"))
								{
									memb = new Member(comm);
									comm = null;
									state = State.member;
								}
								break;
							case State.memberXML:
								if (!String.IsNullOrWhiteSpace(line))
								{
									var mline = line.Replace("///", "");
									if (!string.IsNullOrWhiteSpace(mline))
										comm.add(line);
								}
								if (line.Contains("</summary>"))
								{
									state = State.contractSearch;
								}
								break;
							case State.member:
								if (line.Contains(";"))
								{
									memb.add(line);
									cont.add(memb);
									memb = null;
									state = State.contractSearch;
								}
								break;
							default:
								//don't want to bring in the wrong comment
								comm = null;
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
					if (dp is Contract)
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

			//readonly get of _items
			public string[] items { get { return _items.ToArray(); } }

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
					var htm = s.Replace("///","").Replace("<summary>", "").Replace("</summary>", "");
					if (!String.IsNullOrWhiteSpace(htm))
					{
						sb.Append(htm);
						sb.Append("<br>");
					}
				}
				if (sb.Length > 0)
				{
					sb.Insert(0, "//");
				}
				return sb.ToString();
			}
		}

		class Contract : IDocPart
		{
			private XMLComment _comment;

			private List<Member> _members = new List<Member>();

			private int? _parens = null;
			public int parens
			{
				get
				{
					return _parens != null ? _parens.Value : 0;
				}

				set
				{
					_parens = value;
				}
			}
			List<string> _items = new List<string>();

			public void add(string s)
			{
				var htm = s.Replace("<", "&lt;").Replace(">", "&gt;");
				_items.Add(htm);
			}

			public void add(Member member)
			{
				_members.Add(member);
			}

			public Contract(XMLComment comment)
			{
				_comment = comment;
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				if(_comment != null)
					sb.Append(_comment.toHTML());
				
				foreach (string s in _items)
				{
					sb.Append(s);
					sb.Append("<br>{<br><br>");
				}

				foreach (Member mem in _members)
				{
					sb.Append(mem.toHTML());
					sb.Append("<br>");
				}

				sb.Append("<br>}<br>");
				return sb.ToString();
			}
		}

		class Member : IDocPart
		{
			XMLComment _comment;
			private List<string> _items = new List<string>();
			
			public Member(XMLComment comment)
			{
				_comment = comment;
			}

			public void add(string s)
			{
				var htm = s.Replace("<", "&lt;").Replace(">", "&gt;");
				_items.Add(htm);
			}

			public string toHTML()
			{
				var sb = new StringBuilder();
				if (_comment != null)
					sb.Append(_comment.toHTML());

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

		public enum State
		{
			search,
			xml,
			contract,
			contractSearch,
			memberXML,
			member,
		}
	}
}
