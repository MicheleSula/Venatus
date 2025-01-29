using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour
{
    public Light2D torchLight;
    public Light2D playerSpotlight;
    public Transform player;

    public float baseAngle = 45f;
    public float range = 10f;
    public float defaultIntensity = 1f;
    public float spotlightIntensity = 0.8f;

    private void Start()
    {
        if (torchLight == null)
        {
            Debug.LogError("Torch Light non assegnata! Assegna una Light2D.");
        }

        if (playerSpotlight == null)
        {
            Debug.LogError("Player Spotlight non assegnata! Assegna una Light2D.");
        }

        if (player == null)
        {
            Debug.LogError("Player non assegnato alla torcia!");
        }

        torchLight.lightType = Light2D.LightType.Point;
        torchLight.intensity = defaultIntensity;
        torchLight.pointLightOuterRadius = range;
        torchLight.pointLightInnerAngle = 0f;
        torchLight.pointLightOuterAngle = baseAngle;

        playerSpotlight.lightType = Light2D.LightType.Point;
        playerSpotlight.intensity = spotlightIntensity;
        playerSpotlight.pointLightOuterRadius = range / 2;
        playerSpotlight.enabled = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        FollowPlayer();
        RotateTorchToMouse();

        if (Input.GetKeyDown(KeyCode.F))
        {
            torchLight.enabled = !torchLight.enabled;
            playerSpotlight.enabled = torchLight.enabled;
        }
    }

    private void FollowPlayer()
    {
        if (player != null)
        {
            transform.position = player.position;
        }
    }

    private void RotateTorchToMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector3 direction = mousePosition - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Debug.DrawLine(transform.position, transform.position + direction * 5f, Color.yellow);
    }
}