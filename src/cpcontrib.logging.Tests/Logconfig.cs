using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logging_Tests
{
	[TestFixture]
	public class Logconfig
	{
		[Test]
		public void TestMethod()
		{
			//CPLog.Config.LoggingConfiguration

			var Log = CPLog.LogManager.GetLogger(typeof(TestClass1));

			
		}

		public class TestClass1
		{

		}
	}
}
