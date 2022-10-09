//------------------------------------------------------------------------------
// UIEditorMutex.cs
// Created by CYM on 2022/5/27
// 填写类的描述...编辑器模式下的互斥组件,方便开发的时候预览
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace CYM.UI
{

    [ExecuteInEditMode][HideMonoScript]
    public class UIEditorMutex : MonoBehaviour
    {
        UUIView View;
        static Dictionary<UUIView, HashSet<UIEditorMutex>> Data = new ();

        [SerializeField,HideIf("IsHideGroup")]
        public MutexGroup Group = MutexGroup.None;

        #region UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && Application.isEditor)
            {
                UControl temp = gameObject.GetComponent<UControl>();
                if (temp != null)
                {
                    Group = temp.MutexGroup;
                }
                View = GetParent(transform);
                if (View != null)
                {
                    if (!Data.ContainsKey(View))
                        Data.Add(View,new());
                    var list = Data[View];
                    if (!list.Contains(this))
                        list.Add(this);
                }
            }

            UUIView GetParent(Transform self)
            {
                if (self.parent == null)
                    return null;
                UUIView view = self.parent.GetComponent<UUIView>();
                if (view == null)
                {
                    return GetParent(self.parent);
                }
                else
                {
                    return view;
                }
            }
        }
        bool IsHideGroup()
        {
            return gameObject.GetComponent<UControl>() != null;
        }
        #endregion
        private void OnEnable()
        {
            if (!Application.isPlaying && Application.isEditor)
            {
                UControl temp = gameObject.GetComponent<UControl>();
                if (temp != null)
                {
                    Group = temp.MutexGroup;
                }
                if (View != null)
                {
                    if (Group == MutexGroup.None)
                        return;
                    if (!Data.ContainsKey(View))
                        Data.Add(View, new());
                    var list = Data[View];
                    foreach (var item in list)
                    {
                        if (item != this && item.Group == Group)
                        {
                            if(item.gameObject.activeSelf)
                                item.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

    }
}