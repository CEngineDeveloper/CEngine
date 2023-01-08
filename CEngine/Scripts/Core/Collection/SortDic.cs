using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CYM
{
    [Serializable]
    [Unobfus]
    public class SortDic<TKey,TValue>:Dictionary<TKey,TValue>, IDeserializationCallback, ISerializable
    {
        public List<TValue> SortData { get; private set; } = new List<TValue>();
        public SortDic() : base()
        {

        }

        public new void Add(TKey key,TValue value)
        {
            if (ContainsKey(key))
                return;
            SortData.Add(value) ;
            base.Add(key,value);
        }
        public new void Remove(TKey key)
        {
            if (!ContainsKey(key))
                return;
            SortData.Remove(this[key]);
            base.Remove(key);
        }
        public new void Clear()
        {
            SortData.Clear();
            base.Clear();
        }

        #region data
        public SortDic(SerializationInfo info, StreamingContext context):base(info, context)
        {
            SortData = (List<TValue>)info.GetValue("listData", typeof(List<TValue>));
            foreach (var item in this)
            {
                SortData.Add(item.Value);
            }
        }
        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            SortData.Clear();
            foreach (var item in this)
            {
                SortData.Add(item.Value);
            }
            info.AddValue("listData", SortData);
        }
        #endregion
    }
}
