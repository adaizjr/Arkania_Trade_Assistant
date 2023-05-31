using System;
using System.Collections.Generic;
using Bag;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WXB;

namespace TradeAssistant
{
    // Token: 0x02000005 RID: 5
    internal class ItemCell : MonoBehaviour
    {
        // Token: 0x06000020 RID: 32 RVA: 0x0000328C File Offset: 0x0000148C
        private void Start()
        {
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "qsprite", true).sprite = this.m_BaseItem.Value.GetQualityUpSprite();
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "frame", true).sprite = this.m_BaseItem.Value.GetQualitySprite();
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "sprite", true).sprite = this.m_BaseItem.Value.GetIconSprite();
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "name", true).text = this.m_BaseItem.Value.GetName();
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "name", true).font = Assistant.Inst.font2;
            this.m_cText = Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "count", true);
            this.m_cText.text = this.m_BaseItem.Value.Count.ToString();
            this.sframe = Assistant.Inst.GetChild<RectTransform>(base.gameObject, "sframe", true);
            this.RegEventSystem();
            base.GetComponentInParent<TradePanel>().m_ItemCells.Add(this);
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000033DC File Offset: 0x000015DC
        private void RegEventSystem()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerEnter((PointerEventData)data);
            });
            EventTrigger.Entry entry2 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entry2.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerExit((PointerEventData)data);
            });
            EventTrigger.Entry entry3 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entry3.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerUp((PointerEventData)data);
            });
            base.gameObject.AddComponent<EventTrigger>().triggers.AddRange(new EventTrigger.Entry[]
            {
                entry,
                entry2,
                entry3
            });
        }

        // Token: 0x06000022 RID: 34 RVA: 0x0000347C File Offset: 0x0000167C
        private void OnPointerEnter(PointerEventData data)
        {
            if (ToolTipsMag.Inst == null)
            {
                ResManager.inst.LoadPrefab("ToolTips").Inst(NewUICanvas.Inst.transform);
            }
            ToolTipsMag.Inst.Show(this.m_BaseItem.Value, this.m_BaseItem.Value.GetJiaoYiPrice(this.m_BaseItem.Key.ID, false, false), false);
            //Transform parent = (ToolTipsMag.Inst._direction == ToolTipsMag.Direction.右) ? ToolTipsMag.Inst.RightPanel.transform : ToolTipsMag.Inst.LeftPanel.transform;
            Transform parent = ToolTipsMag.Inst.RightPanel.transform;
            GameObject gameObject = ToolTipsMag.Inst.CiTiao.Inst(parent);
            gameObject.SetActive(true);
            gameObject.GetComponentInChildren<SymbolText>().text = "物品来源" + this.m_BaseItem.Key.Name;
            this.sframe.SetActive(true);
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00003562 File Offset: 0x00001762
        private void OnPointerExit(PointerEventData data)
        {
            ToolTipsMag.Inst.Close();
            this.sframe.SetActive(false);
        }

        // Token: 0x06000024 RID: 36 RVA: 0x0000357C File Offset: 0x0000177C
        private void OnPointerUp(PointerEventData data)
        {
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
            TradePanel componentInParent = base.GetComponentInParent<TradePanel>();
            for (int i = 0; i < componentInParent.m_BagCells.Count; i++)
            {
                if (componentInParent.m_BagCells[i].isNull)
                {
                    componentInParent.m_BagCells[i].SetItem(this.m_BaseItem.Key, this.m_BaseItem.Value.Clone(), this.m_BaseItem.Value.Count);
                    this.m_BaseItem.Value.Count = 0;
                    this.Refresh();
                    componentInParent.Refresh();
                    return;
                }
                if (componentInParent.m_BagCells[i].m_BaseItem.Value.Uid == this.m_BaseItem.Value.Uid)
                {
                    componentInParent.m_BagCells[i].SetItem();
                    this.m_BaseItem.Value.Count--;
                    this.Refresh();
                    componentInParent.Refresh();
                    return;
                }
            }
            UIPopTip.Inst.Pop("最多只能购买8个物品！", PopTipIconType.叹号);
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000036AB File Offset: 0x000018AB
        public void SetItem()
        {
            this.m_BaseItem.Value.Count++;
            this.Refresh();
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000036CC File Offset: 0x000018CC
        private void Refresh()
        {
            if (this.m_BaseItem.Value.Count == 0)
            {
                base.GetComponentInParent<TradePanel>().m_ItemCells.Remove(this);
                UnityEngine.Object.Destroy(base.gameObject);
            }
            this.m_cText.text = this.m_BaseItem.Value.Count.ToString();
        }

        // Token: 0x0400000C RID: 12
        public KeyValuePair<UINPCData, BaseItem> m_BaseItem;

        // Token: 0x0400000D RID: 13
        private Text m_cText;

        // Token: 0x0400000E RID: 14
        private GameObject sframe;
    }
}
