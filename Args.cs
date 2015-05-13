using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Documentor
{
	public class Args
	{
		public readonly string[] serviceFiles;

		public readonly string[] dataContractFiles;

		public readonly string writePath;

		public Args(string[] serviceFiles, string[] dataContractFiles, string writePath)
		{
			this.serviceFiles = serviceFiles;
			this.dataContractFiles = dataContractFiles;
			this.writePath = writePath;
		}
	}
}
