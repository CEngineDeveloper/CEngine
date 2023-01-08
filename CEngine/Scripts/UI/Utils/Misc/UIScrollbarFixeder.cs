//------------------------------------------------------------------------------
// UIScrollbarFixeder.cs
// Created by CYM on 2021/11/7
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
namespace CYM.UI
{
    [ExecuteInEditMode][HideMonoScript]
    public class UIScrollbarFixeder : MonoBehaviour
    {
        Scrollbar Scrollbar;

        public void Awake()
        {
            Scrollbar = GetComponent<Scrollbar>();
            if (Scrollbar)
            {
                var image = Scrollbar.handleRect.GetComponent<Image>();
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }
        }
        private void Update()
        {
            if (Scrollbar)
                Scrollbar.size = 0;
        }
        private void LateUpdate()
        {
            if (Scrollbar)
                Scrollbar.size = 0;
        }
        private void FixedUpdate()
        {
            if (Scrollbar)
                Scrollbar.size = 0;
        }
        private void OnEnable()
        {
            if (Scrollbar)
                Scrollbar.onValueChanged.AddListener(OnValueChange);
        }
        private void OnDisable()
        {
            if (Scrollbar)
                Scrollbar.onValueChanged.RemoveListener(OnValueChange);
        }
        private void OnValueChange(float arg0)
        {
            if (isActiveAndEnabled)
                Scrollbar.size = 0;
        }
    }
}