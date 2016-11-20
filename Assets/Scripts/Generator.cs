using UnityEngine;
using Lib;
using System.Collections;

public class Generator : MonoBehaviour {

    // Map generation variables
    public int x = 100, y = 100, difficulty = 30;
    public float x_offset = 0.66f, y_offset = 0.66f; // offset value of each tile
    public float x_origin = 0f, y_origin = 0f; // origin of tile (0,0)
    public float tileDepth = 0f;
    // reference to the map instance created
    public Map map;
    // array of sprites for tiles
    public Sprite[] tileSprites;
    // Prefab of tile gameObject
    public GameObject tilePrefab;

	// Use this for initialization
	void Start ()
    {
        // create a new map object (calls generate())
        map = new Map(x,y,difficulty);
	}

	// Update is called once per frame
	void Update ()
    {
        // If space bar is pressed, generate a new map
	    if(Input.GetKeyDown(KeyCode.Space))
        {
            map.generate(x,y,difficulty);
            InstantiateMap(map);
        }
	}

    // Goes through each element in the map matrix and instantiates the gameObjects
    public void InstantiateMap(Map map)
    {
        // iterate through the matrix, i = y coord j = x coord
        for(int i = 0; i < map.maxX(); i++)
        {
            for(int j = 0; j < map.maxY(); j++)
            {
                // if the element at (i,j) is a tile, find its correct sprite and instantiate it
                if(map.softCheckFlag(i,j,TILE_T.SOLID) )
                {
                    // returns 0 for now
                    int spriteIndex = FindCorrectTileSprite(i, j);
                    Vector3 position = MatrixToWorldSpace(x_origin, y_origin, x_offset, y_offset, i, j, tileDepth);
                    GameObject tileRef = Instantiate(tilePrefab, position, Quaternion.identity) as GameObject;
                    tileRef.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];

                    if(map.softCheckFlag(i,j,TILE_T.STICKY)){
                      // The tile is STICKY
                    } else if(map.softCheckFlag(i,j,TILE_T.SLIPPERY)){
                      // The tile is SLIPPERY
                    }
                } else if(map.softCheckFlag(i,j,TILE_T.SPAWN_POINT)){
                  // Do spawn point code here
                } else if(map.softCheckFlag(i,j,TILE_T.EXIT_POINT)){
                  // Do exit point code here
                } else if(map.softCheckFlag(i,j,TILE_T.HAZARD) )
                {
                    // Do general hazard code here

                    if(map.softCheckFlag(i,j,TILE_T.SPIKE)){
                      // The trap is a spike
                    }

                    else if(map.softCheckFlag(i,j,TILE_T.SCORE)){
                      // The trap gives a score
                    }
                }
            }
        }
    }

    // Find the correct sprite for a tile
    public int FindCorrectTileSprite(int x, int y)
    {
        return 0;
    }

    // Returns the vector3 position of a give tile in the matrix
    public Vector3 MatrixToWorldSpace(float xOrg, float yOrg, float xOffset, float yOffset, int x, int y, float z)
    {
        Vector3 position = new Vector3(xOrg + xOffset * (float)x, yOrg + yOffset * (float)y, z);
        return position;
    }
}
