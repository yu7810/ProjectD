using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// The UI camera survives room changes; each newly loaded room has its own base camera.
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class ProjectDUrpCameraStack : MonoBehaviour
{
    private Camera overlay;
    private Camera currentBase;

    private void OnEnable()
    {
        overlay = GetComponent<Camera>();
        SceneManager.sceneLoaded += SceneLoaded;
        Bind();
    }

    private void Start() => Bind();
    private void SceneLoaded(Scene scene, LoadSceneMode mode) => Bind();

    private void Bind()
    {
        if (overlay == null) return;
        Camera next = Camera.main;
        if (next == overlay) next = null;
        if (currentBase != null && currentBase != next)
            currentBase.GetUniversalAdditionalCameraData().cameraStack?.Remove(overlay);
        currentBase = next;
        if (currentBase == null) return;
        var overlayData = overlay.GetUniversalAdditionalCameraData();
        overlayData.renderType = CameraRenderType.Overlay;
        var stack = currentBase.GetUniversalAdditionalCameraData().cameraStack;
        if (stack != null && !stack.Contains(overlay)) stack.Add(overlay);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (currentBase != null) currentBase.GetUniversalAdditionalCameraData().cameraStack?.Remove(overlay);
        currentBase = null;
    }
}
