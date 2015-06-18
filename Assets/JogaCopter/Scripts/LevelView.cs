using System;
using UniRx;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelView : TypedMonoBehaviour, IDisposable
{
	private const int ToutorialGuideSortOrder = 5;
	private const int DefaultGnomeSortOrder = 4;
	private const float ScrollRevertSpeed = 4.5f;
	private const int GameOverFontSize = 25;
	private const int GameTitleFontSize = 35;
	private BombItemKind _bombKind;
	
	private static readonly Color GameOverFontColor = Color.red;
	private static readonly Color GameTitleFontColor = Color.white;
	private static readonly Color ScoreboardFontColor = Color.black;
	private static readonly Color NewHighScoreFontColor = Color.Lerp(Color.yellow, Color.green, 0.5f);
	private const string TalkTrigger = "Talk";
	private const string IdleTrigger = "Idle";
	private const string MoveStartTrigger = "MoveStart";
	private const string MoveEndTrigger = "MoveEnd";


	[SerializeField] private GameObject[] _collactable;
	[SerializeField] private GameObject _bomb;
	[SerializeField] private BonusItem[] _bonusItems;
	[SerializeField] private GameObject _mainMenu;
	[SerializeField] private GameObject _shadow;
	[SerializeField] private GameObject _toutorial;
	[SerializeField] private Text _inGameScoreText;
	[SerializeField] private GameObject _damageablesRoot;
	[SerializeField] private PlayerController _player;
	[SerializeField] private Text _gameTitle;
	[SerializeField] private GameObject _gardenGnome;
	[SerializeField] private Text _scoreboardText;
	[SerializeField] private GameObject _playBtn;
	[SerializeField] private GameObject _rePlayBtn;
	[SerializeField] private GameObject _heart;
	[SerializeField] private Text _heartText;
	[SerializeField] private GameObject _unknownItemView;
	[SerializeField] private GameObject _intensiveItemView;
	[SerializeField] private GameObject _speedUpPlayerView;
	[SerializeField] private GameObject _noBombsItemView;
	[SerializeField] private GameObject _extraPointsView;
	[SerializeField] private GameObject _pointsMultyView;
	[SerializeField] private GameObject _extraHeartsView;


	private IDisposable _enemiesDispose;
	private IDisposable _scoreDispose;
	private readonly CompositeDisposable _disposables = new CompositeDisposable();

	private IDisposable GenerateItems()
	{
		_damageablesRoot.SetActive(true);
		var cameraWidth = GameManager.Instance.CameraWidth;
		return
			Observable.Interval(TimeSpan.FromSeconds(GameManager.Instance.ItemsGenerationInterval * (1f / cameraWidth))).Subscribe(_ =>
			{
				int collectIndex = UnityEngine.Random.Range(0, GameManager.Instance.CollectableGenerationRatio);
				int bonusIndex = UnityEngine.Random.Range(0, GameManager.Instance.BonusGenerationRatio);
				int bombIndex = UnityEngine.Random.Range(0, GameManager.Instance.DemagableGenerationRatio);
				GameObject obj = (collectIndex < bombIndex &&
								  bonusIndex < bombIndex || 
				                  _bombKind == BombItemKind.AllBombs) && 
					_bombKind != BombItemKind.NoBombs
					? _bomb
					: collectIndex > bonusIndex
						? _collactable[UnityEngine.Random.Range(0, _collactable.Length)]
						: _bonusItems[UnityEngine.Random.Range(0, _bonusItems.Length)].gameObject;
				obj.SetActive(true);
				var randArea = cameraWidth/2f - obj.GetComponent<SpriteRenderer>().bounds.size.x/2f;
				var instanse = (GameObject)Instantiate(obj, new Vector3(UnityEngine.Random.Range(-randArea, randArea), _damageablesRoot.transform.position.y,
							_damageablesRoot.transform.position.z), Quaternion.identity);
				instanse.transform.SetParent(_damageablesRoot.transform, false);
			});
	}
	private void DeleteEnemies()
	{
		if (_enemiesDispose != null)
			_enemiesDispose.Dispose();
		_enemiesDispose = null;
		_damageablesRoot.SetActive(false);
		for (int i = _damageablesRoot.transform.childCount - 1; i >= 0; i--)
			Destroy(_damageablesRoot.transform.GetChild(i).gameObject);
	}
	private void ResetScore()
	{
		if (_scoreDispose != null)
			_scoreDispose.Dispose();
		_scoreDispose = null;
	}
	private void OnScoreChanged(long score)
	{
		_inGameScoreText.text = string.Format("{0}: {1}", GameManager.ScoreText, score);
	}
	private void OnHeartsChanged(int hearts)
	{
		_heart.SetActive(hearts >= 1);
		if(hearts >1)
		{
			_heartText.gameObject.SetActive(true);
			_heartText.text = hearts.ToString();
		}
		else
			_heartText.gameObject.SetActive(false);
	}
	private bool _isInsensitive;
	private void OnBonusPicked(BonusSettings bonusItem)
	{
		if (bonusItem != null)
		{
			if (bonusItem.Type == BonusSettings.SettingsType.Normal)
			{
				if (bonusItem.IsSuperBadItem)
					_unknownItemView.SetActive(true);
				if (bonusItem.SpeedRatio > 1)
					_speedUpPlayerView.SetActive(true);
				if (bonusItem.AdditionHearts > 0)
					_extraHeartsView.SetActive(true);
				if (bonusItem.BombsKind == BombItemKind.NoBombs)
					_noBombsItemView.SetActive(true);
				if (bonusItem.ExtraScores > 0)
					_extraPointsView.SetActive(true);
				if (bonusItem.ScoreMultyplayer > 1)
					_pointsMultyView.SetActive(true);
			}
			if (bonusItem.IsInsensitive)
				_isInsensitive = true;
			else if (!bonusItem.IsInsensitive && bonusItem.OldIsInsensitive)
				_isInsensitive = false;
			_intensiveItemView.SetActive(_isInsensitive);

			bool newEnemiesGenerate = false;
			if(bonusItem.BombsKind != _bombKind)
			{
				_bombKind = bonusItem.BombsKind;
				newEnemiesGenerate = true;
			}
			if(newEnemiesGenerate || bonusItem.Type == BonusSettings.SettingsType.ForceDefault)
			{
				if(_enemiesDispose!=null)
					_enemiesDispose.Dispose();
				_enemiesDispose = GenerateItems();
			}

		}
	}

	private void OnGameStateChanged(GameState gameState)
	{
		_isInsensitive = false;
		switch (gameState)
		{
			case GameState.Start:
				OnStart();
				break;
			case GameState.Ready:
				OnReady();
				break;
			case GameState.Running:
				OnRunning();
				break;
			case GameState.End:
				OnEndGame();
				break;
			default:
				throw new ArgumentOutOfRangeException("gameState");
		}
	}
	private void OnStart()
	{
		BackgroundScroll.StopAll();
		DeleteEnemies();
		ResetScore();
		_mainMenu.SetActive(true);
		_playBtn.SetActive(true);
		_rePlayBtn.SetActive(false);
		_shadow.SetActive(false);
		_inGameScoreText.gameObject.SetActive(false);
		_player.gameObject.SetActive(false);
		_gardenGnome.GetComponent<Animator>().SetTrigger(IdleTrigger);
		_gardenGnome.GetComponent<SpriteRenderer>().sortingOrder = DefaultGnomeSortOrder;
		_toutorial.SetActive(false);
		_gameTitle.gameObject.SetActive(true);
		_gameTitle.text = GameManager.GameName;
		_gameTitle.fontSize = GameTitleFontSize;
		_gameTitle.color = GameTitleFontColor;
		_scoreboardText.gameObject.SetActive(true);
		_scoreboardText.text = string.Format("{0} {1}", GameManager.BestScoreText, GameManager.Instance.HighScore);
		_scoreboardText.color = GameManager.Instance.IsNewHighScore ? NewHighScoreFontColor : ScoreboardFontColor;
		_heart.SetActive(false);
		_heartText.gameObject.SetActive(false);
		_unknownItemView.SetActive(false);
		_intensiveItemView.SetActive(false);
		_speedUpPlayerView.SetActive(false);
		_noBombsItemView.SetActive(false);
		_extraPointsView.SetActive(false);
		_pointsMultyView.SetActive(false);
		_extraHeartsView.SetActive(false);
	}
	private void OnReady()
	{
		BackgroundScroll.RevertEach(ScrollRevertSpeed, null);
		DeleteEnemies();
		ResetScore();
		_mainMenu.SetActive(false);
		_playBtn.SetActive(false);
		_rePlayBtn.SetActive(false);
		_shadow.SetActive(true);
		_inGameScoreText.gameObject.SetActive(false);
		_player.gameObject.SetActive(true);
		_player.GetComponent<Animator>().SetTrigger(MoveStartTrigger);
		_gardenGnome.GetComponent<SpriteRenderer>().sortingOrder = ToutorialGuideSortOrder;
		_gardenGnome.GetComponent<Animator>().SetTrigger(TalkTrigger);
		_toutorial.SetActive(true);
		_gameTitle.gameObject.SetActive(false);
		_scoreboardText.gameObject.SetActive(false);
		_heart.SetActive(false);
		_heartText.gameObject.SetActive(false);
		_unknownItemView.SetActive(false);
		_intensiveItemView.SetActive(false);
		_speedUpPlayerView.SetActive(false);
		_noBombsItemView.SetActive(false);
		_extraPointsView.SetActive(false);
		_pointsMultyView.SetActive(false);
		_extraHeartsView.SetActive(false);
	}
	private void OnRunning()
	{
		BackgroundScroll.ScrollAll();
		_enemiesDispose = GenerateItems();
		_scoreDispose = GameManager.Instance._ScoreChanged.Subscribe(OnScoreChanged);
		_mainMenu.SetActive(false);
		_playBtn.SetActive(false);
		_rePlayBtn.SetActive(false);
		_shadow.SetActive(false);
		_inGameScoreText.gameObject.SetActive(true);
		_player.gameObject.SetActive(true);
		_player.transform.position = PlayerController.DefaultPlayerStartPosition;
		_player.GetComponent<Animator>().SetTrigger(MoveStartTrigger);
		_gardenGnome.GetComponent<Animator>().SetTrigger(IdleTrigger);
		_gardenGnome.GetComponent<SpriteRenderer>().sortingOrder = DefaultGnomeSortOrder;
		_toutorial.SetActive(false);
		_gameTitle.gameObject.SetActive(false);
		_scoreboardText.gameObject.SetActive(false);
		_heart.SetActive(false);
		_heartText.gameObject.SetActive(false);
		_unknownItemView.SetActive(false);
		_intensiveItemView.SetActive(false);
		_speedUpPlayerView.SetActive(false);
		_noBombsItemView.SetActive(false);
		_extraPointsView.SetActive(false);
		_pointsMultyView.SetActive(false);
		_extraHeartsView.SetActive(false);
	}
	private void OnEndGame()
	{
		_player.gameObject.SetActive(true);
		_player.GetComponent<Animator>().SetTrigger(MoveEndTrigger);
		BackgroundScroll.RevertEach(ScrollRevertSpeed, () =>
		{
			_mainMenu.SetActive(true);
			_player.gameObject.SetActive(false);
			_playBtn.SetActive(false);
			_rePlayBtn.SetActive(true);
		});
		DeleteEnemies();
		ResetScore();
		_shadow.SetActive(true);
		_inGameScoreText.gameObject.SetActive(false);
		//_player.SetActive(false);
		_gardenGnome.GetComponent<Animator>().SetTrigger(IdleTrigger);
		_gardenGnome.GetComponent<SpriteRenderer>().sortingOrder = DefaultGnomeSortOrder;
		_toutorial.SetActive(false);
		_gameTitle.gameObject.SetActive(true);
		_gameTitle.text = GameManager.GameOverText;
		_gameTitle.fontSize = GameOverFontSize;
		_gameTitle.color = GameOverFontColor;
		_scoreboardText.gameObject.SetActive(true);
		_scoreboardText.text = string.Format("{0} {1}{2}{3} {4}", GameManager.ScoreText,GameManager.Instance.Score, Environment.NewLine, GameManager.BestScoreText, GameManager.Instance.HighScore);
		_scoreboardText.color = GameManager.Instance.IsNewHighScore ? NewHighScoreFontColor: ScoreboardFontColor;
		_heart.SetActive(false);
		_heartText.gameObject.SetActive(false);
		_unknownItemView.SetActive(false);
		_intensiveItemView.SetActive(false);
		_speedUpPlayerView.SetActive(false);
		_noBombsItemView.SetActive(false);
		_extraPointsView.SetActive(false);
		_pointsMultyView.SetActive(false);
		_extraHeartsView.SetActive(false);
	}

	public override void Start()
	{
		_player._HeartsChanged.Subscribe(OnHeartsChanged).AddTo(_disposables);
		GameManager.Instance._BonusPicked.Subscribe(OnBonusPicked).AddTo(_disposables);
		GameManager.Instance._StateChanged.Subscribe(OnGameStateChanged).AddTo(_disposables);
		//GameManager.Instance._CameraWidthProperty.Subscribe(OnCameraWidthChanged).AddTo(_disposables);
	}
	public override void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		_disposables.Dispose();
		if (_enemiesDispose != null)
			_enemiesDispose.Dispose();
		if (_scoreDispose != null)
			_scoreDispose.Dispose();
	}
}