using System;
using UniRx;
using UnityEngine;

public class BackgroundScroll : ObservableMonoBehaviour
{
	public enum ScrollDirection
	{
		[SerializeField] Up,
		[SerializeField] Down
	}

	private Transform _transform;
    private Vector2 _size = Vector3.zero;
	private Vector3 _startPosition;
	private Vector3[] _childStartPositions;

	private static float _revertSpeed;
	private static Action _onRevertedAction;
	private static bool _isRevert;
	private static bool _isScroll;

	[SerializeField] private bool _isInfinite = true;
	[SerializeField] private ScrollDirection _scrollDirection = ScrollDirection.Up;
	[SerializeField] private float _scrollSpeed = 0.2f;
	[SerializeField] private float _childSpeedRatio = 2f;
	[SerializeField] private bool _scrollChild = true;

	public override void Awake()
    {
        _transform = GetComponent<Transform>();
		_childStartPositions = new Vector3[_transform.childCount];
		for (int i = 0; i < _transform.childCount; i++)
			_childStartPositions[i] = _transform.GetChild(i).position;
    }
	public override void Start()
	{
		_startPosition = transform.position;
		SpriteRenderer sprite = GetComponent<SpriteRenderer>();
		if (sprite)
			_size += new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
	}
	public override void Update()
	{
		if (_isRevert)
		{
			_transform.position = new Vector3(
				Mathf.Lerp(_transform.position.x, _startPosition.x, _revertSpeed*Time.deltaTime),
				Mathf.Lerp(_transform.position.y, _startPosition.y, _revertSpeed*Time.deltaTime)
				);
			if(_scrollChild)
				for (int i = 0; i < _transform.childCount; i++)
				{
					_transform.GetChild(i).position = new Vector3(
						Mathf.Lerp(_transform.GetChild(i).position.x, _childStartPositions[i].x, _revertSpeed*_childSpeedRatio*Time.deltaTime),
						Mathf.Lerp(_transform.GetChild(i).position.y, _childStartPositions[i].y, _revertSpeed*_childSpeedRatio*Time.deltaTime)
						);
				}
			var isRevert = Math.Abs(_transform.position.y - _startPosition.y) > _revertSpeed*Time.deltaTime || 
					   Math.Abs(_transform.position.x - _startPosition.x) > _revertSpeed*Time.deltaTime;
			if (!isRevert && _onRevertedAction != null)
			{
				_transform.position = _startPosition;
				_onRevertedAction();
			}
		}
		else if (_isScroll)
		{
			Vector3 direction = (_scrollDirection == ScrollDirection.Up) ? Vector3.up : Vector3.down;
			if (_isInfinite)
			{
				float newPosition = Mathf.Repeat(Time.time*_scrollSpeed, _size.y);
				_transform.position = _startPosition + direction * newPosition;
				if (_scrollChild)
					for (int i = 0; i < _transform.childCount; i++)
					{
						float newChildPosition = Mathf.Repeat(Time.time * _scrollSpeed * _childSpeedRatio, _size.y);
						_transform.GetChild(i).position = _childStartPositions[i] + direction * newChildPosition;
					}
			}
			else
			{
				if (Mathf.Abs(_transform.position.y + _size.y) <= GameManager.Instance.CameraHeigth)
					_transform.position -= direction*_scrollSpeed*Time.deltaTime;
			}
		}

	}

	public static void ScrollAll()
    {
		_isRevert = false;
        _isScroll = true;
    }
	public static void StopAll()
    {
        _isScroll = false;
		_isRevert = false;
    }
	public static void RevertEach(float revertSpeed, Action onRevertedAction)
	{
		_revertSpeed = revertSpeed;
		_isRevert = true;
		_isScroll = false;
		_onRevertedAction = onRevertedAction;
	}
}