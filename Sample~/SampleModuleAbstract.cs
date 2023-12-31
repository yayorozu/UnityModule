namespace Yorozu.Sample
{
	public abstract class SampleModuleAbstract : ModuleAbstract
	{
		protected SampleModuleControl Owner => GetController<SampleModuleControl>();
	}
}