using UnityEngine;

namespace Assets._Game.Scenes
{
    public class MenuMusic : MonoBehaviour
    {
        [SerializeField]
        public AudioSource source;

        public void Start()
        {
            source.Play();
        }

        public void Update()
        {

        }
    }
}
