using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileDatas : ScriptableObject
{
    public List<TileBase> tiles;
    public float walkingSpeed;
}
