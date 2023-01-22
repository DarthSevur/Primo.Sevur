using LTools.SDK;
using LTools.Common.Helpers;
using LTools.Common.Model;
using LTools.Common.Model.Studio;
using LTools.Common.Model.Serialization;
using LTools.Common.UIElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Primo.Sevur.Network
{
    #region [FileDownload]
    public class FileDownload : PrimoComponentSimple<FileDownload_Form>
    {
        #region [CONSTANTS & VARIABLES]
        private const string PROPERTY_CATEGORY_FILE = "Файл",
                             PROPERTY_URL = "URL для скачивания",
                             PROPERTY_URL_TOOLTIP = "[String]* URL для скачивания",
                             PROPERTY_FULLPATH = "Путь к файлу",
                             PROPERTY_FULLPATH_TOOLTIP = "[String]* Путь к файлу";
        private string fileNameFormula_String,
                       urlFormula_String;
        #endregion

        #region [GROUP NAME]
        public override string GroupName 
        { 
            get => "Sevur" + WFPublishedElementBase.TREE_SEPARATOR + "Network";
            protected set { }
        }
        #endregion

        #region [PROPERTIES]
        //значение свойства будет сохранено в файле процесса
        [StoringProperty]
        //не обязателен и служит для проверки синтаксиса скрипта свойства. Если данный атрибут отсутствует, Студия не будет проверять тип данных свойства
        [ValidateReturnScript(DataType = typeof(string))]
        [Category(PROPERTY_CATEGORY_FILE), DisplayName(PROPERTY_FULLPATH)]
        // здесь тип свойства всегда string для PropertyTypes.SCRIPT и PropertyTypes.VARIABLE, bool для CheckBox и enum для выпадающего списка
        public string fileName_String_IN
        {
            get => fileNameFormula_String;
            set { fileNameFormula_String = value; InvokePropertyChanged(this, nameof(fileName_String_IN)); }
        }

        //значение свойства будет сохранено в файле процесса
        [StoringProperty]
        //не обязателен и служит для проверки синтаксиса скрипта свойства. Если данный атрибут отсутствует, Студия не будет проверять тип данных свойства
        [ValidateReturnScript(DataType = typeof(string))]
        [Category(PROPERTY_CATEGORY_FILE), DisplayName(PROPERTY_URL)]
        // здесь тип свойства всегда string для PropertyTypes.SCRIPT и PropertyTypes.VARIABLE, bool для CheckBox и enum для выпадающего списка
        public string url_String_IN
        {
            get => urlFormula_String;
            set { urlFormula_String = value; InvokePropertyChanged(this, nameof(url_String_IN)); }
        }

        public FileDownload(IWFContainer container) : base(container)
        {
            sdkComponentIcon = "pack://application:,,/Primo.Sevur;component/Network/FileDownload.png";

            sdkComponentName = "Скачать файл по ссылке";
            sdkComponentHelp = 
                "Компонент скачивает файл по ссылке \n" +
                "Свойства\n" +
                " - " + PROPERTY_URL + "*: " + PROPERTY_URL_TOOLTIP + "\n"+
                " - " + PROPERTY_FULLPATH + "*: " + PROPERTY_FULLPATH_TOOLTIP;

            //Добавление производится в конструкторе до вызова метода InitClass
            sdkProperties = new List<WFHelper.PropertiesItem>()
            {
                new WFHelper.PropertiesItem()
                {
                    PropName = nameof(fileName_String_IN),
                    PropertyType = WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.FILE_SELECTOR,
                    DataType = typeof(string),
                    ToolTip = PROPERTY_FULLPATH_TOOLTIP,
                    IsReadOnly = false
                },

                new WFHelper.PropertiesItem()
                {
                    PropName = nameof(url_String_IN),
                    PropertyType = WFHelper.PropertiesItem.PropertyTypes.SCRIPT,
                    EditorType = ScriptEditorTypes.NONE,
                    DataType = typeof(string),
                    ToolTip = PROPERTY_URL_TOOLTIP,
                    IsReadOnly = false
                }
            };

            InitClass(container);

            // Значения по умолчанию задаются после вызова метода инициализации InitClass()
            //fileName_String_IN = "@\"C:\\Out\\file.txt\"";
         }
        #endregion

        #region [EXECUTION]
        public override ExecutionResult SimpleAction(ScriptingData sd)
        {
            bool success_Bool = false;
            string executionResultMessage_String,
                    folder_String;
            try
            {
                string fileName_String = Path.Combine(sd.WorkflowVar.ProjectPath, GetPropertyValue<string>(fileName_String_IN, nameof(fileName_String_IN), sd)),
                        url_String = GetPropertyValue<string>(url_String_IN, nameof(url_String_IN), sd);
                if (string.IsNullOrEmpty(fileName_String) || fileName_String.Trim().Length == 0)
                    executionResultMessage_String = "Имя файла не задано";
                else if (!Directory.Exists(folder_String = Path.GetDirectoryName(fileName_String)) )
                    executionResultMessage_String = "Папка '" + folder_String + "' не существует";
                else if (string.IsNullOrEmpty(url_String) || url_String.Trim().Length == 0)
                    executionResultMessage_String = "URL не задан";
                else
                {
                    System.Net.WebClient download_WebClient = new System.Net.WebClient();
                    download_WebClient.DownloadFile(url_String, fileName_String);

                    executionResultMessage_String = "Файл '" + fileName_String + "' закачивается.";
                    success_Bool = true;
                }
            }
            catch (Exception ex)
            {
                executionResultMessage_String = ex?.Message;
            }

            return success_Bool ? new ExecutionResult() { IsSuccess = true, SuccessMessage = executionResultMessage_String } : new ExecutionResult() { IsSuccess = false, ErrorMessage = executionResultMessage_String };
        }
        #endregion

        #region [VALIDATE]
        public override ValidationResult Validate()
        {
            string propertyName_String;
            ValidationResult ret = new ValidationResult();

            if (string.IsNullOrEmpty(fileNameFormula_String) || Regex.IsMatch(fileNameFormula_String, @"\s*\x22\s*\x22\s*"))
                propertyName_String = PROPERTY_FULLPATH;
            else if (string.IsNullOrEmpty(urlFormula_String) || Regex.IsMatch(urlFormula_String, @"\s*\x22\s*\x22\s*"))
                propertyName_String = PROPERTY_URL;
            else
                return ret;

            ret.Items.Add(new ValidationResult.ValidationItem() { PropertyName = propertyName_String, Error = "не может быть не задан" });

            return ret;
        }
        #endregion
    }
    #endregion
}
