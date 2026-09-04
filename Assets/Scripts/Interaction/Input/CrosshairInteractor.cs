using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interaction.Actions;
using Assets.Scripts.Context;
using Assets.Scripts.Interaction.Interfaces;

namespace Assets.Scripts.Interaction.Input
{
    /// <summary>Manages the crosshair UI and detects interactable objects under the screen center.</summary>
    public sealed class CrosshairInteractor : MonoBehaviour
    {
        private Texture2D _crosshairTexture;
        [SerializeField] private RawImage _crosshairImage;
        [SerializeField] private float _distance = 3f;
        [SerializeField] private LayerMask _interactableLayerMask = ~0;

        /// <summary>
        /// Set by GameDriver on Awake() — explicit dependency injection.
        /// </summary>
        public ActionControl ActionControlRef { get; set; }

        private void Start()
        {
            if (_crosshairTexture == null)
            {
                _crosshairTexture = new Texture2D(1, 1);
                _crosshairTexture.SetPixel(0, 0, Color.white);
                _crosshairTexture.Apply();
            }

            if (_crosshairImage != null)
                _crosshairImage.texture = _crosshairTexture;
        }

        private void Update()
        {
            var ctx = ActionControlRef.Context;
            var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out var hitInfo, _distance, _interactableLayerMask))
            {
                var interactables = hitInfo.collider.GetComponentsInParent<IInteractable>();
                if (interactables.Length > 0)
                {
                    ctx.InteractObject = interactables[0];

                    if (_crosshairImage != null)
                    {
                        _crosshairImage.enabled = true;
                        _crosshairImage.color = Color.green;
                    }
                    return;
                }
            }

            ctx.InteractObject = null;

            if (_crosshairImage != null)
            {
                _crosshairImage.enabled = true;
                _crosshairImage.color = Color.white;
            }
        }
    }
}