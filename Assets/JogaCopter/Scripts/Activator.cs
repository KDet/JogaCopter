using UniRx;

public class Activator : TypedMonoBehaviour
{
	public void SetUnActive()
	{
		gameObject.SetActive(false);
	}
	public void SetActive()
	{
		gameObject.SetActive(true);
	}
}
