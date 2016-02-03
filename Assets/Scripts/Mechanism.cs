using UnityEngine;
using System.Collections;

public sealed class Mechanism : Block {

	public bool receivesInput;
	public bool activated;
	public byte group;
	public string animationName;
	public int inputRequired;
    public bool startOpen;

	private int inputCount;
	
	public void Activate()
	{
		inputCount ++;

		if(inputCount != inputRequired && receivesInput)
			return;

        if(receivesInput && startOpen)
            GetComponent<Animation>()[animationName].speed = -2;
        else
            GetComponent<Animation>()[animationName].speed = 2;

		GetComponent<Animation>().Play(animationName);

		activated = true;
	}

	public void DeActivate()
	{
		inputCount--;

		if((inputCount + 1) != inputRequired && receivesInput)
			return;

		if(!GetComponent<Animation>()[animationName].enabled)
			GetComponent<Animation>()[animationName].time = GetComponent<Animation>()[animationName].length;

        if (receivesInput && startOpen)
            GetComponent<Animation>()[animationName].speed = 2;
        else
            GetComponent<Animation>()[animationName].speed = -2;

		GetComponent<Animation>().Play(animationName);

		activated = false;
	}
}