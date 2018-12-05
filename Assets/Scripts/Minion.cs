using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Minion : MonoBehaviour
{
  public int CurrentX { set; get; }
  public int CurrentY { set; get; }
  public int player;

  public GameObject PowerPrefab;

  private GameObject powerObject;

  private const float POWER_HEIGHT_OFFSET = 0.75f;

  private bool hasPower;

  public bool HasPower
  {
    get
    {
      return hasPower;
    }
    set
    {
      powerObject.SetActive(value);
      UpdatePowerPosition();
      hasPower = value;
    }
  }

  private void Start()
  {
    powerObject = Instantiate(PowerPrefab, GetPowerPosition(), Quaternion.Euler(0, 0, 0));
    powerObject.SetActive(false);
  }

  private void LateUpdate()
  {
    UpdatePowerPosition();
  }

  private void UpdatePowerPosition()
  {
    if (powerObject != null)
    {
      powerObject.transform.position = GetPowerPosition();
    }
  }

  private Vector3 GetPowerPosition()
  {
    Vector3 origin = new Vector3(
      (BoardManager.TILE_SIZE * CurrentX) + BoardManager.TILE_OFFSET,
      POWER_HEIGHT_OFFSET,
      (BoardManager.TILE_SIZE * CurrentY) + BoardManager.TILE_OFFSET
    );

    return origin;
  }

  public void SetPosition(int x, int y)
  {
    CurrentX = x;
    CurrentY = y;
  }

  private bool isOtherPlayerBasePosition(int x, int y)
  {
    for (int i = 0; i < BoardManager.Instance.players.Length; i++)
    {
      if (i == player)
        continue;

      Player p = BoardManager.Instance.players[i];
      // if the player is defeated, it is no man's land
      if (p.baseX == x && p.baseY == y && !p.hasDefeated)
        return true;
    }
    return false;
  }

  public bool[,,] PossibleMove(int step)
  {

    // int step = 3;
    // return new bool[15,15];
    bool[,,] r = new bool[15, 15, 2];

    Minion m;

    for (int i = Mathf.Max(0, CurrentX - step - 1); i < Mathf.Min(15, CurrentX + step + 1); i++)
    {
      for (int j = Mathf.Max(0, CurrentY - step - 1); j < Mathf.Min(15, CurrentY + step + 1); j++)
      {
        m = BoardManager.Instance.Minions[i, j];
        if (manhattanDistance(i, j, CurrentX, CurrentY) == step && (m == null || m.player != this.player))
        {
          bool isOtherBase = isOtherPlayerBasePosition(i, j);
          if (isOtherBase && !hasPower)
            continue;
          r[i, j, 0] = true;
          // TRUE when m is other player's minion
          // OR
          // [i,j] is other player base position
          r[i, j, 1] = m != null || isOtherBase;
        }
      }
    }


    return r;
  }

  private int manhattanDistance(int x1, int y1, int x2, int y2)
  {
    return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
  }

  void OnDisable()
  {
    if (powerObject != null)
      powerObject.SetActive(false);
  }

  void OnEnable()
  {
    if (powerObject != null)
      powerObject.SetActive(hasPower);
  }

  void OnDestroy()
  {
    if (powerObject != null)
    {
			Destroy(powerObject);
    }
  }
}
