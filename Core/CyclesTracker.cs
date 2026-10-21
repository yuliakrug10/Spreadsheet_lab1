using System.Collections.Generic;

namespace Spreadsheet_lab1.Core
{
    public class CyclesTracker
    {
        private readonly Dictionary<string, List<string>> _dependencies = new Dictionary<string, List<string>>();

        public void AddDependency(string cellAddress, string depends)
        {
            if (!_dependencies.ContainsKey(depends))
            {
                _dependencies[depends] = new List<string>();
            }
            if (!_dependencies[depends].Contains(cellAddress))
            {
                _dependencies[depends].Add(cellAddress);
            }
        }

        public void ClearDependencies(string cellAddress)
        {
            foreach (var key in _dependencies.Keys)
            {
                _dependencies[key].Remove(cellAddress);
            }
        }

        public bool CheckCycles(string startCell)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();

            return Dfs(startCell, visited, stack);
        }

        public List<string> GetDependentCells(string cellAddress)
        {
            if (_dependencies.ContainsKey(cellAddress))
            {
                return new List<string>(_dependencies[cellAddress]);
            }
            else
            {
                return new List<string>();
            }
        }

        private bool Dfs(string currentCell, HashSet<string> visited, HashSet<string> stack)
        {
            visited.Add(currentCell);
            stack.Add(currentCell);

            if (_dependencies.ContainsKey(currentCell))
            {
                foreach (var neighbor in _dependencies[currentCell])
                {
                    if (!visited.Contains(neighbor) && Dfs(neighbor, visited, stack))
                    {
                        return true;
                    }
                    else if (stack.Contains(neighbor))
                    {
                        return true;
                    }
                }
            }

            stack.Remove(currentCell);
            return false;
        }
    }
}