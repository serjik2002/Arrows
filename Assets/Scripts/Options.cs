using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace options
{
    /// <summary>
    /// Статична система Options для data-driven роботи з параметрами через XML.
    /// Використовує dot-notation для доступу до атрибутів: "section.subsection.attribute"
    /// </summary>
    public static class Options
    {
        private static XmlDocument xmlDoc;
        private static string filePath;
        private static bool isDirty = false;
        private static bool isInitialized = false;

        private const string ROOT_NODE_NAME = "Options";
        private const string DEFAULT_FILENAME = "options.xml";

        /// <summary>
        /// Ініціалізує систему Options. Викликається автоматично при першому зверненні.
        /// </summary>
        private static void Initialize()
        {
            if (isInitialized) return;


            filePath = Path.Combine(Application.persistentDataPath, DEFAULT_FILENAME);//тут можна змінити шлях до файлу
            xmlDoc = new XmlDocument();

            if (File.Exists(filePath))
            {
                try
                {
                    xmlDoc.Load(filePath);
                    Console.WriteLine($"[Options] Завантажено з {filePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Options] Помилка завантаження XML: {e.Message}. Створюється новий файл.");
                    CreateNewDocument();
                }
            }
            else
            {
                CreateNewDocument();
            }

            isInitialized = true;
        }

        /// <summary>
        /// Створює новий XML-документ з кореневим елементом
        /// </summary>
        private static void CreateNewDocument()
        {
            xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(declaration);

            XmlElement root = xmlDoc.CreateElement(ROOT_NODE_NAME);
            xmlDoc.AppendChild(root);

            isDirty = true;
        }

        /// <summary>
        /// Знаходить або створює XML-ноду за шляхом (без останнього сегмента - атрибута)
        /// </summary>
        private static XmlElement GetOrCreateNode(string path, bool createIfMissing)
        {
            if (!isInitialized) Initialize();

            string[] segments = path.Split('.');
            if (segments.Length < 2)
            {
                Console.WriteLine($"[Options] Некоректний шлях: {path}. Очікується формат 'node.attribute'");
                return null;
            }

            XmlElement current = xmlDoc.DocumentElement;

            // Проходимо всі сегменти крім останнього (останній - це атрибут)
            for (int i = 0; i < segments.Length - 1; i++)
            {
                string segment = segments[i];
                XmlElement child = current[segment];

                if (child == null)
                {
                    if (!createIfMissing) return null;

                    child = xmlDoc.CreateElement(segment);
                    current.AppendChild(child);
                    isDirty = true;
                }

                current = child;
            }

            return current;
        }

        /// <summary>
        /// Отримує ім'я атрибута з повного шляху
        /// </summary>
        private static string GetAttributeName(string path)
        {
            string[] segments = path.Split('.');
            return segments[segments.Length - 1];
        }

        // ===== GET методи =====

        /// <summary>
        /// Отримує int значення за ключем
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            string value = GetString(key, null);
            return value != null && int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Отримує float значення за ключем
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            string value = GetString(key, null);
            return value != null && float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        /// <summary>
        /// Отримує bool значення за ключем
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            string value = GetString(key, null);
            if (value == null) return defaultValue;

            value = value.ToLower();
            if (value == "true" || value == "1") return true;
            if (value == "false" || value == "0") return false;

            return defaultValue;
        }

        /// <summary>
        /// Отримує string значення за ключем
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            XmlElement node = GetOrCreateNode(key, false);
            if (node == null) return defaultValue;

            string attrName = GetAttributeName(key);
            if (!node.HasAttribute(attrName)) return defaultValue;

            return node.GetAttribute(attrName);
        }

        // ===== SET методи =====

        /// <summary>
        /// Записує int значення за ключем
        /// </summary>
        public static void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        /// <summary>
        /// Записує float значення за ключем
        /// </summary>
        public static void SetFloat(string key, float value)
        {
            SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Записує bool значення за ключем
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            SetString(key, value ? "true" : "false");
        }

        /// <summary>
        /// Записує string значення за ключем
        /// </summary>
        public static void SetString(string key, string value)
        {
            XmlElement node = GetOrCreateNode(key, true);
            if (node == null) return;

            string attrName = GetAttributeName(key);
            string currentValue = node.HasAttribute(attrName) ? node.GetAttribute(attrName) : null;

            if (currentValue != value)
            {
                node.SetAttribute(attrName, value);
                isDirty = true;
            }
        }

        // ===== Збереження та керування =====

        /// <summary>
        /// Зберігає XML у файл, якщо були зміни
        /// </summary>
        public static void Save()
        {
            if (!isInitialized) Initialize();
            if (!isDirty) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                xmlDoc.Save(filePath);
                isDirty = false;
                Console.WriteLine($"[Options] Збережено у {filePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Options] Помилка збереження: {e.Message}");
            }
        }

        /// <summary>
        /// Примусово перезавантажує XML з файлу
        /// </summary>
        public static void Reload()
        {
            isInitialized = false;
            isDirty = false;
            Initialize();
        }

        /// <summary>
        /// Перевіряє, чи є незбережені зміни
        /// </summary>
        public static bool HasUnsavedChanges()
        {
            return isDirty;
        }

        /// <summary>
        /// Отримує повний шлях до файлу options.xml
        /// </summary>
        public static string GetFilePath()
        {
            if (!isInitialized) Initialize();
            return filePath;
        }

        /// <summary>
        /// Видаляє атрибут або ноду за ключем
        /// </summary>
        public static void Delete(string key)
        {
            XmlElement node = GetOrCreateNode(key, false);
            if (node == null) return;

            string attrName = GetAttributeName(key);
            if (node.HasAttribute(attrName))
            {
                node.RemoveAttribute(attrName);
                isDirty = true;
            }
        }

        /// <summary>
        /// Перевіряє, чи існує ключ
        /// </summary>
        public static bool HasKey(string key)
        {
            XmlElement node = GetOrCreateNode(key, false);
            if (node == null) return false;

            string attrName = GetAttributeName(key);
            return node.HasAttribute(attrName);
        }
    }
}
