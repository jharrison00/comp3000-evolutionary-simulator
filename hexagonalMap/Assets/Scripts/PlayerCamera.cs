using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float lookSpeedH = 2f;
    public float lookSpeedV = 2f;
    public float dragSpeed = 10f;
    public float moveSpeedH = 10f;
    public float moveSpeedV = 10f;
    public float sidePadding = 10f;

    private float rotX = 0f;
    private float rotY = 0f;

    private float boardWidth, boardHeight;

    private void Awake()
    {
        boardWidth = (sidePadding + (HexGrid.Instance.hexWidth * HexGrid.Instance.gridWidth) / 2);
        boardHeight = (sidePadding + (HexGrid.Instance.hexHeight * HexGrid.Instance.gridHeight) / 2);
    }

    void Update()
    {
        //Look around with Right Mouse
        if (Input.GetMouseButton(1))
        {
            rotX += lookSpeedH * Input.GetAxis("Mouse X");
            rotY -= lookSpeedV * Input.GetAxis("Mouse Y");
            rotY = Mathf.Clamp(rotY, -90f, 90f);

            transform.eulerAngles = new Vector3(rotY, rotX, 0f);
        }

        //When horizontal keys are pressed
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float hMove = Input.GetAxisRaw("Horizontal") * Time.deltaTime * moveSpeedH;
            transform.Translate(hMove, 0, 0);
        }

        //When vertical keys are pressed
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            float vMove = Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeedV;
            transform.Translate(0, 0, vMove);
        }

        // Clamped to be just out of board limits
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0 - boardWidth, boardWidth);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, 10, 50);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, 0 - boardHeight, boardHeight);

        transform.position = clampedPosition;
    }
}