﻿using System.Collections;
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

  public Camera camera;

  private float[,] cameraPositions = new float[4, 6] {
    {7.5f , 10f, -0.5f, 60f,  0f  , 0f},
    {15.5f, 10f,  7.5f, 60f, -90f , 0f},
    {7.5f , 10f, 15.5f, 60f, -180f, 0f},
    {-0.5f, 10f,  7.5f, 60f, -270f, 0f}
  };
  public static BoardManager Instance { set; get; }

  private bool[,] allowedMoves { set; get; }
  private const float TILE_SIZE = 1.0f;
  private const float TILE_OFFSET = 0.5f;
  private const int TILE_COUNT = 15;

  public Minion[,] Minions { set; get; }
  private Minion selectedMinion;

  private int selectionX = -1;
  private int selectionY = -1;

  public List<GameObject> minionPrefabs;
  private List<GameObject> activeMinion;

  private Quaternion orientation = Quaternion.Euler(0, 180, 0);

  public int playerTurn = 0;

  public Text Dice;
  private int dice;

  private void Start()
  {
    Instance = this;
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
    if (allowedMoves[x, y])
    {
      Minion m = Minions[x, y];

      if (m != null && m.player != playerTurn)
      {
        // Capture a piece
        activeMinion.Remove(m.gameObject);
        Destroy(m.gameObject);
      }

      Minions[selectedMinion.CurrentX, selectedMinion.CurrentY] = null;
      selectedMinion.transform.position = GetTileCenter(x, y);
      selectedMinion.SetPosition(x, y);
      Minions[x, y] = selectedMinion;
      // playerTurn = (playerTurn + 1) % 4;
      UpdateNextTurn();
      RollDice();
    }

    BoardHighlights.Instance.HideHighlights();
    selectedMinion = null;

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
    GameObject go = Instantiate(minionPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
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

  private Vector3 GetTileCenter(int x, int y)
  {
    Vector3 origin = Vector3.zero;
    origin.x += (TILE_SIZE * x) + TILE_OFFSET;
    origin.z += (TILE_SIZE * y) + TILE_OFFSET;
    return origin;
  }
}
