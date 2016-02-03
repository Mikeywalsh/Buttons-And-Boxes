using UnityEngine;
using System.Collections;

sealed public class Level{

	private int _levelID;
    private string _name;
    private string _difficulty;
    private Color32 _backgroundColor;
	private char[,] _groundLayout;
    private float _inputCooldown;
	private Entity[,] _entityLayout;
	private Mechanism[,] _mechanismLayout;

    public Level(int levelID, string name, string difficulty, Color32 backgroundColor, char[,] groundLayout, Entity[,] entityLayout, Mechanism[,] mechanismLayout)
    {
        _levelID = levelID;
        _name = name;
        _difficulty = difficulty;
        _backgroundColor = backgroundColor;
        _groundLayout = groundLayout;
        _entityLayout = entityLayout;
        _mechanismLayout = mechanismLayout;
    }

    public bool MoveEntity(Entity toMove, int[] direction, bool canPush, byte clockCycle)
	{
        toMove.clockCycle = clockCycle;
		//Store start and destination array positions in temporary variables to save a lot of typing
		int startX = toMove.ArrayX;
		int startY = toMove.ArrayY;
		int targetX = startX + direction[0];
		int targetY = startY + direction[1];

		//If the current entity is not movable, or is on an edge of the current level, return false
		if(!toMove.moveable || targetX >= Width || targetX < 0 || targetY >= Length || targetY < 0 || _groundLayout[targetX, targetY] == 'Z')
		{
			_entityLayout[startX,startY].IsMoving = false;
			return false;
		}

		//If the current entity is inside a door, or has a door in front of it, and the door is closed, then prevent the entity from moving
		if((_mechanismLayout[startX, startY] != null && _mechanismLayout[startX, startY].receivesInput && !_mechanismLayout[startX, startY].activated || _mechanismLayout[targetX, targetY] != null && _mechanismLayout[targetX, targetY].receivesInput && !_mechanismLayout[targetX, targetY].activated))
			return false;

		//If there is an entity in front of the current entity and the current entity cannot push it, then stop it from moving
		if(_entityLayout[targetX, targetY] != null && !canPush)
			return false;

		//If an entity exists in front of the current entity, and either of them are on ice, then the current entity will stop and move the entity in front
		if(_entityLayout[targetX, targetY] != null && _entityLayout[targetX, targetY].moveable && (_groundLayout[startX, startY] == 'I' || _groundLayout[targetX,targetY] == 'I'))
		{
            if (MoveEntity(_entityLayout[targetX, targetY], direction, false, clockCycle) && _entityLayout[startX, startY].isPlayer)
                _inputCooldown = Time.time + 0.6f;

			_entityLayout[startX,startY].FinishMoving();
			return false;
		}

		//If there is an entity in front of the current entity, try to move that first. If it cannot move, then no entities in the chain can move
		if(_entityLayout[targetX, targetY] != null && !MoveEntity(_entityLayout[targetX, targetY], direction, false, clockCycle))
			return false;

		//If target ground block is a pit, but the current entity cannot fall into pits, return false
		if(GroundLayout[targetX, targetY] == 'P' && !toMove.canFallIntoPits)
			return false;

		//Move the entities gameObject on screen
		iTween.MoveTo(toMove.gameObject, iTween.Hash("x", targetX * 2, "z", targetY * 2, "easeType", iTween.EaseType.linear, "time", 0.3f));

		//Move the entity to its new position in the levels entity array
		_entityLayout[targetX, targetY] = _entityLayout[startX, startY];
		_entityLayout[startX, startY] = null;

		//If the current entity is moving off of a button, deactivate the button
		if(_mechanismLayout[startX, startY] != null && _mechanismLayout[startX, startY].receivesInput == false)
		{
			byte currentGroup = _mechanismLayout[startX, startY].group;
			_mechanismLayout[startX, startY].DeActivate();
			foreach(Mechanism m in _mechanismLayout)
			{
				if(m != null && m.receivesInput == true && m.group == currentGroup)
					m.DeActivate();
			}
		}

		//If target ground block is a pit, then make current entity fall in, else save the entities new position and check for mechanisms and ice
		if(_groundLayout[targetX, targetY] == 'P')
		{
			iTween.MoveBy(toMove.gameObject, iTween.Hash("y", -2, "easeType", iTween.EaseType.linear, "time", 0.2f, "delay", 0.3f));
			_groundLayout[targetX, targetY] = 'F';
			_entityLayout[targetX, targetY] = null;
		}
		else
		{
			//If the current entity is the player, and the target ground block is the finish block, then end the level
			if(_entityLayout[targetX, targetY].isPlayer && _groundLayout[targetX, targetY] == 'X')
				GameObject.Find("Scene Handler").GetComponent<Play>().levelWon = true;

			//If the current entity is moving onto a button, activate the button
			if(_mechanismLayout[targetX, targetY] != null && _mechanismLayout[targetX, targetY].receivesInput == false)
			{
				byte currentGroup = _mechanismLayout[targetX, targetY].group;
				_mechanismLayout[targetX, targetY].Activate();
				foreach(Mechanism m in _mechanismLayout)
				{
					if(m != null && m.receivesInput == true && m.group == currentGroup)
						m.Activate();
				}
			}

			_entityLayout[targetX, targetY].SetDimensions((byte)targetX, (byte)targetY);
			_entityLayout[targetX,targetY].FinishMoving(this, (_groundLayout[targetX,targetY] == 'I') ? true : false, direction);
		}

        //If the player has moved, disable input until it has finished moving
        if (_entityLayout[targetX, targetY] && _entityLayout[targetX, targetY].isPlayer)
            _inputCooldown = Time.time + 0.3f;

		return true;
	}

	public void EndLevel()
	{
		foreach(Transform child in GameObject.Find("Level Objects").transform)
			child.GetComponent<Block>().Despawn();
	}

	public int LevelID
	{
		get{ return _levelID; }
	}

	public char[,] GroundLayout
	{
		get{ return _groundLayout; }
	}

    public float InputCooldown
    {
        get{ return _inputCooldown; }
        set { _inputCooldown = value; }
    }

	public Entity[,] EntityLayout
	{
		get{ return _entityLayout; }
	}

	public Mechanism[,] MechanismLayout
	{
		get{ return _mechanismLayout; }
	}

    public string Name
    {
        get { return _name; }
    }

    public string Difficulty
    {
        get { return _difficulty; }
    }

    public Color32 BackgroundColor
    {
        get { return _backgroundColor; }
    }

	public int Width
	{
		get{ return _groundLayout.GetLength(0); }
	}

	public int Length
	{
		get{ return _groundLayout.GetLength(1); }
	}
}