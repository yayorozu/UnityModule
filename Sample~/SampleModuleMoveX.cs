using System;
using UnityEngine;

namespace Yorozu.Sample
{
	[Serializable]
	public class SampleModuleMoveX : SampleModuleAbstract
	{
		[SerializeField]
		private int _value;

		private float _defaultX;
		private bool _isPong;

		protected override void Prepare()
		{
			_defaultX = Owner.transform.localPosition.x;
		}

		protected override void Update()
		{
			var pos = Owner.transform.localPosition;
			if (_isPong)
			{
				pos.x -= Time.deltaTime;
				if (pos.x < _defaultX - _value)
					_isPong = !_isPong;
			}
			else
			{
				pos.x += Time.deltaTime;
				if (pos.x > _defaultX + _value)
					_isPong = !_isPong;
			}

			Owner.transform.localPosition = pos;
		}
	}
}
