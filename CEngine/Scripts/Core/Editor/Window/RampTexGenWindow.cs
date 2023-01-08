

namespace CYM
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    public class RampTexGenWindow : EditorWindow
    {
        /// <summary>
        /// 保存数据
        /// </summary>
        public class RampTexGenWindowData
        {
            // 纹理大小
            public int width;
            public int height;
            // 渐变
            public Gradient gradient;
            // 保存路径
            public string saveDirectory;
            // 保存名字
            public string saveName;

            public RampTexGenWindowData()
            {
                gradient = new Gradient();
                saveDirectory = "";
                saveName = "hello";
                width = 256;
                height = 8;
            }
        }
        RampTexGenWindowData data;


        public static void ShowWindow()
        {
            // 创建窗口
            RampTexGenWindow window = GetWindow(typeof(RampTexGenWindow),
                false,
                "渐变纹理生成器",
                true) as RampTexGenWindow;

            // 读取存档
            string json = EditorPrefs.GetString("RampTexGenWindowData", null);
            if (json != null)
            {
                window.data = JsonUtility.FromJson<RampTexGenWindowData>(json);
            }

            window.Show();
        }

        public void OnDisable()
        {
        }

        void OnGUI()
        {
            if (data == null)
            {
                data = new RampTexGenWindowData();
            }
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("渐变色：", GUILayout.Width(150f));
                data.gradient = EditorGUILayout.GradientField(data.gradient);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("贴图宽度：", GUILayout.Width(150f));
                data.width = EditorGUILayout.IntField(data.width);

            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("贴图高度：", GUILayout.Width(150f));
                data.height = EditorGUILayout.IntField(data.height);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("保存路径（不用加Assets）：", GUILayout.Width(150f));
                data.saveDirectory = EditorGUILayout.TextField(data.saveDirectory);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("文件名：", GUILayout.Width(150f));
                data.saveName = EditorGUILayout.TextField(data.saveName);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("保存"))
            {
                Texture2D png = new Texture2D(data.width, data.height, TextureFormat.ARGB32, false);
                for (int x = 0; x < data.width; x++)
                {
                    for (int y = 0; y < data.height; y++)
                    {
                        png.SetPixel(x, y, data.gradient.Evaluate(1f * x / data.width));
                    }
                }
                byte[] bytes = png.EncodeToPNG();

                string directory = $"{Application.dataPath}/{data.saveDirectory}/";
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string filePath = $"{directory}/{data.saveName}.png";
                if (File.Exists(filePath)) File.Delete(filePath);
                FileStream file = File.Create(filePath);
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    writer.Write(bytes);
                }
                file.Close();
                DestroyImmediate(png);

                AssetDatabase.Refresh();
                // 保存
                EditorPrefs.SetString("RampTexGenWindowData", JsonUtility.ToJson(data));
                Debug.Log($"保存RampTex成功: {filePath}");
            }

            GUILayout.EndVertical();
        }
    }
}
