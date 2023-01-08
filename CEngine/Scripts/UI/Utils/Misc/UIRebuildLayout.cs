//------------------------------------------------------------------------------
// UIRebuildLayout.cs
// Created by CYM on 2022/5/24
// 填写类的描述...
//------------------------------------------------------------------------------
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    [HideMonoScript]
    public class UIRebuildLayout : MonoBehaviour
    {
        [SerializeField]
        float delay = 0.02f;
        private void OnEnable()
        {
            Util.Invoke(()=> {
                LayoutRebuilder.ForceRebuildLayoutImmediate((transform as RectTransform));
            }, delay);
        }
    }
}