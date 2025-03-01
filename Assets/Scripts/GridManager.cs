using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject[] candies;
    public Camera mainCamera;

    private GameObject[,] grid;
    private GameObject selectedCandy;

    void Start()
    {
        grid = new GameObject[width, height];
        FillGrid();
        CenterCamera();
    }

    void FillGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                int candyIndex;
                do
                {
                    candyIndex = Random.Range(0, candies.Length);
                } while (IsPartOfMatch(x, y, candies[candyIndex]));

                GameObject candy = Instantiate(candies[candyIndex], position, Quaternion.identity);
                candy.GetComponent<Candy>().type = candyIndex;
                grid[x, y] = candy;
            }
        }
    }

    bool IsPartOfMatch(int x, int y, GameObject newCandy)
    {
        int newCandyType = newCandy.GetComponent<Candy>().type;
        bool isMatch = false;

        
        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null)
        {
            if (grid[x - 1, y].GetComponent<Candy>().type == newCandyType &&
                grid[x - 2, y].GetComponent<Candy>().type == newCandyType)
            {
                isMatch = true;
            }
        }

        if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null)
        {
            if (grid[x, y - 1].GetComponent<Candy>().type == newCandyType &&
                grid[x, y - 2].GetComponent<Candy>().type == newCandyType)
            {
                isMatch = true;
            }
        }

        return isMatch;
    }

    void CenterCamera()
    {
        float xPos = (width - 1) / 2.0f;
        float yPos = (height - 1) / 2.0f;
        mainCamera.transform.position = new Vector3(xPos, yPos, -10);
        mainCamera.orthographicSize = Mathf.Max(width, height) / 2.0f + 1;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mousePos.x);
            int y = Mathf.RoundToInt(mousePos.y);
            if (IsValidPosition(x, y))
            {
                if (selectedCandy == null)
                {
                    selectedCandy = grid[x, y];
                }
                else
                {
                    GameObject targetCandy = grid[x, y];
                    if (IsAdjacent(selectedCandy, targetCandy))
                    {
                        StartCoroutine(SwapAndCheck(selectedCandy, targetCandy));
                    }
                    selectedCandy = null;
                }
            }
        }
    }

    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    bool IsAdjacent(GameObject candy1, GameObject candy2)
    {
        Vector2 pos1 = candy1.transform.position;
        Vector2 pos2 = candy2.transform.position;
        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) || (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);
    }

    IEnumerator SwapAndCheck(GameObject candy1, GameObject candy2)
    {
        Vector2 pos1 = candy1.transform.position;
        Vector2 pos2 = candy2.transform.position;

        
        candy1.transform.position = pos2;
        candy2.transform.position = pos1;

        
        int x1 = Mathf.RoundToInt(pos1.x);
        int y1 = Mathf.RoundToInt(pos1.y);
        int x2 = Mathf.RoundToInt(pos2.x);
        int y2 = Mathf.RoundToInt(pos2.y);
        grid[x1, y1] = candy2;
        grid[x2, y2] = candy1;

        yield return new WaitForSeconds(0.5f); 

       
        bool candiesMoved = false;
        if (CheckMatch(x1, y1))
        {
            candiesMoved = true;
        }
        if (CheckMatch(x2, y2))
        {
            candiesMoved = true;
        }

        if (candiesMoved)
        {
            yield return new WaitForSeconds(0.5f); 
            CollapseAndRefill(); 

            yield return new WaitForSeconds(0.5f); 
            while (CheckAllMatches())
            {
                yield return new WaitForSeconds(0.5f); 
                CollapseAndRefill(); 
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
          
            candy1.transform.position = pos1;
            candy2.transform.position = pos2;
            grid[x1, y1] = candy1;
            grid[x2, y2] = candy2;
        }
    }

    bool CheckMatch(int x, int y)
    {
        int candyType = grid[x, y].GetComponent<Candy>().type;

        
        List<GameObject> horizontalMatches = GetMatchesInDirection(x, y, 1, 0); 
        horizontalMatches.AddRange(GetMatchesInDirection(x, y, -1, 0)); 

        
        List<GameObject> verticalMatches = GetMatchesInDirection(x, y, 0, 1); 
        verticalMatches.AddRange(GetMatchesInDirection(x, y, 0, -1)); 

       
        if (horizontalMatches.Count >= 2 || verticalMatches.Count >= 2)
        {
            List<GameObject> matches = new List<GameObject>();
            matches.Add(grid[x, y]);

            
            if (horizontalMatches.Count >= 2)
            {
                matches.AddRange(horizontalMatches);
            }
            if (verticalMatches.Count >= 2)
            {
                matches.AddRange(verticalMatches);
            }

            
            matches = matches.Distinct().ToList();

           
            foreach (GameObject match in matches)
            {
                int matchX = Mathf.RoundToInt(match.transform.position.x);
                int matchY = Mathf.RoundToInt(match.transform.position.y);
                grid[matchX, matchY] = null;
                Destroy(match);
            }

            return true;
        }

        return false;
    }

    List<GameObject> GetMatchesInDirection(int x, int y, int dx, int dy)
    {
        List<GameObject> matches = new List<GameObject>();
        GameObject startCandy = grid[x, y];

        if (startCandy == null)
        {
            return matches;
        }

        int type = startCandy.GetComponent<Candy>().type;

        for (int i = 1; i <= 2; i++)
        {
            int newX = x + i * dx;
            int newY = y + i * dy;

            if (IsValidPosition(newX, newY) && grid[newX, newY] != null &&
                grid[newX, newY].GetComponent<Candy>().type == type)
            {
                matches.Add(grid[newX, newY]);
            }
            else
            {
                break;
            }
        }

        return matches;
    }

    bool CheckAllMatches()
    {
        bool matchFound = false;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null && CheckMatch(x, y))
                {
                    matchFound = true;
                }
            }
        }
        return matchFound;
    }

    void CollapseAndRefill()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int i = y + 1; i < height; i++)
                    {
                        if (grid[x, i] != null)
                        {
                            grid[x, i].transform.position = new Vector2(x, y);
                            grid[x, y] = grid[x, i];
                            grid[x, i] = null;
                            break;
                        }
                    }
                    if (grid[x, y] == null)
                    {
                        Vector2 position = new Vector2(x, y);
                        int candyIndex = Random.Range(0, candies.Length);
                        GameObject candy = Instantiate(candies[candyIndex], position, Quaternion.identity);
                        candy.GetComponent<Candy>().type = candyIndex;
                        grid[x, y] = candy;
                    }
                }
            }
        }
    }
}
