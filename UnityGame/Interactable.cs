using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

[Serializable]
abstract public class Interactable
{
    [SerializeField]
    public float maxHealth;
    [SerializeField]
    public float currentHealth;

    [SerializeField]
    public Vector3Int location;

    [SerializeField]
    public bool dropsItem;

    [SerializeField]
    public Interactable droppedItemInteractable;

    [SerializeField]
    public TileBase droppedItemTile;

    public void SetHealth(float health)
    {
        currentHealth = health;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public abstract void DropItem();

    public abstract void OnRemoval();
}
