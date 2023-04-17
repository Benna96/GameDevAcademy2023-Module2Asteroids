using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : WrapBehaviour
{
    protected override void AddToLevelManager()
    {
        if (LevelManager.instance != null)
            LevelManager.instance.otherLevelContent.Add(GetInstanceID(), gameObject);
    }

    protected override void RemoveFromLevelManager()
    {
        if (LevelManager.instance != null)
            LevelManager.instance.otherLevelContent.Remove(GetInstanceID());
    }
}
