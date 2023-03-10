using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineRenderer victoryLineRenderer;
    public Camera mainCamera;
    public int width, height;
    public GameObject xPrefab, oPrefab;

    private int[,] board;
    private bool player1Turn = true;
    private bool gameOver = false;
    private int timeToResetGame = 5;
    private List<Vector2> winPoints = new List<Vector2>();


    void Start()
    {
        // set height of main camera
        mainCamera.orthographicSize = height / 2;
        // set width of main camera
        mainCamera.aspect = (float)width / height;

        // create line renderer
        GenerateLineRenderer();

        // create board
        board = new int[width, height];
    }
    void Update()
    {
        // check if get mouse button down and game is not over yet and it is player 1 turn, instantiate oPrefab at mouse position
        if (Input.GetMouseButtonDown(0) && !gameOver)
        {
            // get mouse position
            Vector3 mousePosition = Input.mousePosition;

            // convert mouse position to world position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            // get x and y position
            int x = (int)(worldPosition.x + width / 2);
            int y = (int)(worldPosition.y + height / 2);

            // get center position of the cell
            Vector3 centerPosition = new Vector3(x - width / 2 + 0.5f, y - height / 2 + 0.5f, 0);

            // check if position is valid
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                // check if position is empty
                if (board[x, y] == 0)
                {
                    // set position to player
                    board[x, y] = player1Turn ? 1 : 2;

                    // instantiate xPrefab or oPrefab at center position
                    if (player1Turn)
                    {
                        Instantiate(oPrefab, centerPosition, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(xPrefab, centerPosition, Quaternion.identity);
                    }

                    // check if player has won
                    if (CheckForWin(x, y))
                    {
                        // set game over
                        gameOver = true;
                        // show message
                        Debug.Log("Player " + (player1Turn ? 1 : 2) + " has won!");
                        // draw victory line
                        DrawVictoryLine();
                        // reset game
                        Invoke("ResetGame", timeToResetGame);

                    }
                    player1Turn = !player1Turn;
                }
            }

        }
    }

    // draw victory line
    private void DrawVictoryLine()
    {
        // set position count of victory line renderer
        victoryLineRenderer.positionCount = winPoints.Count;
        // set width of victory line renderer
        victoryLineRenderer.SetWidth(0.2f, 0.2f);
        // set positions of victory line renderer
        for (int i = 0; i < winPoints.Count; i++)
        {
            victoryLineRenderer.SetPosition(i, new Vector3(winPoints[i].x - width / 2 + 0.5f, winPoints[i].y - height / 2 + 0.5f, 0));
        }
    }

    // draw grid by line renderer
    private void GenerateLineRenderer()
    {
        float viewportHeight = mainCamera.orthographicSize * 2.0f;
        float viewportWidth = viewportHeight * mainCamera.aspect;

        // get start position
        float startX = mainCamera.transform.position.x - viewportWidth / 2.0f;
        float startY = mainCamera.transform.position.y - viewportHeight / 2.0f;
        // get step size
        float stepX = viewportWidth / width;
        float stepY = viewportHeight / height;

        // draw vertical lines
        for (int i = 0; i < width; i++)
        {
            lineRenderer.positionCount += 4;
            lineRenderer.SetPosition(lineRenderer.positionCount - 4, new Vector3(startX, startY, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 3, new Vector3(startX + i + stepX, startY, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, new Vector3(startX + i + stepX, startY + viewportHeight, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(startX, startY + viewportHeight, 0));
        }
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(startX, startY, 0));

        // draw horizontal lines
        for (int i = 0; i < height; i++)
        {
            lineRenderer.positionCount += 4;
            lineRenderer.SetPosition(lineRenderer.positionCount - 4, new Vector3(startX, startY, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 3, new Vector3(startX, startY + i + stepY, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, new Vector3(startX + viewportWidth, startY + i + stepY, 0));
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(startX + viewportWidth, startY, 0));
        }

        // set width of line renderer
        lineRenderer.SetWidth(0.05f, 0.05f);
    }

    // reset game
    private void ResetGame()
    {
        // reset board
        board = new int[width, height];
        // destroy all game objects
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Cell");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            Destroy(gameObjects[i]);
        }
        // reset player turn and game over
        player1Turn = true;
        gameOver = false;
        // reset victory line renderer
        victoryLineRenderer.positionCount = 0;
        Debug.Log("Game has been reset!");
    }

    // check if player has won
    private bool CheckForWin(int x, int y)
    {
        int player = player1Turn ? 1 : 2;

        // check horizontal
        int count = 0;
        for (int i = 0; i < width; i++)
        {
            if (board[i, y] == player)
            {
                winPoints.Add(new Vector2(i, y));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
        }

        // check vertical
        count = 0;
        winPoints.Clear();
        for (int i = 0; i < height; i++)
        {
            if (board[x, i] == player)
            {
                winPoints.Add(new Vector2(x, i));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
        }

        // check diagonal 1
        count = 0;
        winPoints.Clear();
        int row = y;
        int col = x;
        while (row < height && col < width)
        {
            if (board[col, row] == player)
            {
                winPoints.Add(new Vector2(col, row));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
            row++;
            col++;
        }

        row = y - 1;
        col = x - 1;
        while (row >= 0 && col >= 0)
        {
            if (board[col, row] == player)
            {
                winPoints.Add(new Vector2(col, row));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
            row--;
            col--;
        }

        // check diagonal 2
        count = 0;
        winPoints.Clear();
        row = y;
        col = x;
        while (row < height && col >= 0)
        {
            if (board[col, row] == player)
            {
                winPoints.Add(new Vector2(col, row));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
            row++;
            col--;
        }

        row = y - 1;
        col = x + 1;
        while (row >= 0 && col < width)
        {
            if (board[col, row] == player)
            {
                winPoints.Add(new Vector2(col, row));
                count++;
                if (count == 5)
                {
                    return true;
                }
            }
            else
            {
                winPoints.Clear();
                count = 0;
            }
            row--;
            col++;
        }

        return false;
    }
}
