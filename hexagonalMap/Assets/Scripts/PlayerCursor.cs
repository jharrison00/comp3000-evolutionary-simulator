using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    public GameObject selectedObject;
    public GameObject highlightedObject;
    public LayerMask selectableLayer;

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

    }
}