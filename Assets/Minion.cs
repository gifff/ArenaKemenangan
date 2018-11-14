using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
	public int CurrentX{set;get;}
	public int CurrentY{set;get;}
	public int player;

	public void SetPosition(int x, int y) 
	{
		CurrentX = x;
		CurrentY = y;
	}

	public bool PossibleMove(int x, int y)
	{
		return true;
	}
}
