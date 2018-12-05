using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{

  /* Camera Movement:
  7.5  10 -0.5 (rot: 60  0   0)
  15.5 10  7.5 (rot: 60 -90  0)
  7.5  10 15.5 (rot: 60 -180 0)
  -0.5 10  7.5 (rot: 60 -270 0)

   */

  // TODOS
  // highlight enemy base with RED color when minion can capture base [done]
  // update possible move to exclude enemy's base when minion has no power [done]

  public Camera camera;

  private float[,] cameraPositions = new float[4, 6] {
    {7.5f , 10f, -0.5f, 60f,  0f  , 0f},
    {15.5f, 10f,  7.5f, 60f, -90f , 0f},
    {7.5f , 10f, 15.5f, 60f, -180f, 0f},
    {-0.5f, 10f,  7.5f, 60f, -270f, 0f}
  };
  public static BoardManager Instance { set; get; }

  private bool[,,] allowedMoves { set; get; }
  public const float TILE_SIZE = 1.0f;
  public const float TILE_OFFSET = 0.5f;
  private const int TILE_COUNT = 15;
  private float MINION_OFFSET_FIX = 0.7f + TILE_OFFSET;

  public Minion[,] Minions { set; get; }
  private Minion selectedMinion;
  private int[] remainMinions = new int[4] { 4, 4, 4, 4 };

  private int selectionX = -1;
  private int selectionY = -1;

  public List<GameObject> minionPrefabs;
  private List<GameObject> activeMinion;

  // private Quaternion orientation = Quaternion.Euler(0, 180, 0);
  // for new minion model
  private Quaternion orientation = Quaternion.Euler(14.84f, 180f, 0);

  public int playerTurn = 0;

  public Player[] players;

  public Text Dice;
  private int dice;

  public Text WinText;

  public List<Minion> midStack = new List<Minion>();

  private void Start()
  {
    Instance = this;

    if (WinText != null)
      WinText.enabled = false;

    players = new Player[] {
      new Player(1),
      new Player(2),
      new Player(3),
      new Player(4)
    };
    players[0].baseX = 7;
    players[0].baseY = 0;
    players[0].hasDefeated = false;

    players[1].baseX = 14;
    players[1].baseY = 7;
    players[1].hasDefeated = false;

    players[2].baseX = 7;
    players[2].baseY = 14;
    players[2].hasDefeated = false;

    players[3].baseX = 0;
    players[3].baseY = 7;
    players[3].hasDefeated = false;

    SpawnAllMinions();
    RollDice();
  }

  private void Update()
  {
    UpdateSelection();
    DrawBoard();

    if (Input.GetMouseButtonDown(0))
    {
      if (selectionX >= 0 && selectionY >= 0)
      {
        if (selectedMinion == null)
        {
          // Select the minion
          SelectMinion(selectionX, selectionY);
        }
        else
        {
          // Move the minion
          MoveMinion(selectionX, selectionY);
        }
      }
    }
  }

  private void LateUpdate()
  {
    // camera.transform.rotation = Quaternion.RotateTowards(
    //   camera.transform.rotation,
    //   Quaternion.Euler(cameraPositions[playerTurn, 3], cameraPositions[playerTurn, 4], cameraPositions[playerTurn, 5]),
    //   3 * Time.deltaTime
    //   );

    camera.transform.rotation = Quaternion.Lerp(
      camera.transform.rotation,
      Quaternion.Euler(cameraPositions[playerTurn, 3], cameraPositions[playerTurn, 4], cameraPositions[playerTurn, 5]),
      Time.maximumDeltaTime - Time.smoothDeltaTime
    );

    // camera.transform.position = Vector3.MoveTowards(
    //   camera.transform.position,
    //   new Vector3(cameraPositions[playerTurn, 0], cameraPositions[playerTurn, 1], cameraPositions[playerTurn, 2]),
    //   0.1f
    //   );
    camera.transform.position = Vector3.Lerp(
      camera.transform.position,
      new Vector3(cameraPositions[playerTurn, 0], cameraPositions[playerTurn, 1], cameraPositions[playerTurn, 2]),
      Time.maximumDeltaTime - Time.smoothDeltaTime
    );
  }

  private void SelectMinion(int x, int y)
  {
    if (Minions[x, y] == null)
      return;

    if (Minions[x, y].player != playerTurn)
      return;

    allowedMoves = Minions[x, y].PossibleMove(dice);
    selectedMinion = Minions[x, y];
    BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
  }

  private void MoveMinion(int x, int y)
  {
    if (allowedMoves[x, y, 0])
    {
      Minion m = Minions[x, y];

      if (m != null && m.player != playerTurn)
      {
        // Capture a piece
        activeMinion.Remove(m.gameObject);
        Destroy(m.gameObject);
        AudioManager.Instance.PlayKillSfx();
      } else {
        AudioManager.Instance.PlayMoveSfx();
      }

      Minions[selectedMinion.CurrentX, selectedMinion.CurrentY] = null;
      // playerTurn = (playerTurn + 1) % 4;
      selectedMinion.SetPosition(x, y);
      selectedMinion.transform.position = GetTileCenter(selectedMinion.player, x, y);
      Minions[x, y] = selectedMinion;

      // check if minion is in the center (pick power)
      if (x == 7 && y == 7)
      {
        selectedMinion.HasPower = true;
      }

      CheckDefeating();

      // spawn more minions
      SpawnRemainingMinions();
      HideMiddleMinion();

      UpdateNextTurn();
      ShowCurrentPlayerMiddleMinion();
      RollDice();
    }

    BoardHighlights.Instance.HideHighlights();
    selectedMinion = null;

  }

  private void HideMiddleMinion()
  {
    Minion m = Minions[7, 7];
    if (m == null)
      return;
    if (m.CurrentX == 7 && m.CurrentY == 7) {
      // Add to midstack
      midStack.Add(m);
      m.gameObject.SetActive(false);
      Minions[7, 7] = null;
    }
  }

  private void ShowCurrentPlayerMiddleMinion() 
  {
    foreach(Minion m in midStack) {
      if(m.player == playerTurn) {
        m.gameObject.SetActive(true);
        Minions[7, 7] = m;
        midStack.Remove(m);
        return;
      }
    }
  }

  private void UpdateNextTurn()
  {

    int nextPlayerTurn = (playerTurn + 1) % 4;

    // stop when there's only one minion left
    if (activeMinion.Count == 1)
      return;

    foreach (GameObject go in activeMinion)
    {
      Minion m = go.GetComponent<Minion>();
      if (m.player == nextPlayerTurn)
      {
        playerTurn = nextPlayerTurn;
        return;
      }
    }
    playerTurn = nextPlayerTurn;
    UpdateNextTurn();
  }

  private void RollDice()
  {
    // 1 to 6
    dice = (int)Random.Range(1, 6);
    Dice.text = "Dice: " + dice;
  }

  private void UpdateSelection()
  {
    if (!Camera.main)
      return;

    RaycastHit hit;
    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Plane")))
    {
      selectionX = (int)hit.point.x;
      selectionY = (int)hit.point.z;
    }
    else
    {
      selectionX = -1;
      selectionY = -1;
    }
  }

  private void SpawnRemainingMinions()
  {
    if (Minions[7, 0] == null && remainMinions[0] > 0 && !players[0].hasDefeated)
    {
      remainMinions[0]--;
      SpawnMinion(0, 7, 0);
    }
    else if (Minions[14, 7] == null && remainMinions[1] > 0 && !players[1].hasDefeated)
    {
      remainMinions[1]--;
      SpawnMinion(1, 14, 7);
    }
    else if (Minions[7, 14] == null && remainMinions[2] > 0 && !players[2].hasDefeated)
    {
      remainMinions[2]--;
      SpawnMinion(2, 7, 14);
    }
    else if (Minions[0, 7] == null && remainMinions[3] > 0 && !players[3].hasDefeated)
    {
      remainMinions[3]--;
      SpawnMinion(3, 0, 7);
    }
  }

  private void DrawBoard()
  {
    Vector3 widthLine = Vector3.right * TILE_COUNT;
    Vector3 heightLine = Vector3.forward * TILE_COUNT;

    for (int i = 0; i <= TILE_COUNT; i++)
    {
      Vector3 start = Vector3.forward * i;
      Debug.DrawLine(start, start + widthLine);
      for (int j = 0; j <= TILE_COUNT; j++)
      {
        start = Vector3.right * j;
        Debug.DrawLine(start, start + heightLine);
      }
    }

    // Draw the selection
    if (selectionX >= 0 && selectionX >= 0)
    {
      Debug.DrawLine(
          Vector3.forward * selectionY + Vector3.right * selectionX,
          Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1)
      );

      Debug.DrawLine(
          Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
          Vector3.forward * selectionY + Vector3.right * (selectionX + 1)
      );
    }
  }

  private void SpawnMinion(int index, int x, int y)
  {
    GameObject go = Instantiate(minionPrefabs[index], GetTileCenter(index, x, y), GetMinionOrientation(index)) as GameObject;
    go.transform.SetParent(transform);
    Minions[x, y] = go.GetComponent<Minion>();
    Minions[x, y].SetPosition(x, y);
    activeMinion.Add(go);
  }

  private void SpawnAllMinions()
  {
    activeMinion = new List<GameObject>();
    Minions = new Minion[15, 15];

    // Player 1
    SpawnMinion(0, 7, 0);

    // Player 2
    SpawnMinion(1, 14, 7);

    // Player 3
    SpawnMinion(2, 7, 14);

    // Player 4
    SpawnMinion(3, 0, 7);

  }

  private void CheckDefeating()
  {
    Player p;
    Minion m;
    int playersWithMinion = 0;
    for (int i = 0; i < players.Length; i++)
    {
      if (i == playerTurn)
        continue;
      p = players[i];
      if (p.hasDefeated)
        continue;

      m = Minions[p.baseX, p.baseY];
      if (/* !p.hasDefeated &&  */m != null && m.HasPower && m.player != i)
      {
        // set player defeat state
        p.hasDefeated = true;
        remainMinions[i] = 0;
        DestroyPlayerMinions(i);

        // reset minion power
        m.HasPower = false;

        continue;
      }

      bool hasMinion = false;

      // check when player has no more minions
      foreach(GameObject go in activeMinion) {
        Minion m2 = go.GetComponent<Minion>();
        if (m2.player == i) {
          hasMinion = true; 
          break;
        }
      }
      if(hasMinion){
        playersWithMinion++;
      } else {
        Debug.Log("Player: " + i + " has no more minion");
        p.hasDefeated = true;
      }
    }

    // Check for winning condition
    // value is 0 because current player turn is not included
    if(playersWithMinion == 0) {
      // Debug.Log("winning condition");
      // Debug.Log("Player (" + (playerTurn+1) + ") Wins the game!");
      if (WinText != null) {
        WinText.enabled = true;
        WinText.text = "Player " + (playerTurn + 1) + " Wins!";
      }
    }
  }

  private void DestroyPlayerMinions(int player)
  {
    Minion m;
    Debug.Log("Player: " + player + ": has been defeated");
    for (int i = 0; i < TILE_COUNT; i++)
    {
      for (int j = 0; j < TILE_COUNT; j++)
      {
        m = Minions[i, j];
        if (m != null && m.player == player)
        {
          activeMinion.Remove(m.gameObject);
          Destroy(m.gameObject);
          Minions[i, j] = null;
        }


      }
    }

    Minion m3 = null;
    // destroy players in midstack
    foreach(Minion m2 in midStack) {
      if (m2.player == player) 
      {
        m3 = m2;
        break;
      }
    }
    if (m3 != null) {
        midStack.Remove(m3);
        activeMinion.Remove(m3.gameObject);
        Destroy(m3.gameObject);
    }
  }

  private Quaternion GetMinionOrientation(int player)
  {
    return Quaternion.Euler(
      orientation.eulerAngles.x,
      player * -90f,
      orientation.eulerAngles.z
    );
  }

  private Vector3 GetTileCenter(int player, int x, int y)
  {
    // Vector3 origin = Vector3.zero;
    Vector3 origin = new Vector3(0, -4.5f, 0f);
    
    // Adjust Minion position
    switch(player) {
      case 0: {
        origin.z -= MINION_OFFSET_FIX;
        break;
      }
      case 1: {
        origin.x += MINION_OFFSET_FIX;
        break;
      }
      case 2: {
        origin.z += MINION_OFFSET_FIX;
        break;
      }
      case 3: {
        origin.x -= MINION_OFFSET_FIX;
        break;
      }
    }
    origin.x += (TILE_SIZE * x) + TILE_OFFSET;
    origin.z += (TILE_SIZE * y) + TILE_OFFSET;
    return origin;
  }
}
