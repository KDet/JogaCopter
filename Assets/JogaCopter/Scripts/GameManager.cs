using System;
using System.Collections;
using UniRx;
using UniRx.UI;
using UnityEngine;

public enum GameState
{
    [SerializeField] Start,
    [SerializeField] Ready,
    [SerializeField] Running,
    [SerializeField] End
}

public class GameManager : TypedMonoBehaviour, IDisposable
{
	public const string ScoreText = "Score";
	public const string GameName = "JOGA COPTER";
	public const string GameOverText = "Game over";
	public static readonly string BestScoreText = "Best";

	private const string HighScoreText = "HighScore";

	//private IDisposable _timerDisposable;
	
	[SerializeField] private float _itemsGenerationInterval = 0.9f;
	[SerializeField] [Range(0, 10)] private int _demagableGenerationRatio = 6;
	[SerializeField] [Range(0, 10)] private int _bonusGenerationRatio = 1;
	[SerializeField] [Range(0, 10)] private int _collectableGenerationRatio = 3;
	[SerializeField] private Camera _camera;
	[SerializeField] private GameState _state;
	[SerializeField] private long _score;

	[HideInInspector] public IObservable<GameState> _StateChanged;
	[HideInInspector] public IObservable<long> _ScoreChanged;
	[HideInInspector] public Subject<BonusSettings> _BonusPicked;
//  [HideInInspector] public IObservable<float> _CameraWidthProperty;

	private readonly CompositeDisposable _disposables = new CompositeDisposable();

	public float ItemsGenerationInterval
	{
		get { return _itemsGenerationInterval; }
	}
	public int DemagableGenerationRatio
	{
		get { return _demagableGenerationRatio; }
	}
	public int BonusGenerationRatio
	{
		get { return _bonusGenerationRatio; }
	}
	public int CollectableGenerationRatio
	{
		get { return _collectableGenerationRatio; }
	}
	public GameState State
	{
		get { return _state; }
	}
	public long Score
	{
		get { return _score; }
	}
	public float CameraWidth
	{
		get { return _camera.orthographicSize*_camera.aspect*2f; }
	}
	public float CameraHeigth
	{
		get { return 2f*_camera.orthographicSize; }
	}
	public long HighScore { get; private set; }
	public bool IsNewHighScore { get; private set; }

	public static GameManager Instance { get; private set; }

	private static IEnumerator PickBonusCoroutine(BonusSettings bonusItem)
	{
		yield return bonusItem;
		yield return new WaitForSeconds(bonusItem.ActiveTime);
		yield return bonusItem.Revert();
	}

	public override void Awake()
	{
		#region Create singeltone
		if (Instance != null && Instance != this)
			Destroy(gameObject);
		Instance = this;
		DontDestroyOnLoad(gameObject);
		#endregion

		_StateChanged = _state.ObserveEveryValueChanged(state => _state);
		_ScoreChanged = _score.ObserveEveryValueChanged(score => _score);
		_BonusPicked = _BonusPicked ?? (_BonusPicked = new Subject<BonusSettings>());
		_camera = (_camera ?? Camera.main);
	}
	public override void Start()
	{
		HighScore = LoadData();
		IsNewHighScore = false;
		_state = GameState.Start;
		_score = 0;
//		if (_timerDisposable != null)
//			_timerDisposable.Dispose();
	}
	public override void OnDestroy()
	{
		Dispose();
	}

	public long LoadData()
	{
		return (long)PlayerPrefs.GetFloat(HighScoreText, 0f);
	}
	public void SaveData(long highScore)
	{
		PlayerPrefs.SetFloat(HighScoreText, highScore);
	}
	public long AddScore(int score)
	{
		_score += score;
		return _score;
	}
	public void PickBonus(BonusSettings bonusItem)
	{
		Observable.FromCoroutineValue<BonusSettings>(() => PickBonusCoroutine(bonusItem), false).Subscribe(item => _BonusPicked.OnNext(item)).AddTo(_disposables);
	}

	public void Play()
	{
		IsNewHighScore = false;
		_state = GameState.Running;
		_score = 0;
//		if (_timerDisposable != null)
//			_timerDisposable.Dispose();
		//_timerDisposable = Observable.Interval(TimeSpan.FromSeconds(ItemsGenerationInterval)).Subscribe(_ => _score++);
	}
	public void SeeTutorial()
	{
		IsNewHighScore = false;
		_state = GameState.Ready;
		_score = 0;
//		if (_timerDisposable != null)
//			_timerDisposable.Dispose();
	}
	public void GameOver()
	{
		if (_score > HighScore)
		{
			HighScore = _score;
			SaveData(_score);
			IsNewHighScore = true;
		}
		_BonusPicked.OnNext(BonusSettings.ForceDefault());
		_state = GameState.End;
//		if (_timerDisposable != null)
//			_timerDisposable.Dispose();
	}

	public void Dispose()
	{
		_disposables.Dispose();
	}
}