/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// @author : Bloc
// @date   : 01.08.2015
// @version: 1.0v
// About : Simple terrain tool which lets you use nearly any texture whether its colored or not.
// 
// This is slightly improved version of old code. All thanks goes to Zahari Pulev for this lovely C# script.
//
//
// Please be caution while you are editing the code. You are not allowed to sell this asset. For basic info please check documentation. Regardless,
// I tried my best to explain everything in the comment sections.
// It's a basic code , you have all right to improve it for your own. I would be happy If you let me know If you find any problem or improvement.
// You can contact with me from cartridgegamestudion@gmail.com / Thanks to Eric Haines (Eric5h5) for his OSP code
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CYM
{
    public class TerrainHeightWindow : EditorWindow
    {
        public class TerraHe
        {
            public static void ApplyHeightmap(Texture2D heightmap, bool isTextureColored, float terrainHeight, bool useImageAsTexture, bool wantFlatTerrain, float offset, float redVal, float greenVal, float blueVal)
            {
                //Checks if heightmap loaded or not
                if (heightmap == null)
                {
                    EditorUtility.DisplayDialog("Texture isn't selected", "Please Select a Texture", "Cancel");
                    return;
                }

                Undo.RegisterCompleteObjectUndo(Terrain.activeTerrain.terrainData, "Heightmap From Texture");

                try
                {
                    TerrainData terrain = Terrain.activeTerrain.terrainData;
                    int terrainWidth = terrain.heightmapResolution;

                    Color[] textureColors = GetTextureColors(heightmap, terrainWidth);
                    float[,] heightmapData = ConvertTextureToHeightmap(terrainWidth, terrainHeight, textureColors, isTextureColored, wantFlatTerrain, offset, redVal, greenVal, blueVal);

                    //Set data to terrain
                    terrain.SetHeights(0, 0, heightmapData);

                    if (useImageAsTexture)
                        AddTextureToTerrain(heightmap, terrain);
                    else
                        RemoveTextureFromTerrain(heightmap, terrain);
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (UnityException e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    EditorUtility.DisplayDialog("Texture Format Problem!", "Texture should be readable!", "Cancel");
                    return;
                }
            }

            private static Color[] GetTextureColors(Texture2D heightmap, int terrainWidth)
            {
                int textureWidth = heightmap.width;
                int textureHeight = heightmap.height;

                bool doesTextureMatchTerrainSize = terrainWidth == textureWidth;
                bool isTextureSquare = textureHeight == textureWidth;

                if (!doesTextureMatchTerrainSize || !isTextureSquare)
                    return ResizeHeightmap(heightmap, terrainWidth);
                else
                {
                    var map = heightmap.GetPixels();
                    System.Array.Resize(ref map, terrainWidth * terrainWidth);

                    return map;
                }
            }

            private static Color[] ResizeHeightmap(Texture2D heightmap, int terrainWidth)
            {
                var resizedTextureColors = new Color[terrainWidth * terrainWidth];

                int textureWidth = heightmap.width;
                int textureHeight = heightmap.height;

                Color[] textureColors = heightmap.GetPixels();

                if (heightmap.filterMode == FilterMode.Point)
                    ResizeUsingNearestNeighbour(textureWidth, textureHeight, terrainWidth, textureColors, resizedTextureColors);
                else
                    ResizeUsingBilinearFiltering(textureWidth, textureHeight, terrainWidth, textureColors, resizedTextureColors);

                EditorUtility.ClearProgressBar();

                return resizedTextureColors;
            }

            private static void ResizeUsingNearestNeighbour(int textureWidth, int textureHeight, int terrainWidth, Color[] textureColors, Color[] resizedTextureColors)
            {
                float dx = (float)textureWidth / terrainWidth;
                float dy = (float)textureHeight / terrainWidth;
                for (int y = 0; y < terrainWidth; y++)
                {
                    if (y % 20 == 0)
                        EditorUtility.DisplayProgressBar("Resize", "Calculating texture", Mathf.InverseLerp(0f, terrainWidth, y));

                    int thisY = (int)(dy * y) * textureWidth;
                    var yw = y * terrainWidth;

                    for (int x = 0; x < terrainWidth; x++)
                        resizedTextureColors[yw + x] = textureColors[thisY + (int)(dx * x)];
                }
            }

            private static void ResizeUsingBilinearFiltering(int textureWidth, int textureHeight, int terrainWidth, Color[] textureColors, Color[] resizedTextureColors)
            {
                float ratioX = 1f / ((float)terrainWidth / (textureWidth - 1));
                float ratioY = 1f / ((float)terrainWidth / (textureHeight - 1));
                for (int y = 0; y < terrainWidth; y++)
                {
                    if (y % 20 == 0)
                        EditorUtility.DisplayProgressBar("Resize", "Calculating texture", Mathf.InverseLerp(0f, terrainWidth, y));

                    var yy = Mathf.Floor(y * ratioY);
                    var y1 = yy * textureWidth;
                    var y2 = (yy + 1) * textureWidth;
                    int yw = y * terrainWidth;

                    for (int x = 0; x < terrainWidth; x++)
                    {
                        float xx = Mathf.Floor(x * ratioX);

                        Color bl = textureColors[(int)(y1 + xx)];
                        Color br = textureColors[(int)(y1 + xx + 1)];
                        Color tl = textureColors[(int)(y2 + xx)];
                        Color tr = textureColors[(int)(y2 + xx + 1)];

                        float xLerp = x * ratioX - xx;
                        resizedTextureColors[yw + x] = Color.Lerp(Color.Lerp(bl, br, xLerp), Color.Lerp(tl, tr, xLerp), y * ratioY - yy);
                    }
                }
            }

            private static float[,] ConvertTextureToHeightmap(int terrainWidth, float terrainHeight, Color[] textureColors, bool isTextureColored, bool wantFlatTerrain, float offset, float redVal, float greenVal, float blueVal)
            {
                float[,] heightmapData = new float[terrainWidth, terrainWidth];

                for (int y = 0; y < terrainWidth; y++)
                    for (int x = 0; x < terrainWidth; x++)
                    {
                        var color = textureColors[y * terrainWidth + x];
                        var grayscale = color.grayscale;

                        float hue = 0, sat = 0, val = 0;
                        //Change RGB values to HSV therefore detecting the value range between colors become more possible
                        Color.RGBToHSV(color, out hue, out sat, out val);

                        if (isTextureColored)
                        {
                            //Check if User want to use Flat Terrain
                            if (wantFlatTerrain)
                            {
                                if (160f < hue * 360f && hue * 360f < 280f)
                                    heightmapData[y, x] = grayscale * (-hue * terrainHeight * blueVal);
                                else if ((val * 100f < 70) && (45 <= hue * 360f) || (val * 100f < 70) && (hue * 360f <= 327))
                                    heightmapData[y, x] = 0.1f * grayscale * terrainHeight * greenVal;
                                else if ((val * 100f < 70) && (hue * 360f <= 45))
                                    heightmapData[y, x] = 0.08f * grayscale * terrainHeight * redVal;
                                else
                                    heightmapData[y, x] = 0.17f * grayscale * terrainHeight;
                            }
                            else
                            {
                                if (160f < hue * 360f && hue * 360f < 280f)
                                    heightmapData[y, x] = grayscale * (-hue * terrainHeight * blueVal);
                                else if (45 <= hue * 360f && hue * 360f < 160)
                                    heightmapData[y, x] = 0.3f * grayscale * terrainHeight * greenVal + offset;
                                else
                                    heightmapData[y, x] = 0.4f * grayscale * terrainHeight * redVal + offset;
                            }
                        }
                        else
                        {
                            if (val * 100f < 2)
                                heightmapData[y, x] = grayscale * -terrainHeight + offset;
                            else
                                heightmapData[y, x] = (val * 0.3f) * terrainHeight + offset;
                        }
                    }

                return heightmapData;
            }

            private static void AddTextureToTerrain(Texture2D heightmap, TerrainData terrain)
            {
                terrain.splatPrototypes = new SplatPrototype[]{
            new SplatPrototype()
            {
                texture = heightmap,
                tileOffset = Vector2.zero,
                tileSize = new Vector2(terrain.size.x, terrain.size.z)
            }
         };
                ;
            }

            private static void RemoveTextureFromTerrain(Texture2D heightmap, TerrainData terrain)
            {
                List<SplatPrototype> textures = new List<SplatPrototype>();

                //If the given texture matches a texture stored in the terrain, remove that texture
                foreach (var terrainTexture in terrain.splatPrototypes)
                    if (terrainTexture.texture != heightmap)
                        textures.Add(terrainTexture);

                terrain.splatPrototypes = textures.ToArray();
            }
        }

        private Texture2D texture;
        private bool coloredPick = true;
        private bool colorUse = true;
        private bool flat = false;
        private bool changeColorVals = false;
        private float terraHeight = 1.0f;

        private float offset = 0f;

        private float redVal = 1.0f;
        private float greenVal = 1.0f;
        private float blueVal = 1.0f;

        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow(typeof(TerrainHeightWindow));
            window.position = new Rect(50, 50, 600, 600);
            window.Show();
        }

        private void OnGUI()
        {
            terraHeight = EditorGUILayout.Slider("Terrain Height : ", terraHeight, 0.1f, 2);
            offset = EditorGUILayout.Slider("Land Height Offset: ", offset, 0, 0.5f);
            colorUse = EditorGUILayout.Toggle("Use Image as Texture : ", colorUse);

            EditorGUILayout.Space();
            coloredPick = EditorGUILayout.BeginToggleGroup("Colored Texture?", coloredPick);
            flat = EditorGUILayout.Toggle("Make Flat Areas : ", flat);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

            changeColorVals = EditorGUILayout.BeginToggleGroup("Customize Color Heights", changeColorVals);
            redVal = EditorGUILayout.Slider("Red Color Height : ", redVal, 0.5f, 3);
            greenVal = EditorGUILayout.Slider("Green Color Height : ", greenVal, 0.5f, 3);
            blueVal = EditorGUILayout.Slider("Blue Color Height : ", blueVal, 0.5f, 3);
            EditorGUILayout.EndToggleGroup();

            texture = EditorGUILayout.ObjectField("Add a Texture:", texture, typeof(Texture), false, GUILayout.MaxWidth(200)) as Texture2D;

            if (!changeColorVals)
            {
                redVal = 1.0f;
                greenVal = 1.0f;
                blueVal = 1.0f;
            }

            bool textureAssigned = texture != null;

            GUI.enabled = textureAssigned;
            if (GUILayout.Button("Make It", GUILayout.MinWidth(50), GUILayout.MaxWidth(300)))
            {
                PrepareTexture();

                //Copy the new texture
                TerraHe.ApplyHeightmap(texture, coloredPick, terraHeight, colorUse, flat, offset, redVal, greenVal, blueVal);
            }

            if (!textureAssigned)
            {
                GUI.enabled = true;
                EditorGUILayout.LabelField("You must assign a texture");
            }
            else
                EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(300), GUILayout.Height(300)), texture);
        }

        private void PrepareTexture()
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            importer.isReadable = true;

            if (importer.filterMode != FilterMode.Point && importer.filterMode != FilterMode.Bilinear)
                importer.filterMode = FilterMode.Bilinear;

            Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
            if (asset != null)
                EditorUtility.SetDirty(asset);
        }
    }
}