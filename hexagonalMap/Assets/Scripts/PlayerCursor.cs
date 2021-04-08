using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCursor : MonoBehaviour
{
    public GameObject selectedObject;
    public GameObject highlightedObject;

    private GameObject currentHighlightedObject;
    private GameObject currentSelectedObject;

    public Shader outlineShader;
    public Shader standardShader;
    public LayerMask selectableLayer;

    void Update()
    {
        if (!PauseMenu.isPaused)
        {
            GetObjects();
            DoOutlining();
        }
    }

    private void GetObjects()
    {
        RaycastHit hitData;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitData, 1000, selectableLayer))
        {
            highlightedObject = hitData.transform.gameObject;

            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = hitData.transform.gameObject;
                SelectedAnimalStatsUpdater.Instance.SetPanel(selectedObject);
            }
        }
        else
        {
            highlightedObject = null;          

            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    selectedObject = null;
                    SelectedAnimalStatsUpdater.Instance.RemovePanel();
                }
            }
        }
    }

    private void DoOutlining()
    {
        if (selectedObject != null) 
        {
            if (currentSelectedObject != selectedObject && currentSelectedObject != null)
            {
                ApplyShaderToObject(selectedObject, outlineShader);
                ApplyShaderToObject(currentSelectedObject, standardShader);
            }
            if (currentSelectedObject == null)
            {
                ApplyShaderToObject(selectedObject, outlineShader);
            }
            currentSelectedObject = selectedObject;
        }
        else if (currentSelectedObject != null)
        {
            ApplyShaderToObject(currentSelectedObject, standardShader);
            currentSelectedObject = null;
        }

        if (highlightedObject != null)
        {
            if (currentHighlightedObject != highlightedObject && currentHighlightedObject != null)
            {
                ApplyShaderToObject(highlightedObject, outlineShader);
                if (currentHighlightedObject != currentSelectedObject)
                {
                    ApplyShaderToObject(currentHighlightedObject, standardShader);
                }
            }
            if (currentHighlightedObject == null)
            {
                ApplyShaderToObject(highlightedObject, outlineShader);
            }
            currentHighlightedObject = highlightedObject;
        }
        else if (currentHighlightedObject != null && currentHighlightedObject != currentSelectedObject)  
        {
            ApplyShaderToObject(currentHighlightedObject, standardShader);
            currentHighlightedObject = null;
        }
    }

    private void ApplyShaderToObject(GameObject gameObject,Shader shader)
    {
        SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>();
        Material material = skin.material;
        material.shader = shader;

    }
}