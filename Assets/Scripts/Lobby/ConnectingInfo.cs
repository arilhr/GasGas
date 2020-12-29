using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConnectingInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    
    public void SetText(string _text, bool isRemoved)
    {
        infoText.text = _text;

        if (isRemoved)
            StartCoroutine(RemoveText(2f));
    }

    private IEnumerator RemoveText(float _waitTime)
    {
        yield return new WaitForSeconds(_waitTime);

        this.gameObject.SetActive(false);
    }
}
