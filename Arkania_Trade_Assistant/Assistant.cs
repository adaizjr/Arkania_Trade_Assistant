using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TradeAssistant
{
	// Token: 0x02000002 RID: 2
	[BepInPlugin("Arkania.Trade.Assistant", "Arkania Trade Assistant", "1.0")]
	public class Assistant : BaseUnityPlugin
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		private void Start()
		{
			Assistant.Inst = this;
			Harmony.CreateAndPatchAll(typeof(Assistant), null);
			base.gameObject.name = "Arkania Trade Assistant";
			this.selection = new Dictionary<string, bool>();
			this.m_GoDict = new Dictionary<string, GameObject>();
			base.StartCoroutine(this.LoadAssetAsync());
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020A0 File Offset: 0x000002A0
		public void PopInputPanel()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_GoDict["InputPanel"], UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.AddComponent<InputPanel>();
			foreach (UINPCSVItem uinpcsvitem in UINPCLeftList.Inst.RNpcList)
			{
				NpcJieSuanManager.inst.SortNpcPack(uinpcsvitem.NPCData.ID);
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000214C File Offset: 0x0000034C
		public GameObject GetChild<T>(GameObject gameObject, string name, bool showError = true) where T : Component
		{
			foreach (T t in gameObject.GetComponentsInChildren<T>(true))
			{
				if (t.name == name)
				{
					return t.gameObject;
				}
			}
			if (showError)
			{
				Debug.LogError("对象" + gameObject.name + "不存在子对象" + name);
			}
			return null;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000021B8 File Offset: 0x000003B8
		public Stack<GameObject> GetChildren<T>(GameObject go, string name, bool showError = true) where T : Component
		{
			T[] componentsInChildren = go.GetComponentsInChildren<T>(true);
			Stack<GameObject> stack = new Stack<GameObject>();
			foreach (T t in componentsInChildren)
			{
				if (t.name.Contains(name))
				{
					base.Logger.Log(LogLevel.Message, t.name);
					stack.Push(t.gameObject);
				}
			}
			if (stack.Count > 0)
			{
				return stack;
			}
			if (showError)
			{
				Debug.LogError("对象" + go.name + "不存在子对象" + name);
			}
			return null;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002250 File Offset: 0x00000450
		public List<GameObject> GetChildren<T>(GameObject go) where T : Component
		{
			T[] componentsInChildren = go.GetComponentsInChildren<T>(true);
			List<GameObject> list = new List<GameObject>();
			foreach (T t in componentsInChildren)
			{
				list.Add(t.gameObject);
			}
			if (list.Count > 0)
			{
				return list;
			}
			Debug.LogError(string.Format("GameObject {0} do not have any child with Component {1}", go.name, typeof(T)));
			return null;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000022C0 File Offset: 0x000004C0
		public T GetComponentInChildren<T>(GameObject gameObject, string name, bool showError = true) where T : Component
		{
			GameObject child = this.GetChild<RectTransform>(gameObject, name, showError);
			if (child == null)
			{
				return default(T);
			}
			if (child.GetComponent<T>() == null)
			{
				child.AddComponent<T>();
			}
			return child.GetComponent<T>();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000230A File Offset: 0x0000050A
		public T GetComponent<T>(GameObject gameObject) where T : Component
		{
			if (gameObject.GetComponent<T>() == null)
			{
				gameObject.AddComponent<T>();
			}
			return gameObject.GetComponent<T>();
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000232C File Offset: 0x0000052C
		private IEnumerator GetFont()
		{
			yield return new WaitForSeconds(8f);
			GameObject go = null;
			while (go == null)
			{
				go = GameObject.Find("Gonggao");
				yield return new WaitForSeconds(0.1f);
			}
			this.font1 = go.transform.GetChild(2).GetComponent<Text>().font;
			if (this.font1 != null)
			{
				base.Logger.Log(LogLevel.Message, "Font:" + this.font1.name + " already loaded!");
			}
			yield break;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000233B File Offset: 0x0000053B
		public IEnumerator LoadAssetAsync()
		{
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(BepInEx.Paths.PluginPath + "/Arkania's Vanilla Expanded/tradeassistant");
			yield return assetBundleCreateRequest;
			AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
			if (assetBundle == null)
			{
				Debug.LogError("Failed to load AssetBundle:tradeassistant!");
				yield break;
			}
			GameObject[] assetLoadRequestGo = assetBundle.LoadAllAssets<GameObject>();
			yield return assetLoadRequestGo;
			foreach (GameObject gameObject in assetLoadRequestGo)
			{
				AudioSource component = gameObject.GetComponent<AudioSource>();
				if (component != null)
				{
					component.volume = SystemConfig.Inst.GetEffectVolume();
				}
				this.m_GoDict.TryAdd(gameObject.name, gameObject, "");
			}
			yield return base.StartCoroutine(this.GetFont());
			yield break;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000234A File Offset: 0x0000054A
		public void Log(object data, LogLevel level = LogLevel.Message)
		{
			base.Logger.Log(level, data);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000235C File Offset: 0x0000055C
		[HarmonyPostfix]
		[HarmonyPatch(typeof(UINPCLeftList), "Awake")]
		private static void UINPCLeftList_Awake_Patch(UINPCLeftList __instance)
		{
			Assistant assistant = UnityEngine.Object.FindObjectOfType<Assistant>();
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(assistant.m_GoDict["Searchbtn"], __instance.transform);
			gameObject.transform.localPosition = new Vector3(40f, 640f);
			gameObject.transform.transform.localScale = Vector3.one;
			gameObject.name = "SearchBtn";
			gameObject.GetComponentInChildren<Text>().font = Assistant.Inst.font1;
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerClick
			};
			entry.callback.AddListener(delegate(BaseEventData data)
			{
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(Assistant.Inst.m_GoDict["clickSE"]), 1f);
				if (UINPCJiaoHu.Inst != null)
				{
					if (UINPCLeftList.Inst.RNpcList.Count > 0)
					{
						assistant.PopInputPanel();
						return;
					}
					UIPopTip.Inst.Pop("当前没有NPC角色！", PopTipIconType.叹号);
				}
			});
			gameObject.AddComponent<EventTrigger>().triggers.Add(entry);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000241D File Offset: 0x0000061D
		[HarmonyPostfix]
		[HarmonyPatch(typeof(LoadingTips), "RandomTip")]
		private static void LoadingTips_RandomTip_Patch(LoadingTips __instance)
		{
			Assistant.Inst.font2 = __instance.TipText.font;
		}

		// Token: 0x04000001 RID: 1
		public Font font1;

		// Token: 0x04000002 RID: 2
		public Font font2;

		// Token: 0x04000003 RID: 3
		public static Assistant Inst;

		// Token: 0x04000004 RID: 4
		public Dictionary<string, bool> selection;

		// Token: 0x04000005 RID: 5
		public Dictionary<string, GameObject> m_GoDict;
	}
}
