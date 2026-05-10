using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenealogyTree
{
	
    public enum Gender { Male, Female, Unknown }


    public class Person
    {

        public int Id { get; set; }


        public string LastName { get; set; } = string.Empty;


        public string FirstName { get; set; } = string.Empty;


        public string Patronymic { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }


        public DateTime? DeathDate { get; set; }


        public string? BirthPlace { get; set; }


        public string? Citizenship { get; set; }

        public string? Notes { get; set; }

  
        public Gender Gender { get; set; } = Gender.Unknown;

      
        public int? FatherId { get; set; }

      
        public int? MotherId { get; set; }

      
        [JsonIgnore]
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

     
        [JsonIgnore]
        public string DisplayInfo
        {
            get
            {
                var year = BirthDate.HasValue ? BirthDate.Value.Year.ToString() : "?";
                return $"{FullName} ({year})";
            }
        }

        public override string ToString() => DisplayInfo;
    }


    public class FamilyRepository
    {
        private List<Person> _persons = new();
        private int _nextId = 1;


        public Person Add(Person person)
        {
            person.Id = _nextId++;
            _persons.Add(person);
            return person;
        }


        public void Update(Person updated)
        {
            var idx = _persons.FindIndex(p => p.Id == updated.Id);
            if (idx >= 0) _persons[idx] = updated;
        }

  
        public void Delete(int id)
        {
            _persons.RemoveAll(p => p.Id == id);
            foreach (var p in _persons)
            {
                if (p.FatherId == id) p.FatherId = null;
                if (p.MotherId == id) p.MotherId = null;
            }
        }


        public IReadOnlyList<Person> GetAll() => _persons.AsReadOnly();

  
        public Person? GetById(int id) => _persons.FirstOrDefault(p => p.Id == id);



    
        public IReadOnlyList<Person> GetChildren(int parentId)
            => _persons.Where(p => p.FatherId == parentId || p.MotherId == parentId).ToList();

       
        public IReadOnlyList<Person> GetDescendants(int rootId)
        {
            var result = new List<Person>();
            var queue = new Queue<int>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in GetChildren(current))
                {
                    if (result.All(r => r.Id != child.Id))
                    {
                        result.Add(child);
                        queue.Enqueue(child.Id);
                    }
                }
            }
            return result;
        }

  
        public IReadOnlyList<Person> GetAncestors(int rootId)
        {
            var result = new List<Person>();
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var person = GetById(current);
                if (person == null || visited.Contains(current)) continue;
                visited.Add(current);

                if (person.FatherId.HasValue)
                {
                    var father = GetById(person.FatherId.Value);
                    if (father != null && result.All(r => r.Id != father.Id))
                    {
                        result.Add(father);
                        queue.Enqueue(father.Id);
                    }
                }
                if (person.MotherId.HasValue)
                {
                    var mother = GetById(person.MotherId.Value);
                    if (mother != null && result.All(r => r.Id != mother.Id))
                    {
                        result.Add(mother);
                        queue.Enqueue(mother.Id);
                    }
                }
            }
            return result;
        }



        private record SerializationData(List<Person> Persons, int NextId);

     
        public void SaveToFile(string path)
        {
            var data = new SerializationData(_persons, _nextId);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        public void LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SerializationData>(json)
                       ?? throw new InvalidDataException("Файл пошкоджено або має невірний формат.");
            _persons = data.Persons;
            _nextId = data.NextId;
        }
    }
}
