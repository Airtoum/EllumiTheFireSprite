using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkNotifier : MonoBehaviour
{

    [SerializeField] private float animation_time = 0.4f;
    private float animation_timer = 0;

    void Start()
    {
        Appear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Appear()
    {
        animation_timer = 0;
        StartCoroutine(AnimateEnter());
    }

    public void GoAway()
    {
        animation_timer = 0;
        StartCoroutine(AnimateExit());
    }

    private IEnumerator AnimateExit()
    {
        while (animation_timer < animation_time) {
            animation_timer += Time.deltaTime;
            float percent = animation_timer / animation_time;
            transform.localScale = Vector3.one * (1 - percent);
            yield return null;
        }
        Destroy(gameObject);
    }
    
    private IEnumerator AnimateEnter()
    {
        while (animation_timer < animation_time) {
            animation_timer += Time.deltaTime;
            float percent = animation_timer / animation_time;
            transform.localScale = Vector3.one * percent;
            yield return null;
        }
    }
}
