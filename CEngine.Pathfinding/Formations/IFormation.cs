using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding.Formations
{

    /// <summary>
    /// Defines the contact that all formations must implement.
    /// Formation should be generated or provided on the fly
    /// by calling <see cref="GetPositions(int)"/>.
    /// </summary>
    public interface IFormation
    {
        List<Vector3> GetPositions(int unitCount);
    }

}