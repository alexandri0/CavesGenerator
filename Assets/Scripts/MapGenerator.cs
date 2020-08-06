using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MapGenerator : MonoBehaviour
{
    public bool MainLevelTilemap;

    public Controller sr;

    [Range(0,100)]
    public int randomFillPercent = 45;
    public string seed;
    public bool useRandomSeed = true;

    public int smoothIterCount = 5;
    public Tile mainTile;

    public Tile[] downRightRamp;
    public Tile[] downLeftRamp;

    public Tile[] topRightRamp;
    public Tile[] topLeftRamp;

    public Tile[] stalactites;

    public Tile[] leftWall;
    public Tile[] rightWall;

    public Tile[] ground;

    public Tile[] precipiceLeft;
    public Tile[] precipiceRight;

    public Tile[] grass;

    public Tilemap decorativeTileMap;


    int[,] map;
    int width;
    int height;
    System.Random random;
    Tilemap tilemap;

    struct Neighbors
    {
        public int TopLeft;
        public int Top;
        public int TopRight;
        public int Left;
        public int Right;
        public int BotLeft;
        public int Bottom;
        public int BotRight;
    }

    private void Start()
    {
        sr.Loading += 1;
        if (useRandomSeed)
            seed = UnityEngine.Random.Range(0f, 99999f).ToString();
        random = new System.Random(seed.GetHashCode());

        tilemap = GetComponent<Tilemap>();
        width = tilemap.size.x;
        height = tilemap.size.y;

        GeneratMap(width, height);
        sr.Loading -= 1;
    }

    private void GeneratMap(int width, int height)
    {
        map = new int[width, height];
        FillMap();
        
        for (int i = 0; i < smoothIterCount; i++)
        {
            SmoothMap();
        }

        FillTileMap();
        DecorateTileMap();

        if(MainLevelTilemap)
        {
            var tmc = gameObject.AddComponent<TilemapCollider2D>();
            gameObject.AddComponent<CompositeCollider2D>();
            tmc.usedByComposite = true;
            tmc.attachedRigidbody.bodyType = RigidbodyType2D.Static;
        }
    }

    // smoothing based on 8 neighbor around
    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 1; y < height-1; y++)
            {
                int ncount = GetNeighborCount(x, y);
                if (ncount > 4)
                    map[x, y] = 1;
                else if(ncount < 4)
                    map[x, y] = 0;
            }
    }

    private int GetNeighborCount(int cx,int cy)
    {
        int ncount = 0;
        for (int neighborX = cx-1; neighborX <= cx+1; neighborX++)
            for (int neighborY = cy - 1; neighborY <= cy + 1; neighborY++)
            {
                if (neighborX == cx && neighborY == cy)
                    continue;
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    ncount += map[neighborX, neighborY];
            }
        
        return ncount;
    }

    //fill map with random values based on @randomFillPercent 
    private void FillMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1)
                    map[x, y] = 0;
                else if (y == 0 || y == height - 1)
                    map[x, y] = 1;
                else
                    map[x, y] = random.Next(0, 100) < randomFillPercent ? 1 : 0;
            }
    }

    private void _SetTile(int x, int y, Tile t)
    {
        tilemap.SetTile(new Vector3Int(tilemap.cellBounds.xMin + x, tilemap.cellBounds.yMin + y, 0), t);
    }

    private void FillTileMap()
    {
        Tile tile = mainTile;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y] == 1)
                    _SetTile(x, y, tile);
    }

    private void DecorateTileMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int current = map[x, y];
                var neighbors = GetNeighbors(x, y);
                if(current == 0)
                {
                    if (neighbors.Top == 1 && neighbors.TopLeft == 1 && neighbors.TopRight == 1)
                        if (decorativeTileMap != null)
                            decorativeTileMap.SetTile(new Vector3Int(tilemap.cellBounds.xMin + x, tilemap.cellBounds.yMin + y, 0), GetRandomTile(stalactites));
                        else
                            _SetTile(x, y, GetRandomTile(stalactites));
                    else if (neighbors.Bottom == 1 && neighbors.BotLeft == 1 && neighbors.BotRight == 1)
                        if (decorativeTileMap != null)
                            decorativeTileMap.SetTile(new Vector3Int(tilemap.cellBounds.xMin + x, tilemap.cellBounds.yMin + y, 0), GetRandomTile(grass));
                        else
                            _SetTile(x,y,GetRandomTile(grass));
                }
                else
                {
                    if (neighbors.Top == 0)
                    {
                        if (neighbors.Bottom == 1)
                        {
                            if (neighbors.Right == 1)
                                if (neighbors.Left == 1)
                                    _SetTile(x, y, GetRandomTile(ground));
                                else
                                    _SetTile(x, y, GetRandomTile(downLeftRamp));
                            else if (neighbors.Left == 1)
                                _SetTile(x, y, GetRandomTile(downRightRamp));
                        }
                        else
                        {
                            if(neighbors.Right == 0)
                                _SetTile(x, y, GetRandomTile(precipiceRight));
                            else if (neighbors.Left == 0)
                                _SetTile(x, y, GetRandomTile(precipiceLeft));
                            else
                                _SetTile(x, y, GetRandomTile(ground));
                        }
                    }
                    else
                    {
                        if (neighbors.Bottom == 1)
                        {
                            if (neighbors.Right == 1)
                            {
                                if (neighbors.Left == 0)
                                    _SetTile(x, y, GetRandomTile(leftWall));
                            }
                            else if (neighbors.Left == 1)
                            {
                                if (neighbors.Right == 0)
                                    _SetTile(x, y, GetRandomTile(rightWall));
                            }
                        }
                        else
                        {
                            if (neighbors.Right == 1)
                            {
                                if (neighbors.Left == 0)
                                    _SetTile(x, y, GetRandomTile(topLeftRamp));
                            }
                            else if (neighbors.Left == 1)
                                _SetTile(x, y, GetRandomTile(topRightRamp));
                        }
                    }
                }
            }
    }

    //getting neighbors
    private Neighbors GetNeighbors(int x, int y)
    {
        int[] values = new int[8];
        values.DefaultIfEmpty(0);
        int i = 0;
        for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                if (neighborX == x && neighborY == y)
                    continue;
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    values[i++] = map[neighborX, neighborY];
                else
                    i++;
            }
        Neighbors n = new Neighbors()
        {
            BotLeft = values[0],
            Left = values[1],
            TopLeft = values[2],
            Bottom = values[3],
            Top = values[4],
            BotRight = values[5],
            Right = values[6],
            TopRight = values[7]
        };
        return n;
    }

    //getting random tile from group of same type tiles
    private Tile GetRandomTile(Tile[] group)
    {
        if (group.Length == 0)
            return null;
        int val = random.Next(group.Length);
        return group[val];
    }
}
