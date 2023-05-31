using System;
using System.Collections.Generic;
using Bag;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WXB;

namespace TradeAssistant
{
    // Token: 0x02000006 RID: 6
    internal class BagCell : MonoBehaviour
    {
        // Token: 0x0600002B RID: 43 RVA: 0x0000375C File Offset: 0x0000195C
        private void Start()
        {
            this.isNull = true;
            this.sframe = Assistant.Inst.GetChild<RectTransform>(base.gameObject, "sframe", true);
            this.m_cText = Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "count", true);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entry.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerUp((PointerEventData)data);
            });
            EventTrigger.Entry entry2 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry2.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerEnter((PointerEventData)data);
            });
            EventTrigger.Entry entry3 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entry3.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnPointerExit((PointerEventData)data);
            });
            base.gameObject.AddComponent<EventTrigger>().triggers.AddRange(new EventTrigger.Entry[]
            {
                entry,
                entry2,
                entry3
            });
        }

        // Token: 0x0600002C RID: 44 RVA: 0x0000383C File Offset: 0x00001A3C
        private void OnPointerUp(PointerEventData data)
        {
            if (this.isNull)
            {
                return;
            }
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
            TradePanel componentInParent = base.GetComponentInParent<TradePanel>();
            foreach (ItemCell itemCell in componentInParent.m_ItemCells)
            {
                if (itemCell.m_BaseItem.Value.Uid == this.m_BaseItem.Value.Uid)
                {
                    itemCell.SetItem();
                    this.m_BaseItem.Value.Count--;
                    this.Refresh();
                    componentInParent.Refresh();
                    return;
                }
            }
            UnityEngine.Object.Instantiate<GameObject>(Assistant.Inst.m_GoDict["itemcella"], componentInParent.m_Content.transform).AddComponent<ItemCell>().m_BaseItem = new KeyValuePair<UINPCData, BaseItem>(this.m_BaseItem.Key, BaseItem.Create(this.m_BaseItem.Value.Id, this.m_BaseItem.Value.Count, this.m_BaseItem.Value.Uid, this.m_BaseItem.Value.Seid));
            this.m_BaseItem.Value.Count = 0;
            this.Refresh();
            componentInParent.Refresh();
        }

        // Token: 0x0600002D RID: 45 RVA: 0x000039AC File Offset: 0x00001BAC
        private void OnPointerEnter(PointerEventData data)
        {
            if (this.isNull)
            {
                return;
            }
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

        // Token: 0x0600002E RID: 46 RVA: 0x00003A9B File Offset: 0x00001C9B
        private void OnPointerExit(PointerEventData data)
        {
            if (this.isNull)
            {
                return;
            }
            ToolTipsMag.Inst.Close();
            this.sframe.SetActive(false);
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003ABC File Offset: 0x00001CBC
        public void SetItem(UINPCData npc, BaseItem item,int i_count)
        {
            this.m_BaseItem = new KeyValuePair<UINPCData, BaseItem>(npc, item);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "qsprite", true).SetActive(true);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "frame", true).SetActive(true);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "sprite", true).SetActive(true);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "name", true).SetActive(true);
            this.m_cText.gameObject.SetActive(true);
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "qsprite", true).sprite = this.m_BaseItem.Value.GetQualityUpSprite();
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "frame", true).sprite = this.m_BaseItem.Value.GetQualitySprite();
            Assistant.Inst.GetComponentInChildren<Image>(base.gameObject, "sprite", true).sprite = this.m_BaseItem.Value.GetIconSprite();
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "name", true).text = this.m_BaseItem.Value.GetName();
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "name", true).font = Assistant.Inst.font2;
            this.m_BaseItem.Value.Count = i_count;
            this.isNull = false;
            this.Refresh();
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00003C46 File Offset: 0x00001E46
        public void SetItem()
        {
            this.m_BaseItem.Value.Count++;
            this.Refresh();
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003C66 File Offset: 0x00001E66
        private void Refresh()
        {
            if (this.m_BaseItem.Value.Count == 0)
            {
                this.SetNull();
            }
            this.m_cText.text = this.m_BaseItem.Value.Count.ToString();
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00003CA0 File Offset: 0x00001EA0
        public void SetNull()
        {
            this.isNull = true;
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "qsprite", true).SetActive(false);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "frame", true).SetActive(false);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "sprite", true).SetActive(false);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "name", true).SetActive(false);
            this.m_cText.gameObject.SetActive(false);
            this.sframe.SetActive(false);
        }

        // Token: 0x0400000F RID: 15
        public bool isNull;

        // Token: 0x04000010 RID: 16
        public KeyValuePair<UINPCData, BaseItem> m_BaseItem;

        // Token: 0x04000011 RID: 17
        private GameObject sframe;

        // Token: 0x04000012 RID: 18
        private Text m_cText;
    }
}
