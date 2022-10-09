using UnityEngine;
namespace CYM.AI.BT
{
    public class BTTest : MonoBehaviour
    {
        public float speed = 1;
        Tree _tree;

        void Start()
        {
            _tree = new Tree(
                BT.RepSeq(
                    BT.Log("hello my name is {0}", name),
                    BT.Do(() => MoveTo(new Vector2(0, 2))),
                    BT.Do(() => MoveTo(new Vector2(2, 2))),
                    BT.Log("voila!")
                ));
        }

        public Status MoveTo(Vector2 dest)
        {
            if ((dest - (Vector2)transform.position).sqrMagnitude < 0.01f)
            {
                return Status.Succ;
            }
            transform.position = Vector2.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            return Status.Run;
        }

        void Update()
        {
            _tree.Update();
        }
    }
}
