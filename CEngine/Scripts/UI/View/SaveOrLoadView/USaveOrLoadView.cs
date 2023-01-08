//------------------------------------------------------------------------------
// BaseSaveOrLoadView.cs
// Copyright 2019 2019/5/27 
// Created by CYM on 2019/5/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class USaveOrLoadView : UStaticUIView<USaveOrLoadView>
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField]
        UScroll RemoteScroll;
        [FoldoutGroup("Inspector"), SerializeField]
        UInput InputField;
        [FoldoutGroup("Inspector"), SerializeField]
        UButton BntSaveOrLoad;
        [FoldoutGroup("Inspector"), SerializeField]
        UButton BntBack;
        #endregion

        #region prop
        SaveOrLoad SaveOrLoad = SaveOrLoad.Load;
        IArchiveFile CurAchieve;
        #endregion

        #region life
        protected override string TitleKey => "存档";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            RemoteScroll?.Init(
                GetData,
                OnRefreshAchieve
            );
            BntSaveOrLoad.Init(new UButtonData()
            {
                Name = () =>GetStr(SaveOrLoad == SaveOrLoad.Load ? "加载" : "保存"),
                OnClick = OnBntSaveOrLoad,
                IsInteractable = GetBntSaveOrLoadInteractable
            });
            BntBack.Init(new UButtonData { 
                NameKey = "Back",
                OnClick = OnBntBack,
            });
        }
        public override void Awake()
        {
            base.Awake();
            if (BaseGlobal.DBMgr != null)
            {
                BaseGlobal.DBMgr.Callback_OnSaveState += OnSaveState;
            }
        }
        public override void OnDestroy()
        {
            if (BaseGlobal.DBMgr != null)
            {
                BaseGlobal.DBMgr.Callback_OnSaveState -= OnSaveState;
            }
            base.OnDestroy();
        }
        public override void Refresh()
        {
            base.Refresh();
            RefreshKeyElement();
        }
        public void Show(SaveOrLoad saveOrLoad)
        {
            SaveOrLoad = saveOrLoad;
            Show(true);
        }
        #endregion

        #region get
        protected virtual void OnArchiveItemRefresh(USaveOrLoadItem item, IArchiveFile file)
        {

        }
        #endregion

        #region utile
        private IList GetData()
        {
            var item = BaseGlobal.DBMgr.GetAchieveMgr();
            var all = item.GetAllBaseArchives(true);
            return all;
        }
        void OnRefreshAchieve(object p, object d)
        {
            USaveOrLoadItem item = p as USaveOrLoadItem;
            IArchiveFile itemData = d as IArchiveFile;
            if (item.IName) item.IName.text = itemData.Name;
            if (item.Time) item.Time.text = itemData.SaveTime.ToShortDateString();
            if (item.Duration) item.Duration.text = itemData.PlayTime.ToString();
            if (item.BntClose)
            {
                item.BntClose.Data.OnClick = OnClickDelete;                
                item.BntClose.NameText = GetStr("删除");
            }
            item.Data.OnClick = OnSaveOrLoadItemClick;
            bool IsInData = Version.IsInData(itemData.Header.Version);
            if (!IsInData)
            {
                item.NameText = string.Format($"<color=red>{"！"}{item.NameText}</color>");
            }
            if (item.ArchiveIcon)
            {
                item.ArchiveIcon.overrideSprite = itemData.Header.PlayerID.GetIcon(false);//GetArchiveIcon(itemData);
            }
            OnArchiveItemRefresh(item, itemData);
        }
        IArchiveFile GetArchiveFile(int index = SysConst.INT_Inv)
        {
            return RemoteScroll.GetData<IArchiveFile>(index);
        }
        void RefreshKeyElement()
        {
            CurAchieve = GetArchiveFile();
            if (SaveOrLoad == SaveOrLoad.Load)
            {
                InputField.EnableInput(false);
                if (CurAchieve != null)
                    InputField.InputText = CurAchieve.Name;
                else
                    InputField.InputText = "None";
            }
            else
            {
                InputField.EnableInput(true);
                if (CurAchieve != null)
                    InputField.InputText = CurAchieve.Name;
                else
                    InputField.InputText = BaseGlobal.DBMgr.GetDefaultSaveName();
            }
            //刷新按钮
            BntSaveOrLoad.Refresh();
        }
        #endregion

        #region Callback
        private void OnSaveOrLoadItemClick(UControl arg1, PointerEventData arg2)
        {
            CurAchieve = GetArchiveFile(arg1.Index);
            SetDirtyRefresh();
        }

        private void OnBntSaveOrLoad(UControl arg1, PointerEventData arg2)
        {
            if (SaveOrLoad == SaveOrLoad.Save)
            {
                if (InputField.InputText.IsInv())
                    return;

                if (BaseGlobal.DBMgr.IsHaveSameArchives(InputField.InputText))
                {
                    UModalBoxView.Default?.ShowOKCancle("Msg_覆盖存档",
                        () =>
                        {
                            BaseGlobal.DBMgr.SaveAs(InputField.InputText,false,GameConfig.Ins.DBSaveAsyn,true,false);
                        },
                        null
                        );
                }
                else
                {
                    BaseGlobal.DBMgr.SaveAs(InputField.InputText, false, GameConfig.Ins.DBSaveAsyn, true, false);
                }
            }
            else
            {
                BaseGlobal.BattleMgr.LoadGame(InputField.InputText);
                Close();
            }
        }
        private void OnClickDelete(UControl arg1, PointerEventData arg2)
        {
            UModalBoxView.Default?.ShowOKCancle("Msg_删除存档",
                   () =>
                   {
                       BaseGlobal.DBMgr.DeleteArchives(GetArchiveFile(arg1.Index).Name);
                       RemoteScroll.SelectIndex(0);
                       SetDirtyData();
                   },
                   null
                   );
        }
        private bool GetBntSaveOrLoadInteractable(int arg)
        {
            if (SaveOrLoad == SaveOrLoad.Load)
            {
                if (CurAchieve == null)
                    return false;
                return true;
            }
            else
            {

            }

            return true;
        }
        private void OnSaveState(bool arg1)
        {
            if (IsShow)
            {
                if (!arg1)
                {

                }
                else
                {
                    RemoteScroll.SelectIndex(0);
                    SetDirtyData();
                }
            }
        }
        private void OnBntBack(UControl arg1, PointerEventData arg2)
        {
            Close();
        }
        #endregion
    }
}