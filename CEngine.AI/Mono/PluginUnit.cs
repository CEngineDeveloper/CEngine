using CYM.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        public BaseAIMgr AIMgr { get; protected set; }
        public float Importance { get; set; }
    }
}