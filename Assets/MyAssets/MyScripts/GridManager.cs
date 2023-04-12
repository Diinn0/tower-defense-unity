using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    
    [SerializeField] private TileScript _tilePrefab;

    [SerializeField] private Transform _cam;

    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        int a=0, b;
        for (int x = 0; x < _width; x++)
        {
            b = 0;
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3((x + a) - 655, (y + b) - 300), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x%2 != 0 && y%2 == 0);

                b += 65;
            }

            a += 65;
        }
        
        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }
}
