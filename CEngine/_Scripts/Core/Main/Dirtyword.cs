//------------------------------------------------------------------------------
// FilterHelper.cs
// Created by CYM on 2021/3/22
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using CYM.Excel;

namespace CYM
{
    #region 非法关键字过滤 bate 1.1
    /// <summary>
    /// 非法关键词过滤(自动忽略汉字数字字母间的其他字符)
    /// </summary>
    public sealed class Dirtyword
    {
        /// <summary>
        /// 内存词典
        /// </summary>
        static private WordGroup[] MEMORYLEXICON = new WordGroup[(int)char.MaxValue];
        /// <summary>
        /// 检测源
        /// </summary>
        static public string SourctText { get; private set; }
        /// <summary>
        /// 检测源游标
        /// </summary>
        static int cursor = 0;
        /// <summary>
        /// 匹配成功后偏移量
        /// </summary>
        static int wordlenght = 0;
        /// <summary>
        /// 检测词游标
        /// </summary>
        static int nextCursor = 0;
        /// <summary>
        /// 检测到的非法词集
        /// </summary>
        static public List<string> IllegalWords { get; private set; } = new List<string>();
        /// <summary>
        /// 判断是否是中文
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        static private bool IsCHS(char character)
        {
            //  中文表意字符的范围 4E00-9FA5
            int charVal = (int)character;
            return (charVal >= 0x4e00 && charVal <= 0x9fa5);
        }
        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        static private bool IsNum(char character)
        {
            int charVal = (int)character;
            return (charVal >= 48 && charVal <= 57);
        }
        /// <summary>
        /// 判断是否是字母
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        static private bool IsAlphabet(char character)
        {
            int charVal = (int)character;
            return ((charVal >= 97 && charVal <= 122) || (charVal >= 65 && charVal <= 90));
        }
        /// <summary>
        /// 转半角小写的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        static private string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c).ToLower();
        }
        /// <summary>
        /// 检测
        /// </summary>
        /// <param name="blackWord"></param>
        /// <returns></returns>
        static private bool Check(string blackWord)
        {
            wordlenght = 0;
            //检测源下一位游标
            nextCursor = cursor + 1;
            bool found = false;
            //遍历词的每一位做匹配
            for (int i = 0; i < blackWord.Length; i++)
            {
                //特殊字符偏移游标
                int offset = 0;
                if (nextCursor >= SourctText.Length)
                {
                    break;
                }
                else
                {
                    //检测下位字符如果不是汉字 数字 字符 偏移量加1
                    for (int y = nextCursor; y < SourctText.Length; y++)
                    {

                        if (!IsCHS(SourctText[y]) && !IsNum(SourctText[y]) && !IsAlphabet(SourctText[y]))
                        {
                            offset++;
                            //避让特殊字符，下位游标如果>=字符串长度 跳出
                            if (nextCursor + offset >= SourctText.Length) break;
                            wordlenght++;

                        }
                        else break;
                    }

                    if ((int)blackWord[i] == (int)SourctText[nextCursor + offset])
                    {
                        found = true;
                    }
                    else
                    {
                        found = false;
                        break;
                    }


                }
                nextCursor = nextCursor + 1 + offset;
                wordlenght++;


            }
            return found;
        }
        /// <summary>
        /// 具有相同首字符的词组集合
        /// </summary>
        class WordGroup
        {
            /// <summary>
            /// 集合
            /// </summary>
            private List<string> groupList;

            public WordGroup()
            {
                groupList = new List<string>();
            }

            /// <summary>
            /// 添加词
            /// </summary>
            /// <param name="word"></param>
            public void Add(string word)
            {
                groupList.Add(word);
            }

            /// <summary>
            /// 获取总数
            /// </summary>
            /// <returns></returns>
            public int Count()
            {
                return groupList.Count;
            }

            /// <summary>
            /// 根据下标获取词
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string GetWord(int index)
            {
                return groupList[index];
            }
        }

        #region pub
        public static void Load(byte[] buffer)
        {
            if (buffer == null)
                return;
            HashSet<string> dirtyWord = new HashSet<string>();
            WorkBook workBook = BaseExcelMgr.ReadWorkbook(buffer);
            if (workBook != null && workBook.Count > 0)
            {
                foreach (var sheet in workBook)
                {
                    if (sheet.Name.StartsWith(SysConst.Prefix_Lang_Notes))
                        continue;
                    foreach (var row in sheet)
                    {
                        if (row.Cells.Count > 0)
                        {
                            var text = row.Cells[0].Text;
                            if (!text.IsInv())
                                dirtyWord.Add(text);
                        }
                    }
                }
            }
            Load(dirtyWord);
        }
        /// <summary>
        /// 加载内存词库
        /// </summary>
        public static void Load(HashSet<string> words)
        {
            List<string> wordList = new List<string>();
            Array.Clear(MEMORYLEXICON, 0, MEMORYLEXICON.Length);
            foreach (string word in words)
            {
                string key = ToDBC(word);
                wordList.Add(key);
            }
            for (int i = wordList.Count - 1; i > 0; i--)
            {
                if (wordList[i].ToString() == wordList[i - 1].ToString())
                {
                    wordList.RemoveAt(i);
                }
            }
            foreach (var word in wordList)
            {
                if (word.Length > 0)
                {
                    WordGroup group = MEMORYLEXICON[(int)word[0]];
                    if (group == null)
                    {
                        group = new WordGroup();
                        MEMORYLEXICON[(int)word[0]] = group;

                    }
                    group.Add(word.Substring(1));
                }
            }
            CLog.Info("加载脏词库成功");
        }
        /// <summary>
        /// 查找并替换
        /// </summary>
        /// <param name="replaceChar"></param>
        public static string Filter(string text, char replaceChar='*')
        {
            cursor = 0;
            wordlenght = 0;
            IllegalWords.Clear();
            SourctText = text;
            if (SourctText != string.Empty)
            {
                char[] tempString = SourctText.ToCharArray();
                for (int i = 0; i < SourctText.Length; i++)
                {
                    //查询以该字为首字符的词组
                    WordGroup group = MEMORYLEXICON[(int)ToDBC(SourctText)[i]];
                    if (group != null)
                    {
                        for (int z = 0; z < group.Count(); z++)
                        {
                            string word = group.GetWord(z);
                            if (word.Length == 0 || Check(word))
                            {
                                string blackword = string.Empty;
                                for (int pos = 0; pos < wordlenght + 1; pos++)
                                {
                                    if ((pos + cursor) >= tempString.Length)
                                        continue;
                                    blackword += tempString[pos + cursor].ToString();
                                    tempString[pos + cursor] = replaceChar;
                                }
                                IllegalWords.Add(blackword);
                                cursor = cursor + wordlenght;
                                i = i + wordlenght;

                            }
                        }
                    }
                    cursor++;
                }
                return new string(tempString);
            }
            else
            {
                return string.Empty;
            }

        }
        #endregion
    }

    #endregion
}