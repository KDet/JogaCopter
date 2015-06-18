using UniRx;
using UnityEngine;

public enum BombItemKind
{
	[SerializeField] Normal,
	[SerializeField] NoBombs,
	[SerializeField] AllBombs,
}
public class BonusSettings
{
	public enum SettingsType
	{
		Normal,
		Reverted,
		ForceDefault
	}

	private float _activeTime;
	
	private bool _isInsensitive;
	private BombItemKind _bombsKind;
	private int _additionHearts;
	private float _scoreMultyplayer;
	private float _speedRatio;
	private int _extraScores;

	private void SetDefault()
	{
		_extraScores = 0;
		_speedRatio = 1.0f;
		_scoreMultyplayer = 1;
		_additionHearts = 0;
		_bombsKind = BombItemKind.Normal;
		_isInsensitive = false;
		_activeTime = 0.0f;
		Type = SettingsType.Normal;
		IsSuperBadItem = false;
	}

	public float ActiveTime
	{
		get { return _activeTime; }
	}
	public bool IsInsensitive
	{
		get { return _isInsensitive; }
	}
	public BombItemKind BombsKind
	{
		get { return _bombsKind; }
	}
	public int AdditionHearts
	{
		get { return _additionHearts; }
	}
	public float ScoreMultyplayer
	{
		get { return _scoreMultyplayer; }
		private set { _scoreMultyplayer = value; }
	}
	public float SpeedRatio
	{
		get { return _speedRatio; }
		private set { _speedRatio = value; }
	}
	public int ExtraScores
	{
		get { return _extraScores; }
	}

	public bool OldIsInsensitive { get; private set; }
	public SettingsType Type { get; private set; }
	public bool IsSuperBadItem { get; private set; }

	public BonusSettings Revert()
	{
		var res =  new BonusSettings();
		res.OldIsInsensitive = _isInsensitive;
		res._scoreMultyplayer = 1f/_scoreMultyplayer;
		res._speedRatio = 1f/_speedRatio;
		res.Type = SettingsType.Reverted;
		return res;
	}
	public static BonusSettings ForceDefault()
	{
		var res =  new BonusSettings();
		res.SetDefault();
		res.Type = SettingsType.ForceDefault;
		return res;
	}

	public BonusSettings()
	{
		SetDefault();
		Type = SettingsType.Normal;
	}
	public BonusSettings(float activeTime, bool isInsensitive, BombItemKind bombsKind, int additionHearts, float scoreMultyplayer, float speedRatio, int extraScores, bool isSuperBadItem)
	{
		_activeTime = activeTime;
		_isInsensitive = isInsensitive;
		_bombsKind = bombsKind;
		_additionHearts = additionHearts;
		_scoreMultyplayer = scoreMultyplayer;
		_speedRatio = speedRatio;
		_extraScores = extraScores;
		IsSuperBadItem = isSuperBadItem;
	}
}

public class BonusItem : TypedMonoBehaviour
{
	[SerializeField] private bool _isSuperBadItem;
	[SerializeField] private float _activeTime = 0.0f;
	
	[SerializeField] private bool _isInsensitive = false;
	[SerializeField] private BombItemKind _bombsKind = BombItemKind.Normal;
	[SerializeField] private int _additionHearts = 0;
	[SerializeField] private float _scoreMultyplayer = 1;
	[SerializeField] private float _speedRatio = 1.0f;
	[SerializeField] private int _extraScores = 0;
	
	private BonusSettings _settings;

	public float ActiveTime
	{
		get { return _activeTime; }
	}
	public bool IsInsensitive
	{
		get { return _isInsensitive; }
	}
	public BombItemKind BombsKind
	{
		get { return _bombsKind; }
	}
	public int AdditionHearts
	{
		get { return _additionHearts; }
	}
	public float ScoreMultyplayer
	{
		get { return _scoreMultyplayer; }
		private set { _scoreMultyplayer = value; }
	}
	public float SpeedRatio
	{
		get { return _speedRatio; }
		private set { _speedRatio = value; }
	}
	public int ExtraScores
	{
		get { return _extraScores; }
	}
	public bool IsSuperBadItem
	{
		get { return _isSuperBadItem; }
	}

	public BonusSettings GetSettings()
	{
		return _settings ??
		       (_settings = new BonusSettings(_activeTime, _isInsensitive, _bombsKind, _additionHearts, _scoreMultyplayer, _speedRatio, _extraScores, _isSuperBadItem));
	}
}