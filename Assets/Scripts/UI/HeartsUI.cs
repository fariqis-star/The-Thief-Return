using RunAndGun.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace RunAndGun.UI
{
    /// <summary>
    /// Draws a row of hearts for a Health component. Full hearts = current HP,
    /// empty (gray) hearts = missing HP. Assumes 1 heart = 1 HP.
    ///
    /// It spawns one Image per max HP under this object (put a Horizontal Layout Group
    /// on this same object so they line up automatically), then swaps each heart's sprite
    /// between the full and empty sprite whenever health changes.
    /// </summary>
    public class HeartsUI : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("The Health to display. Usually the Player's Health component.")]
        [SerializeField] private Health _health;

        [Header("Heart Sprites")]
        [Tooltip("Sprite for a full (alive) heart.")]
        [SerializeField] private Sprite _fullHeart;

        [Tooltip("Sprite for an empty (lost) heart.")]
        [SerializeField] private Sprite _emptyHeart;

        [Header("Layout")]
        [Tooltip("Prefab/template for a single heart Image. If empty, plain Images are created. " +
                 "Give this a fixed size (e.g. via the layout group / LayoutElement).")]
        [SerializeField] private Image _heartTemplate;

        [Tooltip("Pixel size of each heart if no template is provided.")]
        [SerializeField] private Vector2 _heartSize = new Vector2(48f, 48f);

        // The heart Images we created, one per max HP.
        private Image[] _hearts;

        private void OnEnable()
        {
            if (_health != null) _health.OnHealthChanged += Redraw;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnHealthChanged -= Redraw;
        }

        private void Start()
        {
            if (_health == null)
            {
                Debug.LogWarning($"{name}: no Health assigned to HeartsUI.", this);
                return;
            }

            BuildHearts(_health.Max);
            Redraw(_health.Current, _health.Max);
        }

        /// <summary>Creates one heart Image per max HP.</summary>
        private void BuildHearts(int max)
        {
            _hearts = new Image[max];

            for (int i = 0; i < max; i++)
            {
                Image heart;

                if (_heartTemplate != null)
                {
                    heart = Instantiate(_heartTemplate, transform);
                    heart.gameObject.SetActive(true);
                }
                else
                {
                    // Create a bare Image if no template was provided.
                    GameObject go = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(transform, false);
                    heart = go.GetComponent<Image>();
                    heart.rectTransform.sizeDelta = _heartSize;
                }

                heart.sprite = _fullHeart;
                _hearts[i] = heart;
            }
        }

        /// <summary>Sets each heart to full or empty based on current HP.</summary>
        private void Redraw(int current, int max)
        {
            if (_hearts == null) return;

            for (int i = 0; i < _hearts.Length; i++)
            {
                // Hearts below the current HP count are full; the rest are empty.
                _hearts[i].sprite = i < current ? _fullHeart : _emptyHeart;
            }
        }
    }
}
