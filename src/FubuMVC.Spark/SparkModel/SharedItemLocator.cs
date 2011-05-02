﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using Spark;

namespace FubuMVC.Spark.SparkModel
{
    public interface ISharedItemLocator
    {
        SparkItem LocateItem(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items);
    }
	
    public class SharedItemLocator : ISharedItemLocator
    {
        private readonly ISharedDirectoryProvider _sharedDirectoryProvider;

        public SharedItemLocator() : this(new SharedDirectoryProvider())
        {
        }
        public SharedItemLocator(ISharedDirectoryProvider sharedDirectoryProvider)
        {
            _sharedDirectoryProvider = sharedDirectoryProvider;
        }

        public SparkItem LocateItem(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items)
        {
            var reachables = _sharedDirectoryProvider.GetDirectories(fromItem, items);
            return items.ByName(sparkName).InDirectories(reachables).FirstOrDefault();
        }
    }

    public interface ISharedDirectoryProvider
    {
        IEnumerable<string> GetDirectories(SparkItem item, IEnumerable<SparkItem> items);
    }

    public class SharedDirectoryProvider : ISharedDirectoryProvider
    {
        private readonly ISharedPathBuilder _sharedPathBuilder;

        public SharedDirectoryProvider()
            : this(new SharedPathBuilder())
        {
        }

        public SharedDirectoryProvider(ISharedPathBuilder sharedPathBuilder)
        {
            _sharedPathBuilder = sharedPathBuilder;
        }

        public IEnumerable<string> GetDirectories(SparkItem item, IEnumerable<SparkItem> items)
        {
            foreach (var directory in _sharedPathBuilder.BuildFrom(item.FilePath, item.RootPath))
            {
                yield return directory;
            }
			
            if (item.Origin == FubuSparkConstants.HostOrigin)
            {
                yield break;
            }
			
            var hostRoot = findHostRoot(items);
            if (hostRoot.IsEmpty())
            {
                yield break;
            }

            foreach (var sharedFolder in _sharedPathBuilder.SharedFolderNames)
            {
                yield return Path.Combine(hostRoot, sharedFolder);
            }
        }
		
		private static string findHostRoot(IEnumerable<SparkItem> items)
		{
			return items.ByOrigin(FubuSparkConstants.HostOrigin).FirstValue(x => x.RootPath);
		}
    }

    public interface ISharedPathBuilder
    {
        IEnumerable<string> BuildFrom(string path, string root);
        IEnumerable<string> SharedFolderNames { get; }
    }

    public class SharedPathBuilder : ISharedPathBuilder
    {
        private readonly IEnumerable<string> _sharedFolderNames;

        public SharedPathBuilder() : this(new[] { Constants.Shared })
        {
        }

        public SharedPathBuilder(IEnumerable<string> sharedFolderNames)
        {
            _sharedFolderNames = sharedFolderNames;
        }

        public IEnumerable<string> BuildFrom(string path, string root)
        {
            if (path == root) yield break;

            do
            {
                path = Path.GetDirectoryName(path);
                if (path == null) break;
                foreach (var sharedFolder in _sharedFolderNames)
                {
                    yield return Path.Combine(path, sharedFolder);
                }

            } while (path.IsNotEmpty() && path.PathRelativeTo(root).IsNotEmpty());
        }

        public IEnumerable<string> SharedFolderNames
        {
            get { return _sharedFolderNames; }
        }
    }
}