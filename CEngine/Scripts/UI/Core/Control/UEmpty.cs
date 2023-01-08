using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.UI
{
    [AddComponentMenu("UI/Control/UEmpty")]
    [HideMonoScript]
    public class UEmpty : UPres<UData>
    {
        public override bool IsAtom => true;

    }

}