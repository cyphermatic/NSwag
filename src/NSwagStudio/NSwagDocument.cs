﻿//-----------------------------------------------------------------------
// <copyright file="NSwagSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.IO;
using MyToolkit.Model;
using Newtonsoft.Json;
using NSwag.Commands;
using NSwagStudio.Utilities;

namespace NSwagStudio
{
    public class NSwagDocument : ObservableObject
    {
        private string _path;

        private int _selectedSwaggerGenerator;
        private int _selectedClientGenerator;

        private string _latestData;

        public NSwagDocument()
        {
            WebApiToSwaggerCommand = new WebApiToSwaggerCommand();
            AssemblyTypeToSwaggerCommand = new AssemblyTypeToSwaggerCommand();

            SwaggerToTypeScriptCommand = new SwaggerToTypeScriptCommand();
            SwaggerToCSharpClientCommand = new SwaggerToCSharpClientCommand();
            SwaggerToCSharpWebApiControllerCommand = new SwaggerToCSharpWebApiControllerCommand();
        }

        public static NSwagDocument LoadDocument(string filePath)
        {
            var data = File.ReadAllText(filePath);

            var document = JsonConvert.DeserializeObject<NSwagDocument>(data);
            document.Path = filePath;
            document.ConvertToAbsolutePaths();

            document._latestData = JsonConvert.SerializeObject(document, Formatting.Indented);

            // Legacy file support
            if (document.SwaggerToCSharpClientCommand.DateTimeType == "0")
                document.SwaggerToCSharpClientCommand.DateTimeType = "DateTime";
            document.WebApiToSwaggerCommand.ControllerName = "";

            return document;
        }

        public void Save()
        {
            ConvertToRelativePaths();

            var previousAssemblyTypeAssemblyPath = AssemblyTypeToSwaggerCommand.AssemblyPath;
            if (!string.IsNullOrEmpty(previousAssemblyTypeAssemblyPath))
                AssemblyTypeToSwaggerCommand.AssemblyPath = PathUtilities.MakeRelativePath(AssemblyTypeToSwaggerCommand.AssemblyPath, System.IO.Path.GetDirectoryName(Path));

            _latestData = JsonConvert.SerializeObject(this, Formatting.Indented);

            ConvertToAbsolutePaths();

            File.WriteAllText(Path, _latestData);
        }

        private void ConvertToAbsolutePaths()
        {
            WebApiToSwaggerCommand.AssemblyPath = ConvertToAbsolute(WebApiToSwaggerCommand.AssemblyPath);
            AssemblyTypeToSwaggerCommand.AssemblyPath = ConvertToAbsolute(AssemblyTypeToSwaggerCommand.AssemblyPath);
            WebApiToSwaggerCommand.AssemblyConfig = ConvertToAbsolute(WebApiToSwaggerCommand.AssemblyConfig);
            AssemblyTypeToSwaggerCommand.AssemblyConfig = ConvertToAbsolute(AssemblyTypeToSwaggerCommand.AssemblyConfig);
        }

        private string ConvertToAbsolute(string path)
        {
            if (!string.IsNullOrEmpty(path) && !System.IO.Path.IsPathRooted(path))
                return PathUtilities.MakeAbsolutePath(System.IO.Path.GetDirectoryName(Path), path);
            return path;
        }

        private void ConvertToRelativePaths()
        {
            WebApiToSwaggerCommand.AssemblyPath = ConvertToRelativePath(WebApiToSwaggerCommand.AssemblyPath);
            AssemblyTypeToSwaggerCommand.AssemblyPath = ConvertToRelativePath(AssemblyTypeToSwaggerCommand.AssemblyPath);
            WebApiToSwaggerCommand.AssemblyConfig = ConvertToRelativePath(WebApiToSwaggerCommand.AssemblyConfig);
            AssemblyTypeToSwaggerCommand.AssemblyConfig = ConvertToRelativePath(AssemblyTypeToSwaggerCommand.AssemblyConfig);
        }

        private string ConvertToRelativePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                return PathUtilities.MakeRelativePath(path, System.IO.Path.GetDirectoryName(Path));
            return path; 
        }

        public static NSwagDocument CreateDocument()
        {
            var document = new NSwagDocument();
            document.Path = "Untitled";
            document._latestData = JsonConvert.SerializeObject(document, Formatting.Indented);
            return document;
        }

        [JsonIgnore]
        public string Path
        {
            get { return _path; }
            set
            {
                if (Set(ref _path, value))
                    RaisePropertyChanged(() => Name);
            }
        }

        [JsonIgnore]
        public string Name => System.IO.Path.GetFileName(Path);

        [JsonIgnore]
        public bool IsDirty => _latestData != JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>Gets or sets the selected Swagger generator. </summary>
        [JsonProperty("SelectedSwaggerGenerator")]
        public int SelectedSwaggerGenerator
        {
            get { return _selectedSwaggerGenerator; }
            set { Set(ref _selectedSwaggerGenerator, value); }
        }

        /// <summary>Gets or sets the selected client generator. </summary>
        [JsonIgnore]
        public int SelectedClientGenerator
        {
            get { return _selectedClientGenerator; }
            set { Set(ref _selectedClientGenerator, value); }
        }

        [JsonProperty("InputSwagger")]
        public string InputSwagger { get; set; }

        [JsonProperty("InputSwaggerUrl")]
        public string InputSwaggerUrl { get; set; }
        
        [JsonProperty("InputJsonSchema")]
        public string InputJsonSchema { get; set; }

        [JsonProperty("WebApiToSwaggerCommand")]
        public WebApiToSwaggerCommand WebApiToSwaggerCommand { get; set; }

        [JsonProperty("AssemblyTypeToSwaggerCommand")]
        public AssemblyTypeToSwaggerCommand AssemblyTypeToSwaggerCommand { get; set; }

        [JsonProperty("SwaggerToTypeScriptCommand")]
        public SwaggerToTypeScriptCommand SwaggerToTypeScriptCommand { get; set; }

        [JsonProperty("SwaggerToCSharpClientCommand")]
        public SwaggerToCSharpClientCommand SwaggerToCSharpClientCommand { get; set; }

        [JsonProperty("SwaggerToCSharpWebApiControllerCommand")]
        public SwaggerToCSharpWebApiControllerCommand SwaggerToCSharpWebApiControllerCommand { get; set; }
    }
}
