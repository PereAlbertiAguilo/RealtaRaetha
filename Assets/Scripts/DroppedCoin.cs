using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DroppedCoin : MonoBehaviour
{
    Rigidbody2D rb;

    public float dropForce;

    public int coinValue = 1;

    public int bounceIndex = 0;
    public int bounceForce = 3;
    enum CoinValues
    {
        One,
        Five,
        Ten
    };

    [SerializeField] CoinValues coinValues;

#if UNITY_EDITOR
    [CustomEditor(typeof(DroppedCoin)), CanEditMultipleObjects]
    public class DroppedCoinEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DroppedCoin droppedCoin = (DroppedCoin)target;

            switch (droppedCoin.coinValues)
            {
                case CoinValues.One:

                    droppedCoin.coinValue = 1;

                    break;
                case CoinValues.Five:

                    droppedCoin.coinValue = 5;

                    break;
                case CoinValues.Ten:

                    droppedCoin.coinValue = 10;

                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        float x = Random.Range(-.5f, .5f) * (dropForce / 1.5f);
        float y = Random.Range(.5f, 1.2f) * dropForce;

        Vector2 dir = new Vector2(x, y);

        rb.AddForce(dir, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounceIndex >= 3)
        {
            bounceIndex = 0;
            rb.velocity = Vector2.zero;

            return;
        }

        rb.AddForce(Vector2.up * bounceForce);

        bounceForce /= 2;
        bounceIndex++;
    }
}
