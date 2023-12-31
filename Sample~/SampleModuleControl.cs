using System;

namespace Yorozu.Sample
{
	public class SampleModuleControl : ModuleControlAbstract
	{
		private void Awake()
		{
			SetUp();
		}

		public override Type ModuleType()
		{
			return typeof(SampleModuleAbstract);
		}
	}
}