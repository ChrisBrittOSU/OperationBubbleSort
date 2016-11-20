using UnityEngine;
using Lib;
using UnityEngine.UI;
using System.Collections;

public class Generator : MonoBehaviour {

    // Map generation variables
    public int x = 100, y = 100, difficulty = 50;
    public float x_offset = 0.66f, y_offset = 0.66f; // offset value of each tile
    public float x_origin = 0f, y_origin = 0f; // origin of tile (0,0)
    public float tileDepth = 0f, playerDepth = -1f;
    // reference to the map instance created
    public Map map;
    // array of sprites/materials for tiles
    public Sprite[] tileSprites;
    public Material[] tileMaterials;
    public PhysicsMaterial2D[] tilePhysicsMaterials;
    // Prefab of tile gameObject
    public GameObject tilePrefab;
    public GameObject spikePrefab;
    public GameObject spawnPoint;
    public GameObject exitPoint;
    // Prefab of player object
    public GameObject playerPrefab;
    // Ref of player
    public GameObject player;

    // parent transform for all instantiated objects
    public GameObject prefabParentPrefab;
    public Transform prefabParent;
    public Vector3 spawnLocation;

    // text for score display
    public Text scoreText;

	// Use this for initialization
	void Start ()
    {
        // create a new map object (calls generate())
        map = new Map(x,y,difficulty);
	}

	// Update is called once per frame
	void Update ()
    {
        if(player.GetComponent<Player>().gameOver)
        {
            player.transform.position = spawnLocation;
            player.GetComponent<Player>().gameOver = false;
            difficulty = difficulty > 0 ? difficulty - 1 : 0;
        }
        if(player.GetComponent<Player>().askForNewLevel)
        {
            difficulty += 10;
            InstantiateMap(map);
            player.GetComponent<Player>().askForNewLevel = false;
        }
        scoreText.text = "Score: " + (difficulty - 10);
	}

    // Goes through each element in the map matrix and instantiates the gameObjects
    public void InstantiateMap(Map map)
    {
        int defaultMat = Random.Range(0, 3);
        // iterate through the matrix, i = y coord j = x coord
        for(int i = 0; i < map.maxX(); ++i)
        {
            for(int j = 0; j < map.maxY(); ++j)
            {
                // if the element at (i,j) is a tile, find its correct sprite and instantiate it
                if(map.softCheckFlag(i,j,TILE_T.SOLID) )
                {
                    // returns 0 for now
                    int spriteIndex = (int)map.getFace(i, j);
                    GameObject tileRef = CreateNewTile(tilePrefab, x_origin, y_origin, x_offset, y_offset, i, j, tileDepth);
                    SpriteRenderer tileSprite = tileRef.GetComponent<SpriteRenderer>();
                    tileSprite.sprite = tileSprites[spriteIndex];

                    if(map.softCheckFlag(i,j,TILE_T.STICKY)){
                        tileRef.GetComponent<BoxCollider2D>().sharedMaterial = tilePhysicsMaterials[0];
                        tileSprite.material = tileMaterials[3];
                    } else if(map.softCheckFlag(i,j,TILE_T.SLIPPERY)){
                        tileRef.GetComponent<BoxCollider2D>().sharedMaterial = tilePhysicsMaterials[1];
                        tileSprite.material = tileMaterials[4];
                    }
                    else
                    {
                        tileRef.GetComponent<BoxCollider2D>().sharedMaterial = tilePhysicsMaterials[2];
                        tileSprite.material = tileMaterials[defaultMat];
                    }
                } else if(map.softCheckFlag(i,j,TILE_T.SPAWN_POINT)){
                    CreateNewTile(spawnPoint, x_origin, y_origin, x_offset, y_offset, i, j, tileDepth);
                    // if we have a reference to a player object, destory it and make a new one
                    if(player != null)
                    {
                        Destroy(player.gameObject);
                    }
                    Vector3 position = MatrixToWorldSpace(x_origin, y_origin, x_offset, y_offset, i, map.maxY() - j, playerDepth);
                    player = Instantiate(playerPrefab, position, Quaternion.identity) as GameObject;
                    spawnLocation = position;
                } else if(map.softCheckFlag(i,j,TILE_T.EXIT_POINT)){
                    CreateNewTile(exitPoint, x_origin, y_origin, x_offset, y_offset, i, j, tileDepth);
                } else if(map.softCheckFlag(i,j,TILE_T.HAZARD) )
                {
                    // Do general hazard code here
                    if(map.softCheckFlag(i,j,TILE_T.SPIKE)){
                        CreateNewTile(spikePrefab, x_origin, y_origin, x_offset, y_offset, i, j, tileDepth);
                    }

                    else if(map.softCheckFlag(i,j,TILE_T.SCORE)){
                      // The trap gives a score
                    }
                }
            }
        }
    }

    // creates a give tile prefab and returns a reference to it;
    public GameObject CreateNewTile(GameObject prefab, float xOrg, float yOrg, float xOffset, float yOffset, int x, int y, float z)
    {
        Vector3 position = MatrixToWorldSpace(xOrg,yOrg,xOffset,yOffset,x,map.maxY() - y,z);
        return Instantiate(prefab, position, Quaternion.identity, prefabParent) as GameObject;
    }

    // Returns the vector3 position of a give tile in the matrix
    public Vector3 MatrixToWorldSpace(float xOrg, float yOrg, float xOffset, float yOffset, int x, int y, float z)
    {
        Vector3 position = new Vector3(xOrg + xOffset * (float)x, yOrg + yOffset * (float)y, z);
        return position;
    }

    // Calls the Generator to create a new map
    public void Generate()
    {
        // If we have a reference here, then we must destroy it
        if (prefabParent)
        {
            Destroy(prefabParent.gameObject);
        }
        if (player)
        {
            Destroy(player.gameObject);
        }
        // generate a new matrix
        map.generate(x, y, difficulty);
        // create a new parent object and create a temporary reference
        GameObject refOfPrefabParent = Instantiate(prefabParentPrefab, transform.position, transform.rotation) as GameObject;
        // create a permanent reference to this parent transform
        prefabParent = refOfPrefabParent.transform;
        map.printFile("Grid.txt");
        InstantiateMap(map);
        // after the map has been created, let the player start the game
        player.GetComponent<Player>().gameOver = false;
    }
}
