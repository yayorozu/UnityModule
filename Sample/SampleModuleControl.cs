using System;

namespace Yorozu.Sample
{
	public class SampleModuleControl : ModuleControlAbstract
	{
		private void Awake()
		{
			SetUp();
		}

		public override Type PartsType()
		{
			return typeof(SampleModuleAbstract);
		}
	}
}