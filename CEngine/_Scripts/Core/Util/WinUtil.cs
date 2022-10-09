//------------------------------------------------------------------------------
// BaseWinUtils.cs
// Copyright 2019 2019/4/14 
// Created by CYM on 2019/4/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CYM
{
    public class WinUtil
    {
        public static int MessageBox(String message, String title)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return MessageBox(IntPtr.Zero, message, title, 0);
#endif
        }

        public static void DisableSysMenuButton()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN    
            if (!UnityEngine.Application.isEditor && 
                !BuildConfig.Ins.IsShowWinClose)
            {
                IntPtr hWindow = GetForegroundWindow();                                                       
                IntPtr hMenu = GetSystemMenu(hWindow, false);
                int count = GetMenuItemCount(hMenu);
                RemoveMenu(hMenu, count - 1, MF_BYPOSITION);
                RemoveMenu(hMenu, count - 2, MF_BYPOSITION);
            }
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private const int MF_BYPOSITION = 0x00000400;
        /// <summary>
        /// 取得指定窗口的系统菜单的句柄。
        /// </summary>
        /// <param name="hwnd">指向要获取系统菜单窗口的 <see cref="IntPtr"/> 句柄。</param>
        /// <param name="bRevert">获取系统菜单的方式。设置为 <b>true</b>，表示接收原始的系统菜单，否则设置为 <b>false</b> 。</param>
        /// <returns>指向要获取的系统菜单的 <see cref="IntPtr"/> 句柄。</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);


        // <summary>
        /// 获取指定的菜单中条目（菜单项）的数量。
        /// </summary>
        /// <param name="hMenu">指向要获取菜单项数量的系统菜单的 <see cref="IntPtr"/> 句柄。</param>
        /// <returns>菜单中的条目数量</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMenuItemCount(IntPtr hMenu);


        /// <summary>
        /// 删除指定的菜单条目。
        /// </summary>
        /// <param name="hMenu">指向要移除的菜单的 <see cref="IntPtr"/> 。</param>
        /// <param name="uPosition">欲改变的菜单条目的标识符。</param>
        /// <param name="uFlags"></param>
        /// <returns>非零表示成功，零表示失败。</returns>
        /// <remarks>
        /// 如果在 <paramref name="uFlags"/> 中使用了<see cref="MF_BYPOSITION"/> ，则在 <paramref name="uPosition"/> 参数表示菜单项的索引；
        /// 如果在 <paramref name="uFlags"/> 中使用了 <b>MF_BYCOMMAND</b>，则在 <paramref name="uPosition"/> 中使用菜单项的ID。
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

        [DllImport("user32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr handle, String message, String title, int type);//具体方法
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        static extern long GetWindowLong(IntPtr hwnd, long nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern long GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, long nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

#endif
    }
}