using UnityEngine;

namespace Assets._Game.Scenes
{
    public class DrawnBackground : MonoBehaviour
    {
        public float speedX = 50f;
        public float speedY = 50f;
        private RectTransform _rect;

        public void Start()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void Update()
        {
            _rect.anchoredPosition += new Vector2(speedX, speedY) * Time.deltaTime;
        }
    }
}
