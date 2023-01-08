using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    [AddComponentMenu("UI/Control/URawImage")]
    [HideMonoScript]
    public class URawImage : UPres<UImageData> 
    {
        #region 组建
        [FoldoutGroup("Inspector"), ChildGameObjectsOnly,Required]
        public RawImage IImage;
        #endregion

        #region life
        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Icon != null && IImage != null)
            {
                Sprite temp = Data.Icon.Invoke();
                if (temp == null)
                    return;
                IImage.texture = temp.texture;
            }
        }
        public void Refresh(string icon)
        {
            if (!icon.IsInv())
                IImage.texture = icon.GetIcon().texture;
        }
        public void Refresh(Sprite icon)
        {
            IImage.texture = icon.texture;
        }
        #endregion

    }
}