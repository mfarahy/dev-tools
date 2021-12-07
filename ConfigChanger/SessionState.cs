using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigChanger
{
  internal class SessionState
  {

    public SessionState(string? defaultPath)
    {
      this.Path = defaultPath;
      Pointers = new List<Pointer>();
    }
    public SessionState()
    {
      Pointers = new List<Pointer>();
    }
    public string Name { get; set; }
    public string Extension { get; set; }
    public bool Recursive { get; set; }
    public string Path { get; set; }
    public List<Pointer> Pointers { get; set; }

    public int FindIndex(string id)
    {
      if (!String.IsNullOrEmpty(id) && int.TryParse(id, out int key))
      {
        int index = -1;
        for (var i = 0; i < Pointers.Count; i++)
        {
          if (Pointers[i].ID == key)
          {
            index = i; break;
          }
        }
        if (index >= 0)
        {
          return index;
        }
      }
      return -1;
    }
  }
}
