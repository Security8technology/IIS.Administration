﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.IIS.Administration.Files
{
    using Core;
    using Core.Utils;
    using System;
    using System.Dynamic;
    using System.IO;
    using System.Linq;

    public class FilesHelper
    {
        private static readonly Fields RefFields = new Fields("name", "id", "type", "physical_path");

        private IFileProvider _fileProvider;

        public FilesHelper(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public object ToJsonModel(string physicalPath, Fields fields = null, bool full = true)
        {
            FileType fileType;

            try {
                fileType = GetFileType(physicalPath);
            }
            catch (FileNotFoundException) {
                return InfoToJsonModel(_fileProvider.GetFile(physicalPath), fields, full);
            }

            switch (fileType) {

                //
                // Must construct fileinfo rather than use _fileProvider to get reference models for children of allowed directories
                case FileType.File:
                    return FileToJsonModel(_fileProvider.GetFile(physicalPath), fields, full);

                case FileType.Directory:
                    return DirectoryToJsonModel(_fileProvider.GetDirectory(physicalPath), fields, full);

                default:
                    return null;
            }
        }

        public object ToJsonModelRef(string physicalPath, Fields fields = null)
        {
            if (fields == null || !fields.HasFields) {
                return ToJsonModel(physicalPath, RefFields, false);
            }
            else {
                return ToJsonModel(physicalPath, fields, false);
            }
        }

        public static string GetLocation(string id)
        {
            return $"/{Defines.FILES_PATH}/{id}";
        }

        internal object DirectoryToJsonModel(IDirectoryInfo info, Fields fields = null, bool full = true)
        {
            if (fields == null) {
                fields = Fields.All;
            }

            dynamic obj = new ExpandoObject();
            var fileId = FileId.FromPhysicalPath(info.Path);
            bool? exists = null;

            //
            // name
            if (fields.Exists("name")) {
                obj.name = info.Name;
            }

            //
            // id
            if (fields.Exists("id")) {
                obj.id = fileId.Uuid;
            }

            //
            // type
            if (fields.Exists("type")) {
                obj.type = Enum.GetName(typeof(FileType), FileType.Directory).ToLower();
            }

            //
            // physical_path
            if (fields.Exists("physical_path")) {
                obj.physical_path = info.Path;
            }

            //
            // exists
            if (fields.Exists("exists")) {
                exists = exists ?? info.Exists;
                obj.exists = exists.Value;
            }

            //
            // created
            if (fields.Exists("created")) {
                exists = exists ?? info.Exists;
                obj.created = exists.Value ? (object)info.Creation.ToUniversalTime() : null;
            }

            //
            // last_modified
            if (fields.Exists("last_modified")) {
                exists = exists ?? info.Exists;
                obj.last_modified = exists.Value ? (object)info.LastModified.ToUniversalTime() : null;
            }

            //
            // last_access
            if (fields.Exists("last_access")) {
                exists = exists ?? info.Exists;
                obj.last_access = exists.Value ? (object)info.LastAccess.ToUniversalTime() : null;
            }

            //
            // total_files
            // We check for the 'full' flag to avoid unauthorized exception when referencing directories
            // Listing a directories content requires extra permissions
            if (fields.Exists("total_files") && full) {
                exists = exists ?? info.Exists;
                if (_fileProvider.IsAccessAllowed(info.Path, FileAccess.Read)) {
                    obj.total_files = exists.Value ? _fileProvider.GetFiles(info.Path, "*").Count() + _fileProvider.GetDirectories(info.Path, "*").Count() : 0;
                }
            }

            //
            // parent
            if (fields.Exists("parent")) {
                obj.parent = GetParentJsonModelRef(info.Path, fields.Filter("parent"));
            }

            //
            // claims
            if (fields.Exists("claims")) {
                obj.claims = _fileProvider.GetClaims(info.Path);
            }

            return Core.Environment.Hal.Apply(Defines.DirectoriesResource.Guid, obj, full);
        }

        public object DirectoryToJsonModelRef(IDirectoryInfo info, Fields fields = null)
        {
            if (fields == null || !fields.HasFields) {
                return DirectoryToJsonModel(info, RefFields, false);
            }
            else {
                return DirectoryToJsonModel(info, fields, false);
            }
        }

        internal object FileToJsonModel(IFileInfo info, Fields fields = null, bool full = true)
        {
            if (fields == null) {
                fields = Fields.All;
            }

            dynamic obj = new ExpandoObject();
            var fileId = FileId.FromPhysicalPath(info.Path);
            bool? exists = null;
            
            //
            // name
            if (fields.Exists("name")) {
                obj.name = info.Name;
            }

            //
            // id
            if (fields.Exists("id")) {
                obj.id = fileId.Uuid;
            }

            //
            // type
            if (fields.Exists("type")) {
                obj.type = Enum.GetName(typeof(FileType), FileType.File).ToLower();
            }

            //
            // physical_path
            if (fields.Exists("physical_path")) {
                obj.physical_path = info.Path;
            }

            //
            // exists
            if (fields.Exists("exists")) {
                exists = exists ?? info.Exists;
                obj.exists = exists.Value;
            }

            //
            // size
            if (fields.Exists("size")) {
                exists = exists ?? info.Exists;
                obj.size = exists.Value ? info.Size : 0;
            }

            //
            // created
            if (fields.Exists("created")) {
                exists = exists ?? info.Exists;
                obj.created = exists.Value ? (object)info.Creation.ToUniversalTime() : null;
            }

            //
            // last_modified
            if (fields.Exists("last_modified")) {
                exists = exists ?? info.Exists;
                obj.last_modified = exists.Value ? (object)info.LastModified.ToUniversalTime() : null;
            }

            //
            // last_access
            if (fields.Exists("last_access")) {
                exists = exists ?? info.Exists;
                obj.last_access = exists.Value ? (object)info.LastAccess.ToUniversalTime() : null;
            }

            //
            // e_tag
            if (fields.Exists("e_tag")) {
                exists = exists ?? info.Exists;
                obj.e_tag = exists.Value ? ETag.Create(info).Value : null;
            }

            //
            // parent
            if (fields.Exists("parent")) {
                obj.parent = GetParentJsonModelRef(info.Path, fields.Filter("parent"));
            }

            //
            // claims
            if (fields.Exists("claims")) {
                obj.claims = _fileProvider.GetClaims(info.Path);
            }


            return Core.Environment.Hal.Apply(Defines.FilesResource.Guid, obj, full);
        }

        public object FileToJsonModelRef(IFileInfo info, Fields fields = null)
        {
            if (fields == null || !fields.HasFields) {
                return FileToJsonModel(info, RefFields, false);
            }
            else {
                return FileToJsonModel(info, fields, false);
            }
        }

        internal object InfoToJsonModel(IFileSystemInfo info, Fields fields = null, bool full = true)
        {
            if (fields == null) {
                fields = Fields.All;
            }

            dynamic obj = new ExpandoObject();
            var fileId = FileId.FromPhysicalPath(info.Path);
            bool? exists = null;

            //
            // name
            if (fields.Exists("name")) {
                obj.name = info.Name;
            }

            //
            // id
            if (fields.Exists("id")) {
                obj.id = fileId.Uuid;
            }

            //
            // type
            if (fields.Exists("type")) {
                obj.type = null;
            }

            //
            // physical_path
            if (fields.Exists("physical_path")) {
                obj.physical_path = info.Path;
            }

            //
            // exists
            if (fields.Exists("exists")) {
                exists = exists ?? info.Exists;
                obj.exists = exists.Value;
            }

            //
            // created
            if (fields.Exists("created")) {
                exists = exists ?? info.Exists;
                obj.created = exists.Value ? (object)info.Creation.ToUniversalTime() : null;
            }

            //
            // last_modified
            if (fields.Exists("last_modified")) {
                exists = exists ?? info.Exists;
                obj.last_modified = exists.Value ? (object)info.LastModified.ToUniversalTime() : null;
            }

            //
            // last_access
            if (fields.Exists("last_access")) {
                exists = exists ?? info.Exists;
                obj.last_access = exists.Value ? (object)info.LastAccess.ToUniversalTime() : null;
            }

            //
            // parent
            if (fields.Exists("parent")) {
                obj.parent = GetParentJsonModelRef(info.Path, fields.Filter("parent"));
            }

            //
            // claims
            if (fields.Exists("claims")) {
                obj.claims = _fileProvider.GetClaims(info.Path);
            }


            return Core.Environment.Hal.Apply(Defines.FilesResource.Guid, obj, full);
        }

        public object InfoToJsonModelRef(IFileSystemInfo info, Fields fields = null)
        {
            if (fields == null || !fields.HasFields) {
                return InfoToJsonModel(info, RefFields, false);
            }
            else {
                return InfoToJsonModel(info, fields, false);
            }
        }

        internal static string GetPhysicalPath(string root, string path)
        {
            if (string.IsNullOrEmpty(root)) {
                throw new ArgumentNullException(nameof(root));
            }

            return PathUtil.GetFullPath(Path.Combine(root, path.TrimStart(PathUtil.SEPARATORS)));
        }

        internal string UpdateFile(dynamic model, string physicalPath)
        {
            DateTime? created = null;
            DateTime? lastAccess = null;
            DateTime? lastModified = null;

            var file = _fileProvider.GetFile(physicalPath);

            if (model == null) {
                throw new ApiArgumentException("model");
            }

            created = DynamicHelper.To<DateTime>(model.created);
            lastAccess = DynamicHelper.To<DateTime>(model.last_access);
            lastModified = DynamicHelper.To<DateTime>(model.last_modified);

            if (model.name != null) {

                string name = DynamicHelper.Value(model.name);

                if (!PathUtil.IsValidFileName(name)) {
                    throw new ApiArgumentException("name");
                }

                var newPath = Path.Combine(file.Parent.Path, name);

                if (!newPath.Equals(physicalPath, StringComparison.OrdinalIgnoreCase)) {

                    if (_fileProvider.FileExists(newPath) || _fileProvider.DirectoryExists(newPath)) {
                        throw new AlreadyExistsException("name");
                    }

                    _fileProvider.Move(physicalPath, newPath);

                    physicalPath = newPath;
                }
            }

            _fileProvider.SetFileTime(physicalPath, lastAccess, lastModified, created);

            return physicalPath;
        }

        internal string UpdateDirectory(dynamic model, string directoryPath)
        {
            DateTime? created = null;
            DateTime? lastAccess = null;
            DateTime? lastModified = null;

            var directory = _fileProvider.GetDirectory(directoryPath);

            if (model == null) {
                throw new ApiArgumentException("model");
            }

            created = DynamicHelper.To<DateTime>(model.created);
            lastAccess = DynamicHelper.To<DateTime>(model.last_access);
            lastModified = DynamicHelper.To<DateTime>(model.last_modified);

            if (model.name != null) {
                string name = DynamicHelper.Value(model.name);

                if (!PathUtil.IsValidFileName(name)) {
                    throw new ApiArgumentException("name");
                }

                var newPath = Path.Combine(directory.Parent.Path, name);

                if (!newPath.Equals(directoryPath, StringComparison.OrdinalIgnoreCase)) {

                    if (_fileProvider.FileExists(newPath) || _fileProvider.DirectoryExists(newPath)) {
                        throw new AlreadyExistsException("name");
                    }

                    _fileProvider.Move(directoryPath, newPath);

                    directoryPath = newPath;
                }
            }

            _fileProvider.SetFileTime(directoryPath, lastAccess, lastModified, created);

            return directoryPath;
        }

        internal static FileType GetFileType(string physicalPath)
        {
            return File.GetAttributes(physicalPath).HasFlag(FileAttributes.Directory) ? FileType.Directory : FileType.File;
        }

        private object GetParentJsonModelRef(string physicalPath, Fields fields = null)
        {
            object ret = null;

            var parentPath = PathUtil.GetParentPath(physicalPath);

            if (parentPath != null && _fileProvider.IsAccessAllowed(parentPath, FileAccess.Read)) {
                ret = DirectoryToJsonModelRef(_fileProvider.GetDirectory(parentPath), fields);
            }

            return ret;
        }
    }
}
