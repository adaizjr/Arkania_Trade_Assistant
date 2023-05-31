using System;
using System.Collections;
using System.Collections.Generic;
using Bag;
using KBEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TradeAssistant
{
    // Token: 0x02000007 RID: 7
    internal class TradePanel : MonoBehaviour, IESCClose
    {
        // Token: 0x06000037 RID: 55 RVA: 0x00003D73 File Offset: 0x00001F73
        private IEnumerator Start()
        {
            Tools.canClickFlag = false;
            ESCCloseManager.Inst.RegisterClose(this);
            base.transform.localPosition = this.startPostion;
            this.m_transform = base.GetComponent<RectTransform>();
            this.m_BagCells = new List<BagCell>();
            this.m_ItemCells = new List<ItemCell>();
            this.m_Content = Assistant.Inst.GetChild<RectTransform>(base.gameObject, "Content", true);
            this.m_Bag = Assistant.Inst.GetChild<RectTransform>(base.gameObject, "bag", true);
            this.m_LSum = Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "lssum", true);
            this.m_CSum = Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "costsum", true);
            foreach (KeyValuePair<UINPCData, BaseItem> baseItem in this.m_BaseItems)
            {
                UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["itemcella"], this.m_Content.transform).AddComponent<ItemCell>().m_BaseItem = baseItem;
            }
            for (int i = 0; i < 8; i++)
            {
                this.m_BagCells.Add(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["itemcellb"], this.m_Bag.transform).AddComponent<BagCell>());
            }
            this.m_LSum.text = Tools.instance.getPlayer().money.ToString();
            this.RegEventSystem();
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1f;
                this.m_transform.localPosition = Vector2.Lerp(this.m_transform.localPosition, this.finalPosition, t);
                yield return null;
            }
            yield break;
        }

        // Token: 0x06000038 RID: 56 RVA: 0x00003D84 File Offset: 0x00001F84
        private void RegEventSystem()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            EventTrigger.Entry entry2 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnConfirmClick((PointerEventData)data);
            });
            entry2.callback.AddListener(delegate (BaseEventData data)
            {
                this.OnQuitClick((PointerEventData)data);
            });
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "ConfirmBtn", true).AddComponent<EventTrigger>().triggers.Add(entry);
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "QuitBtn", true).AddComponent<EventTrigger>().triggers.Add(entry2);
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00003E28 File Offset: 0x00002028
        private void OnConfirmClick(PointerEventData data)
        {
            Avatar player = PlayerEx.Player;
            ulong num = ulong.Parse(this.m_CSum.text);
            if (player.money < num)
            {
                UIPopTip.Inst.Pop("你的灵石不够！", PopTipIconType.叹号);
                return;
            }
            foreach (BagCell bagCell in this.m_BagCells)
            {
                if (!bagCell.isNull)
                {
                    KeyValuePair<UINPCData, BaseItem> baseItem = bagCell.m_BaseItem;
                    NpcJieSuanManager.inst.RemoveItem(baseItem.Key.ID, baseItem.Value.Id, baseItem.Value.Count, baseItem.Value.Uid);
                    new NpcSetField().AddNpcMoney(baseItem.Key.ID, baseItem.Value.GetJiaoYiPrice(baseItem.Key.ID, false, false) * baseItem.Value.Count);
                    player.addItem(baseItem.Value.Id, baseItem.Value.Seid, baseItem.Value.Count);
                    bagCell.SetNull();
                }
            }
            if (num > 0UL)
            {
                ulong money = player.money;
                player.money -= num;
                this.m_CSum.text = "0";
                base.StartCoroutine(this.SetNowMoney(money, player.money));
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["purchaseSE"]), 2f);
                return;
            }
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00003FE0 File Offset: 0x000021E0
        private void OnQuitClick(PointerEventData data)
        {
            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
            this.Close();
        }

        // Token: 0x0600003B RID: 59 RVA: 0x0000400C File Offset: 0x0000220C
        public void Refresh()
        {
            int num = 0;
            foreach (BagCell bagCell in this.m_BagCells)
            {
                if (!bagCell.isNull)
                {
                    KeyValuePair<UINPCData, BaseItem> baseItem = bagCell.m_BaseItem;
                    num += baseItem.Value.GetJiaoYiPrice(baseItem.Key.ID, false, false) * baseItem.Value.Count;
                }
            }
            this.m_CSum.text = num.ToString();
        }

        // Token: 0x0600003C RID: 60 RVA: 0x00004098 File Offset: 0x00002298
        private IEnumerator SetNowMoney(ulong money, ulong nowmoney)
        {
            if (money - nowmoney > 60UL)
            {
                ulong tmp = money;
                ulong stepcost = money / 60UL;
                while (tmp > nowmoney)
                {
                    tmp -= stepcost;
                    this.m_LSum.text = tmp.ToString();
                    yield return null;
                }
            }
            this.m_LSum.text = nowmoney.ToString();
            yield break;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000040B5 File Offset: 0x000022B5
        private void Close()
        {
            Tools.canClickFlag = true;
            ESCCloseManager.Inst.UnRegisterClose(this);
            UnityEngine.Object.Destroy(base.gameObject);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x000040D3 File Offset: 0x000022D3
        public bool TryEscClose()
        {
            this.Close();
            return true;
        }

        // Token: 0x04000013 RID: 19
        public List<KeyValuePair<UINPCData, BaseItem>> m_BaseItems;

        // Token: 0x04000014 RID: 20
        public List<BagCell> m_BagCells;

        // Token: 0x04000015 RID: 21
        public List<ItemCell> m_ItemCells;

        // Token: 0x04000016 RID: 22
        public UnityEngine.GameObject m_Content;

        // Token: 0x04000017 RID: 23
        public RectTransform m_transform;

        // Token: 0x04000018 RID: 24
        private UnityEngine.GameObject m_Bag;

        // Token: 0x04000019 RID: 25
        private Text m_LSum;

        // Token: 0x0400001A RID: 26
        private Text m_CSum;

        // Token: 0x0400001B RID: 27
        private Vector2 startPostion = new Vector2(0f, -1080f);

        // Token: 0x0400001C RID: 28
        private Vector2 finalPosition = new Vector2(0f, 0f);
    }
}
