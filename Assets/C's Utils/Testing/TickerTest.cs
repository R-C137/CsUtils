using CsUtils.Systems.Tick;
using UnityEngine;

public class TickerTest : MonoBehaviour
{
    public float elapsed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Ticker.SubscribeTick(10.5f, Callback);
    }
    private void Callback()
    {
        Debug.Log(elapsed);
        elapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
    }
}
