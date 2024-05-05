using System.Collections;
using UnityEngine;

namespace Characters.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 _target;
        
        private void Start()
        {
            StartCoroutine(LifeTime());
            
            var objects = GameObject.FindGameObjectWithTag("Player");
            _target = objects.transform.position + new Vector3(0, 1, 0);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, 20f * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }
        
        private IEnumerator LifeTime()
        {
            var time = 0;

            while (time <= 6)
            {
                yield return new WaitForSeconds(1);
                time ++;
            }
            
            Destroy(gameObject);
        }
    }
}
