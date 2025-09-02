using TMPro;
using UnityEngine;

namespace Siroshton.Masaya.UI
{

    public class TextItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public string text { get => _text.text; set => _text.text = value; }
    }

}