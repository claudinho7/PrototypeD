using System.Collections;
using UnityEngine;

namespace Characters.Projectiles
{
    public class PlayerProjectile : MonoBehaviour
    {
        private Vector3 _target;
        private EnemyController _monster;
        
        private void Start()
        {
            StartCoroutine(LifeTime());
            
            _monster = GameObject.FindGameObjectWithTag("Monster").GetComponent<EnemyController>();
            
            _monster.shouldDodge = true;
            
            _target = _monster.transform.position + new Vector3(0, 1, 0);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, 30f * Time.deltaTime);
            transform.LookAt(_target);
        }

        private void OnTriggerEnter(Collider other)
        {
            _monster.shouldDodge = false;
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
            _monster.shouldDodge = false;
            
            Destroy(gameObject);
        }
    }
}
