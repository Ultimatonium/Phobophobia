using System;
using TMPro;
using UnityEngine;

public class QuoteReader : MonoBehaviour
{
    [SerializeField]
    private TextAsset quoteFile;
    [SerializeField]
    private TextMeshProUGUI quotePresenter;

    private void Start()
    {
        string[] quotes = quoteFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        quotePresenter.text = quotes[UnityEngine.Random.Range(0, quotes.Length)];
    }
}
