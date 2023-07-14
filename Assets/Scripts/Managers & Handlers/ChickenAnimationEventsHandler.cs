using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenAnimationEventsHandler : AnimationEventsHandler
{
    public void LauchEgg()
    {
        ChickenEnemy chickenEnemy = enemy as ChickenEnemy;
        chickenEnemy.EnableEggnade();
    }
}
