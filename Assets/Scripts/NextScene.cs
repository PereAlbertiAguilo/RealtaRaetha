using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class NextScene : MonoBehaviour
{
    public SceneReference sceneName;

    [SerializeField] float delay = 1.5f;
    [SerializeField] Vector2 direction;
    [SerializeField] Vector3 nextSceneStartPos;
    PlayerController pc;

    [SerializeField] Animator animator;

    [SerializeField] bool isDoor = false;
    [SerializeField, HideInInspector] TextMeshProUGUI entryText;
    [SerializeField, HideInInspector] float crossFadeSpeed = 3f;

    bool enter = false;

#if UNITY_EDITOR
    [CustomEditor(typeof(NextScene)), CanEditMultipleObjects]
    public class NextSceneEditor : Editor
    {
        SerializedProperty entryText, crossFadeSpeed;

        protected virtual void OnEnable()
        {
            setvars();
        }

        void setvars()
        {
            entryText = serializedObject.FindProperty("entryText");
            crossFadeSpeed = serializedObject.FindProperty("crossFadeSpeed");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            NextScene nextScene = (NextScene)target;

            if (nextScene.isDoor)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(entryText);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(crossFadeSpeed);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        StartCoroutine(SetStartTrigger());
    }

    private void Update()
    {
        if(pc.verticalInput > pc.yAxisThreshold && pc.horizontalInput < pc.moveAxisThreshold && pc.isGrounded && enter)
        {
            enter = false;
            DeactivatePlayer();
            StartCoroutine(ChangeScene.instance.ChangeSceneFunc(delay, sceneName, false, nextSceneStartPos));
        }
    }

    IEnumerator SetStartTrigger()
    {
        GetComponent<BoxCollider2D>().isTrigger = false;

        yield return new WaitForSeconds(1f);

        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    IEnumerator SetTrigger()
    {
        yield return new WaitForSeconds(.25f);

        GetComponent<BoxCollider2D>().isTrigger = false;
    }

    IEnumerator EnterTextCrossFade(float speed)
    {
        Color color = entryText.color;

        entryText.gameObject.SetActive(true);

        enter = true;

        while (color.a < 1 && enter)
        {
            color.a += Time.deltaTime * speed;
            entryText.color = color;

            yield return null;
        }

        color.a = enter ? 1 : color.a;

        entryText.color = color;

        entryText.gameObject.SetActive(true);
    }

    IEnumerator ExitTextCrossFade(float speed)
    {
        Color color = entryText.color;

        entryText.gameObject.SetActive(true);

        enter = false;

        while (color.a > 0 && !enter)
        {
            color.a -= Time.deltaTime * speed;
            entryText.color = color;

            yield return null;
        }

        color.a = !enter ? 0 : color.a;

        entryText.color = color;

        entryText.gameObject.SetActive(false);
    }

    void DeactivatePlayer()
    {
        pc.gravityScale = 0;
        pc.enabled = false;
        pc.transform.GetComponent<Animator>().StopPlayback();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!isDoor)
            {
                StartCoroutine(ChangeScene.instance.ChangeSceneFunc(delay, sceneName, false, nextSceneStartPos));
                StartCoroutine(SetTrigger());

                if (pc != null)
                {
                    PlayerPrefs.SetInt("flip", pc.GetComponent<SpriteRenderer>().flipX ? 1 : 0);

                    DeactivatePlayer();

                    pc.GetComponent<Rigidbody2D>().AddForce(direction * 20, ForceMode2D.Impulse);
                }
            }
            else
            {
                StartCoroutine(EnterTextCrossFade(crossFadeSpeed));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isDoor)
        {
            StartCoroutine(ExitTextCrossFade(crossFadeSpeed));
        }
    }
}
