using System;
using UniRx;
using UnityEngine;

public class CollectableItem : TypedMonoBehaviour
{
	[SerializeField] private int _score;
	public int Score
	{
		get { return _score; }
	}
}