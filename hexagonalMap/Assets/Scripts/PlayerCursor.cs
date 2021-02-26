using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    public GameObject selectedObject;
    public GameObject highlightedObject;
    public Shader outlineShader;
    public Shader standardShader;
    public LayerMask selectableLayer;
    private Player player;
    private Player existingPlayer;

    Ray ray;
    RaycastHit hitData;

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitData, 1000, selectableLayer))
        {
            highlightedObject = hitData.transform.gameObject;

            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = hitData.transform.gameObject;              
            }
        }
        else
        {
            highlightedObject = null;

            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = null;
            }
        }
        OutlinePlayer();

    }

    private void OutlinePlayer()
    {
        if (selectedObject != null)
        {
            player = selectedObject.GetComponent<Player>();
            if (player != null)
            {
                player.material.shader = outlineShader;
                existingPlayer = player;
            }
            else if (player == null && existingPlayer != null)
            {
                existingPlayer.material.shader = standardShader;
            }
        }
    }

    public bool IsPlayerSelected()
    {
        if (selectedObject != null)
        {
            player = selectedObject.GetComponent<Player>();
            if (player != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

}