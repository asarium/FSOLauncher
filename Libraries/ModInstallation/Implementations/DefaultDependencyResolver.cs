#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations
{
    internal class DependencyNode
    {
        public DependencyNode([NotNull] IPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            Package = package;

            Dependencies = new List<DependencyNode>();
        }

        [NotNull]
        public IPackage Package { get; private set; }

        [NotNull]
        public IList<DependencyNode> Dependencies { get; private set; }

        protected bool Equals(DependencyNode other)
        {
            return Package.Equals(other.Package);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((DependencyNode) obj);
        }

        public override int GetHashCode()
        {
            return Package.GetHashCode();
        }

        public static bool operator ==(DependencyNode left, DependencyNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DependencyNode left, DependencyNode right)
        {
            return !Equals(left, right);
        }
    }

    internal class DependencyGraph
    {
        public DependencyGraph()
        {
            Nodes = new List<DependencyNode>();
        }

        [NotNull]
        public IList<DependencyNode> Nodes { get; private set; }

        [CanBeNull]
        public DependencyNode FindNode([NotNull] IPackage package)
        {
            return Nodes.FirstOrDefault(n => n.Package == package);
        }

        [NotNull]
        public IEnumerable<DependencyNode> IncomingEdges([NotNull] DependencyNode node)
        {
            return from n in Nodes
                from dep in n.Dependencies
                where Equals(dep, node)
                select dep;
        }
    }

    public class DefaultDependencyResolver : IDependencyResolver
    {
        #region IDependencyResolver Members

        public IEnumerable<IPackage> ResolveDependencies(IPackage package, IList<IModification> allModifications, IErrorHandler handler)
        {
            // This implements a variation of Kahns topological sorting algorithm
            var graph = BuildDependencyGraph(package, allModifications, handler);

            var result = new List<IPackage>();

            var openSet = new HashSet<DependencyNode>(graph.Nodes.Where(n => !graph.IncomingEdges(n).Any()));

            while (openSet.Count > 0)
            {
                var node = openSet.First();
                openSet.Remove(node);

                result.Add(node.Package);

                // Copy the dependencies list
                var nodeDependencies = node.Dependencies.ToList();
                node.Dependencies.Clear();

                foreach (var noIncoming in 
                    nodeDependencies.Where(n => !graph.IncomingEdges(n).Any()))
                {
                    openSet.Add(noIncoming);
                }
            }

            if (!graph.Nodes.Any(n => n.Dependencies.Count > 0))
            {
                return result;
            }

            throw new InvalidOperationException("Cyclic dependency detected!");
        }

        #endregion

        [NotNull]
        private static DependencyGraph BuildDependencyGraph([NotNull] IPackage rootPackage, [NotNull] IList<IModification> allModifications,
            [CanBeNull] IErrorHandler handler)
        {
            var graph = new DependencyGraph();

            AddPackageToGraph(graph, rootPackage, allModifications, handler);

            return graph;
        }

        [NotNull]
        private static DependencyNode AddPackageToGraph([NotNull] DependencyGraph graph, [NotNull] IPackage rootPackage,
            [NotNull] IList<IModification> allModifications, [CanBeNull] IErrorHandler handler)
        {
            var existingNode = graph.FindNode(rootPackage);
            if (existingNode != null)
            {
                return existingNode;
            }

            var rootNode = new DependencyNode(rootPackage);
            graph.Nodes.Add(rootNode);

            var packages = GetPackageDependencies(rootPackage, allModifications, handler);

            foreach (var dependencyNode in packages.Select(package => AddPackageToGraph(graph, package, allModifications, handler)))
            {
                rootNode.Dependencies.Add(dependencyNode);
            }

            return rootNode;
        }

        [NotNull]
        public static IEnumerable<IPackage> GetPackageDependencies([NotNull] IPackage package, [NotNull] IList<IModification> allModifications,
            [CanBeNull] IErrorHandler handler)
        {
            if (package.Dependencies == null)
            {
                return Enumerable.Empty<IPackage>();
            }

            var outList = new List<IPackage>();
            foreach (var modDependency in package.Dependencies)
            {
                var dependency = modDependency;

                var matchingMods = from mod in allModifications
                    where mod.Id == dependency.ModId && dependency.VersionMatches(mod.Version)
                    orderby mod.Version descending
                    select mod;

                if (!matchingMods.Any())
                {
                    if (handler != null)
                    {
                        handler.HandleError(package, "Failed to satisfy dependency '" + modDependency + "'!");
                    }

                    continue;
                }

                // The mods are order by version which means the first has the highest version available
                var matchedMod = matchingMods.First();

                IEnumerable<IPackage> packages;
                if (!modDependency.PackageNames.Any())
                {
                    // If no packages were specified, depend on the required packages
                    packages = matchedMod.Packages.Where(p => p.Status == PackageStatus.Required);
                }
                else
                {
                    var list = new List<IPackage>();
                    foreach (var packageName in dependency.PackageNames)
                    {
                        var p = matchedMod.Packages.FirstOrDefault(pack => pack.Name == packageName);

                        if (p != null)
                        {
                            list.Add(p);
                        }
                        else if (handler != null)
                        {
                            handler.HandleError(package, string.Format("Failed to find package '{0}' in mod '{1}'.", packageName, matchedMod.Id));
                        }
                    }

                    packages = list;
                }

                foreach (var packageDepend in packages.Where(packageDepend => !outList.Contains(packageDepend)))
                {
                    outList.Add(packageDepend);
                }
            }

            return outList;
        }
    }
}
