using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCreation : MonoBehaviour
{
    public GameObject tileBase;
    public Dictionary<string, WaveformTile> grid = new Dictionary<string, WaveformTile>();

    public Text widthTextObject;
    public Text heightTextObject;

    private int currentHeight = 0;
    private int currentWidth = 0;
    private int maximumHeight = 0;
    private int maximumWidth = 0;
    private int DEFAULT_WIDTH = 10;
    private int DEFAULT_HEIGHT = 10;

    private bool started = false;

    public static MapCreation global;

    // Start is called before the first frame update
    void Start()
    {
        print("MAP CREATION");
        //Application.targetFrameRate = 60;
        //GenerateGrid(3, 3);
        //GenerateGrid(200, 200);
        //GetComponent<WaveformCollapse>().PopulateGrid(grid, currentWidth, currentHeight);
        global = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            started = true;
            PostStart();
        }
    }

    public void PostStart()
    {
        widthTextObject.transform.parent.GetComponent<InputField>().text = "" + DEFAULT_WIDTH;
        heightTextObject.transform.parent.GetComponent<InputField>().text = "" + DEFAULT_HEIGHT;
        GenerateGrid(DEFAULT_WIDTH, DEFAULT_HEIGHT);
    }

    public void RegenerateGrid()
    {
        int newWidth, newHeight;
        string widthString = widthTextObject.text;
        string heightString = heightTextObject.text;
        if (!int.TryParse(widthString, out newWidth))
        {
            newWidth = DEFAULT_WIDTH;
        }
        if (!int.TryParse(heightString, out newHeight))
        {
            newHeight = DEFAULT_HEIGHT;
        }
        GenerateGrid(newWidth, newHeight);
    }

    public void GenerateGrid(int width, int height)
    {
        int dX = width - currentWidth;
        int dY = height - currentHeight;

        if (dX > 0)
        {
            //Add or enable tiles with higher X-value
            int xEnable = maximumWidth - currentWidth;
            int xExpand = 0;
            if (dX > xEnable)
                xExpand = dX - xEnable;
            else
                xEnable = dX;

            int enableHeight = Mathf.Min(maximumHeight, currentHeight + dY);

            //Enable loop
            //Start at i = 0 since currentWidth will be the first X-coordinate of the newly enabled tiles (currentWidth = 3 means the highest X-coordinate of any enabled tile should be 2)
            for (int i=0; i < xEnable; i++)
            {
                int newX = currentWidth + i;
                //Iterate over enableHeight since we wish to keep all tiles within the new Y-boundary but only enable tiles beyond the current X-boundary
                for (int j=0; j < enableHeight; j++)
                {
                    string posKey = GetPosKey(newX, j);
                    EnableTile(grid[posKey]);
                }
            }

            //Expansion loop
            //Start at i = 0 since maximumWidth will be the first X-coordinate of the newly created tiles (maximumWidth = 5 means the highest X-coordinate of any existing tile should be 4)
            for (int i=0; i < xExpand; i++)
            {
                int newX = maximumWidth + i;
                //Iterate over height since we wish to keep all tiles within the new Y-boundary but only enable tiles beyond the current X-boundary
                for (int j=0; j < height; j++)
                {
                    CreateTile(newX, j);
                }
            }
        }
        else if(dX < 0)
        {
            //We need to disable tiles
            //Start at i = dX since the first tile to be disabled needs to have a lower X-coordinate than currentWidth
            for (int i=dX; i < 0; i++)
            {
                int newX = currentWidth + i;
                //Iterate over currentHeight since new Y-space generated from a higher height is irrelevant as we are shrinking in X-space
                for (int j=0; j < currentHeight; j++)
                {
                    string posKey = GetPosKey(newX, j);
                    print("DISABLING " + posKey);
                    DisableTile(grid[posKey]);
                }
            }
        }

        //All relevant X-space has been handled, lets do the remaining Y-space
        //Ensure that currentWidth is greater than 0 in order to not do double creation
        if (dY > 0 && currentWidth > 0)
        {
            //Enable and Expand
            int yEnable = maximumHeight - currentHeight;
            int yExpand = 0;
            if (dY > yEnable)
                yExpand = dY - yEnable;
            else
                yEnable = dY;

            int relevantWidth = currentWidth;
            if (dX < 0)
            {
                relevantWidth = width;
            }

            //Enable loop
            //Iterate over yEnable and add to currentHeight since the first tile to be enabled should have currentHeight as its Y-coordinate
            for (int i = 0; i < yEnable; i++)
            {
                int newY = currentHeight + i;
                //Start at 0 and iterate over currentWidth since all X-space beyond width has been handled already
                for (int j = 0; j < relevantWidth; j++)
                {
                    string posKey = GetPosKey(j, newY);
                    EnableTile(grid[posKey]);
                }
            }

            int expandWidth = Mathf.Min(maximumWidth, currentWidth + dX);

            //Expand loop
            //Iterate over yExpand and add to maximumHeight since the first tile to be enabled should have maximumHeight as its Y-coordinate
            for (int i = 0; i < yExpand; i++)
            {
                int newY = maximumHeight + i;
                //Start at 0 and iterate over expandWidth since all Y-space above the X-space that was enabled has not been handled
                for (int j = 0; j < expandWidth; j++)
                {
                    CreateTile(j, newY);
                }
            }
        }
        else if (dY < 0)
        {
            //We need to disable tiles
            //Start at i = dY since the first tile to be disabled needs to have a lower Y-coordinate than currentHeight
            for (int i = dY; i < 0; i++)
            {
                int newY = currentHeight + i;
                //Start at 0 and iterate over width since all X-space beyond width has been handled already
                for (int j = 0; j < width; j++)
                {
                    string posKey = GetPosKey(j, newY);
                    DisableTile(grid[posKey]);
                }
            }
        }

        //At this point, all necessary tiles should either have been created, disabled or enabled to support the new size
        //Update current and maximum boundaries
        currentHeight = height;
        currentWidth = width;

        if(currentHeight > maximumHeight)
        {
            maximumHeight = currentHeight;
        }

        if (currentWidth > maximumWidth)
        {
            maximumWidth = currentWidth;
        }

        CameraHandler.SetCameraCenter(new Vector3(currentWidth / 2, currentHeight / 2, -Mathf.Max(currentWidth, currentHeight)));
        CameraHandler.CenterCamera();
        GetComponent<WaveformCollapse>().Setup(grid, currentWidth, currentHeight);
    }

    public void CreateTile(int x, int y)
    {
        GameObject newTile = GameObject.Instantiate(tileBase, new Vector3(x, y, 0), tileBase.transform.rotation);
        WaveformTile tile = newTile.GetComponent<WaveformTile>();
        tile.x = x;
        tile.y = y;
        grid[GetPosKey(x, y)] = tile;
    }

    public void EnableTile(WaveformTile obj)
    {
        obj.GetComponent<Renderer>().enabled = true;
    }

    public void DisableTile(WaveformTile obj)
    {
        obj.GetComponent<Renderer>().enabled = false;
    }

    public static string GetPosKey(int x, int y)
    {
        return $"{x},{y}";
    }

    private static WaveformTile Get(int x, int y)
    {
        return global.grid[GetPosKey(x, y)];
    }

    public static List<WaveformTile> GetNeighbors(WaveformTile tile)
    {
        return GetNeighbors(tile.x, tile.y);
    }

    public static List<WaveformTile> GetNeighbors(int x, int y)
    {
        List<WaveformTile> ret = new List<WaveformTile>();
        if (x > 0)
            ret.Add(Get(x - 1, y));
        if (y > 0)
            ret.Add(Get(x, y - 1));
        if (x + 1 < global.currentWidth)
            ret.Add(Get(x + 1, y));
        if (y + 1 < global.currentHeight)
            ret.Add(Get(x, y + 1));
        return ret;
    }

    public static int GetTotalCurrentTiles()
    {
        return global.currentWidth * global.currentHeight;
    }
}
