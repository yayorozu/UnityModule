using UnityEngine;

namespace Yorozu.Sample
{
	public class SamplePartsRotate : SamplePartsAbstract
	{
		[SerializeField]
		private float _speed;

		protected override void OnUpdate()
		{
			Owner.transform.Rotate(Vector3.one, _speed);
		}
	}
}