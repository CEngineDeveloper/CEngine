//------------------------------------------------------------------------------
// UIRebuildLayout.cs
// Created by CYM on 2022/5/24
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UIRebuildLayout : MonoBehaviour
    {
        [SerializeField]
        float delay = 0.2f;
        private void OnEnable()
        {
            Util.Invoke(()=> {
                LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
            }, delay);
        }
    }
}