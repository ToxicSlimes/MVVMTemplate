using Core.Abstractions.Services;
using UnityEngine;

namespace Services
{
  public class PrefsService : IPrefsService
  {
    public void Save<T>(string key, T data)
    {
      if (string.IsNullOrEmpty(key))
        return;
            
      if (data == null)
      {
        if (!PlayerPrefs.HasKey(key))
          return;
                
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
                
        return;
      }
            
      var json = JsonUtility.ToJson(data);
      PlayerPrefs.SetString(key, json);
      PlayerPrefs.Save();
    }

    public T Load<T>(string key)
    {
      if (string.IsNullOrEmpty(key))
        return default;
            
      if (!PlayerPrefs.HasKey(key))
        return default;
            
      var json = PlayerPrefs.GetString(key);
            
      return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
    }

    public bool HasKey(string key)
    {
      return !string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key);
    }

    public void Delete(string key)
    {
      if (string.IsNullOrEmpty(key))
        return;
            
      if (!PlayerPrefs.HasKey(key))
        return;
            
      PlayerPrefs.DeleteKey(key);
      PlayerPrefs.Save();
    }
  }
}