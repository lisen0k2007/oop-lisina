using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MyTasks
{
    public class TodoList
    {
        private List<TodoItem> _items = new List<TodoItem>();
        private const string FilePath = "tasks.json";

        public void Add(string title, DateTime? dueDate)
        {
            var item = new TodoItem(title, dueDate);
            _items.Add(item);
        }

        public void Remove(TodoItem item)
        {
            if (item != null) _items.Remove(item);
        }

        public List<TodoItem> GetAll()
        {
            return _items;
        }

        public List<TodoItem> GetFiltered(bool showCompleted, bool showActive, string searchText)
        {
            var query = _items.AsQueryable();

            if (showCompleted && !showActive)
                query = query.Where(x => x.IsDone);
            else if (!showCompleted && showActive)
                query = query.Where(x => !x.IsDone);

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(x => x.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            return query.OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).ToList();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_items, options);
            File.WriteAllText(FilePath, json);
        }

        public void Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    var loadedItems = JsonSerializer.Deserialize<List<TodoItem>>(json);
                    if (loadedItems != null)
                        _items = loadedItems;
                }
                catch
                {
                    _items = new List<TodoItem>();
                }
            }
        }
    }
}
