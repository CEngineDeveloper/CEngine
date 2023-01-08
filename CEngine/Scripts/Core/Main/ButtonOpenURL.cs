using UnityEngine;
using System.Collections;

namespace CYM
{
    public class ButtonOpenURL : MonoBehaviour
    {
        public string url = "http://www.baidu.com/";

        void OpenUrl()
        {
            Application.OpenURL(url);
        }
    }
}