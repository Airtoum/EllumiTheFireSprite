using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<GameObject> MaterialObjects = new List<GameObject>();
    [SerializeField] GameObject levelTileMap;

    [SerializeField] public float xCoordLow = 0;
    [SerializeField] public float xCoordHigh = 0;
    [SerializeField] public float yCoordLow = 0;
    [SerializeField] public float yCoordHigh = 0;

    void Start()
    {
        Renderer tileRenderer = levelTileMap.GetComponent<Renderer>();
        xCoordHigh = tileRenderer.bounds.max.x;
        yCoordHigh = tileRenderer.bounds.max.y;
        xCoordLow = tileRenderer.bounds.min.x;
        yCoordLow = tileRenderer.bounds.min.y;
    }

    void Update()
    {
        
    }
}