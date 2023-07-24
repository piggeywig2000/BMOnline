using System.Collections.Generic;
using System.Linq;
using BMOnline.Mod.Players;
using BMOnline.Mod.Settings;
using Flash2;
using UnityEngine;

namespace BMOnline.Mod.PlayerCount
{
    internal class PlayerCountManager
    {
        private IBmoSettings settings;
        private readonly IPlayerCountSet<object>[] counts;
        private uint lastTick = uint.MaxValue;

        public PlayerCountManager(IBmoSettings settings)
        {
            this.settings = settings;
            this.settings.PlayerCountMode.OnChanged += (s, e) =>
            {
                if (settings.PlayerCountMode.Value != PlayerCountOption.Disabled)
                    RecreatePlayerCountsIfNeeded();
                else
                    DestroyAllItems();
            };

            Transform uiList = AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform;
            counts = new IPlayerCountSet<object>[]
            {
                new PlayerCountSet<SelTextItemData>(uiList, "c_sel_r_scr_T_0", (itemData, counts) =>
                {
                    //Mode selection
                    SelMgModeItemData modeData = itemData.TryCast<SelMgModeItemData>();
                    if (modeData != null)
                    {
                        if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                        {
                            return counts.ModeCounts[modeData.mainGamemode];
                        }
                        else
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
                                    }).Sum(stage => counts.StageCounts[(ushort)stage]);
                                case SelectorDef.MainGameKind.Challenge_SMB1:
                                    if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed)
                                        return new MainGameDef.eCourse[]
                                        {
                                            MainGameDef.eCourse.Smb1_Casual,
                                            MainGameDef.eCourse.Smb1_Normal,
                                            MainGameDef.eCourse.Smb1_Expert,
                                            MainGameDef.eCourse.Smb1_Master,
                                            MainGameDef.eCourse.Smb1_Marathon
                                        }.Sum(course => counts.CourseCounts[course]);
                                    else
                                        return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                        {
                                            MainGameDef.eCourse.Smb1_Casual,
                                            MainGameDef.eCourse.Smb1_Normal,
                                            MainGameDef.eCourse.Smb1_Expert,
                                            MainGameDef.eCourse.Smb1_Master,
                                            MainGameDef.eCourse.Smb1_Marathon
                                        }).Sum(stage => counts.StageCounts[(ushort)stage]);
                                case SelectorDef.MainGameKind.Challenge_SMB2:
                                    if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed)
                                        return new MainGameDef.eCourse[]
                                        {
                                            MainGameDef.eCourse.Smb2_Casual,
                                            MainGameDef.eCourse.Smb2_Normal,
                                            MainGameDef.eCourse.Smb2_Expert,
                                            MainGameDef.eCourse.Smb2_Master,
                                            MainGameDef.eCourse.Smb2_Marathon
                                        }.Sum(course => counts.CourseCounts[course]);
                                    else
                                        return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                        {
                                            MainGameDef.eCourse.Smb2_Casual,
                                            MainGameDef.eCourse.Smb2_Normal,
                                            MainGameDef.eCourse.Smb2_Expert,
                                            MainGameDef.eCourse.Smb2_Master,
                                            MainGameDef.eCourse.Smb2_Marathon
                                        }).Sum(stage => counts.StageCounts[(ushort)stage]);
                                case SelectorDef.MainGameKind.Practice_SMB1:
                                    return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                    {
                                        MainGameDef.eCourse.Smb1_Casual,
                                        MainGameDef.eCourse.Smb1_Normal,
                                        MainGameDef.eCourse.Smb1_Expert,
                                        MainGameDef.eCourse.Smb1_Master,
                                        MainGameDef.eCourse.Smb1_Marathon
                                    }).Sum(stage => counts.StageCounts[(ushort)stage]);
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
                                    }).Sum(stage => counts.StageCounts[(ushort)stage]);
                                case SelectorDef.MainGameKind.Special:
                                    return GetUniqueStagesInCourses(new MainGameDef.eCourse[]
                                    {
                                        MainGameDef.eCourse.SpecialReverse,
                                        MainGameDef.eCourse.SpecialRotten,
                                        MainGameDef.eCourse.SpecialGolden,
                                        MainGameDef.eCourse.SpecialOriginal,
                                        MainGameDef.eCourse.SmbDx
                                    }).Sum(stage => counts.StageCounts[(ushort)stage]);
                            }
                        }
                    }
                    return 0;
                }),
                new PlayerCountSet<SelIconItemData>(uiList, "c_sel_r_scr_CT_0", (itemData, counts) =>
                {
                    //Story/special mode course selection
                    MainGameDef.eCourse course = itemData.TryCast<SelMgCourseItemData>()?.course ?? itemData.TryCast<SelMgSpCourseItemData>()?.course ?? MainGameDef.eCourse.Invalid;
                    if (course != MainGameDef.eCourse.Invalid)
                    {
                        if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                            return GetStagesInCourse(course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                        else if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                            return counts.CourseCounts[course];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelIconAndBestTimeItemData>(uiList, "c_sel_r_scr_CTB_0", (itemData, counts) =>
                {
                    //Story/special mode stage selection
                    ushort stageId = GetStageFromTextKey(itemData.textKey);
                    return counts.StageCounts[stageId];
                }),
                new PlayerCountSet<SelIconAndBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_CTB_0", (itemData, counts) =>
                {
                    //Challenge mode course selection
                    SelMgCmCourseItemData courseData = itemData.TryCast<SelMgCmCourseItemData>();
                    if (courseData != null)
                    {
                        if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                            return counts.CourseCounts[courseData.course];
                        else if (settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                            return GetStagesInCourse(courseData.course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_TB_0", (itemData, counts) =>
                {
                    //SMB1 practice mode course selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                        return GetStagesInCourse(course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                    else if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                        return counts.CourseCounts[course];
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_TB_0", (itemData, counts) =>
                {
                    //SMB1 practice mode stage selection
                    ushort stageId = GetStageFromTextKey(itemData.textKey);
                    return counts.StageCounts[stageId];
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_u_scr_TB_0", (itemData, counts) =>
                {
                    //SMB2 practice mode story course and both story and challenge stage selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    if (course >= MainGameDef.eCourse.Smb2_StoryWorld01 && course <= MainGameDef.eCourse.Smb2_StoryWorld10)
                    {
                        if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                            return GetStagesInCourse(course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                        else if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                            return counts.CourseCounts[course];
                    }
                    ushort stage = GetStageFromTextKey(itemData.textKey);
                    return counts.StageCounts[stage];
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_r_scr_mode_scr_info_TB_0", (itemData, counts) =>
                {
                    //SMB2 practice mode challenge course selection
                    MainGameDef.eCourse course = GetCourseFromTextKey(itemData.textKey);
                    if (course >= MainGameDef.eCourse.Smb1_Casual && course <= MainGameDef.eCourse.Smb1_Marathon)
                    {
                        //GetCourseFromTextKey returns the SMB1 courses so we need to transform it to be SMB2
                        course = (MainGameDef.eCourse)((byte)MainGameDef.eCourse.Smb2_Casual + ((byte)course - (byte)MainGameDef.eCourse.Smb1_Casual));
                        if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                            return GetStagesInCourse(course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                        else if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                            return counts.CourseCounts[course];
                    }
                    return 0;
                }),
                new PlayerCountSet<SelBestTimeItemData>(uiList, "c_sel_timeattack_course_select_0", (itemData, counts) =>
                {
                    //Time attack course selection
                    SelMgTaCourseItemData courseData = itemData.TryCast<SelMgTaCourseItemData>();
                    if (courseData != null)
                    {
                        if (settings.PlayerCountMode.Value == PlayerCountOption.Mixed || settings.PlayerCountMode.Value == PlayerCountOption.ExactMode)
                            return counts.CourseCounts[courseData.course];
                        else if (settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages)
                            return GetStagesInCourse(courseData.course).Sum(stageId => counts.StageCounts[(ushort)stageId]);
                    }
                    return 0;
                })
            };
        }

        private IEnumerable<int> GetStagesInCourse(MainGameDef.eCourse course)
        {
            HashSet<int> stages = new HashSet<int>();
            MgCourseDatum courseDatum = MgCourseDataManager.GetCourseDatum(course);
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
            return textKey switch
            {
                "maingame_casual" => MainGameDef.eCourse.Smb1_Casual,
                "maingame_normal" => MainGameDef.eCourse.Smb1_Normal,
                "maingame_expert" => MainGameDef.eCourse.Smb1_Expert,
                "maingame_master" => MainGameDef.eCourse.Smb1_Master,
                "maingame_marathon" => MainGameDef.eCourse.Smb1_Marathon,
                "maingame_reversemode" => MainGameDef.eCourse.SpecialReverse,
                "maingame_rottenbananamode" => MainGameDef.eCourse.SpecialRotten,
                "maingame_golden_banana_mode" => MainGameDef.eCourse.SpecialGolden,
                "maingame_originalstage_mode" => MainGameDef.eCourse.SpecialOriginal,
                "maingame_deluxe_mode" => MainGameDef.eCourse.SmbDx,
                _ => MainGameDef.eCourse.Invalid,
            };
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
            if (settings.PlayerCountMode.Value == PlayerCountOption.Disabled) return;
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

        public void UpdatePlayerCounts(IEnumerable<IOnlinePlayer> players, uint currentTick)
        {
            if (settings.PlayerCountMode.Value == PlayerCountOption.Disabled || currentTick == lastTick) return;
            lastTick = currentTick;

            PlayerCountData data = new PlayerCountData();
            foreach (IOnlinePlayer player in players)
            {
                SelectorDef.MainGameKind mode = SelectorDef.MainGameKind.Invalid;
                switch (player.Mode)
                {
                    case MainGameDef.eGameKind.Story: mode = SelectorDef.MainGameKind.Story; break;
                    case MainGameDef.eGameKind.Challenge:
                        if (player.Course >= MainGameDef.eCourse.Smb1_Casual && player.Course <= MainGameDef.eCourse.Smb1_Marathon)
                            mode = SelectorDef.MainGameKind.Challenge_SMB1;
                        else if (player.Course >= MainGameDef.eCourse.Smb2_Casual && player.Course <= MainGameDef.eCourse.Smb2_Marathon)
                            mode = SelectorDef.MainGameKind.Challenge_SMB2;
                        else if (player.Course == MainGameDef.eCourse.SpecialOriginal)
                            mode = SelectorDef.MainGameKind.Special;
                        else if (player.Course == MainGameDef.eCourse.SmbDx)
                            mode = SelectorDef.MainGameKind.Special;
                        break;
                    case MainGameDef.eGameKind.Practice:
                        if (player.Course >= MainGameDef.eCourse.Smb1_Casual && player.Course <= MainGameDef.eCourse.Smb1_Marathon)
                            mode = SelectorDef.MainGameKind.Practice_SMB1;
                        else if (player.Course >= MainGameDef.eCourse.Smb2_Casual && player.Course <= MainGameDef.eCourse.Smb2_Marathon)
                            mode = SelectorDef.MainGameKind.Practice_SMB2;
                        break;
                    case MainGameDef.eGameKind.TimeAttack: mode = SelectorDef.MainGameKind.TimeAttack; break;
                    case MainGameDef.eGameKind.Reverse: mode = SelectorDef.MainGameKind.Special; break;
                    case MainGameDef.eGameKind.Rotten: mode = SelectorDef.MainGameKind.Special; break;
                    case MainGameDef.eGameKind.Golden: mode = SelectorDef.MainGameKind.Special; break;
                }
                data.ModeCounts.IncrementCount(mode);
                if (settings.PlayerCountMode.Value == PlayerCountOption.ExactMode || (settings.PlayerCountMode.Value == PlayerCountOption.Mixed && (player.Mode == MainGameDef.eGameKind.Challenge || player.Mode == MainGameDef.eGameKind.TimeAttack)))
                    data.CourseCounts.IncrementCount(player.Course);
                if (player.Stage.HasValue)
                    data.StageCounts.IncrementCount(player.Stage.Value);
            }

            foreach (IPlayerCountSet<object> set in counts)
            {
                set.UpdateText(data);
            }
        }
    }
}
