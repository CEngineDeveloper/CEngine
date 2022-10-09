using System;
namespace CYM.Utile
{
    public static class CustomSubstitutions
    {

        public static string DateShort()
        {

            return DateTime.Now.ToShortDateString();

        }

        public static string DateYear()
        {

            return DateTime.Now.Year.ToString();

        }

        public static string Author_CYM()
        {
            return "CYM";
        }

        public static string Name_MainUIView()
        {
            return BuildConfig.Ins.MainUIView;
        }

        public static string Name_Space()
        {
            return BuildConfig.Ins.NameSpace;
        }

        public static String Name_CpoyRight()
        {
            return "Shanghai Yi Ming Network Technology Co., Ltd.";
        }

    }
}