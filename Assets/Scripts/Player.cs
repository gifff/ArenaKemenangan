using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
	public int id {set; get;}
	public bool hasDefeated{set; get;}
	public int baseX {set; get;}
	public int baseY {set; get;}

	public Player() {

	}

	public Player(int id) {
		this.id = id;
	}
}
