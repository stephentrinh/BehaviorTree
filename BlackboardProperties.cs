using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/***************************************************************************************
*    Title: Behavior Tree
*    Author: Stephen Trinh
*    Date: 4/28/18
*    Code version: 1.0
*    Availability: https://github.com/stephentrinh/BehaviorTree
*
***************************************************************************************/

namespace STBehaviorTree
{
    public class BlackboardProperties
    {
        public static BlackboardProperty<BlackboardString> Message = new BlackboardProperty<BlackboardString>("Message");
    }
}