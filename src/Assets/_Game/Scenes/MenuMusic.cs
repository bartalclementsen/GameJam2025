using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
