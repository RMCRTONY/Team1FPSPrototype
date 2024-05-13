using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct serializableVector3
{
    public float x, y, z;

    public Vector3 getPos()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class Stats
{
    //player specific
    public int health;
    public int mana;
    public int speed;
    public int sprintMod;
    public serializableVector3 pos;

    //game options
    public int musicVol;
    public int environmentVol;
    public int playerVol;
    public int enemyVol;

    //camera options
    public int fov;
    public int sensitivity;
    public bool invertY;

}
