using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Siroshton.Masaya.UI
{

    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private TMP_Text _column1;
        [SerializeField] private TMP_Text _column2;

        public void RefreshData()
        {
            string c1 = "";
            string c2 = "";

            List<string> stats = Player.Player.instance.attributeModifiers.GetStats();
            int count = 0;
            foreach (string stat in stats)
            {
                if( count % 2 == 0) c1 += stat + "\n";
                else c2 += stat + "\n";

                count++;
            }

            _column1.text = c1;
            _column2.text = c2;
        }

    }

}