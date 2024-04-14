using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float damage = 10;
    [SerializeField] float destroyTimer = 20f;

    void Start()
    {
        Destroy(this.gameObject, destroyTimer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<HitTarget>().TakeDamage(damage);
        }

        Destroy(this.gameObject);
    }
}
