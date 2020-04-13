using System;

namespace Yorozu.Sample
{
	public class SamplePartsControl : PartsControlAbstract
	{
		public override Type PartsType()
		{
			return typeof(SamplePartsAbstract);
		}

		private void Awake()
		{
			SetUp();
		}
	}
}