// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.CodeGeneration;
using Microsoft.Extensions.CodeGeneration.EntityFramework;

namespace Microsoft.Extensions.CodeGenerators.Mvc.Controller
{
    public abstract class ControllerGeneratorBase
    {
        protected IApplicationEnvironment ApplicationEnvironment
        {
            get;
            private set;
        }
        protected ICodeGeneratorActionsService CodeGeneratorActionsService
        {
            get;
            private set;
        }
        protected ILibraryManager LibraryManager
        {
            get;
            private set;
        }
        protected IServiceProvider ServiceProvider
        {
            get;
            private set;
        }
        protected ILogger Logger
        {
            get;
            private set;
        }

        public ControllerGeneratorBase(
            [NotNull]ILibraryManager libraryManager,
            [NotNull]IApplicationEnvironment environment,
            [NotNull]ICodeGeneratorActionsService codeGeneratorActionsService,
            [NotNull]IServiceProvider serviceProvider,
            [NotNull]ILogger logger)
        {
            LibraryManager = libraryManager;
            ApplicationEnvironment = environment;
            CodeGeneratorActionsService = codeGeneratorActionsService;
            ServiceProvider = serviceProvider;
            Logger = logger;
        }

        protected virtual IEnumerable<string> TemplateFolders
        {
            get
            {
                return TemplateFoldersUtilities.GetTemplateFolders(
                    containingProject: Constants.ThisAssemblyName,
                    applicationBasePath: ApplicationEnvironment.ApplicationBasePath,
                    baseFolders: new[] { "ControllerGenerator", "ViewGenerator" },
                    libraryManager: LibraryManager);
            }
        }

        protected string GetControllerNamespace()
        {
            // Review: MVC scaffolding used ActiveProject's MSBuild RootNamespace property
            // That's not possible in command line scaffolding - the closest we can get is
            // the name of assembly??
            var appName = LibraryManager.GetLibrary(ApplicationEnvironment.ApplicationName).Name;
            return appName + "." + Constants.ControllersFolderName;
        }

        public abstract Task Generate(CommandLineGeneratorModel controllerGeneratorModel);

        protected string ValidateAndGetOutputPath(CommandLineGeneratorModel controllerGeneratorModel)
        {
            string outputFolder = String.IsNullOrEmpty(controllerGeneratorModel.RelativeFolderPath)
                ? ApplicationEnvironment.ApplicationBasePath
                : Path.Combine(ApplicationEnvironment.ApplicationBasePath, controllerGeneratorModel.RelativeFolderPath);

            var outputPath = Path.Combine(outputFolder, controllerGeneratorModel.ControllerName + Constants.CodeFileExtension);

            if (File.Exists(outputPath) && !controllerGeneratorModel.Force)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The file {0} exists, use -f option to overwrite",
                    outputPath));
            }

            return outputPath;
        }
    }
}