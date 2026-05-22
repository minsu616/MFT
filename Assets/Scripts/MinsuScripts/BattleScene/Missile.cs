using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _elapsed;
    private bool _launched;

    public float arcHeight = 8f;
    public float flightDuration = 1.2f;
    public GameObject explosionPrefab;
    public float explosionLifetime = 2f;

    [HideInInspector] public int damage;
    [HideInInspector] public int attackCount;
    [HideInInspector] public string targetEnemyName;

    public System.Action<Missile> OnArrived;

    public void Launch(Vector3 start, Vector3 end)
    {
        _startPos = start;
        _endPos = end;
        _elapsed = 0f;
        _launched = true;
    }

    void Update()
    {
        if (!_launched) return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / flightDuration);

        Vector3 linearPos = Vector3.Lerp(_startPos, _endPos, t);
        float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
        transform.position = linearPos + Vector3.up * arc;

        if (t < 0.99f)
        {
            Vector3 nextLinear = Vector3.Lerp(_startPos, _endPos, t + 0.02f);
            float nextArc = arcHeight * Mathf.Sin(Mathf.PI * (t + 0.02f));
            Vector3 nextPos = nextLinear + Vector3.up * nextArc;
            Vector3 dir = (nextPos - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        if (t >= 1f)
        {
            _launched = false;
            Arrive();
        }
    }

    void Arrive()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, _endPos, Quaternion.identity);
            Destroy(fx, explosionLifetime);
        }

        OnArrived?.Invoke(this);
        Destroy(gameObject);
    }
}