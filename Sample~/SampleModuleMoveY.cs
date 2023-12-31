using System;
using UnityEngine;

namespace Yorozu.Sample
{
	[Serializable]
	public class SampleModuleMoveY : SampleModuleAbstract
	{
		[SerializeField]
		private int _value;

		private float _defaultY;
		private bool _isPong;

		protected override void Prepare()
		{
			_defaultY = Owner.transform.localPosition.y;
		}

		protected override void Update()
		{
			var pos = Owner.transform.localPosition;
			if (_isPong)
			{
				pos.y -= Time.deltaTime;
				if (pos.y < _defaultY - _value)
					_isPong = !_isPong;
			}
			else
			{
				pos.y += Time.deltaTime;
				if (pos.y > _defaultY + _value)
					_isPong = !_isPong;
			}

			Owner.transform.localPosition = pos;
		}
	}
}
