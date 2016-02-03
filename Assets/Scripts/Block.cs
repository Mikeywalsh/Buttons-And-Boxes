using UnityEngine;

public class Block : MonoBehaviour {

    private char _ID;
	private float _spawnDelay;
    private bool _despawning;

	public void Spawn(int x, char ID)
	{
		transform.localScale = Vector3.zero;

		while(x >= 20)
		{
			x -= 20;
		}
		_spawnDelay = x / 10f;

		iTween.ScaleTo(gameObject, iTween.Hash("x", 1, "y", 1, "z", 1, "easeType", iTween.EaseType.easeOutBack, "delay", _spawnDelay / 2, "time", 0.5f));
        _ID = ID;
    }

	public void Despawn()
	{
        _despawning = true;
		iTween.ScaleTo(gameObject, iTween.Hash("x", 0, "y", 0, "z", 0, "easeType", iTween.EaseType.easeInBack, "delay", _spawnDelay / 2, "time", 0.5f, "onComplete", "DestroyMe"));
	}

	private void DestroyMe()
	{
		Destroy(gameObject);
	}

    public char ID
    {
        get { return _ID; }
    }

    public bool Despawning
    {
        get { return _despawning; }
    }
}