using UnityEditor;

public class AutoAddLayer : AssetPostprocessor
{
    private static string TagsAdd = "";

    private static string TagsLayer = "Floor";

    private const string ScriptsPath = "Assets/Editor/AutoAddLayer.cs";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)

    {
        foreach (string s in importedAssets)

        {
            //此脚本需要存在的路径

            if (s.Equals(ScriptsPath))

            {
                string[] tags = TagsAdd.Split('&');

                string[] Layers = TagsLayer.Split('&');

                foreach (var tag in tags) AddTag(tag);

                foreach (var layer in Layers) AddLayer(layer);

                //Debug.Log("loading....");

                return;
            }
        }
    }

    private static void AddTag(string tag)

    {
        if (!isHasTag(tag))

        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty it = tagManager.GetIterator();

            //Debug.Log("prop is:"+it);

            while (it.NextVisible(true))

            {
                if (it.name == "tags")

                {
                    //Debug.Log("NextVisible is:" + it.name+" size is:"+it.arraySize);

                    it.arraySize++;

                    for (int i = 0; i < it.arraySize; i++)

                    {
                        SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);

                        //Debug.Log("loading.... " + dataPoint.stringValue);

                        if (string.IsNullOrEmpty(dataPoint.stringValue))

                        {
                            dataPoint.stringValue = tag;

                            tagManager.ApplyModifiedProperties();

                            //Debug.Log("is loading tag");

                            return;
                        }
                    }
                }
            }
        }
    }

    private static void AddLayer(string layer)

    {
        if (!isHasLayer(layer))

        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty it = tagManager.GetIterator();

            while (it.NextVisible(true))

            {
                //Debug.Log("prop is:"+ it.name);

                if (it.name == "layers")

                {
                    for (int i = 0; i < it.arraySize; i++)

                    {
                        SerializedProperty dataPoint = it.GetArrayElementAtIndex(i + 1);

                        //Debug.Log("loading.... " + dataPoint.stringValue+"  i:"+i);

                        //默认前把层无法设置

                        if (string.IsNullOrEmpty(dataPoint.stringValue) && i >= 8)

                        {
                            dataPoint.stringValue = layer;

                            tagManager.ApplyModifiedProperties();

                            //Debug.Log("is loading layer");

                            return;
                        }
                    }
                }
            }
        }
    }

    private static bool isHasTag(string tag)

    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)

        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))

                return true;
        }

        return false;
    }

    private static bool isHasLayer(string layer)

    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)

        {
            if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))

                return true;
        }

        return false;
    }
}