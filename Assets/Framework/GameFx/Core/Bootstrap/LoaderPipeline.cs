using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace GameFx.Core.Bootstrap
{
    public class LoaderPipeline
    {
        private readonly HashSet<ILoader> _seenLoaders = new();
        private readonly Dictionary<ILoader, List<ILoader>> _reverseDependencies = new();
        private readonly Dictionary<ILoader, int> _inDegree = new();


        public List<ILoader> ExecutionOrder { get; } = new();
        public int SeenLoaderCount => _seenLoaders.Count;


        // Build the full dependency graph and detect cycles
        public async UniTask BuildAsync(IEnumerable<ILoader> roots)
        {
            _seenLoaders.Clear();
            _reverseDependencies.Clear();
            _inDegree.Clear();
            ExecutionOrder.Clear();


            foreach (var loader in roots)
            {
                var visiting = new HashSet<ILoader>();
                var path = new List<ILoader>();
                await Visit(loader, visiting, path);
            }


            foreach (var loader in _seenLoaders)
            {
                _inDegree[loader] = loader.Dependencies.Count;
                foreach (var dep in loader.Dependencies)
                {
                    if (!_reverseDependencies.ContainsKey(dep))
                        _reverseDependencies[dep] = new List<ILoader>();
                    _reverseDependencies[dep].Add(loader);
                }
            }
        }


        // Detect loops and collect all nodes
        private async UniTask Visit(ILoader loader, HashSet<ILoader> visiting, List<ILoader> path)
        {
            if (_seenLoaders.Contains(loader)) return;


            if (visiting.Contains(loader))
            {
                path.Add(loader);
                string cyclePath = string.Join(" -> ", path.Select(l => l.GetType().Name));
                throw new Exception($"[LoaderPipeline] Cycle detected: {cyclePath}");
            }


            visiting.Add(loader);
            path.Add(loader);


            foreach (var dep in loader.Dependencies)
            {
                if (dep != null)
                    await Visit(dep, visiting, path);
            }


            visiting.Remove(loader);
            path.RemoveAt(path.Count - 1);


            _seenLoaders.Add(loader);
        }


        // Execute in topological order with parallel batches
        public async UniTask ExecuteAsync()
        {
            var ready = new Queue<ILoader>(_inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var running = new List<UniTask>();


            while (ready.Count > 0 || running.Count > 0)
            {
                while (ready.Count > 0)
                {
                    var loader = ready.Dequeue();
                    ExecutionOrder.Add(loader);
                    running.Add(RunLoaderAsync(loader));
                }


                await UniTask.WhenAll(running);
                running.Clear();
            }
        }

        private async UniTask RunLoaderAsync(ILoader loader)
        {
            await LoadTheGivenLoader(loader);

            if (_reverseDependencies.TryGetValue(loader, out var children))
            {
                foreach (var child in children)
                {
                    _inDegree[child]--;
                }
            }
        }


        private async UniTask LoadTheGivenLoader(ILoader loader)
        {
            await loader.Load();
        }


        // Export dependency graph in DOT format
        public string GenerateDotGraph()
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph LoaderGraph {");


            foreach (var loader in _seenLoaders)
            {
                string parent = loader.GetType().Name;
                foreach (var dep in loader.Dependencies)
                {
                    if (dep != null)
                    {
                        string child = dep.GetType().Name;
                        sb.AppendLine($" \"{parent}\" -> \"{child}\";");
                    }
                }
            }


            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}