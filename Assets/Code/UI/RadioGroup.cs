    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class RadioGroup : MonoBehaviour {
        private ToggleGroup tg;

        private void Start() {
            tg = GetComponent<ToggleGroup>();
        }

        public int GetSelected() {
            var temp = tg.GetFirstActiveToggle().GetComponent<VotingObject>();
            return temp.characterIndex;
        }
    }