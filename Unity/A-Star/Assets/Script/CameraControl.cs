using UnityEngine;

/// <summary>
/// Controls the camera so you can fly over the map
/// </summary>
public class CameraControl : MonoBehaviour
{
    float Speed = 4;
    
    /// <summary>
    /// Move the camera
    /// </summary>
    void Update()
    {
        transform.Translate(
            Input.GetAxis("Horizontal") * Speed * Time.deltaTime,
            0,
            Input.GetAxis("Vertical") * Speed * Time.deltaTime,
            Space.World);
    }
}
