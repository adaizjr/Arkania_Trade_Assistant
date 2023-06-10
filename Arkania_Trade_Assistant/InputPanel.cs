using System;
using System.Collections.Generic;
using System.Linq;
using Bag;
using JSONClass;
using KBEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TradeAssistant
{
    // Token: 0x02000003 RID: 3
    internal class InputPanel : MonoBehaviour, IESCClose
    {
        // Token: 0x0600000E RID: 14 RVA: 0x0000243C File Offset: 0x0000063C
        private void Awake()
        {
            this.qualityDict = new Dictionary<int, UnityEngine.GameObject>();
            this.typeDict = new Dictionary<int, UnityEngine.GameObject>();
            foreach (UnityEngine.GameObject gameObject in Assistant.Inst.GetChildren<Toggle>(base.gameObject))
            {
                gameObject.AddComponent<SelectBtn>().inputPanel = this;
                if (gameObject.name.Contains("Quality"))
                {
                    this.qualityDict.Add(int.Parse(gameObject.name.Replace("Quality", "")), gameObject);
                }
                else if (gameObject.name.Contains("Type"))
                {
                    this.typeDict.Add(int.Parse(gameObject.name.Replace("Type", "")), gameObject);
                }
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x0000252C File Offset: 0x0000072C
        private void Start()
        {
            this.specialGroup = Assistant.Inst.GetChild<RectTransform>(base.gameObject, "Special1", true).GetComponent<ToggleGroup>();
            this.SetEventTrigger();
            this.SetTextFont();
            ESCCloseManager.Inst.RegisterClose(this);
            Tools.canClickFlag = false;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000256C File Offset: 0x0000076C
        private void SetEventTrigger()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener(delegate (BaseEventData data)
            {
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
                this.Close();
            });
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "QuitBtn", true).AddComponent<EventTrigger>().triggers.Add(entry);
            EventTrigger.Entry entry2 = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry2.callback.AddListener(delegate (BaseEventData data)
            {
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
                this.OnConfirmClick();
            });
            Assistant.Inst.GetChild<RectTransform>(base.gameObject, "ConfirmBtn", true).AddComponent<EventTrigger>().triggers.Add(entry2);
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002610 File Offset: 0x00000810
        private void OnConfirmClick()
        {
            if (Assistant.Inst.selection["Special1"])
            {
                UIPopTip.Inst.Pop("一键购物！", PopTipIconType.叹号);
                yijian_gouwu(false);
                return;
            }

            if (Assistant.Inst.selection["Special2"])
            {
                shentong_zhenqi();
                return;
            }
            if (Enumerable.Any<KeyValuePair<int, UnityEngine.GameObject>>(this.qualityDict, (KeyValuePair<int, UnityEngine.GameObject> t) => t.Value.GetComponent<Toggle>().isOn) && Enumerable.Any<KeyValuePair<int, UnityEngine.GameObject>>(this.typeDict, (KeyValuePair<int, UnityEngine.GameObject> t) => t.Value.GetComponent<Toggle>().isOn))
            {
                UIPopTip.Inst.Pop("除非使用特殊选择，否则必须各选择一个品阶和类型！", PopTipIconType.叹号);
                return;
            }
            IEnumerable<_ItemJsonData> enumerable = Enumerable.Where<_ItemJsonData>(_ItemJsonData.DataDict.Values, delegate (_ItemJsonData item)
            {
                if (Assistant.Inst.selection["Special2"])
                {
                    return item.ItemFlag.Contains(710);
                }
                List<int> list = new List<int>();
                List<int> list2 = new List<int>();
                foreach (int num in this.qualityDict.Keys)
                {
                    if (this.qualityDict[num].GetComponent<Toggle>().isOn)
                    {
                        list.Add(num);
                    }
                }
                foreach (int num2 in this.typeDict.Keys)
                {
                    if (this.typeDict[num2].GetComponent<Toggle>().isOn)
                    {
                        list2.Add(num2);
                    }
                }
                return list.Contains(item.quality) && list2.Contains(item.type);
            });
            if (Enumerable.Count<_ItemJsonData>(enumerable) == 0)
            {
                UIPopTip.Inst.Pop("没有任何有效物品!", PopTipIconType.叹号);
                this.Close();
                return;
            }
            this.CalcResult1(enumerable);
        }

        // Token: 0x06000012 RID: 18 RVA: 0x000026E8 File Offset: 0x000008E8
        private void CalcResult1(IEnumerable<_ItemJsonData> result)
        {
            IEnumerable<UINPCSVItem> enumerable = Enumerable.Where<UINPCSVItem>(UINPCLeftList.Inst.RNpcList, (UINPCSVItem npc) => npc.NPCData.FavorLevel > 2);
            List<KeyValuePair<UINPCData, BaseItem>> list = new List<KeyValuePair<UINPCData, BaseItem>>();
            foreach (_ItemJsonData itemJsonData in result)
            {
                foreach (UINPCSVItem uinpcsvitem in enumerable)
                {
                    foreach (JSONObject jsonobject in uinpcsvitem.NPCData.BackpackJson["Backpack"].list)
                    {
                        if (jsonobject["ItemID"].I == itemJsonData.id)
                        {
                            BaseItem value = BaseItem.Create(jsonobject["ItemID"].I, jsonobject["Num"].I, jsonobject["UUID"].Str, jsonobject["Seid"]);
                            list.Add(new KeyValuePair<UINPCData, BaseItem>(uinpcsvitem.NPCData, value));
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["tradepanel"], UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
                gameObject.AddComponent<TradePanel>().m_BaseItems = list;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localScale = Vector3.one;
            }
            else
            {
                UIPopTip.Inst.Pop("此处NPC没有对应物品！", PopTipIconType.叹号);
            }
            this.Close();
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000028FC File Offset: 0x00000AFC
        private void CalcResult1(List<int> items)
        {
            IEnumerable<UINPCSVItem> enumerable = Enumerable.Where<UINPCSVItem>(UINPCLeftList.Inst.RNpcList, (UINPCSVItem npc) => npc.NPCData.FavorLevel > 2);
            List<KeyValuePair<UINPCData, BaseItem>> list = new List<KeyValuePair<UINPCData, BaseItem>>();
            foreach (int num in items)
            {
                foreach (UINPCSVItem uinpcsvitem in enumerable)
                {
                    foreach (JSONObject jsonobject in uinpcsvitem.NPCData.BackpackJson["Backpack"].list)
                    {
                        if (jsonobject["ItemID"].I == num)
                        {
                            BaseItem value = BaseItem.Create(jsonobject["ItemID"].I, jsonobject["Num"].I, jsonobject["UUID"].Str, jsonobject["Seid"]);
                            list.Add(new KeyValuePair<UINPCData, BaseItem>(uinpcsvitem.NPCData, value));
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(Assistant.Inst.m_GoDict["tradepanel"], UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
                gameObject.AddComponent<TradePanel>().m_BaseItems = list;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localScale = Vector3.one;
            }
            else
            {
                UIPopTip.Inst.Pop("此处NPC没有对应物品！", PopTipIconType.叹号);
            }
            this.Close();
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002AEC File Offset: 0x00000CEC
        private void CollectItem()
        {
            Avatar player = PlayerEx.Player;
            string text = "";
            string nowSceneName = SceneEx.NowSceneName;
            if (nowSceneName == "S19" || (nowSceneName.Contains("S119") && nowSceneName.Length > 4))
            {
                text = "S1198";
            }
            else if (nowSceneName == "S20" || (nowSceneName.Contains("S120") && nowSceneName.Length > 4))
            {
                text = "S1209";
            }
            else if (nowSceneName == "S22" || (nowSceneName.Contains("S122") && nowSceneName.Length > 4))
            {
                text = "S1225";
            }
            else if (nowSceneName == "S29" || (nowSceneName.Contains("S129") && nowSceneName.Length > 4))
            {
                text = "S1296";
            }
            else if (nowSceneName == "S30" || (nowSceneName.Contains("S130") && nowSceneName.Length > 4))
            {
                text = "S1303";
            }
            else if (nowSceneName == "S31" || (nowSceneName.Contains("S131") && nowSceneName.Length > 4))
            {
                text = "S1314";
            }
            else if (nowSceneName == "S32" || (nowSceneName.Contains("S132") && nowSceneName.Length > 4))
            {
                text = "S1326";
            }
            if (text == "")
            {
                UIPopTip.Inst.Pop("你未在有告示任务的城中。", PopTipIconType.叹号);
                this.Close();
                return;
            }
            GaoShiLeiXing gaoShiLeiXing = GaoShiLeiXing.DataDict[text];
            List<int> list = new List<int>();
            if (player.GaoShi.HasField(text) && gaoShiLeiXing != null)
            {
                List<JSONObject> list2 = player.GaoShi[text]["GaoShiList"].list;
                for (int i = 0; i < list2.Count; i++)
                {
                    JSONObject jsonobject = list2[i];
                    GaoShi gaoShi = GaoShi.DataDict[jsonobject["GaoShiID"].I];
                    if (gaoShi.type == 1 && !jsonobject["YiShouGou"].b && player.getItemNum(gaoShi.itemid) < gaoShi.num)
                    {
                        list.Add(gaoShi.itemid);
                    }
                }
            }
            if (list.Count > 0)
            {
                this.CalcResult1(list);
            }
        }
        class remove_item_npcid
        {
            public int tmp_npcid;
            public int tmp_item_id;
            public int tmp_item_num;
            public string tmp_item_uuid;
            public JSONObject tmp_item_seid;
            public int tmp_zongjia;
        }
        private void yijian_gouwu(bool b_dengjia)
        {
            Avatar player = PlayerEx.Player;
            IEnumerable<UINPCSVItem> enumerable = Enumerable.Where<UINPCSVItem>(UINPCLeftList.Inst.RNpcList, (UINPCSVItem npc) => npc.NPCData.FavorLevel > 2);
            List<remove_item_npcid> l_remove = new List<remove_item_npcid>();
            int tmp_xiaofei = 0;
            foreach (UINPCSVItem uinpcsvitem in enumerable)
            {
                int tmp_npcid = uinpcsvitem.NPCData.ID;
                foreach (JSONObject jsonobject in uinpcsvitem.NPCData.BackpackJson["Backpack"].list)
                {
                    int tmp_item_id = jsonobject["ItemID"].I;
                    //if (tmp_item_id >= 5000 && tmp_item_id <= 9000)
                    {
                        int tmp_item_num = jsonobject["Num"].I;
                        string tmp_item_uuid = jsonobject["UUID"].Str;
                        JSONObject tmp_item_seid = jsonobject["Seid"];
                        BaseItem tmp_item = BaseItem.Create(jsonobject["ItemID"].I, jsonobject["Num"].I, jsonobject["UUID"].Str, jsonobject["Seid"]);
                        int tmp_now_jiage = tmp_item.GetJiaoYiPrice(tmp_npcid, false, false);
                        int tmp_zongjia = tmp_now_jiage * tmp_item_num;
                        if ((tmp_item.Type == 6 && tmp_item.GetBaseQuality() == 5 && tmp_now_jiage <= 4350 * 9)
                             || (tmp_item.Type == 6 && tmp_now_jiage <= tmp_item.BasePrice * 1.05f + 1)
                             || (tmp_item.Type == 5 && tmp_item.GetBaseQuality() >= 3 && tmp_now_jiage < tmp_item.BasePrice)
                             || ((tmp_item.Id == 5315 || tmp_item.Id == 5309 || tmp_item.Id == 5213) && tmp_now_jiage <= tmp_item.BasePrice * 1.05f + 1)
                             || (tmp_item.Id == 6310)
                             || (tmp_item.Type == 8 && tmp_item.GetBaseQuality() >= 5 && tmp_now_jiage <= tmp_item.BasePrice * 1.05f + 1))
                        {
                            remove_item_npcid tmp_rin = new remove_item_npcid();
                            tmp_rin.tmp_npcid = tmp_npcid;
                            tmp_rin.tmp_item_id = tmp_item_id;
                            tmp_rin.tmp_item_num = tmp_item_num;
                            tmp_rin.tmp_item_seid = tmp_item_seid;
                            tmp_rin.tmp_item_uuid = tmp_item_uuid;
                            tmp_rin.tmp_zongjia = tmp_zongjia;
                            l_remove.Add(tmp_rin);
                        }
                    }
                }
            }
            foreach (var tmp_rin in l_remove)
            {
                int tmp_npcid = tmp_rin.tmp_npcid;
                int tmp_item_id = tmp_rin.tmp_item_id;
                int tmp_item_num = tmp_rin.tmp_item_num;
                string tmp_item_uuid = tmp_rin.tmp_item_uuid;
                JSONObject tmp_item_seid = tmp_rin.tmp_item_seid;
                int tmp_zongjia = tmp_rin.tmp_zongjia;
                if ((ulong)tmp_zongjia < player.money)
                {
                    NpcJieSuanManager.inst.RemoveItem(tmp_npcid, tmp_item_id, tmp_item_num, tmp_item_uuid);
                    new NpcSetField().AddNpcMoney(tmp_npcid, tmp_zongjia);
                    player.addItem(tmp_item_id, tmp_item_seid, tmp_item_num);
                    player.money -= (ulong)tmp_zongjia;
                    tmp_xiaofei += tmp_zongjia;
                }
            }
            UIPopTip.Inst.Pop("共消费" + tmp_xiaofei.ToString(), PopTipIconType.叹号);
        }

        private void shentong_zhenqi()
        {
            IEnumerable<_ItemJsonData> enumerable = Enumerable.Where<_ItemJsonData>(_ItemJsonData.DataDict.Values, delegate (_ItemJsonData item)
            {
                return item.type == 16 || item.type == 3 || item.type == 13 || item.type == 9 || (item.quality >= 5 && item.type == 6);
            });
            this.CalcResult1(enumerable);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002D3C File Offset: 0x00000F3C
        private void SetTextFont()
        {
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "gourp1text", true).font = Assistant.Inst.font1;
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "gourp2text", true).font = Assistant.Inst.font1;
            Assistant.Inst.GetComponentInChildren<Text>(base.gameObject, "gourp3text", true).font = Assistant.Inst.font1;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002DB8 File Offset: 0x00000FB8
        private void Close()
        {
            Tools.canClickFlag = true;
            ESCCloseManager.Inst.UnRegisterClose(this);
            UnityEngine.Object.Destroy(base.gameObject);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002DD6 File Offset: 0x00000FD6
        public bool TryEscClose()
        {
            this.Close();
            return true;
        }

        // Token: 0x04000006 RID: 6
        public ToggleGroup specialGroup;

        // Token: 0x04000007 RID: 7
        public Dictionary<int, UnityEngine.GameObject> qualityDict;

        // Token: 0x04000008 RID: 8
        public Dictionary<int, UnityEngine.GameObject> typeDict;
    }
}
