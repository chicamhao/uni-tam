using Actions;
using Interfaces;
using UnityEngine;

public sealed class CrosshairInteractor : MonoBehaviour
{
    public Texture2D CrosshairTexture;
    public Vector2 CrosshairSize = new(32, 32);
    [SerializeField] private float _distance = 3f;
    [SerializeField] private LayerMask _interactableLayerMask = ~0;

    private ActionControl _actionControl;
    private bool _hovering;

    private void Start()
    {
        _actionControl = FindAnyObjectByType<ActionControl>();

        if (CrosshairTexture == null)
        {
            CrosshairTexture = new Texture2D(1, 1);
            CrosshairTexture.SetPixel(0, 0, Color.white);
            CrosshairTexture.Apply();
        }
    }

    private void Update()
    {
        if (_actionControl == null)
        {
            _actionControl = FindAnyObjectByType<ActionControl>();
            if (_actionControl == null) return;
        }

        var ctx = _actionControl.Context;
        if (ctx == null) return;

        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        if (Physics.Raycast(ray, out var hitInfo, _distance, _interactableLayerMask))
        {
            var interactables = hitInfo.collider.GetComponentsInParent<IInteractable>();
            if (interactables.Length > 0)
            {
                ctx.InteractObject = interactables[0];
                _hovering = true;
                return;
            }
        }

        ctx.InteractObject = null;
        _hovering = false;
    }

    void OnGUI()
    {
        if (!enabled)
            return;

        var centerX = (Screen.width - CrosshairSize.x) / 2;
        var centerY = (Screen.height - CrosshairSize.y) / 2;
        GUI.color = _hovering ? Color.green : Color.white;
        GUI.DrawTexture(new Rect(centerX, centerY, CrosshairSize.x, CrosshairSize.y), CrosshairTexture);
    }
}