using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
	public int CurrentX{set;get;}
	public int CurrentY{set;get;}
	public int player;

	public bool hasPower {set; get;}

	public void SetPosition(int x, int y) 
	{
		CurrentX = x;
		CurrentY = y;
	}

	public bool[,] PossibleMove(int step)
	{

		// int step = 3;
		// return new bool[15,15];
		bool[,] r = new bool[15, 15];

		Minion m;

		for (int i = Mathf.Max(0, CurrentX-step-1); i < Mathf.Min(15, CurrentX+step+1); i++)
		{
			for(int j = Mathf.Max(0, CurrentY-step-1); j < Mathf.Min(15, CurrentY+step+1); j++)
			{
				m = BoardManager.Instance.Minions[i, j];
				if (manhattanDistance(i, j, CurrentX, CurrentY) == step && (m == null || m.player != this.player))
				{
					r[i,j] = true;
				}
			}
		}


		return r;
	}

	private int manhattanDistance(int x1, int y1, int x2, int y2)
	{
		return Mathf.Abs(x1-x2) + Mathf.Abs(y1-y2);
	}
}
