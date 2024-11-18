#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshCombines : MonoBehaviour
{
    public string folderPath = "Assets/Meshes";

    [ContextMenu("Save Combined Mesh")]
    private void CreateMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        // Проверяем наличие папки, если её нет, то создаем
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (MeshFilter filter in meshFilters)
        {
            Mesh mesh = filter.sharedMesh; // Получаем меш из компонента MeshFilter

            if (mesh != null)
            {
                string fileName = Guid.NewGuid() + ".asset"; // Имя файла
                string filePath = Path.Combine(folderPath, fileName); // Формируем путь для сохранения

                // Проверяем, существует ли файл, и если да, удаляем его
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    AssetDatabase.Refresh(); // Обновляем базу данных ресурсов Unity
                }

                // Сохраняем меш в файл формата .asset
                AssetDatabase.CreateAsset(mesh, filePath);
                Debug.Log("Mesh saved: " + filePath);
            }
        }
    }
}
#endif
