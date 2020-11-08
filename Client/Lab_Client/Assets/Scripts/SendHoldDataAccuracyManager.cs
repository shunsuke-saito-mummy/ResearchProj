﻿using System.Collections;
using System.Text;
using UnityEngine;

public class SendHoldDataAccuracyManager : MonoBehaviour
{
    [Header("Test Static Parameters")]
    [Tooltip("送信するビットデータの長さ")]
    [SerializeField] private int bitDataLength;
    [Tooltip("VSyncCount: 0の場合ディスプレイの垂直同期とは独立した更新になる")]
    [SerializeField] private int vSyncCount;
    [Tooltip("TargetFrameRate: 十分に高い値を設定すると良い")]
    [SerializeField] private int targetFrameRate;

    [Header("Test Dynamic Parameters")] 
    [SerializeField] private float holdDurationInSec;
    [SerializeField] private float testMaxCount;
    [Tooltip("デバイスの処理速度によって発生する微妙な遅延を調整するための値")]
    [SerializeField] private float deviceDelayAdjustInSec;

    private int[] _bitData;

    private bool _isPressing;

    /// <summary>
    /// データの受信中フラグ
    /// </summary>
    private bool _isDataReceiving;

    private readonly StringBuilder _sb = new StringBuilder();

    private float _sendStartTime;

    private float _lastPressDownTime;

    private bool _isBitHigh;

    private void Start()
    {
        _bitData = new int[bitDataLength];
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
    }

    public void OnClickReceivingButton()
    {
        if (_isDataReceiving)
        {
            return;
        }
        StartCoroutine(DataReceivingCoroutine());
    }

    private void Update()
    {
        if (_isDataReceiving == false)
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || 
                touch.phase == TouchPhase.Moved ||
                touch.phase == TouchPhase.Stationary)
            {
                if (_isBitHigh == false)
                {
                    _isPressing = true;
                    _isBitHigh = true;
                    _lastPressDownTime = Time.realtimeSinceStartup;
                    Debug.Log($"_lastPressDownTime: {Time.realtimeSinceStartup}");
                }
            } else if (touch.phase == TouchPhase.Ended)
            {
                _isBitHigh = false;
                _isPressing = false;
                _sb.Append($"=====OnPointerUp======\nTime.realtimeSinceStartup: {Time.realtimeSinceStartup}\nHoldStartTime: {Time.realtimeSinceStartup - _sendStartTime}\nHoldDuration: {Time.realtimeSinceStartup - _lastPressDownTime}\n");
            }
        }
    }

    /// <summary>
    /// データ送信開始ビットを受け取ったら起動するコルーチン
    /// 10bitを受け取る時間が経過したら、_isDataReceivingフラグをOFFにする
    /// </summary>
    private IEnumerator DataReceivingCoroutine()
    {
        // データ送信開始タッチの後holdDurationInSec分待機時間が発生するためそれを待つ
        _isDataReceiving = true;
        yield return new WaitForSeconds(holdDurationInSec);
        _sendStartTime = Time.realtimeSinceStartup;
        float timer = 0f;
        float limit = holdDurationInSec * _bitData.Length + deviceDelayAdjustInSec;
        while (timer < limit)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        _isDataReceiving = false;
        LogBitToInt();
        Debug.Log(_sb.ToString());
        _sb.Clear();
    }

    private void LogBitToInt()
    {
        int num = 0;
        string hoge = "";
        for (var i = 0; i < _bitData.Length; i++)
        {
            num += _bitData[i] << i;
            hoge = _bitData[i] + hoge;
        }
        Debug.Log($"num: {num}\n{hoge}");
    }
}
