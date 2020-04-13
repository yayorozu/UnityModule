namespace Yorozu.Sample
{
	public abstract class SamplePartsAbstract : PartsAbstract
	{
		protected SamplePartsControl Owner => GetController<SamplePartsControl>();
	}
}