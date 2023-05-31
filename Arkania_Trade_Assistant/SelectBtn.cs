using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TradeAssistant
{
    // Token: 0x02000004 RID: 4
    internal class SelectBtn : MonoBehaviour
    {
        // Token: 0x0600001C RID: 28 RVA: 0x00002F64 File Offset: 0x00001164
        private void Start()
        {
            this.m_Image = Assistant.Inst.GetChild<Image>(base.gameObject, "Background", true).GetComponent<Image>();
            this.m_Image.color = Color.grey;
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener(delegate (BaseEventData data)
            {
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
            });
            base.gameObject.AddComponent<EventTrigger>().triggers.Add(entry);
            this.m_Toggle = base.GetComponent<Toggle>();
            this.m_Toggle.onValueChanged.AddListener(delegate (bool state)
            {
                this.OnValueChanged(state);
            });
            if (!Assistant.Inst.selection.ContainsKey(base.gameObject.name))
            {
                Assistant.Inst.selection.Add(base.gameObject.name, this.m_Toggle.isOn);
            }
            else
            {
                this.m_Toggle.isOn = Assistant.Inst.selection[base.gameObject.name];
            }
            Text componentInChildren = Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "Label", false);
            if (componentInChildren != null)
            {
                componentInChildren.font = Assistant.Inst.font1;
            }
        }

        // Token: 0x0600001D RID: 29 RVA: 0x000030B0 File Offset: 0x000012B0
        private void OnValueChanged(bool state)
        {
            if (this.m_Toggle.isOn)
            {
                this.m_Image.color = Color.yellow;
                if (base.gameObject.name.Contains("Quality"))
                {
                    this.inputPanel.specialGroup.SetAllTogglesOff();
                    goto IL_177;
                }
                if (base.gameObject.name.Contains("Type"))
                {
                    this.inputPanel.specialGroup.SetAllTogglesOff();
                    goto IL_177;
                }
                if (!base.gameObject.name.Contains("Special"))
                {
                    goto IL_177;
                }
                Assistant.Inst.GetChild<RectTransform>(base.gameObject, "anim", true).SetActive(state);
                foreach (GameObject gameObject in this.inputPanel.typeDict.Values)
                {
                    gameObject.GetComponent<Toggle>().isOn = false;
                }
                using (Dictionary<int, GameObject>.ValueCollection.Enumerator enumerator = this.inputPanel.qualityDict.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        GameObject gameObject2 = enumerator.Current;
                        gameObject2.GetComponent<Toggle>().isOn = false;
                    }
                    goto IL_177;
                }
            }
            this.m_Image.color = Color.grey;
            if (base.gameObject.name.Contains("Special"))
            {
                Assistant.Inst.GetChild<RectTransform>(base.gameObject, "anim", true).SetActive(state);
            }
            IL_177:
            Assistant.Inst.selection[base.gameObject.name] = this.m_Toggle.isOn;
        }

        // Token: 0x04000009 RID: 9
        private Toggle m_Toggle;

        // Token: 0x0400000A RID: 10
        private Image m_Image;

        // Token: 0x0400000B RID: 11
        public InputPanel inputPanel;
    }
}
