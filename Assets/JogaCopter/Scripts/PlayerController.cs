using System;
using UniRx;
using UniRx.UI;
using UnityEngine;

public class PlayerController : TypedMonoBehaviour, IDisposable
{
    public static readonly Vector3 DefaultPlayerStartPosition = Vector3.zero;

	[SerializeField] private float _speed = 0.75f;

	[HideInInspector] public IObservable<int> _HeartsChanged;

    private Transform _transform;
    private bool _isLookAtRight = true;
    private float _maxMove;
	private Action _itemCollectedAction;
	private IDisposable _bonusPickedDispose;
	private bool _isInsensitive;
	private int _heards = 0;
	private int _scoreMulty = 1;
	private float _speedRatio = 1f;

	public int Heards
	{
		get { return _heards; }
	}
	
	private float MaxMoveCalculate(float cameraWidth)
	{
		return (cameraWidth - GetComponent<SpriteRenderer>().bounds.size.x) / 2;
	}
	private void OnBonusPicked(BonusSettings bonusItem)
	{
		_heards += bonusItem.AdditionHearts;
		GameManager.Instance.AddScore(bonusItem.ExtraScores * _scoreMulty);

		if (bonusItem.IsInsensitive)
			_isInsensitive = true;
		else if (!bonusItem.IsInsensitive && bonusItem.OldIsInsensitive)
			_isInsensitive = false;

		_scoreMulty = bonusItem.Type == BonusSettings.SettingsType.ForceDefault
			? (int)bonusItem.ScoreMultyplayer
			: (int)Mathf.Max(1f, _scoreMulty * bonusItem.ScoreMultyplayer);

		var newSpeed = bonusItem.Type == BonusSettings.SettingsType.ForceDefault ? bonusItem.SpeedRatio : _speedRatio * bonusItem.SpeedRatio;
		if (Math.Abs(newSpeed - _speedRatio) > Time.deltaTime)
			_speedRatio = newSpeed;
	}

	public override void Awake()
    {
        _transform = GetComponent<Transform>();  
		_HeartsChanged = _heards.ObserveEveryValueChanged(heards => _heards);
    }
	public override void Start()
    {
        _maxMove = MaxMoveCalculate(GameManager.Instance.CameraWidth);
        _bonusPickedDispose = GameManager.Instance._BonusPicked.Subscribe(OnBonusPicked);

    }
	public override void Update()
	{
		if (GameManager.Instance.State == GameState.Running)
		{
			Move(_speed*_speedRatio);
			if (Input.GetMouseButtonDown(0))
				Flip();
		}
//		if (GameManager.Instance.State == GameState.End)
//		{
//			_transform.position = DefaultPlayerStartPosition;
//			_transform.gameObject.SetActive(false);
//		}
	}
	public override void OnTriggerEnter2D(Collider2D other)
	{
		BonusItem bonusItem = other.GetComponent<BonusItem>();
		if (bonusItem)
		{
			GameManager.Instance.PickBonus(bonusItem.GetSettings());
		}
		else
		{
			CollectableItem collectable = other.GetComponent<CollectableItem>();
			if (collectable)
			{
				GameManager.Instance.AddScore(collectable.Score * _scoreMulty);
				if (_itemCollectedAction != null)
					_itemCollectedAction();
			}
			else if (other.GetComponent<DeadlyItem>() && !_isInsensitive)
			{
				if(_heards <= 0)
					GameManager.Instance.GameOver();
				if(_heards > 0)
					_heards--;
			}
		}
		Destroy(other.gameObject);
	}
	public override void OnDestroy()
	{
		Dispose();
	}

	public void Flip()
    {
        _transform.localScale = new Vector3(-_transform.localScale.x,
                    _transform.localScale.y,
                    _transform.localScale.z);
        _isLookAtRight = !_isLookAtRight;
    }
	public void Move(float speed)
    {
        if (Mathf.Abs(_transform.position.x) > _maxMove)
			Flip();

		_transform.position = new Vector3(
			Mathf.Lerp(transform.position.x, (_isLookAtRight ? _maxMove : - _maxMove), speed*Time.deltaTime),
			_transform.position.y,
			_transform.position.z);
    }

	public void SetUpActions(Action itemCollectedAction)
	{
		_itemCollectedAction = itemCollectedAction;
	}
	public void DeleteActions()
	{
		_itemCollectedAction = null;
	}

	public void Dispose()
	{
		if(_bonusPickedDispose != null)
			_bonusPickedDispose.Dispose();
	}
}