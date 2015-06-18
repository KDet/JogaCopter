using UnityEngine;
using System.Collections;
using UniRx;

public class BombView : DeadlyItem
{
	private Transform _transform;

	[SerializeField] private GameObject _explosion;
	[SerializeField] private ParticleSystem[] _effects;

	public override void Awake()
	{
		_transform = GetComponent<Transform>();
	}
	public override void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.GetComponent<PlayerController>())
		{
			Instantiate(_explosion, _transform.position, _transform.rotation);
			foreach (var effect in _effects)
			{
				effect.transform.parent = null;
				effect.Stop();
				Destroy(effect.gameObject, 1f);
			}
			Destroy(gameObject);
		}
	}
}