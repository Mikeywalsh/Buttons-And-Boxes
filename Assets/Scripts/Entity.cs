using UnityEngine;
using System.Collections;

sealed public class Entity : Block {

    public bool isPlayer;
    public byte clockCycle;
	public bool moveable;
	public bool canFallIntoPits;

	private byte _arrayX;
	private byte _arrayY;
	private bool _isMoving;

	public void SetDimensions(byte arrayX, byte arrayY)
	{
		_arrayX = arrayX;
		_arrayY = arrayY;
	}

	public void FinishMoving(Level currentLevel, bool onIce, int[] direction)
	{
		StartCoroutine(MoveCooldown(currentLevel, onIce, direction));
	}

	public void FinishMoving()
	{
		StartCoroutine(MoveCooldown());
	}

	private IEnumerator MoveCooldown(Level currentLevel, bool onIce, int[] direction)
	{
		_isMoving = true;
		yield return new WaitForSeconds(0.3f);
		if(onIce)
		{
			if(!currentLevel.MoveEntity(this, direction, true, clockCycle))
				_isMoving = false;
		}
		else
			_isMoving = false;
	}
	
	private IEnumerator MoveCooldown()
	{
		_isMoving = true;
		yield return new WaitForSeconds(0.3f);
		_isMoving = false;
	}

	public byte ArrayX
	{
		get{ return _arrayX; }
	}

	public byte ArrayY
	{
		get{ return _arrayY; }
	}

	public bool IsMoving
	{
		get{ return _isMoving; }
		set{ _isMoving = value; }
	}
}