using System;
using System.Collections.Generic;
using System.Linq;
using Flash2;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod
{
    internal class PlayerCountManager
    {
        class PlayerCountItem<T> where T : class
        {
            private readonly GameObject root;
            private readonly SelScrollRectCellBase<T, SelDiagonalScrollRectContext> cellData;
            private readonly Func<T, Dictionary<byte, ushort>, Dictionary<ushort, ushort>, int> getPlayerCountFunc;
            private readonly GameObject countRoot;
            private readonly Text countText;
            private int lastValue;

            public PlayerCountItem(GameObject root, Func<T, Dictionary<byte, ushort>, Dictionary<ushort, ushort>, int> getPlayerCountFunc)
            {
                this.root = root;
                cellData = root.GetComponent<SelScrollRectCellBase<T, SelDiagonalScrollRectContext>>();
                this.getPlayerCountFunc = getPlayerCountFunc;
                //Instantiate player count graphic
                countRoot = GameObject.Instantiate(AssetBundleItems.PlayerCountPrefab, root.transform);
                countRoot.transform.localPosition = new Vector3(-449, 0, 0);
                countRoot.transform.localRotation = Quaternion.identity;
                countRoot.transform.localScale = Vector3.one;
                countText = countRoot.GetComponentInChildren<Text>();
                lastValue = -1;
            }

            public bool IsRootDestroyed => root == null;

            public void Destroy()
            {
                GameObject.Destroy(countRoot);
            }

            public void UpdateText(Dictionary<byte, ushort> courseCounts, Dictionary<ushort, ushort> stageCounts)
            {
                int playerCount = cellData?.itemData != null ? getPlayerCountFunc(cellData.itemData, courseCounts, stageCounts) : 0;
                bool active = playerCount != 0;
                if (active != countRoot.activeSelf)
                    countRoot.SetActive(active);
                if (playerCount != lastValue && countRoot.activeInHierarchy)
                {
                    countText.text = playerCount.ToString();
                    lastValue = playerCount;
                }
            }
        }

        interface IPlayerCountSet<out T> where T : class
        {
            void RecreateItemsIfNeeded();
            void DestroyAllItems();
            void UpdateText(Dictionary<byte, ushort> courseCounts, Dictionary<ushort, ushort> stageCounts);
        }

        class PlayerCountSet<T> : IPlayerCountSet<T> where T : class
        {
            private readonly Transform uiList;
            private GameObject itemContainer;
            private readonly Func<T, Dictionary<byte, ushort>, Dictionary<ushort, ushort>, int> getPlayerCountFunc;
            private PlayerCountItem<T>[] items;

            public PlayerCountSet(Transform uiList, string key, Func<T, Dictionary<byte, ushort>, Dictionary<ushort, ushort>, int> getPlayerCountFunc)
            {
                Key = key;
                this.uiList = uiList;
                itemContainer = null;
                this.getPlayerCountFunc = getPlayerCountFunc;
                items = Array.Empty<PlayerCountItem<T>>();
            }

            public string Key { get; }

            public void RecreateItemsIfNeeded()
            {
                //Try to find container if it has been destroyed
                if (itemContainer == null)
                {
                    //Look for itemContainer
                    itemContainer = uiList.Find(Key)?.Find("safe_area")?.Find("root")?.Find("00")?.Find("ScrollView")?.Find("Viewport")?.Find("Content")?.gameObject;

                    //Clear list if itemContainer is not found
                    if (itemContainer == null && items.Length > 0)
                    {
                        items = Array.Empty<PlayerCountItem<T>>();
                    }
                }

                //If we found container, check if we need to recreate the items
                if (itemContainer != null && (items.Length != itemContainer.transform.childCount || items.Any(i => i.IsRootDestroyed)))
                {
                    foreach (PlayerCountItem<T> item in items)
                    {
                        item.Destroy();
                    }
                    items = new PlayerCountItem<T>[itemContainer.transform.childCount];
                    for (int i = 0; i < itemContainer.transform.childCount; i++)
                    {
                        items[i] = new PlayerCountItem<T>(itemContainer.transform.GetChild(i).gameObject, getPlayerCountFunc);
                    }
                }
            }

            public void DestroyAllItems()
            {
                foreach (PlayerCountItem<T> item in items)
                {
                    item.Destroy();
                }
                items = Array.Empty<PlayerCountItem<T>>();
            }

            public void UpdateText(Dictionary<byte, ushort> courseCounts, Dictionary<ushort, ushort> stageCounts)
            {
                if (itemContainer == null || !itemContainer.activeInHierarchy) return;
                foreach (PlayerCountItem<T> item in items)
                {
                    item.UpdateText(courseCounts, stageCounts);
                }
            }
        }

        private ModSettings settings;
        private MgCourseDataManager courseDataManager;
        private readonly IPlayerCountSet<object>[] counts;
        private uint lastTick = uint.MaxValue;

        public PlayerCountManager(ModSettings settings)
        {
            this.settings = settings;
            settings.OnSettingChanged += HandleOnSettingChanged;

            Transform uiList = AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform;
            counts = new IPlayerCountSet<object>[]
            {
                new PlayerCountSet<SelTextItemData>(uiList, "c_sel_r_scr_T_0", (itemData, courseCounts, stageCounts) =>
                {
                    //Mode selection
                    SelMgModeItemData modeData = itemData.TryCast<SelMgModeItemData>();
                    if (modeData != null)
                    {
                        switch (modeData.mainGamemode)
                        {
                            case SelectorDef.MainGameKind.Story:
                                return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.Smb2_StoryWorld01,
                                    MainGameDef.eCourse.Smb2_StoryWorld02,
                                    MainGameDef.eCourse.Smb2_StoryWorld03,
                                    MainGameDef.eCourse.Smb2_StoryWorld04,
                                    MainGameDef.eCourse.Smb2_StoryWorld05,
                                    MainGameDef.eCourse.Smb2_StoryWorld06,
                                    MainGameDef.eCourse.Smb2_StoryWorld07,
                                    MainGameDef.eCourse.Smb2_StoryWorld08,
                                    MainGameDef.eCourse.Smb2_StoryWorld09,
                                    MainGameDef.eCourse.Smb2_StoryWorld10
                                }).Sum(stage => stageCounts[(ushort)stage]);
                            case SelectorDef.MainGameKind.Challenge_SMB1:
                                return new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.Smb1_Casual,
                                    MainGameDef.eCourse.Smb1_Normal,
                                    MainGameDef.eCourse.Smb1_Expert,
                                    MainGameDef.eCourse.Smb1_Master,
                                    MainGameDef.eCourse.Smb1_Marathon
                                }.Sum(course => courseCounts[(byte)course]);
                            case SelectorDef.MainGameKind.Challenge_SMB2:
                                return new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.Smb2_Casual,
                                    MainGameDef.eCourse.Smb2_Normal,
                                    MainGameDef.eCourse.Smb2_Expert,
                                    MainGameDef.eCourse.Smb2_Master,
                                    MainGameDef.eCourse.Smb2_Marathon
                                }.Sum(course => courseCounts[(byte)course]);
                            case SelectorDef.MainGameKind.Practice_SMB1:
                                return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.Smb1_Casual,
                                    MainGameDef.eCourse.Smb1_Normal,
                                    MainGameDef.eCourse.Smb1_Expert,
                                    MainGameDef.eCourse.Smb1_Master,
                                    MainGameDef.eCourse.Smb1_Marathon
                                }).Sum(stage => stageCounts[(ushort)stage]);
                            case SelectorDef.MainGameKind.Practice_SMB2:
                                return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.Smb2_Casual,
                                    MainGameDef.eCourse.Smb2_Normal,
                                    MainGameDef.eCourse.Smb2_Expert,
                                    MainGameDef.eCourse.Smb2_Master,
                                    MainGameDef.eCourse.Smb2_Marathon,
                                    MainGameDef.eCourse.Smb2_StoryWorld01,
                                    MainGameDef.eCourse.Smb2_StoryWorld02,
                                    MainGameDef.eCourse.Smb2_StoryWorld03,
                                    MainGameDef.eCourse.Smb2_StoryWorld04,
                                    MainGameDef.eCourse.Smb2_StoryWorld05,
                                    MainGameDef.eCourse.Smb2_StoryWorld06,
                                    MainGameDef.eCourse.Smb2_StoryWorld07,
                                    MainGameDef.eCourse.Smb2_StoryWorld08,
                                    MainGameDef.eCourse.Smb2_StoryWorld09,
                                    MainGameDef.eCourse.Smb2_StoryWorld10
                                }).Sum(stage => stageCounts[(ushort)stage]);
                            case SelectorDef.MainGameKind.Special:
                                return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                {
                                    MainGameDef.eCourse.SpecialReverse,
                                    MainGameDef.eCourse.SpecialRotten,
                                    MainGameDef.eCourse.SpecialGolden,
                                    MainGameDef.eCourse.SpecialOriginal,
                                    MainGameDef.eCourse.SmbDx
                                }).Sum(stage => stageCounts[(ushort)stage]);
                        }
                    }
                    return 0;
                }),
                new PlayerCountSet<SelIconItemData>(uiList, "c_sel_r_scr_CT_0", (itemData, courseCounts, stageCounts) =>
                {
                    //Story/special mode course selection
                    SelMgCourseItemData courseData = itemData.TryCast<SelMgCourseItemData>();
                    if (courseData != null)
                    {
                        return GetStagesInCourse(courseData.course).Sum(stageId => stageCounts[(ushort)stageId]);
                    }
                    return 0;
                }),
                new PlayerCountSet<SelIconAndBestTimeItemData>(uiList, "c_sel_r_scr_CTB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //Story/special mode stage selection
                    ushort stageId = GetStageFromTextKey(itemData.textKey);
                    if (stageCounts.ContainsKey(stageId))
                    {
                        return stageCounts[stageId];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelIconAndBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_CTB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //Challenge mode course selection
                    SelMgCmCourseItemData courseData = itemData.TryCast<SelMgCmCourseItemData>();
                    if (courseData != null && courseCounts.ContainsKey((byte)courseData.course))
                    {
                        return courseCounts[(byte)courseData.course];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_TB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //SMB1 practice mode course selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    return GetStagesInCourse(course).Sum(stageId => stageCounts[(ushort)stageId]);
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_TB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //SMB1 practice mode stage selection
                    ushort stageId = GetStageFromTextKey(itemData.textKey);
                    if (stageCounts.ContainsKey(stageId))
                    {
                        return stageCounts[stageId];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_u_scr_TB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //SMB2 practice mode story course and both story and challenge stage selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    if (course >= MainGameDef.eCourse.Smb2_StoryWorld01 && course <= MainGameDef.eCourse.Smb2_StoryWorld10)
                    {
                        return GetStagesInCourse(course).Sum(stageId => stageCounts[(ushort)stageId]);
                    }
                    ushort stage = GetStageFromTextKey(itemData.textKey);
                    if (stageCounts.ContainsKey(stage))
                    {
                        return stageCounts[stage];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_info_TB_0", (itemData, courseCounts, stageCounts) =>
                {
                    //SMB2 practice mode challenge course selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    if (course >= MainGameDef.eCourse.Smb1_Casual && course <= MainGameDef.eCourse.Smb1_Marathon)
                    {
                        //GetCourseFromTextKey returns the SMB1 courses so we need to transform it to be SMB2
                        course = (MainGameDef.eCourse)((byte)MainGameDef.eCourse.Smb2_Casual + ((byte)course - (byte)MainGameDef.eCourse.Smb1_Casual));
                        return GetStagesInCourse(course).Sum(stageId => stageCounts[(ushort)stageId]);
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_timeattack_course_select_0", (itemData, courseCounts, stageCounts) =>
                {
                    //Time attack course selection
                    SelMgTaCourseItemData courseData = itemData.TryCast<SelMgTaCourseItemData>();
                    if (courseData != null && courseCounts.ContainsKey((byte)courseData.course))
                    {
                        return courseCounts[(byte)courseData.course];
                    }
                    return 0;
                })
            };
        }

        ~PlayerCountManager()
        {
            settings.OnSettingChanged -= HandleOnSettingChanged;
        }

        private void HandleOnSettingChanged(object s, ModSettings.OnSettingChangedEventArgs e)
        {
            if (e.SettingChanged == ModSettings.Setting.PlayerCounts)
            {
                if (settings.ShowPlayerCounts)
                    RecreatePlayerCountsIfNeeded();
                else
                    DestroyAllItems();
            }
        }

        private IEnumerable<int> GetStagesInCourse(MainGameDef.eCourse course)
        {
            HashSet<int> stages = new HashSet<int>();
            MgCourseDatum courseDatum = courseDataManager.getCourseDatum(course);
            if (courseDatum != null)
            {
                foreach (MgCourseDatum.element_t element in courseDatum.elementList)
                {
                    stages.Add(element.stageId);
                }
            }
            return stages;
        }

        private IEnumerable<int> GetUniqueStagesInCourses(IEnumerable<MainGameDef.eCourse> courses)
        {
            HashSet<int> stages = new HashSet<int>();
            foreach (MainGameDef.eCourse course in courses)
            {
                foreach (int stageId in GetStagesInCourse(course))
                {
                    stages.Add(stageId);
                }
            }
            return stages;
        }

        private MainGameDef.eCourse GetCourseFromTextKey(string textKey)
        {
            //Check for story
            if (textKey.Length == 26 &&
                textKey.StartsWith("maingame_Smb2_StoryWorld") &&
                ushort.TryParse(textKey.Substring(24), out ushort courseIndex) &&
                courseIndex >= 1 && courseIndex <= 10)
            {
                return (MainGameDef.eCourse)((ushort)MainGameDef.eCourse.Smb2_StoryWorld01 + courseIndex - 1);
            }
            //Check for challenge or special
            switch (textKey)
            {
                case "maingame_casual":
                    return MainGameDef.eCourse.Smb1_Casual;
                case "maingame_normal":
                    return MainGameDef.eCourse.Smb1_Normal;
                case "maingame_expert":
                    return MainGameDef.eCourse.Smb1_Expert;
                case "maingame_master":
                    return MainGameDef.eCourse.Smb1_Master;
                case "maingame_marathon":
                    return MainGameDef.eCourse.Smb1_Marathon;
                case "maingame_reversemode":
                    return MainGameDef.eCourse.SpecialReverse;
                case "maingame_rottenbananamode":
                    return MainGameDef.eCourse.SpecialRotten;
                case "maingame_golden_banana_mode":
                    return MainGameDef.eCourse.SpecialGolden;
                case "maingame_originalstage_mode":
                    return MainGameDef.eCourse.SpecialOriginal;
                case "maingame_deluxe_mode":
                    return MainGameDef.eCourse.SmbDx;
                default:
                    return MainGameDef.eCourse.Invalid;
            }
        }

        private ushort GetStageFromTextKey(string textKey)
        {
            if (textKey.Length == 16 &&
                textKey.StartsWith("stagename_st") &&
                ushort.TryParse(textKey.Substring(12), out ushort stageId))
            {
                return stageId;
            }
            return ushort.MaxValue;
        }

        public void RecreatePlayerCountsIfNeeded()
        {
            if (!settings.ShowPlayerCounts) return;
            foreach (IPlayerCountSet<object> set in counts)
            {
                set.RecreateItemsIfNeeded();
            }
        }

        public void DestroyAllItems()
        {
            foreach (IPlayerCountSet<object> set in counts)
            {
                set.DestroyAllItems();
            }
        }

        public void UpdatePlayerCounts(Dictionary<byte, ushort> courseCounts, Dictionary<ushort, ushort> stageCounts, MgCourseDataManager courseDataManager, uint currentTick)
        {
            if (!settings.ShowPlayerCounts || currentTick == lastTick) return;
            lastTick = currentTick;

            this.courseDataManager = courseDataManager;
            foreach (IPlayerCountSet<object> set in counts)
            {
                set.UpdateText(courseCounts, stageCounts);
            }
        }
    }
}
