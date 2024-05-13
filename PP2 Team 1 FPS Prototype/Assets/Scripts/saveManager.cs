using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.Runtime.Serialization;

public class saveManager : MonoBehaviour
{
    private playerController _player;

    public void Awake()
    {
        _player = GameObject.FindObjectOfType<playerController>();
        load();
    }

    public void save()
    {
        Debug.Log("Saving!");

        FileStream file = new FileStream(Application.persistentDataPath + "/Player.dat", FileMode.OpenOrCreate);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, _player.myStats);
        }
        catch (SerializationException e)
        {
            Debug.LogError("There was an issue serializing this data: " + e.Message);
        }
        finally
        {
            file.Close();
        }
    }

    public void load()
    {
        FileStream file = new FileStream(Application.persistentDataPath + "/Player.dat", FileMode.Open);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            _player.myStats = (Stats)formatter.Deserialize(file);
        }
        catch(SerializationException e)
        {
            Debug.LogError("Error Deserializing Data" + e.Message);
        }
        finally
        {
            file.Close();
        }
    }
}
