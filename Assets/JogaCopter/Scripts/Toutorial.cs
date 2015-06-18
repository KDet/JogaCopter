using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Toutorial : TypedMonoBehaviour, IDisposable
{
	[SerializeField] private PlayerController _player;
	[SerializeField] private Text _title;
	
	private IDisposable _movement;

	public override void OnDestroy()
	{
		Dispose();
	}

    public void TappedAction()
    {
        _player.Flip();
    }
	public void Begin()
	{
		_title.gameObject.SetActive(false);
		_player.transform.position = PlayerController.DefaultPlayerStartPosition;
		_player.SetUpActions(() => _title.gameObject.SetActive(true));
		_movement = Observable.EveryFixedUpdate().Subscribe(_ => _player.Move(0.2f));
	}
    public void End()
    {
		_player.DeleteActions();
        _player.transform.position = PlayerController.DefaultPlayerStartPosition;
		_title.gameObject.SetActive(false);
	    Dispose();
    }

	public void Dispose()
	{
		if (_movement != null)
			_movement.Dispose();
	}
}
