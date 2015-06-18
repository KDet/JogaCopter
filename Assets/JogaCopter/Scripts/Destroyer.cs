using UniRx;
using UnityEngine;

public class Destroyer : TypedMonoBehaviour
{
	public override void OnCollisionEnter2D(Collision2D coll)
	{
		Destroy(coll.gameObject);
	}
	public override void OnTriggerEnter2D(Collider2D coll)
	{
		Destroy(coll.gameObject);
	}
}
