using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CYM
{
    public class ConfigCopy<TData>
        where TData : class, new()
    {
        static ConfigCopy()
        {
            _confToData = ConfToData();
        }
        private static Action<TData, TData> _confToData;

        private static Action<TData, TData> ConfToData()
        {
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type typeConfig1 = typeof(TData);
            Type typeConfig2 = typeof(TData);
            ParameterExpression tConfig1 = Expression.Parameter(typeConfig1, "TData"), tConfig2 = Expression.Parameter(typeConfig2, "TData");
            List<BinaryExpression> LsExp = new List<BinaryExpression>();
            foreach (PropertyInfo prop in typeConfig2.GetProperties(flag))
            {
                var cAttr = typeConfig1.GetProperty(prop.Name, flag);
                if (cAttr != null)
                {
                    MemberExpression configMemb1 = Expression.Property(tConfig1, prop.Name);
                    MemberExpression configMemb2 = Expression.Property(tConfig2, prop.Name);
                    BinaryExpression setValue = Expression.Assign(configMemb2, configMemb1);
                    LsExp.Add(setValue);
                }
            }
            BlockExpression body = Expression.Block(typeof(void), LsExp);
            return Expression.Lambda<Action<TData, TData>>(body, tConfig1, tConfig2).Compile();
        }

        public static void To(TData config1, TData config2, bool safe = false)
        {
            if (safe)
            {
                if (config1 == null || config2 == null)
                    return;
            }
            else
            {
                if (config1 == null) config1 = new TData();
                if (config2 == null) config2 = new TData();
            }

            _confToData(config1, config2);
        }
    }
}
